#region Copyright (c) 2009-2013 Misakai Ltd.
/*************************************************************************
 * 
 * ROMAN ATACHIANTS - CONFIDENTIAL
 * ===============================
 * 
 * THIS PROGRAM IS CONFIDENTIAL  AND PROPRIETARY TO  ROMAN  ATACHIANTS AND 
 * MAY  NOT  BE  REPRODUCED,  PUBLISHED  OR  DISCLOSED TO  OTHERS  WITHOUT 
 * ROMAN ATACHIANTS' WRITTEN AUTHORIZATION.
 *
 * COPYRIGHT (c) 2009 - 2012. THIS WORK IS UNPUBLISHED.
 * All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is,  and remains the property 
 * of Roman Atachiants  and its  suppliers,  if any. The  intellectual and 
 * technical concepts contained herein are proprietary to Roman Atachiants
 * and  its suppliers and may be  covered  by U.S.  and  Foreign  Patents, 
 * patents in process, and are protected by trade secret or copyright law.
 * 
 * Dissemination of this information  or reproduction  of this material is 
 * strictly  forbidden  unless prior  written permission  is obtained from 
 * Roman Atachiants.
*************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Spike.Build
{
    public abstract class BuilderBase : IBuilder
    {
        #region Static Output
        public static MultiTextWriter Out { get; set; }
        static BuilderBase()
        {
            if(Out == null)
                Console.SetOut(Out = new MultiTextWriter(Console.Out));
        }
        #endregion

        protected bool fIsValid;

        #region IBuilder Members

        /// <summary>
        /// Verify the XML document in contentReader against the schema in XMLClassGeneratorSchema.xsd
        /// </summary>
        /// <param name="contentReader">TextReader for XML document</param>
        public void Validate(string inputFileContent, string inputFilePath)
        {
            //Validate the XML file against the schema
            using (StringReader contentReader = new StringReader(inputFileContent))
            {

                // Options for XmlReader object can be set only in constructor. After the object is created, 
                // they become read-only. Because of that we need to create XmlSettings structure, 
                // fill it in with correct parameters and pass into XmlReader constructor.
                XmlReaderSettings validatorSettings = new XmlReaderSettings();
                validatorSettings.ValidationType = ValidationType.Schema;
                validatorSettings.XmlResolver = null;
                validatorSettings.ValidationEventHandler += OnSchemaValidationError;

                //Schema is embedded in this assembly. Get its stream
                //Stream schema = this.GetType().Assembly.GetManifestResourceStream("Spike.Build.Runtime.SpikeSchema.xsd");
                using( var schema = new StringReader(Spike.Build.Runtime.Properties.Resources.SpikeSchema))
                using (XmlTextReader schemaReader = new XmlTextReader(schema))
                {
                    try
                    {
                        validatorSettings.Schemas.Add("http://www.spike-engine.com/2011/spml", schemaReader);
                    }
                    // handle errors in the schema itself
                    catch (XmlException e)
                    {
                        OnError(4, "InvalidSchemaFileEmbeddedInGenerator " + e.ToString(), 1, 1);
                        fIsValid = false;
                        return;
                    }
                    // handle errors in the schema itself
                    catch (XmlSchemaException e)
                    {
                        OnError(4, "InvalidSchemaFileEmbeddedInGenerator " + e.ToString(), 1, 1);
                        fIsValid = false;
                        return;
                    }

                    using (XmlReader validator = XmlReader.Create(contentReader, validatorSettings, inputFilePath))
                    {
                        fIsValid = true;

                        while (validator.Read())
                            ;   //empty body
                    }
                }
            }
        }

        /// <summary>
        /// Receives any errors that occur while validating the documents's schema.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Details about the validation error that has occurred</param>
        private void OnSchemaValidationError(object sender, ValidationEventArgs args)
        {
            //signal that validation of document against schema has failed
            fIsValid = false;

            //Report the error (so that it is shown in the error list)
            OnError(4, args.Exception.Message, (uint)args.Exception.LineNumber - 1, (uint)args.Exception.LinePosition - 1);
        }

        /// <summary>
        /// Gets whether the XML schema is valid or not
        /// </summary>
        public bool IsValid
        {
            get { return fIsValid;  }
        }

        /// <summary>
        /// Invokes an error when one is occured
        /// </summary>
        public virtual void OnError(uint level, string message, uint line, uint column)
        {
            if (Error != null)
            {
                Error.Invoke(this, new BuildErrorEventArgs(level, message, line, column));
            }
        }

        public event EventHandler<BuildErrorEventArgs> Error;
        public abstract string GenerateCode(string inputFileContent);

        #endregion

        #region Static Methods
        public static T Deserialize<T>(string inputFileContent) where T : class
        {
            T result = null;
            using (var reader = new StringReader(inputFileContent))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                result = (T)serializer.Deserialize(reader);
            }
            return result;
        }
		
        #endregion
    }

    #region MultiTextWriter
    public class MultiTextWriter : TextWriter
    {
        private List<TextWriter> m_Streams;
        private ConsoleColor DefaultColor;

        public MultiTextWriter(params TextWriter[] streams)
        {
            m_Streams = new List<TextWriter>(streams);

            if (m_Streams.Count < 0)
                throw new ArgumentException("You must specify at least one stream.");

            DefaultColor = Console.ForegroundColor;
        }

        public void Add(TextWriter tw)
        {
            m_Streams.Add(tw);
        }

        public void Remove(TextWriter tw)
        {
            m_Streams.Remove(tw);
        }

        public override void Write(char ch)
        {
            for (int i = 0; i < m_Streams.Count; i++)
                m_Streams[i].Write(ch);
        }

        public override void WriteLine(string line)
        {
            for (int i = 0; i < m_Streams.Count; i++)
                m_Streams[i].WriteLine(line);
        }

        public override void WriteLine(string line, params object[] args)
        {
            WriteLine(String.Format(line, args));
        }


        public void Write(ConsoleColor color, string str)
        {
            Console.ForegroundColor = color;
            Write(str);
            Console.ForegroundColor = DefaultColor;
        }

        public void WriteLine(ConsoleColor color, string line)
        {
            Console.ForegroundColor = color;
            WriteLine(line);
            Console.ForegroundColor = DefaultColor;
        }

        public void WriteLine(ConsoleColor color, string format, params object[] arg)
        {
            Console.ForegroundColor = color;
            WriteLine(format, arg);
            Console.ForegroundColor = DefaultColor;
        }

        public void WriteLine(ConsoleColor color, string format, object arg0)
        {
            Console.ForegroundColor = color;
            WriteLine(format, arg0);
            Console.ForegroundColor = DefaultColor;
        }

        public void WriteLine(ConsoleColor color, string format, object arg0, object arg1)
        {
            Console.ForegroundColor = color;
            WriteLine(format, arg0, arg1);
            Console.ForegroundColor = DefaultColor;
        }


        public void WriteLine(ConsoleColor color, string format, object arg0, object arg1, object arg2)
        {
            Console.ForegroundColor = color;
            WriteLine(format, arg0, arg1, arg2);
            Console.ForegroundColor = DefaultColor;
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
    #endregion
}
