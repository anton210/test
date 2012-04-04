using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using FormConverter.Xsf;
using FormConverter.Xsl;

namespace FormConverter.Processor
{
    class PadFormTrasformProcessor : IFormTransformProcessor
    {
        FilesBundle _outputBundle;

        public FilesBundle ApplyTransform(FilesBundle cabFile)
        {
            _outputBundle = new FilesBundle();
            var manifest = XsfManifest.FromBundle(cabFile);

            ///////////////////////////////////////////////////////////////////////////////////////////////
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddExtensionObject(@"http://mcdean.com/2012-04-04T17:51:00", new XpathExt());

            //Debug
            if(false){ 
                XmlDocument tmpDoc = new XmlDocument();
                tmpDoc.Load(cabFile[manifest.DefaultView.TransformFileName]);
                tmpDoc.Save("originalView.xsl");
            }

            String viewFileName = manifest.DefaultView.TransformFileName;
            //First convert original xsl
            MemoryStream convertedMainViewStream = XslTransformer.Transform("transformView.xsl", cabFile[viewFileName], xslArgs);
            
            //After all xsl was done make corrections
            XmlDocument transformedViewXml = new XmlDocument();
            convertedMainViewStream.Seek(0, SeekOrigin.Begin);
            transformedViewXml.Load(convertedMainViewStream);

            XmlNamespaceManager xmlns = new XmlNamespaceManager(transformedViewXml.NameTable);
            xmlns.AddNamespace("xd", @"http://schemas.microsoft.com/office/infopath/2003");
            PadFormTableCorrector.ApplyTableModes(transformedViewXml, xmlns);
       
            //Obsolete, need to remove 
            //IPadTransformProcessor.ApplySignboxMods(transformedViewXml, xmlns);
            //double adj = IPadTransformProcessor.CalculateWidthAdjustment(transformedViewXml);
            //IPadTransformProcessor.RemoveStyleAttribute(transformedViewXml.DocumentElement, adj);
            //transformedViewXml.Save(Path.Combine(outDir, "TransformedView.xsl"));

            convertedMainViewStream.Dispose();
            convertedMainViewStream = new MemoryStream();
            transformedViewXml.Save(convertedMainViewStream);

            _outputBundle.Add(viewFileName, convertedMainViewStream);
            AddCommonResources();
            AddContentDependentResources(manifest, cabFile);

            //Debug, TODO need to remove
            /*
            if (false)
            {
                convertedMainViewStream.Seek(0, SeekOrigin.Begin);
                using (FileStream debugConvertedXsl = new FileStream("debug.xsl", FileMode.Create, FileAccess.Write))
                {
                    convertedMainViewStream.WriteTo(debugConvertedXsl);
                    //System.Diagnostics.Debug.Write(convertedMainViewStream);
                }

                //Then apply converted view.xsl to template
                using (FileStream templateStream = new FileStream(Path.Combine(extractPath, manifest.InitialTemplateFile), FileMode.Open, FileAccess.Read, FileShare.Read))
                {

                    using (MemoryStream transformResult = XslTransformer.Transform(convertedMainViewStream, templateStream, null))
                    {
                        using (FileStream outData = new FileStream(args[1], FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            transformResult.WriteTo(outData);
                        }

                    }
                }
            }
            */

            return _outputBundle;
        }

        private void AddContentDependentResources(XsfManifest manifest, FilesBundle cabFile)
        {

        }

        private void AddCommonResources()
        {
            _outputBundle.Add("iPadTransform.css", ResourcesManager.GetDocument("iPadTransform.css"), true);
            _outputBundle.Add("iPadTransform.js", ResourcesManager.GetDocument("iPadTransform.js"), true);
            _outputBundle.Add("jquery.mobile-1.1.0-rc.1.min.css", ResourcesManager.GetDocument("jquery.mobile-1.1.0-rc.1.min.css"), true);
            _outputBundle.Add("jquery-1.7.1.min.js", ResourcesManager.GetDocument("jquery-1.7.1.min.js"), true);
            _outputBundle.Add("jquery.mobile-1.1.0-rc.1.min.js", ResourcesManager.GetDocument("jquery.mobile-1.1.0-rc.1.min.js"), true);

            //TODO need to add signature related resoureces only if we use signature in the form
            _outputBundle.Add("Signature.js", ResourcesManager.GetDocument("Signature.js"), true);
            _outputBundle.Add("sign-here.gif", ResourcesManager.GetDocument("sign-here.gif"), true);
        }
    }
}
