using System.Collections.Generic;

namespace Xsf
{
    public class FormField
    { 
        public string Name { get; set; }
        public bool IsContainer { get; set; }
        public IList<FormField> Fields { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}