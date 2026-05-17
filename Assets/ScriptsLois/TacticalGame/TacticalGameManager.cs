using UnityEngine;
using UnityEngine.InputSystem;

public class TacticalGameManager : MonoBehaviour
{
    private InputActionMap m_TacticalGameMovementMap;
    private InputAction m_InputActionCameraUp;

    void Start()
    {
        m_TacticalGameMovementMap = InputSystem.actions.FindActionMap("TacticalGame");
        m_TacticalGameMovementMap.Enable();
    }

    void Update()
    {
        
    }
}
