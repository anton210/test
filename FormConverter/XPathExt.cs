using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xsf;

namespace FormConverter
{
    public class XPathExt
    {
        public System.Boolean Contains(string original, string what)
        {
            return original.IndexOf(what, StringComparison.InvariantCultureIgnoreCase) != -1; 
        }

        public string SetStyle(string original, string style, string value)
        {
            var styleObj = new StyleAttributeString(original);
            styleObj.AssignProperty(style, value);
            return styleObj.ToString();
        }

        public string RemoveStyle(string original, string style)
        {
            var styleObj = new StyleAttributeString(original);
            styleObj.DeleteProperty(style);
            return styleObj.ToString();
        }

        public string GetStyle(string original, string style)
        {
            var styleObj = new StyleAttributeString(original);
            return styleObj.GetProperty(style);
        }

        public string CleanStyles(string original, string toLeave)
        {
            //Remove all styles except that are specified in toLeave param (separated with | )
            var styleObj = new StyleAttributeString(original);
           
            Dictionary<String, bool> leaveDic = new Dictionary<string, bool>();
            foreach (string style in toLeave.Split('|'))
            {
                leaveDic.Add(style, true);
            }

            List<String> toRemoveList = new List<string>();

            foreach (var stylePair in styleObj.Properties)
            {
                if (!leaveDic.ContainsKey(stylePair.Key))
                {
                    toRemoveList.Add(stylePair.Key);
                }
            }

            foreach (string removeName in toRemoveList)
            {
                styleObj.DeleteProperty(removeName);
            }

            return styleObj.ToString();
        }
    }
}

