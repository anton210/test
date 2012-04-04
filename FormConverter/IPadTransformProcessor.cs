using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
//using FormsServer.Transform;
using FormConverter.Xsf;

namespace FormConverter
{
    public class IPadTransformProcessor // : IViewTransformProcessor
    {
        public static readonly Guid ProcessorID = Guid.Parse("ed432bfd-48b1-473b-bc4b-e2962c413662");
        
        public Guid GetProcessorID()
        {
            return IPadTransformProcessor.ProcessorID;
        }

        public void ApplyTransform(XmlDocument originalTransform)
        {
            var xmlDocument = originalTransform;
            
            var xmlns = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlns.AddNamespace("xd", @"http://schemas.microsoft.com/office/infopath/2003");

            ApplyCoreLayoutMods(xmlDocument, xmlns);
            ApplyTextboxMods(xmlDocument, xmlns);
            ApplyCheckboxMods(xmlDocument, xmlns);
            RemoveStyleSections(xmlDocument, xmlns);
            ApplyHeadTransform(xmlDocument, xmlns);
            ApplyTableModes(xmlDocument, xmlns);
            ApplyRadioButtonMods(xmlDocument, xmlns);
            ApplySignboxMods(xmlDocument, xmlns);

            double adj = CalculateWidthAdjustment(xmlDocument);
            RemoveStyleAttribute(xmlDocument.DocumentElement, adj);

            //xmlDocument.Save(xslFileName);
        }

        private void ApplyCoreLayoutMods(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var body = (XmlElement) document.DocumentElement.SelectSingleNode("//body");

            var div = document.CreateElement("div");
            
            div.SetAttribute("data-role", "page");
            div.SetAttribute("id", "mainForm");

            for ( int i = 0; i < body.ChildNodes.Count; i++ )
            {
                var node = body.ChildNodes[i].CloneNode(true);
                div.AppendChild(node);
            }

            body.RemoveAll();
            body.AppendChild(div);
        }

        public Dictionary<string, Stream> GetResources()
        {
            var retValue = new Dictionary<string, Stream>
                               {
                                   {
                                       "iPadTransform.css",
                                       Assembly.GetCallingAssembly().GetManifestResourceStream(
                                           "FormsServer.Transform.iPadTransform.css")
                                   },
                                   {
                                       "iPadTransform.js",
                                       Assembly.GetCallingAssembly().GetManifestResourceStream(
                                           "FormsServer.Transform.iPadTransform.js")
                                   },
                                   {
                                       "sign-here.gif",
                                       Assembly.GetCallingAssembly().GetManifestResourceStream(
                                           "FormsServer.Transform.sign-here.gif")
                                   }
                               };
            return retValue; 
        }

        private static void ApplyHeadTransform(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var head = (XmlElement) document.DocumentElement.SelectSingleNode("//head");
            var list = head.SelectNodes("style | meta");
            foreach (XmlNode node in list)
            {
                head.RemoveChild(node);
            }

            // insert head elements
            //<meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1, maximum-scale=1.0" />
            //<meta name="apple-mobile-web-app-capable" content="yes" />
            //<link rel="stylesheet" href="iPadTransform.css" />
            //<link rel="stylesheet" href="http://code.jquery.com/mobile/1.0/jquery.mobile-1.0.css" />
            //<link rel="stylesheet" href="http://static.thomasjbradley.ca/jquery.signaturepad.css">
            //<script type="text/javascript" src="http://code.jquery.com/jquery-1.6.4.min.js"></script>
            //<script type="text/javascript" src="http://code.jquery.com/mobile/1.0/jquery.mobile-1.0.min.js"></script>
            //<script type="text/javascript" src="http://static.thomasjbradley.ca/jquery.signaturepad.min.js"></script>
            //<script type="text/javascript" src="http://static.thomasjbradley.ca/json2.min.js"></script>

            //<script>$(document).ready(function () { $('.sigPad').signaturePad();}); </script>

            XmlElement meta1 = document.CreateElement("meta");
            meta1.SetAttribute("name", "viewport");
            meta1.SetAttribute("content", "user-scalable=no, width=device-width, initial-scale=1, maximum-scale=1.0");

            XmlElement meta2 = document.CreateElement("meta");
            meta2.SetAttribute("name", "apple-mobile-web-app-capable");
            meta2.SetAttribute("content", "yes");

            XmlElement link1 = document.CreateElement("link");
            link1.SetAttribute("rel", "stylesheet");
            link1.SetAttribute("href", @"iPadTransform.css");

            XmlElement link2 = document.CreateElement("link");
            link2.SetAttribute("rel", "stylesheet");
            link2.SetAttribute("href", @"http://code.jquery.com/mobile/1.0/jquery.mobile-1.0.min.css");

            XmlElement link3 = document.CreateElement("link");
            link3.SetAttribute("rel", "stylesheet");
            link3.SetAttribute("href", @"http://static.thomasjbradley.ca/jquery.signaturepad.css");

            XmlElement script1 = document.CreateElement("script");
            script1.SetAttribute("type", @"text/javascript");
            script1.SetAttribute("src", @"http://code.jquery.com/jquery-1.7.1.js");

            XmlElement script2 = document.CreateElement("script");
            script2.SetAttribute("type", @"text/javascript");
            script2.SetAttribute("src", @"http://code.jquery.com/mobile/1.0/jquery.mobile-1.0.js");

            XmlElement script3 = document.CreateElement("script");
            script3.SetAttribute("type", @"text/javascript");
            script3.SetAttribute("src", @"http://static.thomasjbradley.ca/jquery.signaturepad.min.js");

            XmlElement script4 = document.CreateElement("script");
            script4.SetAttribute("type", @"text/javascript");
            script4.SetAttribute("src", @"http://static.thomasjbradley.ca/json2.min.js");

            XmlElement script5 = document.CreateElement("script");
            script5.SetAttribute("type", @"text/javascript");
            script5.SetAttribute("src", @"iPadTransform.js");


            head.AppendChild(meta1);
            head.AppendChild(meta2);
            head.AppendChild(link1);
            head.AppendChild(link2);
            head.AppendChild(link3);
            head.AppendChild(script1);
            head.AppendChild(script2);
            head.AppendChild(script3);
            head.AppendChild(script4);
            head.AppendChild(script5);

        }

        public static void ApplySignboxMods(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var body = (XmlElement) document.DocumentElement.SelectSingleNode("//body");
            
            var dialog = document.CreateElement("div");
            dialog.SetAttribute("data-role", "page");
            dialog.SetAttribute("id", "popup");
            dialog.SetAttribute("data-theme", "c");

            var header = document.CreateElement("div");
            header.SetAttribute("data-role", "header");
            header.SetAttribute("data-position", "inline");

            var headerText = document.CreateElement("h1");
            headerText.InnerText = "Signature Pad";

            var clearButton = document.CreateElement("a");
            clearButton.SetAttribute("id", "sigPadClearButton");
            clearButton.SetAttribute("href", "#");
            clearButton.SetAttribute("data-icon", "refresh");
            clearButton.InnerText = "Clear";

            var content = document.CreateElement("div");
            content.SetAttribute("data-role", "content");

            var form = document.CreateElement("form");
            form.SetAttribute("method", "post");
            form.SetAttribute("class", "sigPad");
            form.SetAttribute("action", "#");
            form.SetAttribute("id", "sigDialogPad");

            var sigWrapper = document.CreateElement("div");
            SetElementClass(sigWrapper, "sig");
            SetElementClass(sigWrapper, "sigWrapper");

            var sigCanvas = document.CreateElement("canvas");
            SetElementClass(sigCanvas, "pad");
            sigCanvas.SetAttribute("height", "200");
            sigCanvas.SetAttribute("width", "500");

            var sigInput = document.CreateElement("input");
            sigInput.SetAttribute("type", "hidden");
            sigInput.SetAttribute("name", "output");
            sigInput.SetAttribute("class", "output");

            var footer = document.CreateElement("div");
            footer.SetAttribute("data-role", "footer");
            
            var footerText = document.CreateElement("h4");
            footerText.InnerText = "";

            header.AppendChild(headerText);
            header.AppendChild(clearButton);

            sigWrapper.AppendChild(sigCanvas);
            sigWrapper.AppendChild(sigInput);

            form.AppendChild(sigWrapper);

            content.AppendChild(form);

            footer.AppendChild(footerText);

            dialog.AppendChild(header);
            dialog.AppendChild(content);
            dialog.AppendChild(footer);

            body.AppendChild(dialog);
            
            var ns = xmlns.LookupPrefix(@"http://schemas.microsoft.com/office/infopath/2003");
            var ctrlIdAttrName = ns + ":CtrlId";

            var signpads = document.SelectNodes(String.Format("//object[@{0}:xctname='inkpicture']", ns), xmlns);

            foreach ( XmlElement signpad in signpads )
            {
                /*
                 <a class="signLink" href="#popup" id="CTRL88_5" data-role="button" data-rel="dialog" data-transition="none">
						  <input type="hidden" class="output" />
						  <img class="preview"/>
					  </a
                 */

                var controlName = signpad.Attributes[ctrlIdAttrName].Value;
                var styleAttr = signpad.Attributes["style"];
                var style = new StyleAttributeString(styleAttr);

                var parent = (XmlElement) signpad.ParentNode;
                if ( parent.LocalName == "if" ) parent = (XmlElement) parent.ParentNode;

                var button = document.CreateElement("a");
                button.SetAttribute("class", "signLink");
                button.SetAttribute("href", "#popup");
                button.SetAttribute("id", controlName);
                //button.SetAttribute("data-role", "button");
                button.SetAttribute("data-rel", "dialog");
                button.SetAttribute("data-transition", "none");
                //button.InnerText = "Signature pad";

                var input = document.CreateElement("input");
                input.SetAttribute("type", "hidden");
                input.SetAttribute("class", "output");

                var img = document.CreateElement("img");
                img.SetAttribute("class", "preview");
                img.SetAttribute("src", "sign-here.gif");
                if ( style.Properties.ContainsKey("width") )
                {
                    var newStyle = new StyleAttributeString();
                    newStyle.AssignProperty("width", style.Properties["width"]);

                    img.SetAttribute("style", newStyle.ToString());
                }

                button.AppendChild(input);
                button.AppendChild(img);

                parent.AppendChild(button);
            }
        }

        private static void ApplyTextboxMods(XmlDocument document, XmlNamespaceManager xmlns)
        {
            //<input type="text" hideFocus="1" class="xdTextBox"
            
            var ns = xmlns.LookupPrefix(@"http://schemas.microsoft.com/office/infopath/2003");
            var ctrlIdAttrName = ns + ":CtrlId";

            var textboxes = document.SelectNodes(String.Format("//span[@{0}:xctname='PlainText']", ns), xmlns);

            foreach ( XmlElement textbox in textboxes )
            {
                var controlName = textbox.Attributes[ctrlIdAttrName].Value;
                var styleAttr = textbox.Attributes["style"];
                
                var parent = ((XmlElement) textbox.ParentNode);

                StyleAttributeString style = null;
                if ( styleAttr != null ) style = new StyleAttributeString(styleAttr.Value);

                XmlElement inputField = null;
                if ( style != null && style.Properties.ContainsKey("overflow-y") )
                {
                    inputField = document.CreateElement("textarea");
                    
                    style.AssignProperty("height", "100%");
                    style.AssignProperty("min-height", "36px");

                    SetElementClass(parent, "textarea-container");
                }
                else
                {
                    inputField = document.CreateElement("input");
                    if ( style != null && style.Properties.ContainsKey("height") )
                    {
                        style.Properties.Remove("height");
                    }
                }

                inputField.SetAttribute("id", controlName);
                inputField.SetAttribute("name", controlName);
                inputField.SetAttribute("type", "text");

                if ( style != null ) inputField.SetAttribute("style", style.ToString());

                parent.AppendChild(inputField);
                

            }

        }

        public static double CalculateWidthAdjustment(XmlDocument xmlDocument)
        {
            const double screenPadding = 4;
            const double screenWidth = 768;
            var sections = xmlDocument.SelectNodes("//*[contains(@class,'xdLayout')] | //*[contains(@class,'xdSection')]");
            double maxWidth = 0;
            foreach (XmlElement section in sections)
            {
                var style = new StyleAttributeString(section.Attributes["style"].Value);
                if ( style.Properties.ContainsKey("width") )
                {
                    double tmp = GetElementWidth(style.Properties["width"]);
                    maxWidth = maxWidth > tmp ? maxWidth : tmp;
                    
                }
            }
            return (screenWidth - (2 * screenPadding)) / maxWidth;
        }

        private static double GetElementWidth(string s)
        {
            double value = 0;
            string measure = s.Substring(s.Length - 2, 2);
            switch ( measure )
            {
                case "pt":
                    {
                        value = Double.Parse(s.Replace("pt", String.Empty).Trim()) * 4 / 3;
                        //value = Double.Parse(s.Replace("pt", String.Empty).Trim(), CultureInfo.InvariantCulture.NumberFormat) * 4 / 3;
                        break;
                    }
                case "px":
                    {
                        value = Double.Parse(s.Replace("px", String.Empty).Trim());
                        //value = Double.Parse(s.Replace("px", String.Empty).Trim(), CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    }
            }
            return value;
        }

        private static void RemoveStyleSections(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var nodes = document.DocumentElement.SelectNodes("//style");
            foreach (XmlNode node in nodes)
            {
                node.RemoveAll();
            }
        }

        private static void ApplyRadioButtonMods(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var radioButtons = document.SelectNodes("//input[@type='radio']");

            var ns = xmlns.LookupPrefix(@"http://schemas.microsoft.com/office/infopath/2003");
            
            foreach ( XmlElement button in radioButtons )
            {
                var controlName = button.Attributes[ns + ":CtrlId"].Value;
                string value = button.Attributes[ns + ":onValue"].Value;
                string bindingProp = button.Attributes[ns + ":binding"].Value;

                button.SetAttribute("value", value);
                button.SetAttribute("name", bindingProp);
                button.SetAttribute("id", controlName);

                XmlElement parent = ( (XmlElement) button.ParentNode );
                
                XmlElement label = document.CreateElement("label");
                label.SetAttribute("for", controlName);

                parent.AppendChild(label);
            }
        }

        private static void ApplyCheckboxMods(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var checkboxes = document.SelectNodes("//input[@type='checkbox']");
            
            var ns = xmlns.LookupPrefix(@"http://schemas.microsoft.com/office/infopath/2003");
            var ctrlIdAttrName = ns + ":CtrlId";
            
            foreach (XmlElement checkbox in checkboxes)
            {
                var controlName = checkbox.Attributes[ctrlIdAttrName].Value;
                string onValue = checkbox.Attributes[ns + ":onValue"].Value;
                string offValue = checkbox.Attributes[ns + ":offValue"].Value;

                XmlElement parent = ( (XmlElement) checkbox.ParentNode );
                parent.SetAttribute("class", "checkbox-wrapper");

                XmlElement select = document.CreateElement("select");
                select.SetAttribute("id", controlName);
                select.SetAttribute("name", controlName);
                select.SetAttribute("data-role", "slider");

                XmlElement optionYes = document.CreateElement("option");
                optionYes.SetAttribute("value", onValue);
                optionYes.AppendChild(document.CreateTextNode("|"));

                XmlElement optionNo = document.CreateElement("option");
                optionNo.SetAttribute("value", offValue);
                optionNo.AppendChild(document.CreateTextNode("O"));

                select.AppendChild(optionNo);
                select.AppendChild(optionYes);

                parent.AppendChild(select);

            }
        }

        public static void ApplyTableModes(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var tables = document.DocumentElement.SelectNodes("//table");
            if ( tables == null ) return;
            foreach (XmlElement table in tables)
            {
                var rows = table.SelectNodes(@"tr | tbody/tr");
                if ( rows == null ) continue;
                var tableModel = new TableCell[100, 100];
                int tableModelRowCount = 0;
                int tableModelColCount = 0;
                var tableCellsList = new List<TableCell>();
                for ( int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var row = (XmlElement) rows[rowIndex];
                    var cells = row.SelectNodes("th | td");
                    if ( cells == null ) continue;
                    for ( int colIndex = 0; colIndex < cells.Count; colIndex++ )
                    {
                        var cell = (XmlElement) cells[colIndex];

                        var colSpan = cell.Attributes["colSpan"] != null
                                          ? Convert.ToInt32(cell.Attributes["colSpan"].Value)
                                          : 1;

                        var rowSpan = cell.Attributes["rowSpan"] != null
                                          ? Convert.ToInt32(cell.Attributes["rowSpan"].Value)
                                          : 1;

                        var styleAttr = cell.Attributes["style"];

                        var style = new StyleAttributeString(styleAttr != null ? styleAttr.Value : String.Empty);

                        var leftBorderVisible = IsCellBorderVisible(style, "border-left");
                        var rightBorderVisible = IsCellBorderVisible(style, "border-right");
                        var topBorderVisible = IsCellBorderVisible(style, "border-top");
                        var bottomBorderVisible = IsCellBorderVisible(style, "border-bottom");

                        var tableCell = new TableCell(cell)
                                            {
                                                RowNumber = rowIndex, 
                                                ColumnNumber = colIndex, 
                                                TopBorderVisible = topBorderVisible, 
                                                RightBorderVisible = rightBorderVisible,
                                                BottomBorderVisible = bottomBorderVisible,
                                                LeftBorderVisible = leftBorderVisible
                                            };

                        tableCellsList.Add(tableCell);

                        tableModelRowCount = Math.Max(tableModelRowCount, rowIndex + rowSpan);
                        tableModelColCount = Math.Max(tableModelColCount, colIndex + colSpan);

                        for ( int i = 0; i < rowSpan; i++ )
                        {
                            var colOffset = 0;
                            for (int j = 0; j < colSpan; j++)
                            {
                                while ( tableModel[rowIndex + i, colIndex + colOffset + j] != null )
                                {
                                    colOffset++;
                                }
                                tableModel[rowIndex + i, colIndex + colOffset + j] = tableCell;
                            }
                        }
                    }
                }

                for ( int row = 0; row < tableModelRowCount; row++ )
                {
                    for ( int col = 0; col < tableModelColCount; col++ )
                    {
                        var cell = tableModel[row, col];
                        if ( !cell.LeftBorderVisible && ( col == 0 || tableModel[row, col - 1].CellType == 0 ) )
                        {
                            cell.CellType = InternalCellType.Outher;
                        }
                        else if ( !cell.TopBorderVisible && ( row == 0 || tableModel[row - 1, col].CellType == 0 ) )
                        {
                            cell.CellType = InternalCellType.Outher;
                        }

                    }
                }

                for ( int row = 0; row < tableModelRowCount; row++ )
                {
                    for (int col = 0; col < tableModelColCount; col++)
                    {
                        var cell = tableModel[row, col];
                        if (cell.CellType == InternalCellType.Inner)
                        {
                            // Top 
                            if (row == 0 || tableModel[row - 1, col].CellType == InternalCellType.Outher)
                            {
                                cell.Edges |= CellEdgeType.Top;
                            }

                            // Left
                            if (col == 0 || tableModel[row, col - 1].CellType == InternalCellType.Outher) 
                            {
                                cell.Edges |= CellEdgeType.Left;
                            }

                            // Bottom 
                            if ( row == tableModelRowCount - 1 || tableModel[row + 1, col].CellType == InternalCellType.Outher )
                            {
                                cell.Edges |= CellEdgeType.Bottom;
                            }

                            // Right
                            if ( col == tableModelColCount - 1 || tableModel[row, col + 1].CellType == InternalCellType.Outher )
                            {
                                cell.Edges |= CellEdgeType.Right;
                            }
                        }
                    }
                }

                foreach (var tableCell in tableCellsList)
                {
                    if ( tableCell.CellType == InternalCellType.Inner ) SetElementClass(tableCell.XmlElement, "inner-cell");
                    if ( tableCell.CellType == InternalCellType.Outher ) SetElementClass(tableCell.XmlElement, "outher-cell");

                    if ( ( tableCell.Edges & CellEdgeType.Top ) == CellEdgeType.Top &&
                         ( tableCell.Edges & CellEdgeType.Left ) == CellEdgeType.Left ) SetElementClass(tableCell.XmlElement, "ui-corner-tl");

                    if ( ( tableCell.Edges & CellEdgeType.Bottom ) == CellEdgeType.Bottom &&
                         ( tableCell.Edges & CellEdgeType.Left ) == CellEdgeType.Left ) SetElementClass(tableCell.XmlElement, "ui-corner-bl");

                    if ( ( tableCell.Edges & CellEdgeType.Top ) == CellEdgeType.Top &&
                         ( tableCell.Edges & CellEdgeType.Right ) == CellEdgeType.Right ) SetElementClass(tableCell.XmlElement, "ui-corner-tr");

                    if ( ( tableCell.Edges & CellEdgeType.Bottom ) == CellEdgeType.Bottom &&
                         ( tableCell.Edges & CellEdgeType.Right ) == CellEdgeType.Right ) SetElementClass(tableCell.XmlElement, "ui-corner-br");

                    if ( ( tableCell.Edges & CellEdgeType.Top ) == CellEdgeType.Top ) SetElementClass(tableCell.XmlElement, "cell-top");
                    if ( ( tableCell.Edges & CellEdgeType.Right ) == CellEdgeType.Right ) SetElementClass(tableCell.XmlElement, "cell-right");
                    if ( ( tableCell.Edges & CellEdgeType.Bottom ) == CellEdgeType.Bottom ) SetElementClass(tableCell.XmlElement, "cell-bottom");
                    if ( ( tableCell.Edges & CellEdgeType.Left ) == CellEdgeType.Left ) SetElementClass(tableCell.XmlElement, "cell-left");

                }

            }
        }

        private class TableCell
        {
            public TableCell(XmlElement xmlElement)
            {
                XmlElement = xmlElement;
                CellType = InternalCellType.Inner;
                Edges = CellEdgeType.None;
            }

            public int RowNumber { get; set; }
            public int ColumnNumber { get; set; }
            public bool LeftBorderVisible { get; set; }
            public bool RightBorderVisible { get; set; }
            public bool TopBorderVisible { get; set; }
            public bool BottomBorderVisible { get; set; }

            public InternalCellType CellType { get; set; }
            public CellEdgeType Edges { get; set; }

            public XmlElement XmlElement { get; private set; }

            public override string ToString()
            {
                return String.Format("{0}:{1} {2}", RowNumber, ColumnNumber, CellType);
            }
        }

        private enum InternalCellType
        {
            Outher = 0,
            Inner
        }

        [Flags]
        private enum CellEdgeType
        {
            None = 0x0,
            Left = 0x1,
            Right = 0x2,
            Top = 0x4,
            Bottom = 0x8
        }

        private static void SetElementClass(XmlElement cell, string cellClassName)
        {
            string @class = String.Empty;
            if (cell.HasAttribute("class"))
            {
                @class = cell.Attributes["class"].Value + " " + cellClassName;
            }
            else
            {
                @class = cellClassName;
            }

            cell.SetAttribute("class", @class);
        }

        private static bool IsCellBorderVisible(StyleAttributeString style, string styleProperty)
        {
            var solidBorders = new[] { "solid", "dashed", "dotted", "double", "groove", "ridge", "inset", "outset" };
            bool borderVisible = false;
            if ( style.Properties.ContainsKey(styleProperty) )
            {
                foreach (var solidBorder in solidBorders)
                {
                    borderVisible = style.Properties[styleProperty].Contains(solidBorder);
                    if ( borderVisible ) break;
                }
            }
            return borderVisible;
        }

        public static void RemoveStyleAttribute(XmlElement element, double widthAdj)
        {
            if ( element == null ) return;

            var preserveStyles = new List<string>(new[]
                                                      {
                                                          "width",
                                                          "height",
                                                          "border",
                                                          "border-bottom",
                                                          "border-top",
                                                          "border-left",
                                                          "border-right",
                                                          "border-bottom-color",
                                                          "border-bottom-left-radius",
                                                          "border-bottom-right-radius",
                                                          "border-bottom-style",
                                                          "border-bottom-width",
                                                          "border-left-color", 
                                                          "border-left-style", 
                                                          "border-left-width", 
                                                          "border-right-color",
                                                          "border-right-style",
                                                          "border-right-width",
                                                          "border-top-color", 
                                                          "border-top-left-radius",
                                                          "border-top-right-radius",
                                                          "border-top-style",
                                                          "border-top-width",
                                                          "vertical-align",
                                                          "overflow-x",
                                                          "overflow-y"
                                                      });

            var styleAdjustments = new Dictionary<string, string>
                                       {
                                           {" 0.5pt", " 0.75pt"}, 
                                       };

            var removeAttributes = new List<string>(new[]
                                                        {
                                                            "size",
                                                            "face",
                                                            "color"
                                                        });

            
            if ( element.Attributes["style"] != null )
            {
                var style = new StyleAttributeString(element.Attributes["style"].Value);
                var newStyle = new StyleAttributeString("");
                foreach (var property in style.Properties)
                {
                    if ( preserveStyles.Contains(property.Key) )
                    {
                        string newValue = property.Value;
                        foreach ( var adj in styleAdjustments )
                        {
                            newValue = newValue.Replace(adj.Key, adj.Value);
                        }

                        if ( property.Key == "width" && newValue != "auto" && !newValue.Contains("%") )
                        {
                            double tmp = GetElementWidth(newValue) * widthAdj;
                            newValue = String.Concat((int) tmp, "px");
                        }

                        newStyle.Properties.Add(property.Key, newValue);
                    }
                }

                element.Attributes.RemoveNamedItem("style");
                element.SetAttribute("style", newStyle.ToString());

            }
             
            
            foreach (var removeAttribute in removeAttributes)
            {
                if ( element.Attributes[removeAttribute] != null )
                {
                    element.Attributes.RemoveNamedItem(removeAttribute);
                }
            }
            
            foreach (XmlNode node in element.ChildNodes)
            {
                RemoveStyleAttribute(node as XmlElement, widthAdj);        
            }
             
        }

    }
}