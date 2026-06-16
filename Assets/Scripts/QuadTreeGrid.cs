using System.Collections.Generic;
using UnityEngine;

public class QuadTreeGrid
{
    private readonly int m_maxDepth;
    public int MaxDepth { get { return m_maxDepth; } }

    private QuadTreeNode m_root;

    public QuadTreeGrid(float worldSize, int maxDepth)
    {
        // Centre du monde � (0,0) sur XZ
        //TODO : change this using real Ground
        Rect worldRect = new Rect(
            -worldSize * 0.5f,
            -worldSize * 0.5f,
            worldSize,
            worldSize
        );

        m_maxDepth = maxDepth;
        m_root = new QuadTreeNode(bounds: worldRect, depth: 0, maxDepth: m_maxDepth);
    }

    /// <summary>
    /// Ajoute un ennemi dans le QuadTree.
    /// </summary>
    public bool AddEnemy(EnemyModel enemy)
    {
        Vector3 pos = enemy.Position;
        Vector2 point = new Vector2(pos.x, pos.z);

        bool enemyIsInsideRoot = m_root.Insert(enemy, point);
        if (!enemyIsInsideRoot)
            MyLogger.Log($"Grid Insert Enemy Pos : {point.x}, {point.y} => ROOT does NOT contain it ???!!!", MyLogger.LogLevel.Error);
        return enemyIsInsideRoot;
    }

    /// <summary>
    /// Supprime un ennemi du QuadTree.
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

    public Rect Bounds { get; private set; }

    private EnemyModel m_storedEnemy;
    private bool m_hasEnemy;
    private QuadTreeNode[] m_children;
    private int m_depth;

    public QuadTreeNode(Rect bounds, int depth, int maxDepth)
    {
        this.Bounds = bounds;
        this.m_depth = depth;
        this.m_maxDepth = maxDepth;
    }

    public bool IsLeaf => m_children == null;

    public bool Insert(EnemyModel enemy, Vector2 point)
    {
        if (!Bounds.Contains(point))
            return false;

        // Si feuille vide
        if (IsLeaf && !m_hasEnemy)
        {
            m_storedEnemy = enemy;
            m_hasEnemy = true;
            return true;
        }

        // Si profondeur max atteinte
        if (m_depth >= m_maxDepth)
            return false;

        // Si feuille d�j� occup�e -> subdivision
        if (IsLeaf)
        {
            Subdivide();

            // R�injecter ancien ennemi
            Vector3 oldPos = m_storedEnemy.Position;
            Vector2 oldPoint = new Vector2(oldPos.x, oldPos.z);

            EnemyModel oldEnemy = m_storedEnemy;

            m_storedEnemy = null;
            m_hasEnemy = false;

            InsertIntoChildren(oldEnemy, oldPoint);
        }

        return InsertIntoChildren(enemy, point);
    }

    private bool InsertIntoChildren(EnemyModel enemy, Vector2 point)
    {
        foreach (QuadTreeNode child in m_children)
        {
            if (child.Insert(enemy, point))
                return true;
        }

        return false;
    }

    public bool Remove(EnemyModel enemy)
    {
        // Cas feuille
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

        // sinon recherche dans enfants
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
    /// Fusionne les enfants si tous sont vides.
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

        // Bas gauche
        m_children[0] = new QuadTreeNode(
            new Rect(x, y, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // Bas droite
        m_children[1] = new QuadTreeNode(
            new Rect(x + halfWidth, y, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // Haut gauche
        m_children[2] = new QuadTreeNode(
            new Rect(x, y + halfHeight, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );

        // Haut droite
        m_children[3] = new QuadTreeNode(
            new Rect(x + halfWidth, y + halfHeight, halfWidth, halfHeight),
            m_depth + 1, m_maxDepth
        );
    }

    /// <summary>
    /// Trouve les plus grandes cellules vides.
    /// </summary>
    public void CollectLargestEmptyCells(List<Rect> result)
    {
        // Feuille vide -> grande cellule valide
        if (IsLeaf && !m_hasEnemy)
        {
            result.Add(Bounds);
            return;
        }

        // Feuille occup�e
        if (IsLeaf)
            return;

        // Explorer enfants
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
    private void DrawRect(Rect rect)
    {
        Vector3 bottomLeft = new Vector3(rect.xMin, 0f, rect.yMin);
        Vector3 bottomRight = new Vector3(rect.xMax, 0f, rect.yMin);
        Vector3 topRight = new Vector3(rect.xMax, 0f, rect.yMax);
        Vector3 topLeft = new Vector3(rect.xMin, 0f, rect.yMax);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    #endregion
}