using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using MageQuest.IO;

namespace MageQuest.Text.Ini;

public static class IniSerializer
{
    const string DefaultSectionName = "Configuration";

    public static string Serialize<T>(T value)
    {
        IniFile file = new();
        var type = typeof(T);

        foreach(var member in type.GetFields(BindingFlags.Instance | BindingFlags.GetField))
        {
            WriteMember(value, member, file);
        }
        foreach(var member in type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty))
        {
            WriteMember(value, member, file);
        }

        using MemoryStream stream = new();
        file.Write(stream);

        return Encoding.UTF8.GetString(stream.GetBuffer());
    }

    public static T Deserialize<T>(string value) where T : new()
    {
        T obj = new();
        IniFile file = new(value);
        var type = typeof(T);

        foreach(var member in type.GetFields(BindingFlags.Instance | BindingFlags.SetField))
        {
            ReadMember(obj, member, file);
        }
        foreach(var member in type.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty))
        {
            ReadMember(obj, member, file);
        }

        return obj;
    }

    private static void WriteMember<T>(T value, MemberInfo member, IniFile file)
    {
        if(member.GetCustomAttribute<IniIncludeAttribute>(true) is null || member.GetCustomAttribute<IniIgnoreAttribute>(true) is not null)
        {
            return;
        }

        var sectionName = member.GetCustomAttribute<IniSectionAttribute>(true)?.Name ?? typeof(T).GetCustomAttribute<IniDefaultSectionNameAttribute>()?.Name ?? DefaultSectionName;

        var name = member.GetCustomAttribute<IniKeyAttribute>(true)?.Name ?? member.Name;

        if(member is FieldInfo field)
        {
            file.Set(name, (field.GetValue(value) ?? "").ToString(), sectionName);
        }
        else if(member is PropertyInfo property)
        {
            file.Set(name, (property.GetValue(value) ?? "").ToString(), sectionName);
        }
    }

    private static void ReadMember<T>(T value, MemberInfo member, IniFile file)
    {
        if(member.GetCustomAttribute<IniIncludeAttribute>(true) is null || member.GetCustomAttribute<IniIgnoreAttribute>(true) is not null)
        {
            return;
        }

        var sectionName = member.GetCustomAttribute<IniSectionAttribute>(true)?.Name ?? typeof(T).GetCustomAttribute<IniDefaultSectionNameAttribute>()?.Name ?? DefaultSectionName;

        var name = member.GetCustomAttribute<IniKeyAttribute>(true)?.Name ?? member.Name;

        var val = file.Get(name, sectionName);
        object newValue = null;

        if(val is not null && val != "")
        {
            var memberType = member.GetUnderlyingType();
            if(memberType == typeof(string))
            {
                newValue = val;
            }
            else if(memberType == typeof(bool))
            {
                if(bool.TryParse(val, out bool _bool))
                    newValue = _bool;
                else if(val == "true")
                    newValue = true;
                else if(val == "false")
                    newValue = false;
            }
            else if(memberType.IsEnum)
            {
                List<object> values = [..memberType.GetEnumValues()];
                foreach(var enumValue in values)
                {
                    if(memberType.GetEnumName(enumValue) == val)
                    {
                        newValue = enumValue;
                        break;
                    }
                }
            }
            else if(
                memberType.GetMethod(
                    "Parse",
                    BindingFlags.Public | BindingFlags.Static,
                    [typeof(string), typeof(IFormatProvider)]
                ) is MethodInfo parseMethod)
            {
                try
                {
                    newValue = parseMethod.Invoke(null, [val, null]);
                }
                catch(Exception)
                {
                    newValue = null;
                }
            }

            if(newValue is null) return;

            if(member is FieldInfo field)
            {
                field.SetValue(value, newValue);
            }
            else if(member is PropertyInfo property)
            {
                property.SetValue(value, newValue);
            }
        }
    }
}
