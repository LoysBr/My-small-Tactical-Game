using UnityEngine;

/// <summary>
/// Owns the enemy-density grid and decides where new enemies are allowed to spawn,
/// so the population stays spread out with a guaranteed minimum area per enemy.
/// </summary>
public class PopulationDensityController
{
    private readonly QuadTreeGrid m_densityGrid;

    /// <param name="minAreaPerElement">Minimum ground area (in square meters) reserved for each enemy.</param>
    /// <param name="groundPlane">Plane the enemies are spawned on.</param>
    public PopulationDensityController(float minAreaPerElement, PlaneBounds groundPlane)
    {
        m_densityGrid = new QuadTreeGrid(groundPlane.Corners, minAreaPerElement);
    }

    public void AddEnemy(EnemyModel enemy)
    {
        m_densityGrid.AddEnemy(enemy);
    }

    public void RemoveEnemy(EnemyModel enemy)
    {
        m_densityGrid.RemoveEnemy(enemy);
    }

    /// <summary>
    /// Returns a spawn position located in a still-free cell of the density grid.
    /// </summary>
    /// <returns>False when every cell is occupied; no position is produced.</returns>
    public bool TryGetNewEnemyPosition(out Vector3 position)
    {
        return m_densityGrid.TryGetRandomPosition(out position);
    }

    public void DrawDebug()
    {
        m_densityGrid.DrawDebug();
    }
}
