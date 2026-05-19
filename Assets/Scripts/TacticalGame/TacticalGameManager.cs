using SazenGames.Skeleton;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TacticalGameManager : MonoBehaviour
{
    [SerializeField] private TacticalCameraController m_CameraController;
    [SerializeField] private BG3TacticalGroundController m_GroundController; //TODO : pattern Service Locator 
    [SerializeField] private PlayerController m_PlayerController;

    private InputActionMap m_TacticalGameMovementMap;
    
    private ITacticalGroundStrategy m_GroundStrategy; //TODO : pattern Service Locator 

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        m_TacticalGameMovementMap = InputSystem.actions.FindActionMap("TacticalGame");
        m_TacticalGameMovementMap.Enable();

        m_GroundStrategy = m_GroundController; //TODO : pattern Service Locator 
        MyLogger.Init();
    }

    private void OnDestroy()
    {
        if (m_PlayerController)
        {
            m_PlayerController.MoveCharacterToSelectedPositionEvent -= PlayerController_OnMoveCharacterToSelectedPosition;
        }

        m_PlayerController = null;
        TimerManager.ClearReferences();
        GC.Collect();
    }

    private void Start()
    {
        m_PlayerController.MoveCharacterToSelectedPositionEvent += PlayerController_OnMoveCharacterToSelectedPosition;
    }

    private void PlayerController_OnMoveCharacterToSelectedPosition()
    {
        Vector3 moveTo = m_GroundStrategy.IndicateCharacterGroundLocation(m_CameraController.GetPointerScreenToRay(), ITacticalGroundStrategy.IndicationType.MovementConfirmation);
        m_PlayerController.MoveCharacterToSelectedPosition(moveTo);
    }

    private void Update()
    {
        TimerManager.Update(Time.deltaTime);

        //TODO : "if We Are Currently Selecting A Character and Moving A Cursor to Move Character there"
        m_GroundStrategy.IndicateCharacterGroundLocation(m_CameraController.GetPointerScreenToRay(), ITacticalGroundStrategy.IndicationType.MovementPreview);
    }
}
