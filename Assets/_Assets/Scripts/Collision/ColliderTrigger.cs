using UnityEngine;
using UnityEngine.Events;

public class ColliderTrigger : MonoBehaviour
{
    
    [SerializeField] protected ColliderChannel channel;
    
    [SerializeField] protected UnityEvent<CollisionData> onTriggerEnter;
    [SerializeField] protected UnityEvent<CollisionData> onTriggerExit;
    private void OnTriggerEnter(Collider other)
    {
        channel.TriggerEntered(this, other);
        onTriggerEnter.Invoke(new CollisionData(this, other));
    }

    private void OnTriggerExit(Collider other)
    {
        channel.TriggerExited(this, other);
        onTriggerExit.Invoke(new CollisionData(this, other));
    }
    
    
    public void OnReset()
    {
        if (channel == null) channel = ScriptableObjectManager.LoadFirstScriptableObject<ColliderChannel>();
    }
}