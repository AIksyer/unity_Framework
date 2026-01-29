# AI Framework Guide - Unity Framework Index

**Purpose**: Enable AI agents to quickly understand this Unity framework, generate compatible code, and assist users effectively while minimizing token usage and avoiding framework conflicts.

**Framework Version**: 2.0.0  
**Last Updated**: 2024  
**Total Modules**: 14 core modules, 25 classes

---

## 1. Framework Design Philosophy

### 1.1 Core Principles

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SINGLETON-FIRST ARCHITECTURE                     │
│                                                                     │
│  Every manager is a singleton accessible via [Module].Instance     │
│  No dependency injection, no service locator pattern needed         │
│  All managers are pre-initialized and ready to use                  │
└─────────────────────────────────────────────────────────────────────┘
```

**Key Design Rules**:
- **Singleton Pattern**: All managers use `BaseManager<T>` or `SingletonAutoMono<T>`
- **Event-Driven Communication**: Modules communicate via `EventCenter`, NOT direct references
- **Object Pooling**: All frequently created/destroyed objects MUST use `PoolMgr`
- **No Direct Coupling**: Never let one manager hold reference to another manager directly
- **Explicit Over Implicit**: Use events for all inter-module communication

### 1.2 Module Dependency Graph

```
                          ┌─────────────────┐
                          │   EventCenter   │  ←─ CENTER OF ALL COMMUNICATION
                          └────────┬────────┘
                                   │
        ┌──────────┬───────────────┼───────────────┬──────────┐
        │          │               │               │          │
        ▼          ▼               ▼               ▼          ▼
   ┌─────────┐ ┌─────────┐ ┌─────────────┐ ┌─────────┐ ┌─────────┐
   │ ResMgr  │ │PoolMgr  │ │ TimerMgr    │ │ UIMgr   │ │MusicMgr │
   └─────────┘ └─────────┘ └─────────────┘ └─────────┘ └─────────┘
        │          │               │               │          │
        └──────────┴───────────────┴───────────────┴──────────┘
                                   │
                          ┌────────┴────────┐
                          │   MonoMgr       │  ←─ Provides Update/FixedUpdate/LateUpdate
                          └─────────────────┘
```

**Critical Rule**: `EventCenter` is the ONLY communication channel between modules.

---

## 2. Module Quick Reference

### 2.1 Module Summary Table

| Module | Class | Purpose | Key Methods | Used When |
|--------|-------|---------|-------------|-----------|
| **Singleton** | `BaseManager<T>` | Non-Mono singleton | `Instance` | Data managers, pure C# logic |
| **Singleton** | `SingletonAutoMono<T>` | Auto-Mono singleton | `Instance` | MonoBehaviour managers |
| **Mono** | `MonoMgr` | Frame events | `AddUpdateListener()`, `AddFixedUpdateListener()` | Any per-frame logic |
| **Timer** | `TimerMgr` | Delayed/repeated tasks | `CreateTimer()`, `RemoveTimer()` | Cooldowns, countdowns, BUFFs |
| **Pool** | `PoolMgr` | Object pooling | `GetObj<T>()`, `PushObj()` | Bullets, enemies, effects |
| **Event** | `EventCenter` | Event system | `EventTrigger<T>()`, `AddEventListener<T>()` | All inter-module comms |
| **UI** | `UIMgr` | Panel management | `ShowPanel<T>()`, `HidePanel<T>()` | UI screens |
| **UI** | `BasePanel` | Panel base class | `GetControl<T>()`, `ShowMe()`, `HideMe()` | Custom UI panels |
| **Res** | `ResMgr` | Resources loading | `Load<T>()`, `LoadAsync<T>()` | Small, frequent resources |
| **AB** | `ABMgr` | AssetBundle loading | `LoadResAsync<T>()` | Large resources, hot update |
| **AB** | `ABResMgr` | AB/Editor switch | `LoadResAsync<T>()` | Dev vs Production switch |
| **UWQ** | `UWQResMgr` | Network requests | `LoadRes<T>()` | HTTP requests |
| **Music** | `MusicMgr` | Audio playback | `PlaySound()`, `PlayBKMusic()` | All audio |
| **Input** | `InputMgr` | Input handling | `StartOrCloseInputMgr()` | Keyboard/mouse input |
| **Scene** | `SceneMgr` | Scene loading | `LoadSceneAsyn()` | Scene transitions |
| **Util** | MathUtil/TextUtil/EncryptionUtil | Utility functions | Various static methods | Math, string, encryption |

### 2.2 Critical Method Signatures

```csharp
// === SINGLETON ACCESS ===
[Manager].Instance.[Method](params)

// === EVENT SYSTEM ===
EventCenter.Instance.EventTrigger<T>(E_EventType, data);
EventCenter.Instance.AddEventListener<T>(E_EventType, callback);
EventCenter.Instance.RemoveEventListener<T>(E_EventType, callback);
EventCenter.Instance.Clear();  // SCENE TRANSITION: ALWAYS CALL THIS!

// === OBJECT POOL ===
PoolMgr.Instance.GetObj<T>(name);           // Get from pool
PoolMgr.Instance.PushObj(obj);              // Return to pool
PoolMgr.Instance.ClearPool();               // CLEANUP: Call on scene change

// === TIMER (milliseconds) ===
int timerId = TimerMgr.Instance.CreateTimer(
    isRealTime: bool,       // true=unscaled, false=scaled
    allTime: int,           // total milliseconds
    overCallBack: UnityAction,
    intervalTime: int = 0,  // optional interval ms
    callBack: UnityAction = null
);
TimerMgr.Instance.RemoveTimer(timerId);
TimerMgr.Instance.StartTimer(timerId);
TimerMgr.Instance.StopTimer(timerId);

// === UI PANEL ===
UIMgr.Instance.ShowPanel<T>(E_UILayer, callback);
UIMgr.Instance.HidePanel<T>(destroy = false);
UIMgr.Instance.GetPanel<T>(callback);

// === RESOURCE LOADING ===
ResMgr.Instance.Load<T>(path);                              // Sync
ResMgr.Instance.LoadAsync<T>(path, callback);              // Async
ResMgr.Instance.UnloadAsset<T>(path);                       // Unload
ResMgr.Instance.UnloadUnusedAssets(callback);               // Cleanup

// === AUDIO ===
MusicMgr.Instance.PlaySound(name, isLoop = false);
MusicMgr.Instance.PlayBKMusic(name);
MusicMgr.Instance.ChangeBKMusicValue(volume);
MusicMgr.Instance.ChangeSoundValue(volume);
MusicMgr.Instance.ClearSound();  // CLEANUP

// === SCENE ===
SceneMgr.Instance.LoadSceneAsyn(name, callback);
```

---

## 3. Naming Conventions (CRITICAL FOR AI)

### 3.1 Naming Rules

```csharp
// === CLASSES ===
PascalCase:           MonoMgr, TimerMgr, PoolData, E_UILayer

// === METHODS ===
PascalCase:           AddUpdateListener, GetObj, EventTrigger

// === PRIVATE FIELDS ===
camelCase:            timerDic, updateEvent, bkMusicValue

// === CONSTANTS ===
UPPER_SNAKE_CASE:     TIMER_KEY, INTERVAL_TIME

// === ENUMS ===
E_Prefix:             E_UILayer, E_EventType, E_InputType

// === INTERFACES ===
I_Prefix:             IPoolObject

// === PARAMETERS ===
camelCase:            timerId, layerMask, callBack
```

### 3.2 File Organization

```
Framework/
├── AB/ABMgr.cs           ← 1 class per file, filename = classname
├── AB/ABResMgr.cs
├── EventCenter/E_EventType.cs    ← Enum file
├── EventCenter/EventCenter.cs
├── Mono/MonoMgr.cs
├── Pool/PoolMgr.cs
├── Pool/PoolData.cs              ← Related class, same dir
├── Singleton/BaseManager.cs
├── Singleton/SingletonAutoMono.cs
├── Timer/TimerMgr.cs
├── Timer/TimerItem.cs
├── UI/UIMgr.cs
├── UI/BasePanel.cs
└── ...etc
```

### 3.3 Comment Style

```csharp
/// <summary>
/// PUBLIC API: XML documentation required
/// </summary>
/// <param name="paramName">Description</param>
/// <returns>Description</returns>
public void PublicMethod(int paramName) { }

// Internal comments: Chinese (as per codebase convention)
// 这是一个内部注释
```

---

## 4. Common Patterns

### 4.1 Creating a New Manager

```csharp
// WRONG - Don't do this
public class MyManager : MonoBehaviour {
    void Update() { /* ... */ }
}

// RIGHT - Use singleton pattern
public class MyManager : BaseManager<MyManager> {
    private MyManager() { }  // Private constructor
    
    public void MyMethod() { }
}

// OR for MonoBehaviour-based managers
public class MyMonoManager : SingletonAutoMono<MonoMgr> {
    // Auto-creates GameObject when first accessed
}
```

### 4.2 Module Communication (USE EVENTS!)

```csharp
// WRONG - Direct coupling
public class Player : MonoBehaviour {
    public UIManager ui;  // FORBIDDEN!
    void OnDeath() {
        ui.ShowGameOver();  // FORBIDDEN!
    }
}

// RIGHT - Event-driven
public class Player : MonoBehaviour {
    void OnDeath() {
        EventCenter.Instance.EventTrigger(E_GameEvent.E_Player_Dead);
    }
}

// In UIManager:
void OnEnable() {
    EventCenter.Instance.AddEventListener(E_GameEvent.E_Player_Dead, ShowGameOver);
}
```

### 4.3 Object Pooling (ALWAYS POOL!)

```csharp
// WRONG - Creating/destroying frequently
void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
        Instantiate(bulletPrefab);  // BAD!
    }
}

// RIGHT - Use object pool
void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
        var bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");
        bullet.Launch();
    }
}

// And implement IPoolObject on pooled objects
public class Bullet : MonoBehaviour, IPoolObject {
    public void ResetInfo() {
        gameObject.SetActive(false);
    }
}
```

### 4.4 UI Panel Creation

```csharp
// WRONG - Manual Find
public class MyPanel : MonoBehaviour {
    Button startBtn;
    void Awake() {
        startBtn = transform.Find("StartButton").GetComponent<Button>();  // BAD!
    }
}

// RIGHT - Use BasePanel's auto-find
public class MyPanel : BasePanel {
    private Button startButton;  // Auto-magically found by name
    
    protected override void Awake() {
        base.Awake();  // REQUIRED!
        startButton = GetControl<Button>("StartButton");  // Name must match!
    }
    
    public override void ShowMe() { gameObject.SetActive(true); }
    public override void HideMe() { gameObject.SetActive(false); }
}
```

---

## 5. AI Constraints (What NOT to Do)

### 5.1 NEVER Modify Framework Code

```markdown
RULE: Do NOT suggest modifications to any file in Framework/ directory

If user asks to modify framework:
1. Explain the framework already provides the functionality
2. Show how to achieve the goal using existing framework features
3. If truly necessary, suggest extending rather than modifying

EXAMPLES:

❌ DON'T: "Let me modify PoolMgr to add new feature"
✅ DO: "Use the existing PoolMgr.GetObj<T>() and show how to extend it"

❌ DON'T: "I need to change EventCenter to support new event type"
✅ DO: "Add new E_EventType in E_EventType.cs and use existing EventCenter"
```

### 5.2 NEVER Create Direct References

```csharp
// WRONG - AI should never generate this
public class Player : MonoBehaviour {
    public UIManager uiManager;    // FORBIDDEN
    public EnemyManager enemyMgr;  // FORBIDDEN
}

// RIGHT - Always use events/singleton access
public class Player : MonoBehaviour {
    void OnDeath() {
        EventCenter.Instance.EventTrigger(E_GameEvent.E_Player_Dead);
    }
}
```

### 5.3 ALWAYS Use Object Pooling

```csharp
// WRONG - Don't create instances directly
GameObject obj = new GameObject("temp");
Instantiate(enemyPrefab);
Destroy(bullet);

// RIGHT - Always use PoolMgr
var bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");
PoolMgr.Instance.PushObj(bullet);
```

### 5.4 ALWAYS Clean Up on Scene Change

```csharp
// When user needs scene transition, ALWAYS include cleanup:
void ChangeScene(string sceneName) {
    EventCenter.Instance.Clear();        // Clean events
    TimerMgr.Instance.Stop();            // Clean timers
    MusicMgr.Instance.ClearSound();      // Clean audio
    PoolMgr.Instance.ClearPool();        // Clean pools
    ResMgr.Instance.UnloadUnusedAssets(null);  // Clean resources
    SceneMgr.Instance.LoadSceneAsyn(sceneName);
}
```

---

## 6. AI Best Practices

### 6.1 When Helping Users

1. **Understand First**: Read user's requirement completely
2. **Check Framework First**: Use existing framework features before creating new code
3. **Use Existing Patterns**: Follow the patterns shown in this document
4. **Generate Complete Code**: Provide copy-pasteable, working code
5. **Include Cleanup**: Always include proper resource cleanup in generated code

### 6.2 Code Generation Template

```csharp
// 1. Import statements (if needed beyond framework)
using UnityEngine;
using UnityEngine.Events;

// 2. Class definition following framework conventions
public class [FeatureName] : BaseManager<[FeatureName]>
{
    // 3. Private constructor for singleton
    private [FeatureName]() { }
    
    // 4. Public methods
    public void [PublicMethod]() { }
    
    // 5. Event handlers
    private void [Handler]() { }
}

// 6. If MonoBehaviour needed
public class [MonoClass] : BasePanel  // or MonoBehaviour
{
    protected override void Awake() {
        base.Awake();  // ALWAYS call base.Awake()
    }
    
    public override void ShowMe() { }
    public override void HideMe() { }
}
```

### 6.3 Quick Reference Prompts

```markdown
USER WANTS TO:              AI SHOULD USE:
─────────────────────────────────────────
Access global data          → [Manager].Instance.GetData()
Send data between modules   → EventCenter.Instance.EventTrigger()
Load resources              → ResMgr.Instance.Load<T>() or LoadAsync()
Create repeated objects     → PoolMgr.Instance.GetObj<T>()
Timing/Cooldowns            → TimerMgr.Instance.CreateTimer()
Show UI                     → UIMgr.Instance.ShowPanel<T>()
Play sound                  → MusicMgr.Instance.PlaySound()
Handle input                → InputMgr + EventCenter
Save/Load game              → SaveManager with EncryptionUtil
```

---

## 7. Module Integration Examples

### 7.1 Enemy AI (Full Integration)

```csharp
// Enemy Manager (Singleton)
public class EnemyManager : BaseManager<EnemyManager> {
    private EnemyManager() { }
    public Enemy Spawn(Vector3 pos, int level) {
        var enemy = PoolMgr.Instance.GetObj<Enemy>("Enemy");
        enemy.Init(pos, level);
        return enemy;
    }
}

// Enemy State Machine
public class Enemy : MonoBehaviour, IPoolObject {
    public void UpdateAI() {
        // Uses EventCenter to communicate
        // Uses TimerMgr for attack intervals
        // Uses PoolMgr for spawning
    }
    public void ResetInfo() { /* pool reset */ }
}

// Communication
EventCenter.Instance.AddEventListener<int>(E_GameEvent.E_Enemy_Killed, OnEnemyKilled);
```

### 7.2 Skill System (Full Integration)

```csharp
public class SkillManager : BaseManager<SkillManager> {
    public bool UseSkill(string skillId) {
        // 1. Check cooldown with TimerMgr
        // 2. Play sound with MusicMgr
        // 3. Spawn effect with PoolMgr
        // 4. Update UI with EventCenter
        // 5. Return success
    }
}
```

### 7.3 Quest System (Full Integration)

```csharp
public class QuestManager : BaseManager<QuestManager> {
    public void AcceptQuest(string questId) {
        // 1. Register event listeners
        // 2. Start timer if time-limited
        // 3. Update UI via EventCenter
    }
    
    public void UpdateProgress(QuestType type, int targetId) {
        // Update progress, check completion
        // Trigger reward events
    }
}
```

---

## 8. Common Errors to Avoid

### 8.1 AI Should Correct These

| Error | Fix |
|-------|-----|
| `PoolMgr.Instance` missing | Check PoolObj on prefab, verify name matches |
| `GetControl<T>()` returns null | Ensure name matches exactly, call `base.Awake()` |
| `EventTrigger` not received | Check event type matches, listener registered in OnEnable |
| Timer not working | Check `isRealTime` parameter, timer was started |
| UI not showing | Check panel is active, `ShowMe()` called |
| Singleton null | Called before initialization, use `if (Instance != null)` |

### 8.2 Scene Transition Checklist

```csharp
// ALWAYS clean up on scene change:
EventCenter.Instance.Clear();           // Remove all event listeners
TimerMgr.Instance.Stop();               // Stop all timers
MusicMgr.Instance.ClearSound();         // Clean audio
PoolMgr.Instance.ClearPool();           // Clear object pools
// Resources unloaded automatically by Unity on scene load
```

---

## 9. File Locations Quick Index

```
Framework/
├── Singleton/
│   ├── BaseManager.cs          ← Singleton base for non-Mono
│   ├── SingletonAutoMono.cs    ← Auto-Mono singleton (RECOMMENDED)
│   └── SingletonMono.cs        ← Manual Mono singleton
├── Mono/
│   └── MonoMgr.cs              ← Update/FixedUpdate/LateUpdate listeners
├── Timer/
│   ├── TimerMgr.cs             ← Timer management
│   └── TimerItem.cs            ← Timer data (IPoolObject)
├── Pool/
│   ├── PoolMgr.cs              ← Object pooling (CRITICAL!)
│   ├── PoolData.cs             ← Pool data container
│   ├── PoolObj.cs              ← Must attach to pooled prefabs
│   └── IPoolObject.cs          ← Interface for pooled objects
├── EventCenter/
│   ├── EventCenter.cs          ← Event system (COMMUNICATION HUB!)
│   └── E_EventType.cs          ← Event type enum (ADD NEW HERE)
├── UI/
│   ├── UIMgr.cs                ← Panel management
│   └── BasePanel.cs            ← Base class for all panels
├── Res/
│   └── ResMgr.cs               ← Resources loading
├── AB/
│   ├── ABMgr.cs                ← AssetBundle loading
│   └── ABResMgr.cs             ← Dev/Prod switch
├── UWQ/
│   └── UWQResMgr.cs            ← Network requests
├── Music/
│   └── MusicMgr.cs             ← Audio playback
├── Input/
│   ├── InputMgr.cs             ← Input handling
│   └── InputInfo.cs            ← Input data
├── Scene/
│   └── SceneMgr.cs             ← Scene loading
└── Util/
    ├── MathUtil.cs             ← Math helpers
    ├── TextUtil.cs             ← String helpers
    └── EncryptionUtil.cs       ← Simple encryption
```

---

## 10. Quick Start for AI

```markdown
1. USER ASKS FOR NEW FEATURE
   ↓
2. CHECK: Does framework already have this?
   ↓
3. DESIGN: Use existing modules
   - Singleton for managers
   - EventCenter for communication
   - PoolMgr for objects
   - TimerMgr for timing
   ↓
4. GENERATE: Complete, working code
   - Follow naming conventions
   - Include proper cleanup
   - Use XML docs for public APIs
   ↓
5. VERIFY: No framework modifications
   - No direct manager references
   - Object pooling used
   - Events for communication
   ↓
6. EXPLAIN: How to use the code
```

---

## 11. Token-Saving Cheat Sheet

```csharp
// ← 1 char
// TODO:  ← 6 chars
// IMPORTANT: ← 11 chars

// Single line comment for short notes
/// <summary>Long documentation</summary>

// Quick reference:
BaseManager<T>          // Singleton non-Mono
SingletonAutoMono<T>    // Singleton Mono (AUTO)
PoolMgr                 // Object pooling (CRITICAL!)
EventCenter             // Events (ONLY way to communicate!)
TimerMgr                // Timing/cooldowns
UIMgr.ShowPanel<T>()    // Show UI
ResMgr.Load<T>()        // Load resource
MusicMgr.PlaySound()    // Play audio
SceneMgr.LoadSceneAsyn() // Load scene
```

---

## 12. Framework Constraints Summary

**DO**:
- ✅ Use `EventCenter` for all inter-module communication
- ✅ Use `PoolMgr` for all frequently created/destroyed objects
- ✅ Use `SingletonAutoMono<T>` or `BaseManager<T>` for managers
- ✅ Call `base.Awake()` in `BasePanel` subclasses
- ✅ Clean up with `EventCenter.Clear()` on scene change
- ✅ Use XML docs for public APIs
- ✅ Follow naming conventions (PascalCase, camelCase, E_Prefix, etc.)

**DON'T**:
- ❌ Never modify Framework/ directory files
- ❌ Never create direct references between managers
- ❌ Never use `Instantiate`/`Destroy` for frequently created objects
- ❌ Never skip `base.Awake()` in `BasePanel`
- ❌ Never forget cleanup on scene transitions
- ❌ Never use `new MonoBehaviour()` - use `SingletonAutoMono<T>`

---

## 13. Emergency Reference

**Can't find the right module?**
- EventCenter: Communication between anything
- PoolMgr: Object reuse
- TimerMgr: Time-based operations
- UIMgr: Any UI
- ResMgr/ABMgr: Resources
- MusicMgr: Audio
- SceneMgr: Scenes
- InputMgr: Input handling

**Code not working?**
1. Check singleton access: `[Manager].Instance`
2. Check event registration: `AddEventListener` in `OnEnable`
3. Check object pooling: Prefab has `PoolObj` script
4. Check UI naming: `GetControl<T>("Name")` matches hierarchy
5. Check cleanup: Scene transition calls all cleanup methods

---

**END OF AI GUIDE**
