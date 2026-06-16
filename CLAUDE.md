# C# & Unity Coding Conventions

## Naming Conventions

### Classes and Types
- **PascalCase** for class, struct, enum, and interface names
  - `public class PlayerController`
  - `public struct Vector3D`
  - `public enum GameState`
  - `public interface IDamageable`

### Fields and Variables
- **camelCase** for local variables and method parameters
  - `int enemyCount = 10;`
  - `float speed = 5f;`

- **m_camelCase** (m + underscore prefix) for private fields
  - `private int m_health;`
  - `private Transform m_cachedTransform;`

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
- One responsibility per method

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
