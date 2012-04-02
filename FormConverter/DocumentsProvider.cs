using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace FormConverter
{
    class DocumentsProvider
    {

        static public XmlDocument GetDocumentXml(string name)
        {
            Stream stream = GetDocument(name);
            if (stream == null)
            {
                return null;
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(stream);
            return xmlDocument;
        }

        static public Stream GetDocument(string name)
        {
            StringBuilder sb = new StringBuilder(name);
            sb.Replace("/", ".");
            sb.Replace("\\", ".");

            Stream stream = System.Reflection.Assembly.GetExecutingAssembly().
                GetManifestResourceStream(string.Concat("FormConverter.Files.", sb.ToString()));

            return stream;
        }
    }
}
