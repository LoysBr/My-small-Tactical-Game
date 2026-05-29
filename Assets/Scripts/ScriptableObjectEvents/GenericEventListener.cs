using UnityEngine;
using UnityEngine.Events;

public abstract class GenericEventListener<T> : MonoBehaviour
{
    [SerializeField] private GenericScriptableObjectEvent<T> m_event;
    [SerializeField] private UnityEvent<T> m_response; //maybe use a C# event ?

    private void OnEnable()
    {
        m_event.Subscribe(this);
    }

    private void OnDisable()
    {
        m_event.Unsubscribe(this);
    }

    public void React(T _value)
    {
        m_response.Invoke(_value);
    }
}
