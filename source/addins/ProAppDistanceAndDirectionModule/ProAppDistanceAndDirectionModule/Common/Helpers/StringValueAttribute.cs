using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.Common
{
    [System.AttributeUsage(AttributeTargets.All)]
    public class StringValueAttribute : Attribute
    {
        public string StringValue { get; set; }
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }
}
