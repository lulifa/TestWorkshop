using System.ComponentModel;
using System.Reflection;

namespace TestWorkshop;

public static class ToolKitHelper
{
    public static string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null) return null;

        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        return attribute?.Description;
    }

    public static Dictionary<int, object> GetEnumKeyDescriptions<TEnum>(IEnumerable<TEnum> filter = null) where TEnum : Enum
    {
        var enumKeyValues = new Dictionary<int, object>();

        foreach (var value in Enum.GetValues(typeof(TEnum)))
        {
            if (filter == null || filter.Contains((TEnum)value))
            {
                var key = (int)value;
                var description = GetEnumDescription((Enum)value);

                enumKeyValues.Add(key, description);
            }
        }

        return enumKeyValues;
    }

    public static Dictionary<string, object> SerializeDerivedProperties(object obj, bool includeAll = false)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        Type objectType = obj.GetType();

        foreach (var property in objectType.GetProperties())
        {
            if (includeAll)
            {
                if (property != null)
                {
                    dict.Add(property.Name, property.GetValue(obj));
                }
            }
            else
            {
                if (property != null && property.DeclaringType == objectType)
                {
                    dict.Add(property.Name, property.GetValue(obj));
                }
            }

        }

        return dict;
    }

    public static void DeserializeDerivedProperties(object obj, Dictionary<string, object> properties)
    {
        Type objectType = obj.GetType();

        foreach (var pair in properties)
        {
            PropertyInfo property = objectType.GetProperty(pair.Key);

            if (property != null && property.DeclaringType == objectType)
            {
                object value = pair.Value;

                var valuetype = value.GetType();

                Type propertyType = property.PropertyType;

                if (propertyType == typeof(int) && value is long)
                {
                    value = Convert.ToInt32(value);
                }
                else if (propertyType.IsEnum && value is long)
                {
                    value = Convert.ToInt32(value);
                }
                else if (propertyType == typeof(float) && value is double)
                {
                    value = Convert.ToSingle(value);
                }
                else if (propertyType == typeof(double) && value is double)
                {
                    value = Convert.ToDouble(value);
                }
                else if (propertyType == typeof(decimal) && value is double)
                {
                    value = Convert.ToDecimal(value);
                }

                // 设置属性值
                property.SetValue(obj, value);
            }
        }
    }

}
