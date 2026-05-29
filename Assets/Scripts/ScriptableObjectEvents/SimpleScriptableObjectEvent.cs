using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleScriptableObjectEvent", menuName = "Scriptable Objects/SimpleScriptableObjectEvent")]
public class SimpleScriptableObjectEvent : ScriptableObject
{
    private List<SimpleEventListener> m_listOfListeners;

    public void Subscribe(SimpleEventListener _listener)
    {
        m_listOfListeners.Add(_listener);
    }

    public void Unsubscribe(SimpleEventListener _listener)
    {
        m_listOfListeners.Remove(_listener);
    }

    public void Raise()
    {
        for (int i = 0; i < m_listOfListeners.Count; i++)
        {
            m_listOfListeners[i].React();
        }
    }
}
