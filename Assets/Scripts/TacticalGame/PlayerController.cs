using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public event Action MoveCharacterToSelectedPositionEvent;

    [SerializeField] private TacticalCharacterController m_SelectedCharacter;

    private InputActionMap m_PlayerControllerActionMap;
    private InputAction m_MoveCharacterToSelectedPosition;
    private InputAction m_DoCharacterAttack;


    private void OnEnable()
    {
        m_PlayerControllerActionMap = InputSystem.actions.FindActionMap("PlayerController");
        m_PlayerControllerActionMap.Enable();

        m_MoveCharacterToSelectedPosition = m_PlayerControllerActionMap.FindAction("MoveCharacterToSelectedPosition");
        m_DoCharacterAttack = m_PlayerControllerActionMap.FindAction("DoCharacterAttack");
    }

    private void Update()
    {
        if (m_MoveCharacterToSelectedPosition.IsPressed())
        {
            MoveCharacterToSelectedPositionEvent?.Invoke();
        }

        if (m_DoCharacterAttack.IsPressed())
        {
            DoCharacterAttack();
        }
    }

    public void MoveCharacterToSelectedPosition(Vector3 moveToPosition)
    {
        if (m_SelectedCharacter)
        {
            //Debug.Log("MoveCharacterToSelectedPosition(" + position + ")");
            m_SelectedCharacter.MoveCharacterToSelectedPosition(moveToPosition);
        }
    }

    public void DoCharacterAttack()
    {
        if (m_SelectedCharacter)
        {
            m_SelectedCharacter.PlayAttackAnimation();
        }
    }
}
