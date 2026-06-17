using UnityEngine;
using UnityEngine.Events;

public abstract class GenericEventListener<T> : MonoBehaviour
{
    [SerializeField] private GenericScriptableObjectEvent<T> m_event;
    [SerializeField] private UnityEvent<T> m_response;

    private void OnEnable()
    {
        m_event.Subscribe(this);
    }

    private void OnDisable()
    {
        m_event.Unsubscribe(this);
    }

    public void React(T value)
    {
        m_response.Invoke(value);
    }
}
