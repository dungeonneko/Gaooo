using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Guooo
{
    public struct GuoooKeyFrame
    {
        public int TimeInMillisec { get; set; }
        public object Value { get; set; }

        public GuoooKeyFrame(string typeName, XmlNode node)
        {
            TimeInMillisec = int.Parse(node.Attributes["time"].Value);
            switch (typeName)
            {
                case "float":
                    Value = float.Parse(node.Attributes["value"].Value);
                    break;
                case "int":
                    Value = int.Parse(node.Attributes["value"].Value);
                    break;
                case "bool":
                    Value = bool.Parse(node.Attributes["value"].Value);
                    break;
                case "string":
                    Value = node.Attributes["value"].Value;
                    break;
                default:
                    throw new ArgumentException("invalid track type");
            }
         }
    }
}
