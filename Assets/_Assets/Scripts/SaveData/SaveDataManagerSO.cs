// ================================
// Save System with [SaveField] Filtering
// ================================

using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


/// <summary>
/// Generic saveable interface for plain object-based state.
/// </summary>
public interface ISaveable
{
    Dictionary<string, object> CaptureState(); // Extract state as a dictionary
    void RestoreState(Dictionary<string, object> state); // Apply state from dictionary
    void OnSave();
    void OnLoad();
}




/// <summary>
/// Manages saving and loading of ISaveable ScriptableObjects into runtime save saveStates.
/// </summary>
[CreateAssetMenu(menuName = "Save System/Save Data Manager")]
public class SaveDataManagerSO : SerializableScriptableObject, IInitializable, ISceneCycleListener
{
    [OdinSerialize] private SaveFile saveFile = new(); // Holds all saveStates
    [OdinSerialize] private int currentSlotIndex = 0;  // Active slot index

    [SerializeField] private HashSet<ISaveable> saveableObjects = new HashSet<ISaveable>(); // Objects to track
    [SerializeField] private DataFormat dataFormat = DataFormat.JSON; // Serialization format
    [SerializeField] private string fileName = "saveFile.json";       // Filename to write to

    /// <summary>
    /// Full file path used to save/load persistent data.
    /// </summary>
    public string FilePath => Path.Combine(Application.persistentDataPath, fileName);

    /// <summary>
    /// Looks for new ISaveable objects in the project and adds them to the list.
    /// </summary>
    [Button, GUIColor(0.2f, 1f, 0.2f)]
    public void RefreshSaveableObjects()
    {
        ScriptableObject[] allScriptableObjects = Resources.LoadAll<ScriptableObject>("");
        foreach (var so in allScriptableObjects)
        {
            // Check if the ScriptableObject implements ISaveable and if it's not already in the list
            if (so is ISaveable saveable && !saveableObjects.Contains(saveable))
            {
                saveableObjects.Add(saveable);
            }
            
        }
    }

    /// <summary>
    /// Captures the state of all saveables and writes to the currently selected slot.
    /// </summary>
    public async Task SaveAsync()
    {
        var state = new SaveState();

        foreach (var item in saveableObjects)
        {
            item.OnSave();
        }
        foreach (var saveable in saveableObjects)
        {
            if (saveable is SerializableScriptableObject so)
            {
                // Create a reference to the ScriptableObject
                var reference = new ScriptableObjectReference<SerializableScriptableObject>(so);
                
                // Check if the reference already exists in the saved data
                var data = saveable.CaptureState(); // Returns a POCO
                
                // If the reference already exists, update the data
                state.SavedData[reference] = data;
            }
        }
        
        //if our current Slot index is greater than the number of saveStates, add a new one
        if (currentSlotIndex >= saveFile.saveStates.Count)
            saveFile.saveStates.Add(state);
        else
        //replace the current slot with the new state
            saveFile.saveStates[currentSlotIndex] = state;
        
        
        //Capture data to file
        await SaveToFile();
    }

    /// <summary>
    /// Serializes and writes the save file to disk.
    /// </summary>
    private async Task SaveToFile()
    {
        var serialized = SerializationUtility.SerializeValue(saveFile, dataFormat);
        await File.WriteAllBytesAsync(FilePath, serialized);
    }

    /// <summary>
    /// Loads and restores save data from a specific slot index.
    /// Should be called after LoadSaveFile() to ensure the save file is loaded. This is called during Initialization.
    /// </summary>
    public async Task LoadAsync(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        var state = saveFile.saveStates[slotIndex];

        foreach (var keyValuePair in state.SavedData)
        {
            //Get reference to the ScriptableObject from the SOReference key in the dictionary
            if (keyValuePair.Key.Value is ISaveable saveable)
            {
                try
                {
                    saveable.RestoreState(keyValuePair.Value); 
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to restore state for {keyValuePair.Key.Value.name}: {e.Message}");
                }
                
            }
            else
            {
                Debug.LogWarning($"Missing or invalid reference for {keyValuePair.Key}");
            }
        }
        
        foreach (var saveable in saveableObjects)
        {
            saveable.OnLoad();
        }
    }

    /// <summary>
    /// Loads the full save file into memory and rehydrates all saveStates.
    /// </summary>
    [Button, GUIColor(0.6f, 0.8f, 1f)]
    public async Task LoadSaveFile()
    {
        if (!File.Exists(FilePath))
        {
            saveFile = new SaveFile();
            await SaveToFile();
            return;
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(FilePath);
            saveFile = SerializationUtility.DeserializeValue<SaveFile>(bytes, dataFormat);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load save file: {e.Message}");
            return;
        }

        foreach (var slot in saveFile.saveStates)
        {
            slot.RehydrateToRuntimeInstances();
        }
    }
    

    public int SaveSlotCount => saveFile.saveStates.Count;
    public int CurrentSlotIndex => currentSlotIndex;

    public void SetSlot(int index) => currentSlotIndex = index;

    public async Task Init()
    {
        //Loads thje data from the file
        await LoadSaveFile();
        await LoadAsync(CurrentSlotIndex);
    }

    public void OnEditorStopped()
    {
        saveFile.saveStates.Clear();
    }

    public void OnSceneStarted(Scene scene) { }

    public void OnSceneStopped(Scene scene) { }

    // ===============================
    // Editor Button Helpers
    // ===============================

    [Button("Save Game"), GUIColor("blue")]
    public void SaveGame()
    {
        var cmd = new AsyncCommand("Save Game", SaveAsync);
        CommandRunner.ExecuteCommand(cmd);
    }

    [Button("Load Game"), GUIColor("orange")]
    public void LoadGame(int slotIndex)
    {
        var cmd = new AsyncCommand("Load Game", () => LoadAsync(slotIndex));
        CommandRunner.ExecuteCommand(cmd);
    }
}