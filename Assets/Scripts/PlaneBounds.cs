using UnityEngine;

/// <summary>
/// Stores the oriented world corners of a Unity Plane.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class PlaneBounds : MonoBehaviour
{
    /// <summary>
    /// World-space corners of the plane.
    /// Order:
    /// 0 = Bottom Left
    /// 1 = Bottom Right
    /// 2 = Top Right
    /// 3 = Top Left
    /// </summary>
    private Vector3[] m_corners = new Vector3[4];

    private void Awake()
    {
        RefreshCorners();
    }

    /// <summary>
    /// Computes the oriented plane corners from mesh.bounds to 4 world space positions and saves them to m_corners.
    /// </summary>
    public void RefreshCorners()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = meshFilter.sharedMesh;

        Bounds localBounds = mesh.bounds;

        Vector3 min = localBounds.min;
        Vector3 max = localBounds.max;

        // Local corners
        Vector3 bottomLeft = new Vector3(min.x, 0f, min.z);
        Vector3 bottomRight = new Vector3(max.x, 0f, min.z);
        Vector3 topRight = new Vector3(max.x, 0f, max.z);
        Vector3 topLeft = new Vector3(min.x, 0f, max.z);

        // Transform into world space
        m_corners[0] = transform.TransformPoint(bottomLeft);
        m_corners[1] = transform.TransformPoint(bottomRight);
        m_corners[2] = transform.TransformPoint(topRight);
        m_corners[3] = transform.TransformPoint(topLeft);
    }

    /// <summary>
    /// Returns a random point inside the oriented plane (XZ space).
    /// Assumes m_corners are ordered:
    /// 0 = Bottom Left
    /// 1 = Bottom Right
    /// 2 = Top Right
    /// 3 = Top Left
    /// </summary>
    public Vector3 GetRandomPlanePointInsideBounds()
    {
        float u = Random.value;
        float v = Random.value;

        // Bilinear interpolation on the quad
        Vector3 point =
            (1 - u) * (1 - v) * m_corners[0] + // Bottom Left
            u * (1 - v) * m_corners[1] +       // Bottom Right
            u * v * m_corners[2] +             // Top Right
            (1 - u) * v * m_corners[3];        // Top Left

        return point;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        RefreshCorners();

        Gizmos.color = Color.green;

        for (int i = 0; i < 4; i++)
        {
            Vector3 a = m_corners[i];
            Vector3 b = m_corners[(i + 1) % 4];

            Gizmos.DrawLine(a, b);
        }
    }

#endif
}