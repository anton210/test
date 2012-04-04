using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace FormConverter.Xsf
{
    public class XsfManifest
    {
        public XmlDocument Xml { get; private set; }
        public XmlSchema Schema { get; private set; }
        public XsfView DefaultView { get; private set; }
        public IDictionary<string, XsfView> Views { get; private set; }
        public string TemplateName { get; private set; }
        public string InitialTemplateFile { get; private set; }

        private const string XsfNamespace = @"http://schemas.microsoft.com/office/infopath/2003/solutionDefinition";
        private const string XsdNamespace = @"http://www.w3.org/2001/XMLSchema";

        public IList<FormField> Fields = new List<FormField>();

        public static XmlNamespaceManager CreateNameSpaceManager(XmlNameTable nameTable) 
        {
            var xmlns = new XmlNamespaceManager(nameTable);
            xmlns.AddNamespace("xsf", XsfNamespace);
            xmlns.AddNamespace("xsd", XsdNamespace);
            return xmlns;
        }

        public XsfManifest(XmlDocument manifest, XmlSchema schema)
        {
            Xml = manifest;
            var xmlns = CreateNameSpaceManager(manifest.NameTable);

            // Template information
            var template = manifest.SelectSingleNode("/xsf:xDocumentClass/xsf:fileNew/xsf:initialXmlDocument", xmlns);
            if ( template == null ) throw new ArgumentException("Wrong manifest format. Can't find xsf:initialXmlDocument element", "manifest");
            TemplateName = template.Attributes["caption"].Value;
            InitialTemplateFile = template.Attributes["href"].Value;

            // Find and store views 

            XmlNode views = manifest.SelectSingleNode("/xsf:xDocumentClass/xsf:views", xmlns);
            string defaultViewName = views.Attributes["default"].Value;

            Views = new Dictionary<string, XsfView>();

            foreach ( XmlNode viewNode in views.ChildNodes )
            {
                var view = new XsfView(viewNode, xmlns);
                if ( view.Name == defaultViewName ) DefaultView = view;
                Views.Add(view.Name, view);
            }

            // Find root schema namespace

            var schemaMetadata = new XsfSchemaMetadata(manifest);

            xmlns.AddNamespace("my", schemaMetadata.Namespace);

            Schema = schema;

            var alreadyAddedNodes = new List<string>();
            var schemaElements = Schema.Items.Cast<XmlSchemaElement>().ToList();

            for ( int i = 0; i < schemaElements.Count; i++ )
            {
                var xmlSchemaElement = schemaElements[i];
                if ( alreadyAddedNodes.Contains(xmlSchemaElement.Name) ) continue;
                var field = new FormField();
                FillField(field, schemaElements, xmlSchemaElement, alreadyAddedNodes);
                Fields.Add(field);
            }
        }

        public static XsfManifest FromFile(string fileName)
        {
            var manifest = new XmlDocument();
            manifest.Load(fileName);

            var schemaMetadata = new XsfSchemaMetadata(manifest);

            var schemaPath = Path.Combine(Path.GetDirectoryName(fileName), schemaMetadata.FileName);
            XmlSchema schema = null;
            using ( var reader = new StreamReader(schemaPath) )
            {
                schema = XmlSchema.Read(reader, (s, e) => { /* empty validation handler */ });
            }

            return new XsfManifest(manifest, schema);
        }

        public static XsfManifest FromBundle(FilesBundle bundle)
        {
            var manifest = new XmlDocument();
            manifest.Load(bundle["manifest.xsf"]);

            var schemaMetadata = new XsfSchemaMetadata(manifest);
            
            XmlSchema schema = null;
            using (StreamReader reader = new StreamReader(bundle[schemaMetadata.FileName]))
            {
                schema = XmlSchema.Read(reader, (s, e) => { /* empty validation handler */ });
            }

            return new XsfManifest(manifest, schema);
        }
       

        private void FillField(FormField field, List<XmlSchemaElement> container, XmlSchemaElement refNode, IList<string> addedElements)
        {
            try
            {
                if (addedElements.Contains(refNode.Name)) return;

                var node = container.Find(e => (e.Name == refNode.Name || e.Name == refNode.RefName.Name));
            
                string name = node.Name ?? node.RefName.Name;
                field.Name = name;
                addedElements.Add(name);
                var fieldType = node.SchemaType as XmlSchemaComplexType;
                field.IsContainer = fieldType != null;
                if ( fieldType != null )
                {
                    field.Fields = new List<FormField>();
                    foreach ( var item in ((XmlSchemaSequence)(fieldType.Particle)).Items )
                    {
                        var element = item as XmlSchemaElement;
                        if (element == null) continue;
                        
                        var newField = new FormField();
                        FillField(newField, container, element, addedElements);
                        field.Fields.Add(newField);
                    }
                
                }
            }
            catch ( Exception ex)
            {

                throw ex;
            }

        }

    }
}
