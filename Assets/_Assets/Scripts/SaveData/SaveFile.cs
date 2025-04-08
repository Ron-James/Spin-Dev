using System;
using System.Collections.Generic;
using Sirenix.Serialization;

using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine.Serialization;

/// <summary>
/// Holds multiple save slots, each representing a SaveState.
/// </summary>
[Serializable]
public class SaveFile
{
    [FormerlySerializedAs("Slots")] [OdinSerialize]
    public List<SaveState> saveStates = new();
}
