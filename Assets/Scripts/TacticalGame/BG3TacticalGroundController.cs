using System.Collections;
using UnityEngine;

public class BG3TacticalGroundController : MonoBehaviour, ITacticalGroundStrategy 
{
    [SerializeField] private GroundIndicator m_CharMovementPreviewIndicator;
    [SerializeField] private GroundIndicator m_CharMovementConfirmationIndicator;
    [SerializeField] private LayerMask m_TacticalGridLayer;
    [SerializeField] private float m_CharMovementConfirmationShowedDuration = 1f;

    private Coroutine m_HidingCharMovementConfirmation;

    // public Rect m_GroundPlaneBounds;
    private PlaneBounds m_PlaneBounds;

    private void Start()
    {
        m_PlaneBounds = GetComponent<PlaneBounds>();
    }

    public Vector3 IndicateCharacterGroundLocation(Ray screenPointToRay, ITacticalGroundStrategy.IndicationType indicationType)
    {
        if (Physics.Raycast(screenPointToRay, out RaycastHit hitInfo, 1000f, m_TacticalGridLayer))
        {
            GroundIndicator showedIndicator;
            switch (indicationType)
            {
                case ITacticalGroundStrategy.IndicationType.SimpleSelection:
                case ITacticalGroundStrategy.IndicationType.MovementPreview:
                    showedIndicator = m_CharMovementPreviewIndicator;
                    break;
                case ITacticalGroundStrategy.IndicationType.MovementConfirmation:
                    showedIndicator = m_CharMovementConfirmationIndicator;

                    if (m_HidingCharMovementConfirmation != null)
                    {
                        StopCoroutine(m_HidingCharMovementConfirmation);
                    }

                    m_HidingCharMovementConfirmation = StartCoroutine(ShowIndicatorWithDelay(showedIndicator, false, m_CharMovementConfirmationShowedDuration));
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
            m_CharMovementPreviewIndicator?.Show(false);
        }

        return Vector3.zero;
    }

    private IEnumerator ShowIndicatorWithDelay(GroundIndicator indicator, bool show, float delay)
    {
        yield return new WaitForSeconds(delay);

        indicator.Show(show);
        m_HidingCharMovementConfirmation = null;
        yield return null;
    }

    public Vector3 GetRandomGroundLocation()
    {
        return m_PlaneBounds.GetRandomPlanePointInsideBounds();
    }

    ///// <summary>
    ///// Computes the plane bounds using the Renderer bounds. Plane must have an orientation of 0°
    ///// </summary>
    //public void UpdateGroundPlaneBounds()
    //{
    //    Renderer renderer = GetComponent<Renderer>();

    //    //Warnin : this works only because our Plane has an orientation of 0°, since Bounds == AABB Axis-Aligned Bounding Box
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
