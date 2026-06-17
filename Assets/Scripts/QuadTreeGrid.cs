using System.Collections.Generic;
using UnityEngine;

public class QuadTreeGrid
{
    private readonly int m_maxDepth;
    public int MaxDepth { get { return m_maxDepth; } }

    private QuadTreeNode m_root;
    private Vector3[] m_gridCornersWorldPositions;
    private Vector3 m_widthVector;
    private Vector3 m_heightVector;

    /// <param name="gridCornersWorldPositions"> World-space corners of the plane.
    /// Order:
    /// 0 = Bottom Left
    /// 1 = Bottom Right
    /// 2 = Top Right
    /// 3 = Top Left</param>
    /// <param name="maxDepth"></param>
    public QuadTreeGrid(Vector3[] gridCornersWorldPositions, int maxDepth)
    {       
        m_gridCornersWorldPositions = gridCornersWorldPositions;
        m_widthVector = m_gridCornersWorldPositions[1] - m_gridCornersWorldPositions[0];
        m_heightVector = m_gridCornersWorldPositions[3] - m_gridCornersWorldPositions[0];

        // float width = Vector3.Distance(m_gridCornersWorldPositions[0], m_gridCornersWorldPositions[1]);
        // float height = Vector3.Distance(m_gridCornersWorldPositions[0], m_gridCornersWorldPositions[3]);

        Rect gridBounds = new Rect(
            x: 0,
            y: 0,
            width: 1,
            height: 1
        );

        m_maxDepth = maxDepth;
        m_root = new QuadTreeNode(this, gridBounds, depth: 0, maxDepth: m_maxDepth);
    }

    /// <param name="worldPos">Unity World 3D Position</param>
    /// <returns>A value in grid space (0-1 range). X = horizontal/width, Y = vertical/height.</returns>
    public Vector2 WorldPosToGridPos(Vector3 worldPos)
    {
        Vector3 origin = m_gridCornersWorldPositions[0];
        Vector3 widthVector = m_gridCornersWorldPositions[1] - origin;
        Vector3 heightVector = m_gridCornersWorldPositions[3] - origin;

        //should be between 0 and 1, otherwise it means the worldPos is outside of the grid
        float worldPosInGridWidth = Vector3.Dot(worldPos - origin, widthVector) / widthVector.sqrMagnitude; 
        float worldPosInGridHeight = Vector3.Dot(worldPos - origin, heightVector) / heightVector.sqrMagnitude;
        
        return new Vector2(worldPosInGridWidth, worldPosInGridHeight);
    }
    
    public Vector3 GridPosToWorldPos(Vector2 gridPos)
    {
        return m_gridCornersWorldPositions[0] + gridPos.x * m_widthVector + gridPos.y * m_heightVector;
    }
    
    // public Vector3 GetGridWidthVector()
    // {
    //     return m_widthVector;
    // }

    // public Vector3 GetGridHeightVector()
    // {
    //     return m_heightVector;
    // }

    // public float GetGridWith()
    // {
    //     return Vector3.Distance(m_gridCornersWorldPositions[0], m_gridCornersWorldPositions[1]);
    // }

    // public float GetGridHeight()
    // {
    //     return Vector3.Distance(m_gridCornersWorldPositions[0], m_gridCornersWorldPositions[3]);
    // }

    /// <summary>
    /// Add an enemy to the QuadTree.
    /// </summary>
    public bool AddEnemy(EnemyModel enemy)
    {
        Vector3 pos = enemy.Position;
        Vector2 point = WorldPosToGridPos(pos);

        bool enemyIsInsideRoot = m_root.Insert(enemy);
        if (!enemyIsInsideRoot)
            MyLogger.Log($"Grid Insert Enemy Pos : {enemy.Position.x}, {enemy.Position.y}, {enemy.Position.z} => ROOT does NOT contain it.", MyLogger.LogLevel.Error);
        return enemyIsInsideRoot;
    }

    /// <summary>
    /// Remove an enemy from the QuadTree.
    /// </summary>
    public bool RemoveEnemy(EnemyModel enemy)
    {
        return m_root.Remove(enemy);
    }

    public void SetMinimumCellSize(float sizeInMeter)
    {

    }

    /// <summary>
    /// Retourne toutes les plus grandes cellules vides.
    /// </summary>
    public List<Rect> GetLargestEmptyCells()
    {
        List<Rect> result = new List<Rect>();
        m_root.CollectLargestEmptyCells(result);
        return result;
    }

    #region Debug Drawing

    /// <summary>
    /// Draws the entire QuadTree using Gizmos.
    /// </summary>
    public void DrawDebug()
    {
        if (m_root == null)
            return;

        Gizmos.color = Color.cyan;

        m_root.DrawDebug();
    }

    #endregion
}


public class QuadTreeNode
{
    private readonly int m_maxDepth;

    /// <summary>
    /// Bounds of this node in grid space (0-1 range). X = horizontal/width, Y = vertical/height.
    /// </summary>
    public Rect Bounds { get; private set; }

    private readonly QuadTreeGrid m_parentGrid; 
    private EnemyModel m_storedEnemy;
    private bool m_hasEnemy;
    private QuadTreeNode[] m_children;
    private int m_depth;

    public QuadTreeNode(QuadTreeGrid parentGrid, Rect bounds, int depth, int maxDepth)
    {
        this.m_parentGrid = parentGrid;
        this.m_depth = depth;
        this.m_maxDepth = maxDepth;
        this.Bounds = bounds;
    }

    public bool IsLeaf => m_children == null;

    public bool Insert(EnemyModel enemy)
    {
        Vector2 gridPos = m_parentGrid.WorldPosToGridPos(enemy.Position);

        if (!Contains(gridPos))
            return false;

        if (IsLeaf && !m_hasEnemy)
        {
            m_storedEnemy = enemy;
            m_hasEnemy = true;
            return true;
        }

        if (m_depth >= m_maxDepth)
            return false;

        // leaf already busy -> subdivision
        if (IsLeaf)
        {
            Subdivide();

            // Reinject former enemy
            EnemyModel oldEnemy = m_storedEnemy; //buffer for old variable
            m_storedEnemy = null;
            m_hasEnemy = false;
            InsertIntoChildren(oldEnemy);
        }

        return InsertIntoChildren(enemy);
    }

    public bool Contains(Vector2 gridPos)
    {
        if (gridPos.x < Bounds.x || gridPos.x > Bounds.x + Bounds.width 
         || gridPos.y < Bounds.y || gridPos.y > Bounds.y + Bounds.height)
            return false;

        return true;
    }

    private bool InsertIntoChildren(EnemyModel enemy)
    {
        foreach (QuadTreeNode child in m_children)
        {
            if (child.Insert(enemy))
                return true;
        }

        return false;
    }

    public bool Remove(EnemyModel enemy)
    {
        if (IsLeaf)
        {
            if (m_hasEnemy && m_storedEnemy == enemy)
            {
                m_storedEnemy = null;
                m_hasEnemy = false;
                return true;
            }

            return false;
        }

        foreach (QuadTreeNode child in m_children)
        {
            if (child.Remove(enemy))
            {
                TryMerge();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Merge children if all are empty to reduce tree size.
    /// </summary>
    private void TryMerge()
    {
        if (IsLeaf)
            return;

        bool allEmpty = true;

        foreach (QuadTreeNode child in m_children)
        {
            if (!child.IsCompletelyEmpty())
            {
                allEmpty = false;
                break;
            }
        }

        if (allEmpty)
        {
            m_children = null;
        }
    }

    public bool IsCompletelyEmpty()
    {
        if (IsLeaf)
            return !m_hasEnemy;

        foreach (QuadTreeNode child in m_children)
        {
            if (!child.IsCompletelyEmpty())
                return false;
        }

        return true;
    }

    private void Subdivide()
    {
        m_children = new QuadTreeNode[4];

        float halfWidth = Bounds.width * 0.5f;
        float halfHeight = Bounds.height * 0.5f;
        float x = Bounds.x;
        float y = Bounds.y;

        // bottom left
        m_children[0] = new QuadTreeNode(m_parentGrid,
            new Rect(x, y, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // bottom right
        m_children[1] = new QuadTreeNode(m_parentGrid,
            new Rect(x + halfWidth, y, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // top left
        m_children[2] = new QuadTreeNode(m_parentGrid,
            new Rect(x, y + halfHeight, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // top right
        m_children[3] = new QuadTreeNode(m_parentGrid,
            new Rect(x + halfWidth, y + halfHeight, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );
    }

    /// <summary>
    /// Find biggest empty cells recursively and add them to result.
    /// </summary>
    public void CollectLargestEmptyCells(List<Rect> result)
    {
        if (IsLeaf && !m_hasEnemy)
        {
            result.Add(Bounds);
            return;
        }

        // is already busy
        if (IsLeaf)
            return;

        foreach (QuadTreeNode child in m_children)
        {
            child.CollectLargestEmptyCells(result);
        }
    }

    #region Debug Drawing

    /// <summary>
    /// Draws only leaf nodes recursively.
    /// </summary>
    public void DrawDebug()
    {
        // Draw only leaves
        if (IsLeaf)
        {
            DrawRect(Bounds);
            return;
        }

        // Otherwise recurse into children
        foreach (QuadTreeNode child in m_children)
        {
            child.DrawDebug();
        }
    }

    /// <summary>
    /// Draws a Rect in XZ space.
    /// </summary>
    /// //TODO rewrite this to use the world space corners of the grid instead of assuming a 0-1 range in grid space, to be more robust and reusable.
    private void DrawRect(Rect rect)
    {
        Vector3 bottomLeft = m_parentGrid.GridPosToWorldPos(new Vector2(rect.x, rect.y));
        Vector3 bottomRight = m_parentGrid.GridPosToWorldPos(new Vector2(rect.xMax, rect.y));
        Vector3 topRight = m_parentGrid.GridPosToWorldPos(new Vector2(rect.xMax, rect.yMax));
        Vector3 topLeft = m_parentGrid.GridPosToWorldPos(new Vector2(rect.x, rect.yMax));

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    #endregion
}