---
name: check-code-todo
description: Scan the codebase for TODO comments and list them. Use when the user asks to "check todos", "list todos", "find todos", or "show todo comments".
allowed-tools: [Grep]
---

# Check Code TODO

Scan all C# files in `Assets/Scripts/` for TODO comments and present a clear list.

## Instructions

1. Use Grep to search for `TODO` (case-insensitive) across all `.cs` files under `Assets/Scripts/`.
2. Present the results grouped by file, with the line number and the full comment text for each hit.
3. If no TODOs are found, say so clearly.

## Output Format

```
Found X TODOs across Y files:

**Assets/Scripts/SomeFile.cs**
  - Line 42: // TODO : do something

**Assets/Scripts/TacticalGame/OtherFile.cs**
  - Line 17: //TODO : fix this
```

Keep it concise — no extra commentary unless the user asks.
