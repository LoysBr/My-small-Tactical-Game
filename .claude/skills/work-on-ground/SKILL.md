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

- Constructed with `(Vector3[] gridCornersWorldPositions, float minArea)`. Stores
  the corners and derives `m_widthVector = corner1 - corner0` and
  `m_heightVector = corner3 - corner0`.
- `SetMinArea(minArea)` (called from the constructor) derives `m_maxDepth` from
  the wanted **minimum cell area in m²**: each subdivision quarters a cell's area,
  so `m_maxDepth = floor(log4(totalArea / minArea))` (clamped to ≥ 0), keeping the
  deepest level whose cell area is still ≥ `minArea`. `totalArea` uses
  `Vector3.Cross(widthVector, heightVector).magnitude`.
- Operates in **normalized 0–1 grid space**: the root node is `Rect(0, 0, 1, 1)`.
- `WorldPosToGridPos(worldPos)` — projects a world position onto the width/height
  vectors via `Vector3.Dot(...) / sqrMagnitude`, yielding 0–1 coordinates
  (values outside 0–1 mean the point is off the grid).
- `GridPosToWorldPos(gridPos)` — the inverse:
  `origin + gridPos.x * widthVector + gridPos.y * heightVector`.
- `AddEnemy` / `RemoveEnemy` delegate to the root `QuadTreeNode`.
- `TryGetRandomPosition(out Vector3 worldPosition)` — **try-pattern**: returns a
  random world position located in a still-free (empty) cell; returns `false` plus
  an Info log when every cell is occupied. This is the density-aware spawn query.
- `GetLargestEmptyCells()` returns the bounds of the largest empty leaf cells.

#### QuadTreeNode (nested logic)

- Each **leaf holds at most one enemy** (`m_storedEnemy` / `m_hasEnemy`), so one
  min-area cell == room for one enemy.
- Caches `m_subtreeEnemyCount` (enemies in the whole subtree), with `Capacity`
  (`4^(maxDepth-depth)` = min-cells in the subtree), `FreeSlots` and `IsFull`
  helpers, all maintained in `Insert` / `Remove`.
- Inserting where a leaf is already occupied triggers `Subdivide()` into 4
  children (bottom-left, bottom-right, top-left, top-right) up to `maxDepth`,
  then **reinjects the previous enemy** into the new children.
- `Remove` walks down to the leaf; after removal, `TryMerge()` collapses children
  back when they are all empty (`IsCompletelyEmpty`).
- `TryGetRandomFreeGridPos(out Vector2 gridPos)` — O(depth) weighted descent using
  the cached counts: skips full subtrees, returns instantly inside fully-empty
  regions, weights interior children by `FreeSlots`, and for a single-enemy leaf
  picks a random min-cell other than the occupied one (`RandomFreeCellPoint`).
- `CollectLargestEmptyCells(result)` recursively gathers empty-leaf bounds.
- `DrawDebug` / `DrawRect` draw the leaves back in world space via
  `GridPosToWorldPos`; **occupied cells are red, free cells cyan**.

### Density delegation — [PopulationDensityController.cs](../../../Assets/Scripts/TacticalGame/PopulationDensityController.cs)

- **Plain C# class** (component pattern), constructed with
  `(float minAreaPerElement, PlaneBounds groundPlane)`. It **owns and creates** the
  `QuadTreeGrid` (`new QuadTreeGrid(groundPlane.Corners, minAreaPerElement)`).
- Exposes `AddEnemy` / `RemoveEnemy` / `DrawDebug` (passthroughs) and
  `TryGetNewEnemyPosition(out Vector3 position)` → `QuadTreeGrid.TryGetRandomPosition`.

### Ground strategy — [BG3TacticalGroundController.cs](../../../Assets/Scripts/TacticalGame/BG3TacticalGroundController.cs)

- `MonoBehaviour` implementing
  [ITacticalGroundStrategy](../../../Assets/Scripts/TacticalGame/ITacticalGroundStrategy.cs).
- Serialized `m_minAreaPerEnemy` (default `3f`) — minimum ground area (m²) per enemy.
- `Awake`: `m_planeBounds = GetComponent<PlaneBounds>()`, then
  `m_populationDensity = new PopulationDensityController(m_minAreaPerEnemy, m_planeBounds)`.
- `AddEnemy` / `RemoveEnemy` / `TryGetNewEnemyPosition` / `OnDrawGizmos` all
  **delegate to `m_populationDensity`** (no longer holds the grid directly).
- `IndicateCharacterGroundLocation(ray, indicationType)` raycasts against
  `m_tacticalGridLayer` and drives the `GroundIndicator`s (movement preview vs.
  timed movement-confirmation), returning the hit point.
- `GetRandomGroundLocation()` → `PlaneBounds.GetRandomPlanePointInsideBounds()`
  (still on the interface, but the spawn loop now uses `TryGetNewEnemyPosition`).

### Supporting types

- [EnemyModel.cs](../../../Assets/Scripts/TacticalGame/EnemyModel.cs) — what the
  grid stores: wraps a world `Position` and an `EnemyPresenter`.
- [GroundIndicator.cs](../../../Assets/Scripts/TacticalGame/GroundIndicator.cs) —
  show/hide and position the on-ground feedback markers.

### Wiring — [TacticalGameManager.cs](../../../Assets/Scripts/TacticalGame/TacticalGameManager.cs)

- Assigns `m_ground = m_groundController`.
- On `Start`, the test loop tries to spawn 20 enemies **density-aware**: for each, it
  calls `m_ground.TryGetNewEnemyPosition(out pos)`; on success it spawns + registers
  (`m_ground.AddEnemy(m_enemySpawner.CreateEnemy(pos))`) and counts it, otherwise it
  counts a skip. It logs a final `spawned / skipped (grid full)` summary.

## Known TODOs / notes

- Density is enforced at **spawn-position selection** (`TryGetNewEnemyPosition` only
  returns free cells), not in `AddEnemy` — `AddEnemy` records wherever the enemy is.
- `GetRandomGroundLocation()` is still on `ITacticalGroundStrategy` but is no longer
  used by the spawn loop (kept as a generic utility).
