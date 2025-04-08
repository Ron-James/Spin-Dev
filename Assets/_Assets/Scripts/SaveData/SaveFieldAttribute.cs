using System;

using System;

/// <summary>
/// Marks a field to be included during the save process.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SaveAttribute : Attribute { }



[AttributeUsage(AttributeTargets.Field)]
public class DontSaveAttribute : Attribute { }