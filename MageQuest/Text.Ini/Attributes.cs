using System;

namespace MageQuest.Text.Ini;

/// <summary>
/// Specifies that the value should be considered by the <see cref="IniSerializer"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class IniIncludeAttribute : IniAttribute;

/// <summary>
/// Specifies that the value should be ignored by the <see cref="IniSerializer"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class IniIgnoreAttribute : IniAttribute;

/// <summary>
/// Specifies which section the value should be placed in by the <see cref="IniSerializer"/>
/// </summary>
/// <param name="name">The name of the section</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class IniSectionAttribute(string name) : IniAttribute
{
    public string Name => name;
}

/// <summary>
/// Specifies the name of the value to be used by the <see cref="IniSerializer"/>
/// </summary>
/// <param name="name">The name of the value</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class IniKeyAttribute(string name) : IniAttribute
{
    public string Name => name;
}

/// <summary>
/// Base class for all Ini serialization attributes
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IniAttribute : Attribute;

/// <summary>
/// Specifies which section that any value without an <see cref="IniSectionAttribute"/> should be placed in by the <see cref="IniSerializer"/>
/// </summary>
/// <param name="name">The name of the section</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class IniDefaultSectionNameAttribute(string name) : Attribute
{
    public string Name => name;
}
