using UnityEngine;
using UnityEngine.Events;

public class SimpleEventListener : MonoBehaviour
{
    [SerializeField] private SimpleScriptableObjectEvent m_event;
    [SerializeField] private UnityEvent m_response; //maybe use a C# event ?

    private void OnEnable()
    {
        m_event.Subscribe(this);
    }

    private void OnDisable()
    {
        m_event.Unsubscribe(this);
    }

    public void React()
    {
        m_response.Invoke();
    }
}
