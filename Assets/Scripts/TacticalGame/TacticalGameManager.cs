using SazenGames.Skeleton;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TacticalGameManager : MonoBehaviour
{
    [SerializeField] private TacticalCameraController m_cameraController;
    [SerializeField] private BG3TacticalGroundController m_groundController; //TODO : pattern Service Locator 
    [SerializeField] private PlayerController m_playerController;
    [SerializeField] private EnemySpawner m_enemySpawner;

    private InputActionMap m_tacticalGameMovementMap;
    
    private ITacticalGroundStrategy m_ground; //TODO : pattern Service Locator 

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        m_tacticalGameMovementMap = InputSystem.actions.FindActionMap("TacticalGame");
        m_tacticalGameMovementMap.Enable();

        m_ground = m_groundController; //TODO : pattern Service Locator 
        MyLogger.Init();
    }

    private void OnDestroy()
    {
        if (m_playerController)
        {
            m_playerController.MoveCharacterToSelectedPositionEvent -= PlayerController_OnMoveCharacterToSelectedPosition;
        }

        m_playerController = null;
        TimerManager.ClearReferences();
        GC.Collect();
    }

    private void Start()
    {
        m_playerController.MoveCharacterToSelectedPositionEvent += PlayerController_OnMoveCharacterToSelectedPosition;

        //TEST SPAWN ENEMIES
        int spawnedCount = 0;
        int skippedCount = 0;
        for (int i = 0; i < 10; i++)
        {
            if (m_ground.TryGetNewEnemyPosition(out Vector3 spawnPosition))
            {
                m_ground.AddEnemy(m_enemySpawner.CreateEnemy(spawnPosition));
                spawnedCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        MyLogger.Log($"Enemy spawn: {spawnedCount} spawned, {skippedCount} skipped (grid full).", MyLogger.LogLevel.Info);
        //////TEST SPAWN ENEMIES END
    }

    private void PlayerController_OnMoveCharacterToSelectedPosition()
    {
        Vector3 moveTo = m_ground.IndicateCharacterGroundLocation(m_cameraController.GetPointerScreenToRay(), ITacticalGroundStrategy.IndicationType.MovementConfirmation);
        m_playerController.MoveCharacterToSelectedPosition(moveTo);
    }

    private void Update()
    {
        TimerManager.Update(Time.deltaTime);

        //TODO : "if We Are Currently Selecting A Character and Moving A Cursor to Move Character there"
        m_ground.IndicateCharacterGroundLocation(m_cameraController.GetPointerScreenToRay(), ITacticalGroundStrategy.IndicationType.MovementPreview);
    }
}
