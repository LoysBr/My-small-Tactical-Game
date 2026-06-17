using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int m_spawnCount;
    [SerializeField] private int m_spawnDelay; //every Xsec, spawn a new Enemy

    /// <summary>
    /// Enemy prefab used by the pool.
    /// </summary>
    [SerializeField]
    private EnemyPresenter m_enemyPrefab;

    /// <summary>
    /// Initial amount of pooled objects.
    /// </summary>
    [SerializeField]
    private int m_initialPoolSize = 32;

    /// <summary>
    /// Optional parent for pooled objects.
    /// </summary>
    [SerializeField]
    private Transform m_poolParent;

    /// <summary>
    /// Internal presenter pool.
    /// </summary>
    private Queue<EnemyPresenter> m_pool = new Queue<EnemyPresenter>();

    /// <summary>
    /// Active enemy models.
    /// </summary>
    private List<EnemyModel> m_activeEnemies = new List<EnemyModel>();

    /// <summary>
    /// Public read-only access to active enemies.
    /// </summary>
    public IReadOnlyList<EnemyModel> ActiveEnemies => m_activeEnemies;


    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static EnemySpawner Instance { get; private set; }


    //private void Awake()
    //{
    //    EnemyModel enemyController = m_enemyPrefab.GetComponent<EnemyModel>();
    //    if (enemyController == null)
    //    {
    //        MyLogger.Log("Problem with m_enemyPrefab : no script EnemyController attached!", MyLogger.LogLevel.Error);
    //    }
    //}

    //private void Start()
    //{
    //    m_spawnedEnemies = new List<EnemyModel>(m_spawnCount);

    //    SpawnEnemy(Vector3.zero);
    //}

    //private EnemyModel SpawnEnemy(Vector3 position)
    //{
    //    GameObject enemyObject = GameObject.Instantiate(m_enemyPrefab, position,
    //        Quaternion.LookRotation(Vector3.forward, Vector3.up), this.gameObject.transform);

    //    return enemyObject.GetComponent<EnemyModel>();
    //}

    private void Awake()
    {
        InitializeSingleton();
        InitializePool();
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            MyLogger.Log("Error : you should never have several EnemySpawner in the scene.", MyLogger.LogLevel.Error);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Pre-creates pooled presenters.
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < m_initialPoolSize; i++)
        {
            EnemyPresenter presenter = Instantiate(m_enemyPrefab, m_poolParent);
            presenter.gameObject.SetActive(false);
            m_pool.Enqueue(presenter);
        }
    }

    /// <summary>
    /// Creates a new enemy.
    /// </summary>
    /// <param name="position">Spawn world position.</param>
    /// <returns>Created enemy model.</returns>
    public EnemyModel CreateEnemy(Vector3 position)
    {
        // Create Enemy Presenter
        EnemyPresenter presenter = GetPresenterFromPool();

        presenter.transform.position = position;
        presenter.gameObject.SetActive(true);

        // Create Enemy Model
        EnemyModel model = new EnemyModel(presenter);

        m_activeEnemies.Add(model);
        return model;
    }

    /// <summary>
    /// Removes an enemy and returns its presenter to the pool.
    /// </summary>
    /// <param name="model">Enemy model to remove.</param>
    public void RemoveEnemy(EnemyModel model)
    {
        if (model == null)
            return;

        EnemyPresenter presenter = model.Presenter;

        if (presenter != null)
        {
            presenter.Clear();
            presenter.gameObject.SetActive(false);

            m_pool.Enqueue(presenter);
        }

        m_activeEnemies.Remove(model);
    }

    /// <summary>
    /// Retrieves a presenter from the pool.
    /// Automatically expands the pool by 1 element if needed.
    /// </summary>
    private EnemyPresenter GetPresenterFromPool()
    {
        if (m_pool.Count == 0)
        {
            EnemyPresenter presenter = Instantiate(m_enemyPrefab, m_poolParent);
            presenter.gameObject.SetActive(false);
            m_pool.Enqueue(presenter);
        }

        return m_pool.Dequeue();
    }



    private void Update()
    {
        
    }
}
