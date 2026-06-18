---
name: read-scripts
description: Read all C# scripts in this Unity project. Use when the user asks to "read scripts", "load scripts", "read my code", or wants Claude to have full knowledge of the project's C# files before performing a task.
allowed-tools: [Read, Glob]
---

# Read Scripts

Read all C# scripts in `Assets/Scripts/`, excluding `Assets/Scripts/ScriptableObjectEvents/`.

## Instructions

1. Use Glob to find all `.cs` files under `Assets/Scripts/` recursively.
2. Filter out any file whose path contains `ScriptableObjectEvents`.
3. Read every remaining file using the Read tool.
4. Once all files are read, confirm to the user which files were loaded.

Do NOT summarise the files unless the user asks. Just confirm they are read and you are ready.
