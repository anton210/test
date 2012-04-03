using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CabinetFile;
using Xsf;
using System.Xml;
using System.Xml.Xsl;

namespace FormConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Please specify .xsn file and output dir.");
                return;
            }

            
            
            var extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var extractor = new TCabinetFile(args[0]);
            extractor.OutputDirectory = extractPath;
            extractor.ExtractAll();
            
            var manifest = XsfManifest.FromFile(Path.Combine(extractPath, "manifest.xsf"));

            MemoryStream convertedMainViewStream;
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddExtensionObject(@"http://mcdean", new XPathExt());
            //First convert original xsl
            using (FileStream mainViewStream = new FileStream(Path.Combine(extractPath, manifest.DefaultView.TransformFileName), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                //Debug, save original xsl
                XmlDocument tmpDoc = new XmlDocument();
                tmpDoc.Load(mainViewStream);
                tmpDoc.Save("originalView.xsl");

                convertedMainViewStream = XslTransformer.Transform("transformView.xsl", mainViewStream, xslArgs);
            }

            
            //==============================================================
            //After all xsl was done make corrections
            XmlDocument transformedViewXml = new XmlDocument();
            convertedMainViewStream.Seek(0, SeekOrigin.Begin);
            transformedViewXml.Load(convertedMainViewStream);

            var xmlns = new XmlNamespaceManager(transformedViewXml.NameTable);
            xmlns.AddNamespace("xd", @"http://schemas.microsoft.com/office/infopath/2003");
            IPadTransformProcessor.ApplyTableModes(transformedViewXml, xmlns);
            //IPadTransformProcessor.ApplySignboxMods(transformedViewXml, xmlns);

            //double adj = IPadTransformProcessor.CalculateWidthAdjustment(transformedViewXml);
            //IPadTransformProcessor.RemoveStyleAttribute(transformedViewXml.DocumentElement, adj);

            //transformedViewXml.Save(Path.Combine(outDir, "TransformedView.xsl"));

            convertedMainViewStream.Dispose();
            convertedMainViewStream = new MemoryStream();
            transformedViewXml.Save(convertedMainViewStream);
        

            
            convertedMainViewStream.Seek(0, SeekOrigin.Begin);
            using (FileStream debugConvertedXsl = new FileStream("debug.xsl", FileMode.Create, FileAccess.Write))
            {
                convertedMainViewStream.WriteTo(debugConvertedXsl);
                //System.Diagnostics.Debug.Write(convertedMainViewStream);
            }
             
            //Then apply converted view.xsl to template
            using (FileStream templateStream = new FileStream(Path.Combine(extractPath, manifest.InitialTemplateFile), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                /*
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
                */
                using (MemoryStream transformResult = XslTransformer.Transform(convertedMainViewStream, templateStream, null))
                {
                    using (FileStream outData = new FileStream(args[1], FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        transformResult.WriteTo(outData);
                    }
                     
                }
            }
           
        }
    }
}
