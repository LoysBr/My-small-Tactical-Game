using System.Collections;
using UnityEngine;

public class BG3TacticalGroundController : MonoBehaviour, ITacticalGroundStrategy 
{
    [SerializeField] private GroundIndicator m_charMovementPreviewIndicator;
    [SerializeField] private GroundIndicator m_charMovementConfirmationIndicator;
    [SerializeField] private LayerMask m_tacticalGridLayer;
    [SerializeField] private float m_charMovementConfirmationShowedDuration = 1f;

    /// <summary>
    /// Minimum ground area (in square meters) reserved for each spawned enemy.
    /// </summary>
    [SerializeField] private float m_minAreaPerEnemy = 3f;

    private PlaneBounds m_planeBounds;

    /// <summary>
    /// Manages where enemies are allowed to spawn on the Plane and their density,
    /// using a Grid subdivided in Cells.
    /// </summary>
    private PopulationDensityController m_populationDensity;

    private Coroutine m_hidingCharMovementConfirmation;

    private void Awake()
    {
        m_planeBounds = GetComponent<PlaneBounds>();
        m_populationDensity = new PopulationDensityController(m_minAreaPerEnemy, m_planeBounds);
    }

    public void AddEnemy(EnemyModel enemy)
    {
        m_populationDensity.AddEnemy(enemy);
    }

    public void RemoveEnemy(EnemyModel enemy)
    {
        m_populationDensity.RemoveEnemy(enemy);
    }

    public bool TryGetNewEnemyPosition(out Vector3 position)
    {
        return m_populationDensity.TryGetNewEnemyPosition(out position);
    }

    public Vector3 IndicateCharacterGroundLocation(Ray screenPointToRay, ITacticalGroundStrategy.IndicationType indicationType)
    {
        if (Physics.Raycast(screenPointToRay, out RaycastHit hitInfo, 1000f, m_tacticalGridLayer))
        {
            GroundIndicator showedIndicator;
            switch (indicationType)
            {
                case ITacticalGroundStrategy.IndicationType.SimpleSelection:
                case ITacticalGroundStrategy.IndicationType.MovementPreview:
                    showedIndicator = m_charMovementPreviewIndicator;
                    break;
                case ITacticalGroundStrategy.IndicationType.MovementConfirmation:
                    showedIndicator = m_charMovementConfirmationIndicator;

                    if (m_hidingCharMovementConfirmation != null)
                    {
                        StopCoroutine(m_hidingCharMovementConfirmation);
                    }

                    m_hidingCharMovementConfirmation = StartCoroutine(ShowIndicatorWithDelay(showedIndicator, false, m_charMovementConfirmationShowedDuration));
                    break;
                default:
                    showedIndicator = null;
                    break;
            }

            showedIndicator?.Show(true);
            showedIndicator?.SetIndicatorPosition(hitInfo.point);
                
            return hitInfo.point;
        }
        else
        {
            m_charMovementPreviewIndicator?.Show(false);
        }

        return Vector3.zero;
    }

    private IEnumerator ShowIndicatorWithDelay(GroundIndicator indicator, bool show, float delay)
    {
        yield return new WaitForSeconds(delay);

        indicator.Show(show);
        m_hidingCharMovementConfirmation = null;
        yield return null;
    }

    public Vector3 GetRandomGroundLocation()
    {
        return m_planeBounds.GetRandomPlanePointInsideBounds();
    }

    private void OnDrawGizmos()
    {
        if (m_populationDensity == null)
            return;

        m_populationDensity.DrawDebug();
    }

    ///// <summary>
    ///// Computes the plane bounds using the Renderer bounds. Plane must have an orientation of 0�
    ///// </summary>
    //public void UpdateGroundPlaneBounds()
    //{
    //    Renderer renderer = GetComponent<Renderer>();

    //    //Warnin : this works only because our Plane has an orientation of 0�, since Bounds == AABB Axis-Aligned Bounding Box
    //    Bounds bounds = renderer.bounds;

    //    // Convert Unity Bounds (3D) into Rect (2D XZ space)
    //    m_GroundPlaneBounds = new Rect(
    //        bounds.min.x,
    //        bounds.min.z,
    //        bounds.size.x,
    //        bounds.size.z
    //    );
    //}
}
