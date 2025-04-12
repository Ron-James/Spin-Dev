using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;


public interface IValueAsset<T>
{
    T StoredValue { get; set; }
    
}

/// <summary>
/// Wrapper Class for action SOs to store their references. Implements ValueReference on Value asset ActionSO<T>
/// </summary>
/// <typeparam name="TValue">The type you want to store in the value</typeparam>
public class ActionReference<TValue> : ValueReference<TValue, ActionSO<TValue>>
{
    /// <summary>
    /// Default constructor for better serialization
    /// </summary>
    public ActionReference()
    {
        _value = default;
        assetReference = null;
    }
    public BaseActionSO Action => assetReference;
    public ActionSO<TValue> ActionCasted => assetReference;
}





/// <summary>
/// Template class improved with odin includes
/// Dropdown for asset ref field
/// Dynamic switching between regular values and references to value assets
/// It's a wrapper class for value assets to use them in variables
/// Shoutout to Ryan Hipple
/// </summary>
/// <typeparam name="TValue">The return type of the value asset</typeparam>
/// <typeparam name="TAsset">The Asset type your referencing the value from</typeparam>
[InlineProperty]
[LabelWidth(200)]
[Serializable]
public class ValueReference<TValue, TAsset> where TAsset : ScriptableObject, IValueAsset<TValue>
{
    public ValueReference()
    {
        _value = default;
        assetReference = null;
    }
    
    [HorizontalGroup("Reference", MaxWidth = 100)] [ValueDropdown("valueList")] [HideLabel] [SerializeField]
    protected bool useValue = true;


    [ShowIf("useValue", Animate = false)] [HorizontalGroup("Reference")] [HideLabel] [SerializeField]
    protected TValue _value;

    [HideIf("useValue", Animate = false)]
    [HorizontalGroup("Reference")]
    [OnValueChanged("UpdateAsset")]
    [HideLabel]
    [SerializeField]
    [GenericAssetSelector]
    protected TAsset assetReference;

    [ShowIf("@assetReference != null && useValue == false")] [LabelWidth(100)] [SerializeField]
    protected bool editAsset = false;

    [ShowIf("@assetReference != null && useValue == false")]
    [EnableIf("editAsset")]
    [FoldoutGroup("Edit", expanded: false)]
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [LabelWidth(20)]
    [SerializeField]
    protected TAsset _assetReference;


    private static ValueDropdownList<bool> valueList = new ValueDropdownList<bool>()
    {
        { "Reference", false },
        { "Value", true }
    };


    public TValue Value
    {
        get
        {
            if (useValue || assetReference == null)
            {
                return _value;
            }
            else
            {
                return assetReference.StoredValue;
            }
        }
        set
        {
            if (useValue)
            {
                _value = value;
            }
            else
            {
                assetReference.StoredValue = value;
            }
        }
    }

    public void UpdateAsset()
    {
        _assetReference = assetReference;
    }
    
    

    public static implicit operator TValue(ValueReference<TValue, TAsset> reference)
    {
        return reference.Value;
    }
    

    public void SetAsset(TAsset eventObject)
    {
        assetReference = eventObject;
    }
    
}