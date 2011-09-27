/*
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace GXT.IO
{
    public class gxtXMLSerializer
    {
        /// <summary>
        /// Writes passed in data to an appropriate xml file at the
        /// given file path.
        /// </summary>
        /// <typeparam name="T">Generic data type</typeparam>
        /// <param name="filePath">Destination of xml file</param>
        /// <param name="data">Data to write out</param>
        public static void Write<T>(string filePath, T data)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                IntermediateSerializer.Serialize(writer, data, null);
            }
        }

        // read method
    }
}
*/