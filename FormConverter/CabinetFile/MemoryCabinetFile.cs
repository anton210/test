using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FormConverter.CabinetFile
{
    class MemoryCabinetFile : FilesBundle
    {
        public static MemoryCabinetFile CreateFromFile(String cabinetFileName)
        {
            var newMemCabFile = new MemoryCabinetFile();
            newMemCabFile.LoadFromFile(cabinetFileName);
            return newMemCabFile;
        }

        private MemoryCabinetFile()
        {
        }

        private void LoadFromFile(String cabinetFileName)
        {
            _files = new Dictionary<string, Stream>();
            var extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var extractor = new TCabinetFile(cabinetFileName))
                {
                    extractor.OutputDirectory = extractPath;
                    extractor.ExtractAll();
                    foreach (TFile cabFile in extractor)
                    {
                        using (FileStream fs = new FileStream(Path.Combine(extractPath, cabFile.Name), FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            MemoryStream ms = new MemoryStream();
                            fs.CopyTo(ms);
                            Add(cabFile.Name, ms);
                        }
                    }
                }
            }
            finally
            {
                if (Directory.Exists(extractPath))
                {
                    try
                    {
                        Directory.Delete(extractPath, true);
                    }
                    catch (Exception e)
                    {
                        //Intentially ignore all exceptions to avoid crash on produciton.
                        Debug.Fail(e.Message);
                    }
                }
            }
        }
    }
}
