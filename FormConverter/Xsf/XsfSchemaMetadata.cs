using System;
using System.Xml;

namespace Xsf
{
    public class XsfSchemaMetadata
    {
        public string FileName { get; private set; }
        public string Namespace { get; private set; }

        internal XsfSchemaMetadata(XmlDocument manifest)
        {
            // Find root schema namespace

            var xmlns = XsfManifest.CreateNameSpaceManager(manifest.NameTable);

            XmlNode schemaNode =
                manifest.SelectSingleNode(
                    "/xsf:xDocumentClass/xsf:documentSchemas/xsf:documentSchema[@rootSchema='yes']", xmlns);

            if ( schemaNode == null ) throw new ArgumentException("manifest");

            string schemaLocation = schemaNode.Attributes["location"].Value;
            var schemaLocationParts = schemaLocation.Split(new[] { ' ' });

            Namespace = schemaLocationParts[0];
            FileName = schemaLocation.Substring(Namespace.Length + 1);
        }
    }
}