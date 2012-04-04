using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FormConverter
{
    public class FilesBundle
    {
        protected Dictionary<string, Stream> _files = new Dictionary<string,Stream>();

        public Stream this[String name]
        {
            get
            {
                return GetByName(name);
            }
        }

        public void Add(String name, Stream data, Boolean copy)
        {
            var lowerCaseName = name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            Stream localData = data;
            if (copy)
            {
                localData = new MemoryStream();
                data.CopyTo(localData);
            }
            
            _files.Add(lowerCaseName, localData);
        }

        public void Add(String name, Stream data)
        {
            Add(name, data, false);
        }

        public Stream GetByName(String name)
        {
            var lowerCaseName = name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            if (_files.ContainsKey(lowerCaseName))
            {
                //Make sure that we always return stream which can be used immidiately 
                _files[lowerCaseName].Seek(0, SeekOrigin.Begin);
                return _files[lowerCaseName];
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<String> GetFileNames()
        {
            return _files.Keys;
        }
    }
}
