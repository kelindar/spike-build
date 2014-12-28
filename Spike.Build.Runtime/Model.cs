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
    /// <summary>
    /// Represents a SPML Definition model.
    /// 
    /// </summary>
    public sealed class Model
    {
        /// <summary>
        /// Constructs a new instance of an object.
        /// </summary>
        public Model ()
	    {
            this.CustomTypes = new List<CustomType>();
            this.Sends = new List<Operation>();
            this.Receives = new List<Operation>();
	    }

        /// <summary>
        /// Gets the list of complex types in the model.
        /// </summary>
        public List<CustomType> CustomTypes 
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of send operations in the model.
        /// </summary>
        public List<Operation> Sends
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of receive operations in the model.
        /// </summary>
        public List<Operation> Receives
        {
            get;
            private set;
        }

        private Member GetMember(XElement xmember)
        {
            var type = xmember.GetAttributeValue("Type");
            if (type == null)
                throw new ProtocolMalformedException("All members must have a Type.");

            var name = xmember.GetAttributeValue("Name");
            if (name == null)
                throw new ProtocolMalformedException("All members must have a Name.");

            // Is this a list?
            var isList = false;
            var isCustom = false;
            if (type.StartsWith("ListOf"))
            {
                isList = true;
                type = type.Substring(6);
            }

            // In client Enum is Int32
            if (type == "Enum")
                type = "Int32";

            if (type == "ComplexType")
            {
                isCustom = true;
                type = xmember.GetAttributeValue("Class");
                if (type == null)
                    throw new ProtocolMalformedException("All members of type ComplexType/ListOfComplexType must have a Class.");

                // Get the class of the complex type member
                AddComplexType(xmember);
            }

            return new Member(
                name,
                type,
                isList,
                isCustom
                );
        }

        private void AddComplexType(XElement element)
        {
            // Get the type name
            var typeName = element.GetAttributeValue("Class");
            if(typeName == null)
                throw new ProtocolMalformedException("All members of type ComplexType/ListOfComplexType must have a Class.");


            var members = element
                .Elements()
                .Where(member => member.Name.LocalName == "Member");

            //if has members
            if (members.Count() > 0)
            {
                if (CustomTypes.Any(ct => ct.Name == typeName))
                    throw new ProtocolMalformedException("Complex type have 2 definitions");

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

        internal void Load(string location)
        {
            try
            {
                // Load the document
                var document = XDocument.Load(location);
                if (document == null)
                    throw new FileLoadException("Unable to load the document.");

                // Get the protocol name
                var protocolName = document.Root.GetAttributeValue(@"Name");
                if (protocolName == null)
                    throw new ProtocolMalformedException("Protocol name not found.");


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

                    // Check if we have a compression applied
                    var compressSend = false;
                    var compressReceive = false;
                    var compression = xoperation.GetAttributeValue("Compression");
                    if (compression != null)
                    {
                        switch (compression)
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
                    }

                    // Get the direction
                    var direction = xoperation.GetAttributeValue("Direction");
                    if (direction != null && direction == "Push")
                    {
                        SignBuilder.Append("Push");
                        SignBuilder.Append('.');

                        //receive always exist
                        if (xreceive == null)
                            receiveMembers = new List<Member>();
                        else
                            receiveMembers = GetMembers(xreceive);

                        //never send
                        if (xsend != null)
                            throw new ProtocolMalformedException("A push operation can not contain an 'Incoming' element.");
                        sendMembers = null;
                    }
                    else
                    {
                        SignBuilder.Append("Pull");
                        SignBuilder.Append('.');

                        if (xreceive == null)
                            receiveMembers = null;
                        else
                            receiveMembers = GetMembers(xreceive);

                        //add request always
                        if (xsend == null)
                            sendMembers = new List<Member>();
                        else
                            sendMembers = GetMembers(xsend);

                    }

                    var name = xoperation.Attribute("Name").Value;
                    SignBuilder.Append(name);
                    SignBuilder.Append(".[");

                    //add receive members to signature
                    if (receiveMembers != null && receiveMembers.Count > 0)
                        SignBuilder.Append(receiveMembers
                            .Select(member => member.IsList ? string.Format("ListOf{0}", member.IsCustom ? "ComplexType" : member.Type) : member.IsCustom ? "ComplexType" : member.Type).Aggregate((type1, type2) => string.Format("{0}.{1}", type1, type2)));

                    SignBuilder.Append("].[");

                    //add sends members to signature
                    if (sendMembers != null && sendMembers.Count > 0)
                        SignBuilder.Append(sendMembers
                            .Select(member => member.IsList ? string.Format("ListOf{0}", member.IsCustom ? "ComplexType" : member.Type) : member.IsCustom ? "ComplexType" : member.Type).Aggregate((type1, type2) => string.Format("{0}.{1}", type1, type2)));

                    SignBuilder.Append("]");

                    //Console.WriteLine(SignBuilder.ToString());
                    
                    var id = SignBuilder.ToString().GetMurmurHash3();
                    if (receiveMembers != null)
                    {
                        var operation = new Operation(id, name + "Inform", compressReceive);
                        operation.Members.AddRange(receiveMembers);

                        Receives.Add(operation);
                    }

                    if (sendMembers != null)
                    {
                        var operation = new Operation(id, name, compressSend);
                        operation.Members.AddRange(sendMembers);

                        Sends.Add(operation);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("SPML document was not found or unreachable.");
            }            
        }        
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
