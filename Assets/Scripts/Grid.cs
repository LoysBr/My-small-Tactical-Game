using System.Collections.Generic;
using UnityEngine;


public class Grid
{
    private QuadTreeNode m_Root;

    public Grid(float worldSize)
    {
        // Centre du monde à (0,0) sur XZ
        Rect worldRect = new Rect(
            -worldSize * 0.5f,
            -worldSize * 0.5f,
            worldSize,
            worldSize
        );

        m_Root = new QuadTreeNode(worldRect);
    }

    /// <summary>
    /// Ajoute un ennemi dans le QuadTree.
    /// </summary>
    public bool AddEnemy(EnemyModel enemy)
    {
        Vector3 pos = enemy.Position;
        Vector2 point = new Vector2(pos.x, pos.z);

        return m_Root.Insert(enemy, point);
    }

    /// <summary>
    /// Supprime un ennemi du QuadTree.
    /// </summary>
    public bool RemoveEnemy(EnemyModel enemy)
    {
        return m_Root.Remove(enemy);
    }

    /// <summary>
    /// Retourne toutes les plus grandes cellules vides.
    /// </summary>
    public List<Rect> GetLargestEmptyCells()
    {
        List<Rect> result = new List<Rect>();
        m_Root.CollectLargestEmptyCells(result);
        return result;
    }
}


public class QuadTreeNode
{
    private const int MAX_DEPTH = 8;

    public Rect m_Bounds;

    private EnemyModel m_StoredEnemy;
    private bool m_HasEnemy;
    private QuadTreeNode[] m_Children;
    private int m_Depth;

    public QuadTreeNode(Rect bounds, int depth = 0)
    {
        this.m_Bounds = bounds;
        this.m_Depth = depth;
    }

    public bool IsLeaf => m_Children == null;

    public bool Insert(EnemyModel enemy, Vector2 point)
    {
        if (!m_Bounds.Contains(point))
            return false;

        // Si feuille vide
        if (IsLeaf && !m_HasEnemy)
        {
            m_StoredEnemy = enemy;
            m_HasEnemy = true;
            return true;
        }

        // Si profondeur max atteinte
        if (m_Depth >= MAX_DEPTH)
            return false;

        // Si feuille déjà occupée -> subdivision
        if (IsLeaf)
        {
            Subdivide();

            // Réinjecter ancien ennemi
            Vector3 oldPos = m_StoredEnemy.Position;
            Vector2 oldPoint = new Vector2(oldPos.x, oldPos.z);

            EnemyModel oldEnemy = m_StoredEnemy;

            m_StoredEnemy = null;
            m_HasEnemy = false;

            InsertIntoChildren(oldEnemy, oldPoint);
        }

        return InsertIntoChildren(enemy, point);
    }

    private bool InsertIntoChildren(EnemyModel enemy, Vector2 point)
    {
        foreach (var child in m_Children)
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
            if (m_HasEnemy && m_StoredEnemy == enemy)
            {
                m_StoredEnemy = null;
                m_HasEnemy = false;
                return true;
            }

            return false;
        }

        // sinon recherche dans enfants
        foreach (var child in m_Children)
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

        foreach (var child in m_Children)
        {
            if (!child.IsCompletelyEmpty())
            {
                allEmpty = false;
                break;
            }
        }

        if (allEmpty)
        {
            m_Children = null;
        }
    }

    public bool IsCompletelyEmpty()
    {
        if (IsLeaf)
            return !m_HasEnemy;

        foreach (var child in m_Children)
        {
            if (!child.IsCompletelyEmpty())
                return false;
        }

        return true;
    }

    private void Subdivide()
    {
        m_Children = new QuadTreeNode[4];

        float halfWidth = m_Bounds.width * 0.5f;
        float halfHeight = m_Bounds.height * 0.5f;

        float x = m_Bounds.x;
        float y = m_Bounds.y;

        // Bas gauche
        m_Children[0] = new QuadTreeNode(
            new Rect(x, y, halfWidth, halfHeight),
            m_Depth + 1
        );

        // Bas droite
        m_Children[1] = new QuadTreeNode(
            new Rect(x + halfWidth, y, halfWidth, halfHeight),
            m_Depth + 1
        );

        // Haut gauche
        m_Children[2] = new QuadTreeNode(
            new Rect(x, y + halfHeight, halfWidth, halfHeight),
            m_Depth + 1
        );

        // Haut droite
        m_Children[3] = new QuadTreeNode(
            new Rect(x + halfWidth, y + halfHeight, halfWidth, halfHeight),
            m_Depth + 1
        );
    }

    /// <summary>
    /// Trouve les plus grandes cellules vides.
    /// </summary>
    public void CollectLargestEmptyCells(List<Rect> result)
    {
        // Feuille vide -> grande cellule valide
        if (IsLeaf && !m_HasEnemy)
        {
            result.Add(m_Bounds);
            return;
        }

        // Feuille occupée
        if (IsLeaf)
            return;

        // Explorer enfants
        foreach (var child in m_Children)
        {
            child.CollectLargestEmptyCells(result);
        }
    }
}