using System;
using System.Text.Json;

namespace Gaooo
{
    static public class ExtensionMethods
    {
        static public GaoooValue JsonToGaoooValue(this JsonElement elem, bool fromDb = false)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.Array:
                    var list = new GaoooValueList();
                    foreach (var item in elem.EnumerateArray())
                    {
                        list.RawValue.PushBack(item.JsonToGaoooValue(fromDb));
                    }
                    return list;
                case JsonValueKind.String:
                    var str = elem.GetString();
                    if (str == null)
                    {
                        return new GaoooValueString(string.Empty);
                    }
                    else if (fromDb)
                    {
                        return new GaoooValueString(str);
                    }
                    else if (str.StartsWith("*"))
                    {
                        return new GaoooValueLabel(str.Substring(1));
                    }
                    else if (str.StartsWith("$"))
                    {
                        return new GaoooValueArgument(str.Substring(1));
                    }
                    else if ((str.StartsWith("\"") && str.EndsWith("\"")) ||
                            (str.StartsWith("\'") && str.EndsWith("\'")))
                    {
                        return new GaoooValueString(str.Substring(1, str.Length - 2));
                    }
                    else if (str.StartsWith("{") && str.EndsWith("}"))
                    {
                        return new GaoooValueExpression(str.Substring(1, str.Length - 2));
                    }
                    else
                    {
                        return new GaoooValueVariable(str);
                    }
                case JsonValueKind.Number:
                    return new GaoooValueNumber(elem.GetDouble());
                case JsonValueKind.True:
                    return new GaoooValueBoolean(true);
                case JsonValueKind.False:
                    return new GaoooValueBoolean(false);
                default:
                    throw new Exception("Unknown JsonValueKind");
            }
        }

        static public JsonDocument StringToJson(this string str)
        {
            return JsonDocument.Parse(str);
        }

        public static GaoooValue StringToGaoooValue(this string str)
        {
            if (str.StartsWith("*"))
            {
                return new GaoooValueLabel(str.Substring(1));
            }
            else if (str.StartsWith("$"))
            {
                return new GaoooValueArgument(str.Substring(1));
            }
            else if ((str.StartsWith("\"") && str.EndsWith("\"")) ||
                    (str.StartsWith("\'") && str.EndsWith("\'")))
            {
                return new GaoooValueString(str.Substring(1, str.Length - 2));
            }
            else if (str.StartsWith("[") && str.EndsWith("]"))
            {
                return str.StringToJson().RootElement.JsonToGaoooValue();
            }
            else if (str.StartsWith("{") && str.EndsWith("}"))
            {
                return new GaoooValueExpression(str.Substring(1, str.Length - 2));
            }
            else if (str == "true" ||
                    str == "false" ||
                    str == "True" ||
                    str == "False" ||
                    str == "TRUE" ||
                    str == "FALSE")
            {
                return new GaoooValueBoolean(bool.Parse(str.ToLower()));
            }
            else if (str.Contains('.') && double.TryParse(str, out var d))
            {
                return new GaoooValueNumber(d);
            }
            else if (int.TryParse(str, out var i))
            {
                return new GaoooValueNumber(i);
            }
            else
            {
                return new GaoooValueVariable(str);
            }
        }

        public static GaoooValue ObjectToGaoooValue(this object obj)
        {
            switch (obj)
            {
                case bool b:
                    return new GaoooValueBoolean(b);
                case int i:
                    return new GaoooValueNumber(i);
                case double d:
                    return new GaoooValueNumber(d);
                case string s:
                    return StringToGaoooValue(s);
            }

            return obj.ToString().StringToGaoooValue();
        }
    }
}
