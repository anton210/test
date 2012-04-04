using System;
using System.Xml;

namespace FormConverter.Xsf
{
    public class XsfView : IXsfView
    {
        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string PrintViewName { get; private set; }
        public string TransformFileName { get; private set; }

        public XsfView(XmlNode viewNode, XmlNamespaceManager xmlns)
        {
            Name = viewNode.Attributes["name"].Value;
            Caption = viewNode.Attributes["caption"].Value;
            PrintViewName = viewNode.Attributes["printView"] != null ? viewNode.Attributes["printView"].Value : Name;
            string transformQuery = String.Format("/xsf:xDocumentClass/xsf:views/xsf:view[@name='{0}']/xsf:mainpane", Name);
            TransformFileName = viewNode.SelectSingleNode(transformQuery, xmlns).Attributes["transform"].Value;
        }

        public override string ToString()
        {
            return Caption;
        }
    }
}