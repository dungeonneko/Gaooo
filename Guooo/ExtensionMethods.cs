using System;
using System.Linq;
using System.Xml;

namespace Guooo
{
    static public class ExtensionMethods
    {
        static public T GetOrDefault<T>(this XmlAttributeCollection attrs, string name, T defaultValue, Func<string, T> parser)
        {
            return attrs
                .Cast<XmlAttribute>()
                .Where(attr => attr.Name == name)
                .Select(attr => parser(attr.Value))
                .DefaultIfEmpty(defaultValue)
                .First();
        }
    }
}
