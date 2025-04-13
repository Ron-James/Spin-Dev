using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Publishes the message via the ActionReference.
/// </summary>
public class Publisher : MonoBehaviour
{
    [SerializeField] ActionReference<string> actionReference;
    [SerializeField] private string message = "I have just published this message";
    
    [Button]
    public void Publish()
    {
        // Publish the action with a string parameter
        actionReference.Value = message;
        
    }
}
