# C# & Unity Coding Conventions

## Naming Quick Reference

| Member type | Convention | Example |
|---|---|---|
| Class / Struct / Enum / Interface | PascalCase | `PlayerController`, `IDamageable` |
| Public method / property | PascalCase | `GetHealth()`, `Health { get; }` |
| Private method | PascalCase | `UpdatePosition()` |
| Private field | m_camelCase | `m_health`, `m_walkSpeed` |
| Local variable / parameter | camelCase | `enemyCount`, `speed` |
| Constant | UPPER_SNAKE_CASE | `MAX_SPEED`, `LAYER_MASK` |

> **Critical rule**: `m_` prefix is **exclusively** for private fields. Public members never use it.
> **Critical rule**: The character immediately after `m_` must be **lowercase** (`m_walkSpeed`, not `m_WalkSpeed`).
> **Critical rule**: Never use `var` — always write the explicit type.

## Naming Conventions

### Classes and Types
- **PascalCase** for class, struct, enum, and interface names
  - `public class PlayerController`
  - `public struct Vector3D`
  - `public enum GameState`
  - `public interface IDamageable`

### Fields and Variables
- local variables and method parameters : **camelCase**
  - `int enemyCount = 10;`
  - `float speed = 5f;`

- private fields: **m_camelCase** (prefix `m_` followed by a **lowercase** first letter)
  - `private int m_health;`
  - `private Transform m_cachedTransform;`
  - **WRONG**: `private int m_Health;` — the letter after `m_` must be lowercase
  - **WRONG**: `public int m_health;` — `m_` prefix is ONLY for private fields, never public members

- **UPPER_SNAKE_CASE** for constants
  - `private const float MAX_SPEED = 10f;`
  - `public static readonly int LAYER_MASK = 1 << 8;`

### Methods and Properties
- **PascalCase** for methods and properties
  - `public void Move(float speed)`
  - `public int Health { get; private set; }`

## Code Organization

### Class Structure
Order members in this sequence:
1. Events
2. Properties (public)
3. Fields (private first, then protected, then public)
4. Constructors / Awake / OnEnable
5. Lifecycle methods (Update, LateUpdate, OnDisable, etc.)
6. Public methods
7. Private methods

### Method Order (reading order)
Within the method groups above, order methods by **call sequence** so the code reads top-to-bottom: place a method right after the one that first calls it.
- After the constructor, put the first method the constructor calls, then the method called after that, and so on.
- When adding or moving a method, position it where it is first invoked — do not append it at the end of the class.
- Example: if the `QuadTreeGrid` constructor calls `SetMinCellSize` first, `SetMinCellSize` sits immediately after the constructor.

### Class
- If a class had too many members, suggest me a better architecture, it's not good to have a huge Class doing too many things

### File Structure
- One class per file (unless it's a small helper/nested class)
- File name matches class name: `PlayerController.cs` for class `PlayerController`

## C# Style

### Type Usage
- Do not use `var` for type, always explicit types

### Properties vs Fields
- Prefer **properties** over public fields
  ```csharp
  // Good
  public int Health { get; private set; }
  
  // Avoid
  public int Health;
  ```

### Null Handling
- Use null-coalescing operator: `value ?? defaultValue`
- Use null-conditional operator: `obj?.Method()`

### Strings
- Use string interpolation: `$"Player {name} has {health} hp"`
- Avoid string concatenation: `"Player " + name + " has " + health + " hp"`

## Unity-Specific Conventions

### MonoBehaviour Serialization
- Use `[SerializeField]` for editor-exposed private fields
  ```csharp
  [SerializeField] private float moveSpeed = 5f;
  [SerializeField] private Transform targetTransform;
  ```

### Component Caching
- Cache frequently used components in private fields
  ```csharp
  private Rigidbody m_rigidbody;
  private Collider m_collider;
  
  private void Awake()
  {
      m_rigidbody = GetComponent<Rigidbody>();
      m_collider = GetComponent<Collider>();
  }
  ```

### Coroutines
- Use `StartCoroutine()` with explicit method names, not lambdas when possible
- Stop coroutines properly: `StopCoroutine(MethodName());`

### Tags and Layers
- Use layer masks and tag comparisons with constants, not magic strings
  ```csharp
  // Good
  if (gameObject.CompareTag("Player"))
  
  // Avoid
  if (gameObject.tag == "Player")
  ```

## Method Size & Complexity
- Keep methods under 50 lines when possible
- Extract complex logic into separate private methods
- **Single responsibility per method** — a method must do one thing. Do not bundle unrelated concerns into it (e.g. a method that computes a value must not also build/initialize unrelated state; move that to the caller or a dedicated method).

## Comments
- Avoid obvious comments; use them only for complex logic or workarounds
- Explain WHY, not WHAT
  ```csharp
  // Good: explains the reason
  // Cache transform to avoid GetComponent overhead each frame
  private Transform _cachedTransform;
  
  // Avoid: obvious what the code does
  // Get the cached transform
  return _cachedTransform;
  ```

## Code Quality
- Prefer `readonly` keyword for fields that don't change after initialization
- Use `using` statements for proper resource cleanup
- Avoid deep nesting; extract methods if nesting exceeds 3 levels
- Use early returns to reduce nesting in conditional logic

## Spacing & Formatting
- Use 4 spaces for indentation (standard C# convention)
- Use blank lines to separate logical sections within methods
- Keep line length reasonable (under 120 characters when practical)

## Git Commits
- Never add a "Co-Authored-By" line or any AI attribution to commit messages
