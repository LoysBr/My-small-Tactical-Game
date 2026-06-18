---
name: work-on-ground
description: Load full context for working on the Ground/Grid system. Reads all project C# scripts (excluding ScriptableObjectEvents) and surfaces the embedded QuadTreeGrid architecture. Use when the user asks to "work on ground", work on the grid, enemy density, the tactical ground plane, or PlaneBounds/QuadTreeGrid.
allowed-tools: [Read, Glob]
---

# Work On Ground

Prepare to work on the **Ground / Grid System**. This skill loads the full
source context and the architecture summary below.

## Instructions

1. Use Glob to find all `.cs` files under `Assets/Scripts/` recursively.
2. Filter out any file whose path contains `ScriptableObjectEvents`.
3. Read every remaining file using the Read tool.
4. Confirm to the user which files were loaded, then state that the Grid System
   architecture below is now in effect.

Do NOT summarise the files unless the user asks.

## Architecture: the Ground / Grid System

The game's **Ground is a Unity Plane**. To manage the **density of the enemy
population** across that plane, a `QuadTreeGrid` is built from the plane's
surface (its oriented world-space corners).

### Ground = Plane — [PlaneBounds.cs](../../../Assets/Scripts/PlaneBounds.cs)

- `[RequireComponent(typeof(MeshFilter))]`.
- `RefreshCorners()` converts `mesh.bounds` into **4 oriented world-space
  corners** via `transform.TransformPoint`, in the order
  `0 = BottomLeft, 1 = BottomRight, 2 = TopRight, 3 = TopLeft`.
- Corners are exposed via the `Corners` property; refreshed in `Awake` and in
  `OnDrawGizmos`.
- `GetRandomPlanePointInsideBounds()` returns a random point on the quad using
  **bilinear interpolation** over the 4 corners.

### Density grid — [QuadTreeGrid.cs](../../../Assets/Scripts/QuadTreeGrid.cs)

- Constructed with `(Vector3[] gridCornersWorldPositions, int maxDepth)`. Stores
  the corners and derives `m_widthVector = corner1 - corner0` and
  `m_heightVector = corner3 - corner0`.
- Operates in **normalized 0–1 grid space**: the root node is `Rect(0, 0, 1, 1)`.
- `WorldPosToGridPos(worldPos)` — projects a world position onto the width/height
  vectors via `Vector3.Dot(...) / sqrMagnitude`, yielding 0–1 coordinates
  (values outside 0–1 mean the point is off the grid).
- `GridPosToWorldPos(gridPos)` — the inverse:
  `origin + gridPos.x * widthVector + gridPos.y * heightVector`.
- `AddEnemy` / `RemoveEnemy` delegate to the root `QuadTreeNode`.
- `GetLargestEmptyCells()` returns the bounds of the largest empty leaf cells
  (basis for density queries / placement).

#### QuadTreeNode (nested logic)

- Each **leaf holds at most one enemy** (`m_storedEnemy` / `m_hasEnemy`).
- Inserting where a leaf is already occupied triggers `Subdivide()` into 4
  children (bottom-left, bottom-right, top-left, top-right) up to `maxDepth`,
  then **reinjects the previous enemy** into the new children.
- `Remove` walks down to the leaf; after removal, `TryMerge()` collapses children
  back when they are all empty (`IsCompletelyEmpty`).
- `CollectLargestEmptyCells(result)` recursively gathers empty-leaf bounds.
- `DrawDebug` / `DrawRect` draw the leaves back in world space via
  `GridPosToWorldPos`.

### Ground strategy — [BG3TacticalGroundController.cs](../../../Assets/Scripts/TacticalGame/BG3TacticalGroundController.cs)

- `MonoBehaviour` implementing
  [ITacticalGroundStrategy](../../../Assets/Scripts/TacticalGame/ITacticalGroundStrategy.cs).
- `Awake`: `m_planeBounds = GetComponent<PlaneBounds>()`, then
  `m_enemyDensityManagementGrid = new QuadTreeGrid(m_planeBounds.Corners, 32)`.
- `AddEnemy` / `RemoveEnemy` forward to the grid.
- `IndicateCharacterGroundLocation(ray, indicationType)` raycasts against
  `m_tacticalGridLayer` and drives the `GroundIndicator`s (movement preview vs.
  timed movement-confirmation), returning the hit point.
- `GetRandomGroundLocation()` → `PlaneBounds.GetRandomPlanePointInsideBounds()`.

### Supporting types

- [EnemyModel.cs](../../../Assets/Scripts/TacticalGame/EnemyModel.cs) — what the
  grid stores: wraps a world `Position` and an `EnemyPresenter`.
- [GroundIndicator.cs](../../../Assets/Scripts/TacticalGame/GroundIndicator.cs) —
  show/hide and position the on-ground feedback markers.

### Wiring — [TacticalGameManager.cs](../../../Assets/Scripts/TacticalGame/TacticalGameManager.cs)

- Assigns `m_ground = m_groundController`.
- On `Start`, spawns enemies at random ground locations and registers each into
  the grid via `m_ground.AddEnemy(m_enemySpawner.CreateEnemy(m_ground.GetRandomGroundLocation()))`.

## Known TODOs / notes

- `QuadTreeGrid.SetMinimumCellSize` is currently an empty stub.
- `QuadTreeNode.DrawRect` already converts via `GridPosToWorldPos`, so the inline
  TODO about using world-space corners is effectively addressed.
