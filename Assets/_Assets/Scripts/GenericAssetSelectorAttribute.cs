using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class GenericAssetSelectorAttribute : Attribute
{
    public Type TargetGenericType { get; }

    public GenericAssetSelectorAttribute(Type targetGenericType = null)
    {
        TargetGenericType = targetGenericType;
    }
}