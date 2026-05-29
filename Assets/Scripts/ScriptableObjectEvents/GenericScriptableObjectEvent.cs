using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleScriptableObjectEvent", menuName = "Scriptable Objects/SimpleScriptableObjectEvent")]
public abstract class GenericScriptableObjectEvent<T> : ScriptableObject
{
    private List<GenericEventListener<T>> m_listOfListeners;

    public void Subscribe(GenericEventListener<T> _listener)
    {
        m_listOfListeners.Add(_listener);
    }

    public void Unsubscribe(GenericEventListener<T> _listener)
    {
        m_listOfListeners.Remove(_listener);
    }

    public void Raise(T _value)
    {
        for (int i = 0; i < m_listOfListeners.Count; i++)
        {
            m_listOfListeners[i].React(_value);
        }
    }
}
