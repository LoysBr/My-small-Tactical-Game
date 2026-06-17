using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public event Action MoveCharacterToSelectedPositionEvent;

    [SerializeField] private TacticalCharacterController m_selectedCharacter;

    private InputActionMap m_playerControllerActionMap;
    private InputAction m_moveCharacterToSelectedPosition;
    private InputAction m_doCharacterAttack;


    private void OnEnable()
    {
        m_playerControllerActionMap = InputSystem.actions.FindActionMap("PlayerController");
        m_playerControllerActionMap.Enable();

        m_moveCharacterToSelectedPosition = m_playerControllerActionMap.FindAction("MoveCharacterToSelectedPosition");
        m_doCharacterAttack = m_playerControllerActionMap.FindAction("DoCharacterAttack");
    }

    private void Update()
    {
        if (m_moveCharacterToSelectedPosition.IsPressed())
        {
            MoveCharacterToSelectedPositionEvent?.Invoke();
        }

        if (m_doCharacterAttack.IsPressed())
        {
            DoCharacterAttack();
        }
    }

    public void MoveCharacterToSelectedPosition(Vector3 moveToPosition)
    {
        if (m_selectedCharacter)
        {
            //Debug.Log("MoveCharacterToSelectedPosition(" + position + ")");
            m_selectedCharacter.MoveCharacterToSelectedPosition(moveToPosition);
        }
    }

    public void DoCharacterAttack()
    {
        if (m_selectedCharacter)
        {
            m_selectedCharacter.PlayAttackAnimation();
        }
    }
}
