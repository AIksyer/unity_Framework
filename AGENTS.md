# AGENTS.md - Unity C# Framework Development Guide

## Build Commands

This is a Unity C# framework project. All build operations are performed through the Unity Editor.

- **Open in Unity**: Open the Unity Hub and add this project to open it in the Unity Editor
- **Build Player**: Use `File > Build Settings` in Unity Editor
- **Compile Scripts**: Unity automatically compiles C# scripts on save; force recompile via `Assets > Refresh` or Ctrl+R
- **Code Analysis**: Use `Assets > Open C# Project` to open in Visual Studio/ Rider for IDE features

## Testing

- **Test Framework**: No test framework detected in this repository
- **Unity Tests**: If tests exist, run them via `Window > General > Test Runner` in Unity Editor
- **Single Test**: Select a specific test in the Test Runner window and click "Run Selected"

## Code Style Guidelines

### Imports Ordering

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
// Unity imports
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
```

Order: System → Unity → third-party. Group UnityEngine.* before UnityEngine.Events.*.

### Naming Conventions

- **Classes/Structs**: `PascalCase` (e.g., `MonoMgr`, `TimerMgr`, `PoolData`)
- **Public Methods**: `PascalCase` (e.g., `AddUpdateListener`, `GetObj`)
- **Private/Protected Fields**: `camelCase` (e.g., `timerDic`, `updateEvent`)
- **Constants/Statics**: `UPPER_SNAKE_CASE` (e.g., `TIMER_KEY`, `intervalTime`)
- **Enums**: `E_Prefix` (e.g., `E_UILayer`, `E_EventType`)
- **Interfaces**: `I_Prefix` (e.g., `IPoolObject`)
- **Generic Type Parameters**: `T` or `TResult`, `TKey`, `TValue`

### Architecture Patterns

**Singleton Pattern**: Use `BaseManager<T>` for non-MonoBehaviour managers and `SingletonAutoMono<T>` for MonoBehaviour managers.

```csharp
// For non-MonoBehaviour classes
public class ResMgr : BaseManager<ResMgr> { }

// For MonoBehaviour classes
public class MonoMgr : SingletonAutoMono<MonoMgr> { }
```

**Object Pooling**: Use `PoolMgr` for GameObject pooling and `PoolMgr.GetObj<T>()` for data structures implementing `IPoolObject`.

**Event System**: Use `EventCenter` for global events with `AddEventListener` / `RemoveEventListener`.

**Timer System**: Use `TimerMgr` for timed operations with `CreateTimer` / `RemoveTimer`.

### Types and Variables

- Use explicit types for public fields; `var` is acceptable for local variables with clear inference
- Prefer `Dictionary<TKey, TValue>` over `Hashtable`
- Use `List<T>` for ordered collections, `Stack<T>` for LIFO, `Queue<T>` for FIFO
- Initialize collections inline: `private Dictionary<int, TimerItem> timerDic = new Dictionary<int, TimerItem>();`

### Error Handling

- Use `Debug.LogError()` for runtime errors requiring attention
- Use null checks with early returns for validation
- Do not use exceptions for control flow
- Check `ContainsKey` before dictionary access

```csharp
if(timerDic.ContainsKey(keyID))
{
    // handle
}
```

### Formatting

- Use 4 spaces for indentation (not tabs)
- Opening brace on same line: `if (condition) {`
- One space after commas and around operators
- Properties: use expression-bodied syntax when simple, block syntax when complex
- One blank line between method definitions
- Max line length: 200 characters (Unity default)

### Comments

- Use XML documentation (`/// <summary>`) for public APIs
- Include `<param>` tags for method parameters
- Use Chinese comments for internal implementation details (as per existing codebase)
- Comment complex logic, not obvious code
- TODO comments: `// TODO: description`

### File Organization

- One public class per file, matching filename
- File name matches class name exactly
- Related classes in same directory (e.g., `TimerMgr.cs` and `TimerItem.cs` together)
- Editor scripts in `Editor/` folders (not present in this repo)

### Unity-Specific Conventions

- Use `UnityAction` for callback delegates
- Use `Coroutine` for async operations with `StartCoroutine`
- Use `[SerializeField]` for private serialized fields if needed
- Implement `IPoolObject.ResetInfo()` for pooled data objects
- Use `DontDestroyOnLoad()` for persistent managers

### Project Structure

```
Framework/
├── AB/              # Asset Bundle management
├── EditorRes/       # Editor resources
├── EventCenter/     # Global event system
├── Input/           # Input handling
├── Mono/            # MonoBehaviour utilities
├── Music/           # Audio management
├── Pool/            # Object pooling
├── Res/             # Resource loading
├── Scene/           # Scene management
├── Singleton/       # Singleton patterns
├── Timer/           # Timer system
├── UI/              # UI management
├── UWQ/             # WebQuest integration
└── Util/            # Utility classes
```

## No Cursor/Copilot Rules Found

This repository has no existing Cursor rules (`.cursor/rules/` or `.cursorrules`) or Copilot instructions (`.github/copilot-instructions.md`).
