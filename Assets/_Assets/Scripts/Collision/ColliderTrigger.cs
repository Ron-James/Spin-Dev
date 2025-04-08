using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ColliderTrigger : SerializableMonoBehaviour
{
    
    [SerializeField] protected ColliderChannel channel;
    
    [SerializeField] protected UnityEvent<CollisionData> onTriggerEnter;
    [SerializeField] protected UnityEvent<CollisionData> onTriggerExit;
    Collider cachedCollider;
    private void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent<ColliderTrigger>(out var otherTrigger);
        if(otherTrigger == null) return;
        channel.TriggerEntered(otherTrigger);
        onTriggerEnter.Invoke(new CollisionData(otherTrigger));
    }

    private void OnTriggerExit(Collider other)
    {
        other.TryGetComponent<ColliderTrigger>(out var otherTrigger);
        if(otherTrigger == null) return;
        channel.TriggerExited(otherTrigger);
        onTriggerExit.Invoke(new CollisionData(otherTrigger));
    }


    public Collider Collider
    {
        get
        {
            if (cachedCollider == null)
            {
                cachedCollider = GetComponent<Collider>();
            }
            return cachedCollider;
        }
    }
    
    
    public void OnReset()
    {
        if (channel == null) channel = ScriptableObjectManager.LoadFirstScriptableObject<ColliderChannel>();
    }
}