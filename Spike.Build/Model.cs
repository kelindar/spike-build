/************************************************************************
*
* Copyright (C) 2009-2014 Misakai Ltd
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* 
*************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Spike.Build
{
    internal sealed class Model
    {
        internal List<CustomType> CustomTypes { get; } = new List<CustomType>();

        internal List<Operation> Sends { get; } = new List<Operation>();

        internal List<Operation> Receives { get; } = new List<Operation>();

        private Member GetMember(XElement xmember)
        {
            var type = xmember.Attribute("Type")?.Value;
            if (string.IsNullOrWhiteSpace(type))
                Program.Exit("All member must have a Type");

            var name = xmember.Attribute("Name")?.Value;
            if (string.IsNullOrWhiteSpace(type))
                Program.Exit("All member must have a Name");

            var isList = false;
            if (type.StartsWith("ListOf"))
            {
                isList = true;
                type = type.Substring(6);
            }

            //In client Enum is Int32
            if (type == "Enum")
                type = "Int32";

            if (type == "ComplexType")
            {
                type = xmember.Attribute("Class")?.Value;
                if (string.IsNullOrWhiteSpace(type))
                    Program.Exit("All Type ComplexType/ListOfComplexType must have a Class");

                AddComplexType(xmember);
            }



            return new Member(
                name,
                type,
                isList
                );
        }

        private void AddComplexType(XElement element)
        {
            var typeName = element.Attribute("Class")?.Value;
            if (string.IsNullOrWhiteSpace(typeName))
                Program.Exit("All Type ComplexType/ListOfComplexType must have a Class");

            var members = element.Elements().Where(member => member.Name.LocalName == "Member");

            //if has members
            if (members.Count() > 0)
            {
                if (CustomTypes.Any(ct => ct.Name == typeName))
                    Program.Exit("Complex type have 2 definitions");

                var complexType = new CustomType(typeName);

                foreach (var xmember in members)
                    complexType.Members.Add(GetMember(xmember));

                CustomTypes.Add(complexType);

            }
        }



        private List<Member> GetMembers(XElement element)
        {
            var members = new List<Member>();

            foreach (var xmember in element.Elements().Where(member => member.Name.LocalName == "Member"))
                members.Add(GetMember(xmember));

            return members;
        }

        internal static Model GetFrom(string location)
        {
            try
            {
                var model = new Model();
                var document = XDocument.Load(location);


                var protocolName = document?.Root.Attribute(@"Name")?.Value;
                if (protocolName == null)
                    Program.Exit("No protocol name");


                var SignBuilder = new StringBuilder();
                var operations = document.Descendants()
                    .Where(operation => operation.Name.LocalName == "Operation");

                //ProtocolName.Push.OperationName.[MemberTypes].[]
                foreach (var xoperation in operations)
                {
                    SignBuilder.Clear();
                    SignBuilder.Append(protocolName);
                    SignBuilder.Append('.');

                    var xreceive = xoperation.Elements().FirstOrDefault(element => element.Name.LocalName == "Outgoing");
                    var xsend = xoperation.Elements().FirstOrDefault(element => element.Name.LocalName == "Incoming"); ;

                    List<Member> sendMembers;
                    List<Member> receiveMembers;

                    var compressSend = false;
                    var compressReceive = false;

                    switch (xoperation.Attribute("Compression")?.Value)
                    {
                        case "Both":
                            compressSend = true;
                            compressReceive = true;
                            break;
                        case "Incoming":
                            compressSend = true;
                            break;
                        case "Outgoing":
                            compressReceive = true;
                            break;
                    }


                    if (xoperation.Attribute("Direction")?.Value == "Push")
                    {
                        SignBuilder.Append("Push");
                        SignBuilder.Append('.');

                        //receive always exist
                        if (xreceive == null)
                            receiveMembers = new List<Member>();
                        else
                            receiveMembers = model.GetMembers(xreceive);

                        //never send
                        if (xsend != null)
                            Console.WriteLine("error1");
                        sendMembers = null;
                    }
                    else
                    {
                        SignBuilder.Append("Pull");
                        SignBuilder.Append('.');

                        if (xreceive == null)
                            receiveMembers = null;
                        else
                            receiveMembers = model.GetMembers(xreceive);

                        //add request always
                        if (xsend == null)
                            sendMembers = new List<Member>();
                        else
                            sendMembers = model.GetMembers(xsend);

                    }

                    var name = xoperation.Attribute("Name").Value;
                    SignBuilder.Append(name);
                    SignBuilder.Append(".[");

                    //add receive members to signature
                    if (receiveMembers != null && receiveMembers.Count > 0)
                        SignBuilder.Append(receiveMembers.Select(member => member.IsList ? string.Format("ListOf{0}", member.Type) : member.Type).Aggregate((type1, type2) => string.Format("{0}.{1}", type1, type2)));

                    SignBuilder.Append("].[");

                    //add sends members to signature
                    if (sendMembers != null && sendMembers.Count > 0)
                        SignBuilder.Append(sendMembers.Select(member => member.IsList ? string.Format("ListOf{0}", member.Type) : member.Type).Aggregate((type1, type2) => string.Format("{0}.{1}", type1, type2)));

                    SignBuilder.Append("]");

                    var id = SignBuilder.ToString().GetMurmurHash3();
                    if (receiveMembers != null)
                    {
                        var operation = new Operation(id, name + "Inform", compressReceive);
                        operation.Members.AddRange(receiveMembers);
                        model.Receives.Add(operation);
                    }

                    if (sendMembers != null)
                    {
                        var operation = new Operation(id, name, compressSend);
                        operation.Members.AddRange(sendMembers);
                        model.Sends.Add(operation);
                    }
                }

                return model;
            }
            catch (FileNotFoundException)
            {
                Program.Exit("Spml file unreachable");
            }
            return null; //dummy code
        }

        //Avoid default constructor
        private Model() { }
    }
}


/* Todo remove this or push to doc
About spml

en gros PULL:
 - si INCOMING est vide, tu génére quand même un paquet Request, mais il sera sans payload
 - si INCOMING a une valeur, tu fais un paquet Request avec payload
 - si OUTGOING est vide, tu ne génére pas de paquet Inform (et donc pas de receive côté client)
 - si OUTGOING a une valeur, tu fais un paquet Inform avec payload

pour PUSH:
 - à priori y aurait jamais de INCOMING (error si le cas)
 - si OUTGOING est vide, tu génére un Inform sans payload
 - si OUTGOING a une valeur, tu génére un Inform avec payload

par defaut Pull 
*/
