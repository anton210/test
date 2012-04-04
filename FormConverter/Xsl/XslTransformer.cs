using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace FormConverter.Xsl
{
    class XslTransformer
    {
        public static MemoryStream Transform(string transformationName, Stream template, XsltArgumentList arguments)
        {
            return Transform(ResourcesManager.GetDocument(transformationName), template, arguments);
        }

        public static MemoryStream Transform(Stream transformation, Stream template, XsltArgumentList arguments)
        {
            template.Seek(0, SeekOrigin.Begin);
            transformation.Seek(0, SeekOrigin.Begin);

            XslCompiledTransform xslProcessor = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings();
            settings.EnableScript = true;

            XmlReader transform = XmlReader.Create(transformation);
            XmlReader input = XmlReader.Create(template);

            xslProcessor.Load(transform, settings, null);
            MemoryStream resultStream = new MemoryStream();
            //Execute the transformation.
            xslProcessor.Transform(input, arguments, resultStream);
            return resultStream;
        }

        public static MemoryStream Transform(string transformationName, string templateName, XsltArgumentList arguments)
        {
            return Transform(ResourcesManager.GetDocument(transformationName), ResourcesManager.GetDocument(templateName), arguments);
        }

        public static void CopyAdditionalParams(Dictionary<string, string> additionalParams, XsltArgumentList arguments)
        {
            foreach (KeyValuePair<string, string> pair in additionalParams)
            {
                arguments.AddParam(pair.Key, "", pair.Value);
            }
        }
    }
}
