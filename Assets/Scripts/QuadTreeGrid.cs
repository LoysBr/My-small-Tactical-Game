using System.Collections.Generic;
using UnityEngine;

public class QuadTreeGrid
{
    private int m_maxDepth;
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
    /// <param name="minArea">Wanted minimum cell area, in square meters.</param>
    public QuadTreeGrid(Vector3[] gridCornersWorldPositions, float minArea)
    {
        m_gridCornersWorldPositions = gridCornersWorldPositions;
        m_widthVector = m_gridCornersWorldPositions[1] - m_gridCornersWorldPositions[0];
        m_heightVector = m_gridCornersWorldPositions[3] - m_gridCornersWorldPositions[0];

        SetMinArea(minArea);

        Rect gridBounds = new Rect(x: 0, y: 0, width: 1, height: 1);
        m_root = new QuadTreeNode(this, gridBounds, depth: 0, maxDepth: m_maxDepth);

        MyLogger.Log($"Initializing {nameof(QuadTreeGrid)} : root node created with maxDepth {m_maxDepth} (minimum cell area: {minArea} sqm)", MyLogger.LogLevel.Debug);
    }

    /// <summary>
    /// Computes the maximum tree depth from the wanted minimum cell area (in square meters).
    /// Each subdivision quarters a cell's area, so this keeps the deepest level whose
    /// cell area is still greater than or equal to the requested minimum area.
    /// </summary>
    public void SetMinArea(float minArea)
    {
        if (minArea <= 0)
        {
            MyLogger.Log($"{nameof(SetMinArea)} : cell area must be positive (got {minArea}). Falling back to maxDepth 0.", MyLogger.LogLevel.Error);
            m_maxDepth = 0;
            return;
        }

        float totalArea = Vector3.Cross(m_widthVector, m_heightVector).magnitude;

        m_maxDepth = Mathf.Max(0, Mathf.FloorToInt(Mathf.Log(totalArea / minArea, 4f)));
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

    /// <summary>
    /// Add an enemy to the QuadTree.
    /// </summary>
    public bool AddEnemy(EnemyModel enemy)
    {
        bool couldInsert = m_root.Insert(enemy);
        if (!couldInsert)
        {
            MyLogger.Log($"Grid Insert Enemy Pos : {enemy.Position.x}, {enemy.Position.y}, {enemy.Position.z} Impossible. Either position is out of the Ground, or we reached the maximum depth ( {m_maxDepth} )", MyLogger.LogLevel.Error);
        }

        return couldInsert;
    }

    /// <summary>
    /// Remove an enemy from the QuadTree.
    /// </summary>
    public bool RemoveEnemy(EnemyModel enemy)
    {
        return m_root.Remove(enemy);
    }

    /// <summary>
    /// Returns a random world position located in a cell that is still free (not yet
    /// holding an enemy). Returns false when every cell is occupied.
    /// </summary>
    public bool TryGetRandomPosition(out Vector3 worldPosition)
    {
        if (!m_root.TryGetRandomFreeGridPos(out Vector2 gridPos))
        {
            MyLogger.Log($"{nameof(TryGetRandomPosition)} : grid is full, no free cell available. No position returned.", MyLogger.LogLevel.Info);
            worldPosition = Vector3.zero;
            return false;
        }

        worldPosition = GridPosToWorldPos(gridPos);
        return true;
    }

    // /// <summary>
    // /// Returns all the largest empty cells.
    // /// </summary>
    // public List<Rect> GetLargestEmptyCells()
    // {
    //     List<Rect> result = new List<Rect>();
    //     m_root.CollectLargestEmptyCells(result);
    //     return result;
    // }

    #region Debug Drawing

    /// <summary>
    /// Draws the entire QuadTree using Gizmos. Busy cells (red) are drawn first and
    /// empty cells (cyan) last, so an empty cell's color wins on a side it shares with
    /// a busy cell.
    /// </summary>
    public void DrawDebug()
    {
        if (m_root == null)
            return;

        Gizmos.color = Color.red;
        m_root.DrawLeaves(drawOccupied: true);

        Gizmos.color = Color.cyan;
        m_root.DrawLeaves(drawOccupied: false);
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

    /// <summary>
    /// Number of enemies stored in this node's whole subtree. Cached to find a free
    /// cell (or detect a full subtree) without scanning every leaf.
    /// </summary>
    private int m_subtreeEnemyCount;

    public QuadTreeNode(QuadTreeGrid parentGrid, Rect bounds, int depth, int maxDepth)
    {
        this.m_parentGrid = parentGrid;
        this.m_depth = depth;
        this.m_maxDepth = maxDepth;
        this.Bounds = bounds;
    }

    public bool IsLeaf => m_children == null;

    /// <summary>
    /// Maximum number of enemies this node's subtree can hold (one per min-area cell).
    /// </summary>
    private int Capacity => 1 << (2 * (m_maxDepth - m_depth));

    private int FreeSlots => Capacity - m_subtreeEnemyCount;


    public bool Insert(EnemyModel enemy)
    {
        Vector2 gridPos = m_parentGrid.WorldPosToGridPos(enemy.Position);

        if (!Contains(gridPos))
            return false;

        if (IsLeaf)
        {
            if (!m_hasEnemy)
            {
                m_storedEnemy = enemy;
                m_hasEnemy = true;
                m_subtreeEnemyCount++;
                return true;
            }

            // leaf already busy and this is the smallest possible cell -> reject
            if (m_depth >= m_maxDepth)
                return false;

            // leaf already busy -> subdivision
            Subdivide();

            // Reinject former enemy (already counted in m_subtreeEnemyCount)
            EnemyModel oldEnemy = m_storedEnemy; //buffer for old variable
            m_storedEnemy = null;
            m_hasEnemy = false;
            InsertIntoChildren(oldEnemy);

            if (InsertIntoChildren(enemy))
            {
                m_subtreeEnemyCount++;
                return true;
            }

            return false;
        }

        if (InsertIntoChildren(enemy))
        {
            m_subtreeEnemyCount++;
            return true;
        }

        return false;
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
                m_subtreeEnemyCount--;
                return true;
            }

            return false;
        }

        foreach (QuadTreeNode child in m_children)
        {
            if (child.Remove(enemy))
            {
                m_subtreeEnemyCount--;
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
        m_children = SplitIn4();
    }

    private QuadTreeNode[] SplitIn4()
    {
        QuadTreeNode[] children = new QuadTreeNode[4];

        float halfWidth = Bounds.width * 0.5f;
        float halfHeight = Bounds.height * 0.5f;
        float x = Bounds.x;
        float y = Bounds.y;

        // bottom left
        children[0] = new QuadTreeNode(m_parentGrid,
            new Rect(x, y, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // bottom right
        children[1] = new QuadTreeNode(m_parentGrid,
            new Rect(x + halfWidth, y, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // top left
        children[2] = new QuadTreeNode(m_parentGrid,
            new Rect(x, y + halfHeight, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // top right
        children[3] = new QuadTreeNode(m_parentGrid,
            new Rect(x + halfWidth, y + halfHeight, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        return children;
    }

    // /// <summary>
    // /// Find biggest empty cells recursively and add them to result.
    // /// </summary>
    // public void CollectLargestEmptyCells(List<Rect> result)
    // {
    //     if (IsLeaf && !m_hasEnemy)
    //     {
    //         result.Add(Bounds);
    //         return;
    //     }

    //     // is already busy
    //     if (IsLeaf)
    //         return;

    //     foreach (QuadTreeNode child in m_children)
    //     {
    //         child.CollectLargestEmptyCells(result);
    //     }
    // }

    /// <summary>
    /// Returns a random grid-space point located in a min-area cell that is still free.
    /// Uses the cached subtree counts to descend only into branches that have room.
    /// </summary>
    public bool TryGetRandomFreeGridPos(out Vector2 gridPos)
    {
        gridPos = Vector2.zero;

        if (FreeSlots <= 0)
        {
            MyLogger.Log($"{GetNodeLogDescription()} : {nameof(TryGetRandomFreeGridPos)} : No free slots available. => return FALSE", MyLogger.LogLevel.Debug);
            return false;
        }            

        // Smallest possible cell, and not full -> it is empty: any point inside is free.
        if (m_depth >= m_maxDepth)
        {            
            gridPos = RandomPointInRect(Bounds);
            MyLogger.Log($"{GetNodeLogDescription()} : {nameof(TryGetRandomFreeGridPos)} : At max depth, using random point. Return {gridPos}", MyLogger.LogLevel.Debug);
            return true;
        }

        if (IsLeaf) // Leaf : we can find a position inside. A Leaf can have 0 or 1 Enemy
        {
            // 0 Enemy -> any point is free.
            if (!m_hasEnemy) 
            {
                gridPos = RandomPointInRect(Bounds);
                MyLogger.Log($"{GetNodeLogDescription()} : {nameof(TryGetRandomFreeGridPos)} : Empty Leaf, let's use RandomPointInRect(). Return {gridPos}", MyLogger.LogLevel.Debug);
                
                return true;
            }

            // 1 Enemy but is not full 
            // -> check what would be the 4 children and pick a random free cell among them
            // for now we do not insert an enemy so we do not Subdivide() yet
            gridPos = GetRandomPosInDividableLeaf();  
            MyLogger.Log($"{GetNodeLogDescription()} : {nameof(TryGetRandomFreeGridPos)} : Busy Leaf (but not at maxDepth) use GetRandomPosInDividableLeaf(). Return {gridPos}", MyLogger.LogLevel.Debug);
                      
            return true;
        }

        // not a leaf => check children
        MyLogger.Log($"{GetNodeLogDescription()} : {nameof(TryGetRandomFreeGridPos)} : NOT a Leaf => check children", MyLogger.LogLevel.Debug);
        return TryGetRandomFreeGridPosFromChildren(out gridPos);
    }

    /// <summary>
    /// Returns a random grid-space point located in the current Cell. 
    /// To be used on busy Leaves who can still be subdivided (not at max depth yet)
    /// </summary>
    private Vector2 GetRandomPosInDividableLeaf()
    {
        if (!IsLeaf || m_depth >= m_maxDepth)
        {
            MyLogger.Log($"Calling {nameof(GetRandomPosInDividableLeaf)} while this cell is not a leaf or at max depth", MyLogger.LogLevel.Error);
            return Vector2.zero;
        }            

        QuadTreeNode[] childrenIfWeInstert = SplitIn4();
        
        int indexOfOccupiedChild = -1;
        for (int i = 0; i < 3; i++)
        {
            if (childrenIfWeInstert[i].Contains(m_parentGrid.WorldPosToGridPos(m_storedEnemy.Position)))
            {
                MyLogger.Log($"{nameof(GetRandomPosInDividableLeaf)} indexOfOccupiedChild = {i}; ", MyLogger.LogLevel.Debug);
                indexOfOccupiedChild = i;   
                break;
            }
        }

        // Get any other child, randomly
        int randomFreeChildIndex = Random.Range(0, 4);
        while (randomFreeChildIndex == indexOfOccupiedChild)
        {
            randomFreeChildIndex = Random.Range(0, 4);
        }
        MyLogger.Log($"{nameof(GetRandomPosInDividableLeaf)} using randomFreeChildIndex = {randomFreeChildIndex}; ", MyLogger.LogLevel.Debug);

        return RandomPointInRect(childrenIfWeInstert[randomFreeChildIndex].Bounds);   
    }

    private bool TryGetRandomFreeGridPosFromChildren(out Vector2 gridPos)
    {
        gridPos = Vector2.zero;

        int totalFreeSlots = 0;
        foreach (QuadTreeNode child in m_children)
        {
            totalFreeSlots += child.FreeSlots;
        }

        if (totalFreeSlots <= 0)
            return false;

        foreach (QuadTreeNode child in m_children)
        {
            int childFreeSlots = child.FreeSlots;
            if (childFreeSlots <= 0)
                continue;

            return child.TryGetRandomFreeGridPos(out gridPos);
        }

        return false;
    }

    /// <summary>
    /// Picks a random min-area cell inside this (single-enemy) leaf, avoiding the cell
    /// the stored enemy already occupies, and returns a random point inside it.
    /// </summary>
    private Vector2 RandomFreeCellPoint()
    {
        int cellsPerSide = 1 << (m_maxDepth - m_depth);
        float cellWidth = Bounds.width / cellsPerSide;
        float cellHeight = Bounds.height / cellsPerSide;

        Vector2 enemyGridPos = m_parentGrid.WorldPosToGridPos(m_storedEnemy.Position);
        int occupiedColumn = Mathf.Clamp((int)((enemyGridPos.x - Bounds.x) / cellWidth), 0, cellsPerSide - 1);
        int occupiedRow = Mathf.Clamp((int)((enemyGridPos.y - Bounds.y) / cellHeight), 0, cellsPerSide - 1);

        int column;
        int row;
        do
        {
            column = Random.Range(0, cellsPerSide);
            row = Random.Range(0, cellsPerSide);
        }
        while (column == occupiedColumn && row == occupiedRow);

        float cellX = Bounds.x + column * cellWidth;
        float cellY = Bounds.y + row * cellHeight;

        return new Vector2(Random.Range(cellX, cellX + cellWidth), Random.Range(cellY, cellY + cellHeight));
    }

    private static Vector2 RandomPointInRect(Rect rect)
    {
        return new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
    }

    #region Debug Drawing

    /// <summary>
    /// Draws every leaf whose occupied state matches <paramref name="drawOccupied"/>,
    /// using the Gizmos color already set by the caller. Drawing busy then empty in two
    /// passes lets an empty cell's color win on a side it shares with a busy cell.
    /// </summary>
    public void DrawLeaves(bool drawOccupied)
    {
        if (IsLeaf)
        {
            if (m_hasEnemy == drawOccupied)
                DrawRect(Bounds);

            return;
        }

        foreach (QuadTreeNode child in m_children)
        {
            child.DrawLeaves(drawOccupied);
        }
    }

    /// <summary>
    /// Draws a Rect in world space using the grid's corners.
    /// </summary>
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

    private string GetNodeLogDescription()
    {
        return $"Node depth {m_depth}, bounds {Bounds}, hasEnemy {m_hasEnemy}, subtreeEnemyCount {m_subtreeEnemyCount}";
    }

    #endregion
}
