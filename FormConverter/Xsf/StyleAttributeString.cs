using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

namespace FormConverter.Xsf
{
    public class StyleAttributeString
    {
        private Dictionary<string, string> _properties = new Dictionary<string, string>();
        
        public StyleAttributeString()
        {
        }

        public StyleAttributeString(XmlAttribute attr)
            : this(attr != null ? attr.Value : String.Empty)
        {
        }

        public StyleAttributeString(string styleString)
        {
            if ( string.IsNullOrEmpty(styleString) ) return;
            foreach ( var propPairString in styleString.Split(new[] { ';' }) )
            {
                if ( String.IsNullOrEmpty(propPairString) ) continue;
                
                string[] pair = propPairString.Split(new[] {':'});
                Debug.Assert(pair.Length == 2);
                if (pair.Length == 2)
                {
                    _properties.Add(pair[0].Trim().ToLowerInvariant(), pair[1].Trim().ToLowerInvariant());
                }
            }
        }

        public void AssignProperty(string key, string value)
        {
            if ( Properties.ContainsKey(key) )
            {
                Properties[key] = value;
            }
            else
            {
                Properties.Add(key, value);
            }
        }

        public void DeleteProperty(string key)
        {
            if (Properties.ContainsKey(key))
            {
                Properties.Remove(key);
            }
        }

        public string GetProperty(string key)
        {
            if (Properties.ContainsKey(key))
            {
                return Properties[key];
            }
            else
            {
                return String.Empty;
            }
        }

        public Dictionary<string, string> Properties
        {
            get { return _properties; }
        }

        public override string ToString()
        {
            List<string> pairedStrings = new List<string>();
            foreach (var property in _properties)
            {
                pairedStrings.Add(String.Format("{0}: {1}", property.Key, property.Value));
            }
            return String.Join("; ", pairedStrings.ToArray());
        }
    }
}