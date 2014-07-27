using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Spike.Build
{
    internal sealed class Model
    {
        internal List<ComplexType> ComplexTypes { get; } = new List<ComplexType>();

        internal List<Operation> Sends { get; } = new List<Operation>();

        internal List<Operation> Receives { get; } = new List<Operation>();


        private void AddComplexType(XElement element)
        {
            var typeName = element.Attribute("Name").Value;
            var definition = element.Descendants();

            //if has definition
            if (definition.Count() > 0)
            {
                if (ComplexTypes.Any(ct => ct.Name == typeName))
                    Console.WriteLine("error");

                var complexType = new ComplexType(typeName);
                
                foreach (var xmember in element.Descendants().Where(member => member.Name.LocalName == "Member"))
                {
                    var type = xmember.Attribute("Type").Value;
                    //TODO Fix Enum
                    if (type == "Enum")
                        type = "Int32";

                    if (type == "ComplexType")
                    {
                        type = xmember.Attribute("Class").Value;
                        AddComplexType(xmember); //recursivity
                    }
                    complexType.Members.Add(new Member(
                        xmember.Attribute("Name").Value,
                        type
                        ));
                }

                ComplexTypes.Add(complexType);

            }
        }


        private List<Member> GetMembers(XElement element) {          
            var members = new List<Member>();
            foreach (var xmember in element.Descendants().Where(member => member.Name.LocalName == "Member"))
            {
                var type = xmember.Attribute("Type").Value;

                //TODO Fix Enum
                if (type == "Enum")
                    type = "Int32";

                if (type == "ComplexType")
                {
                    type = xmember.Attribute("Class").Value;
                    AddComplexType(xmember); //recursivity
                }
                members.Add(new Member(
                    xmember.Attribute("Name").Value,
                    type
                    ));
            }

            return members;
        }

        internal static Model GetFrom(string location)
        {
            try
            {
                var model = new Model();
                
                //document.Load(location);
                //var document = XDocument.Load("http://54.88.210.109/spml?file=MyChatProtocol"); ////http://54.88.210.109/spml/all
                var document = XDocument.Load("exemple2.spml");
                

                var protocolName = document?.Root.Attribute(@"Name")?.Value;
                if(protocolName == null)
                    Console.WriteLine("protocolName error");


                var SignBuilder = new StringBuilder();
                var operations = document.Descendants()
                                 .Where(operation => operation.Name.LocalName == "Operation");
                
                //ProtocolName.Push.OperationName.[MemberTypes].[]
                foreach (var xoperation in operations)
                {
                    SignBuilder.Clear();
                    SignBuilder.Append(protocolName);
                    SignBuilder.Append('.');

                    var xreceive = xoperation.Descendants().FirstOrDefault(element => element.Name.LocalName == "Outgoing");
                    var xsend = xoperation.Descendants().FirstOrDefault(element => element.Name.LocalName == "Incoming");;

                    List<Member> sendMembers;
                    List<Member> receiveMembers;

                    var compressSend = false;
                    var compressReceive = false;

                    switch (xoperation.Attribute("Compression")?.Value) {
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
                            Console.WriteLine("error");
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

                    //add receive
                    if (receiveMembers != null && receiveMembers.Count > 0)
                        SignBuilder.Append(receiveMembers.Select(member => member.Type).Aggregate((type1, type2) => string.Format("{0}.{1}", type1, type2)));                    


                    SignBuilder.Append("].[");

                    //add sends
                    if (sendMembers != null && sendMembers.Count > 0)
                        SignBuilder.Append(sendMembers.Select(member => member.Type).Aggregate((type1, type2) => string.Format("{0}.{1}", type1, type2)));
                    
                    SignBuilder.Append("]");

                    var id = SignBuilder.ToString().GetMurmurHash3();
                    if (receiveMembers != null)
                    {
                        var operation = new Operation(id, name, compressReceive);
                        operation.Members.AddRange(receiveMembers);
                        model.Receives.Add(operation);
                    }

                    if (sendMembers != null)
                    {
                        var operation = new Operation(id, name, compressSend);
                        operation.Members.AddRange(sendMembers);
                        model.Sends.Add(operation);
                    }
                    //Console.WriteLine(xoperation.Name);
                }

                return model;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.Read();
            }
            return null;
        }

        //Avoid default constructor
        private Model() { }
    }
}


/*
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
