# 快速参考与故障排除

## 快速参考

### 模块速查表

| 需求 | 使用模块 | 主要类 | 关键方法 |
|------|----------|--------|----------|
| 全局访问点 | 单例模块 | BaseManager<T> | Instance |
| 帧更新回调 | Mono模块 | MonoMgr | AddUpdateListener |
| 定时任务 | 定时器模块 | TimerMgr | CreateTimer |
| 对象复用 | 对象池模块 | PoolMgr | GetObj/PushObj |
| 模块通信 | 事件系统 | EventCenter | EventTrigger/AddEventListener |
| 资源加载 | 资源模块 | ResMgr | Load/LoadAsync |
| AB加载 | AB模块 | ABMgr | LoadResAsync |
| UI面板 | UI模块 | UIMgr | ShowPanel/HidePanel |
| 音效播放 | 音频模块 | MusicMgr | PlaySound |
| 输入处理 | 输入模块 | InputMgr | StartOrCloseInputMgr |
| 场景切换 | 场景模块 | SceneMgr | LoadSceneAsyn |

### 常用代码片段

#### 单例访问

```csharp
// 访问任意管理器
[ManagerType].Instance.[Method]([Parameters]);

// 示例
ResMgr.Instance.Load<GameObject>("Player");
PoolMgr.Instance.GetObj("Enemy");
EventCenter.Instance.EventTrigger(E_EventType.E_Game_Start);
```

#### 资源加载

```csharp
// 同步加载
var prefab = ResMgr.Instance.Load<GameObject>("Prefabs/Player");

// 异步加载
ResMgr.Instance.LoadAsync<GameObject>("Prefabs/Enemy", (enemy) =>
{
    Instantiate(enemy);
});

// AB加载
ABMgr.Instance.LoadResAsync<GameObject>("characters", "hero", (hero) =>
{
    Instantiate(hero);
});
```

#### 对象池使用

```csharp
// 获取对象
var bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");

// 使用对象
bullet.transform.position = spawnPos;
bullet.gameObject.SetActive(true);

// 归还对象
PoolMgr.Instance.PushObj(bullet);
```

#### 事件监听

```csharp
// 注册事件
EventCenter.Instance.AddEventListener(E_EventType.E_Score_Add, OnScoreAdd);
EventCenter.Instance.AddEventListener<float>(E_EventType.E_Hp_Change, OnHpChange);

// 触发事件
EventCenter.Instance.EventTrigger(E_EventType.E_Score_Add, 100);
EventCenter.Instance.EventTrigger(E_EventType.E_Hp_Change, 0.5f);

// 移除事件
EventCenter.Instance.RemoveEventListener(E_EventType.E_Score_Add, OnScoreAdd);
```

#### 定时器

```csharp
// 创建一次性定时器
int timerId = TimerMgr.Instance.CreateTimer(
    isRealTime: false,
    allTime: 5000,
    overCallBack: () => { /* 5秒后执行 */ }
);

// 创建循环定时器
int loopTimer = TimerMgr.Instance.CreateTimer(
    isRealTime: true,
    allTime: 60000,
    overCallBack: null,
    intervalTime: 1000,
    callBack: () => { /* 每秒执行 */ }
);

// 定时器控制
TimerMgr.Instance.StopTimer(timerId);      // 暂停
TimerMgr.Instance.StartTimer(timerId);     // 继续
TimerMgr.Instance.ResetTimer(timerId);     // 重置
TimerMgr.Instance.RemoveTimer(timerId);    // 移除
```

#### UI操作

```csharp
// 显示面板
UIMgr.Instance.ShowPanel<MainPanel>(E_UILayer.Middle);

// 隐藏面板
UIMgr.Instance.HidePanel<MainPanel>();

// 获取面板
UIMgr.Instance.GetPanel<MainPanel>((panel) =>
{
    if (panel != null)
    {
        // 使用面板
    }
});
```

### 事件类型速查

```csharp
// 在E_EventType.cs中定义
public enum E_EventType 
{
    // 游戏状态
    E_Game_Start,
    E_Game_Pause,
    E_Game_Resume,
    E_Game_Over,
    
    // 玩家状态
    E_Player_Damaged,
    E_Player_Dead,
    E_Player_LevelUp,
    
    // 战斗
    E_Enemy_Spawn,
    E_Enemy_Dead,
    E_Projectile_Fire,
    E_Projectile_Hit,
    
    // 资源
    E_Coin_Collected,
    E_Item_Collected,
    E_XP_Gained,
    
    // UI
    E_Show_Panel,
    E_Hide_Panel,
    E_Update_HpBar,
    E_Update_Score,
    
    // 系统
    E_Scene_Load,
    E_Save_Game,
    E_Load_Game,
    
    // 输入
    E_Input_Jump,
    E_Input_Attack,
    E_Input_Pause,
}
```

## 故障排除

### 问题与解决方案

#### 问题1：单例为null

**症状：** 访问Manager.Instance时返回null

**原因：**
1. 在单例初始化之前访问
2. 场景中单例对象被销毁
3. 脚本编译错误导致单例未注册

**解决方案：**

```csharp
// 错误示例：在Awake中过早访问
void Awake()
{
    // 此时单例可能还未初始化
    PoolMgr.Instance.ClearPool();  // 可能为null
}

// 正确示例：在Start中访问
void Start()
{
    // 单例已初始化完成
    PoolMgr.Instance.ClearPool();
}

// 正确示例：使用IsInitialized检查
void SomeMethod()
{
    if (PoolMgr.Instance != null)
    {
        PoolMgr.Instance.ClearPool();
    }
}

// 确保脚本编译正确
// 检查控制台错误
// 必要时执行Assets -> Refresh
```

#### 问题2：事件不触发

**症状：** 注册了事件监听器，但回调从不执行

**原因：**
1. 事件类型不匹配
2. 事件在监听注册之前就已经触发
3. 场景切换后事件被清理但未重新注册
4. 忘记调用EventTrigger

**解决方案：**

```csharp
// 检查1：事件类型是否一致
// 注册时
EventCenter.Instance.AddEventListener<int>(E_EventType.E_Score_Add, OnScoreAdd);

// 触发时
EventCenter.Instance.EventTrigger(E_EventType.E_Score_Add, 100);  // 错误：类型不匹配
EventCenter.Instance.EventTrigger<int>(E_EventType.E_Score_Add, 100);  // 正确

// 检查2：监听时机
void OnEnable()
{
    EventCenter.Instance.AddEventListener(E_EventType.E_Game_Start, OnGameStart);
}

void OnDisable()
{
    EventCenter.Instance.RemoveEventListener(E_EventType.E_Game_Start, OnGameStart);
}

// 检查3：场景切换后重新注册
void OnSceneLoaded(Scene scene)
{
    EventCenter.Instance.AddEventListener(E_EventType.E_Score_Add, OnScoreAdd);
}

// 检查4：确保EventTrigger被调用
void AddScore(int amount)
{
    score += amount;
    EventCenter.Instance.EventTrigger(E_EventType.E_Score_Add, amount);  // 确保调用
}
```

#### 问题3：对象池不工作

**症状：** 对象池获取不到对象或对象数量异常

**原因：**
1. 预制体未放在Resources文件夹
2. PoolObj脚本未正确挂载
3. 对象池达到最大数量限制
4. 对象名称不匹配

**解决方案：**

```csharp
// 检查1：预制体位置
// 预制体必须在Resources文件夹下
// Assets/Resources/Prefabs/Bullet.prefab

// 检查2：PoolObj脚本
// 在预制体根物体上挂载PoolObj脚本
[PoolObj]
public class Bullet : MonoBehaviour { }

// 设置maxNum属性控制池大小
// public int maxNum = 10;

// 检查3：对象名称
// GetObj时使用的名称必须与预制体名称一致
PoolMgr.Instance.GetObj("Bullet");  // 预制体名称必须是Bullet
PoolMgr.Instance.GetObj("Prefabs/Bullet");  // 可以带路径

// 检查4：调试对象池状态
void DebugPool()
{
    // 临时在PoolMgr中添加调试方法
    Debug.Log($"Pool Count: {poolDic.Count}");
    foreach (var kvp in poolDic)
    {
        Debug.Log($"Pool {kvp.Key}: Count={kvp.Value.Count}");
    }
}
```

#### 问题4：资源加载失败

**症状：** Load或LoadAsync返回null

**原因：**
1. 资源路径错误
2. 资源类型不匹配
3. Resources文件夹中无此资源
4. AB包未正确打包

**解决方案：**

```csharp
// 检查1：路径正确性
// 错误
var player = ResMgr.Instance.Load<GameObject>("player");  // 大小写敏感

// 正确
var player = ResMgr.Instance.Load<GameObject>("Prefabs/Player");
var player = ResMgr.Instance.Load<GameObject>("Assets/Resources/Prefabs/Player");  // 完整路径错误

// 检查2：Resources文件夹
// 确保资源在Assets/Resources文件夹下
// Assets/Resources/Prefabs/Player.prefab
// 调用: ResMgr.Instance.Load<GameObject>("Prefabs/Player")

// 检查3：类型匹配
// 错误：预制体是GameObject
var sprite = ResMgr.Instance.Load<Sprite>("Icons/Player");  // 类型错误

// 正确
var sprite = ResMgr.Instance.Load<Sprite>("Icons/Player");
var prefab = ResMgr.Instance.Load<GameObject>("Prefabs/Player");

// 检查4：AB包
// 确保AB包已正确打包
// 检查StreamingAssets文件夹下的AB包
// 确认AB包名称和资源名称正确
```

#### 问题5：定时器不执行

**症状：** 定时器创建后回调从不执行

**原因：**
1. 定时器被暂停
2. 定时器被移除
3. isRealTime参数设置错误
4. 场景切换后定时器丢失

**解决方案：**

```csharp
// 检查1：定时器状态
int timerId = TimerMgr.Instance.CreateTimer(...);
TimerMgr.Instance.StartTimer(timerId);  // 确保已启动

// 检查2：定时器移除
TimerMgr.Instance.RemoveTimer(timerId);  // 这会移除定时器

// 检查3：时间模式
// isRealTime: true - 不受Time.timeScale影响
// isRealTime: false - 受Time.timeScale影响
// 如果游戏暂停(timeScale=0)，isRealTime=false的定时器也会暂停

// 检查4：场景切换
// 定时器在场景切换时会丢失
// 场景切换前保存定时器ID
// 场景加载后重新创建定时器

// 调试定时器
void DebugTimer(int timerId)
{
    // 临时添加调试代码
    Debug.Log($"Timer {timerId} state check");
}
```

#### 问题6：UI控件获取为null

**症状：** GetControl返回null

**原因：**
1. 控件名称不匹配
2. 控件不在面板的子物体中
3. BasePanel.Awake未执行
4. 面板未激活

**解决方案：**

```csharp
// 检查1：控件名称
// 面板层级结构
// - MainPanel (挂载MainPanel脚本)
//   - Background
//   - StartBtn (Button)
//   - SettingsBtn (Button)

// 获取
protected override void Awake()
{
    base.Awake();  // 确保调用
    
    Button startBtn = GetControl<Button>("StartBtn");  // 名称必须完全一致
    Button settingsBtn = GetControl<Button>("SettingsBtn");
}

// 检查2：控件类型
Image image = GetControl<Image>("Background");  // Image控件
Text text = GetControl<Text>("ScoreText");      // Text控件
Slider slider = GetControl<Slider>("Volume");   // Slider控件

// 检查3：面板激活
void ShowPanel()
{
    UIMgr.Instance.ShowPanel<MainPanel>((panel) =>
    {
        // 在回调中获取控件，此时面板已激活
        Button btn = panel.GetControl<Button>("StartBtn");
    });
}

// 检查4：默认控件名称过滤
// BasePanel会过滤默认名称，如Image、Text、Background等
// 使用有意义的名称作为控件名
```

#### 问题7：内存泄漏

**症状：** 游戏运行一段时间后内存持续增长

**原因：**
1. 事件监听未移除
2. 定时器未清理
3. 对象池无限增长
4. 资源未卸载

**解决方案：**

```csharp
// 场景切换时清理
void OnSceneChange(string sceneName)
{
    // 清理事件
    EventCenter.Instance.Clear();
    
    // 清理定时器
    TimerMgr.Instance.Stop();
    
    // 清理音效
    MusicMgr.Instance.ClearSound();
    
    // 清理对象池
    PoolMgr.Instance.ClearPool();
    
    // 卸载资源
    ResMgr.Instance.UnloadUnusedAssets(null);
}

// 组件销毁时清理
void OnDestroy()
{
    // 移除事件监听
    EventCenter.Instance.RemoveEventListener(E_EventType.E_Score_Add, OnScoreAdd);
    
    // 移除定时器
    if (timerId > 0)
        TimerMgr.Instance.RemoveTimer(timerId);
}

// 定期内存检查
void MonitorMemory()
{
    long memory = System.GC.GetTotalMemory(true);
    Debug.Log($"Memory: {(memory / 1024 / 1024):F2} MB");
    
    if (memory > 500 * 1024 * 1024)  // 500MB
    {
        // 强制清理
        ResMgr.Instance.UnloadUnusedAssets(null);
        PoolMgr.Instance.ClearPool();
    }
}
```

### 调试技巧

#### 启用调试日志

```csharp
// 在关键位置添加日志
public class DebugConfig
{
    public static bool EnableLog = true;  // 发布时设为false
    
    public static void Log(string message)
    {
        if (EnableLog)
            Debug.Log(message);
    }
    
    public static void LogWarning(string message)
    {
        if (EnableLog)
            Debug.LogWarning(message);
    }
    
    public static void LogError(string message)
    {
        if (EnableLog)
            Debug.LogError(message);
    }
}
```

#### 使用Unity Profiler

1. 打开Window -> Analysis -> Profiler
2. 监控Memory、CPU、GPU使用情况
3. 查找内存分配热点
4. 优化高开销操作

#### 场景调试

```csharp
// 调试模式下显示额外信息
void OnGUI()
{
    if (DebugConfig.EnableLog)
    {
        GUILayout.Label($"FPS: {(1f / Time.deltaTime):F1}");
        GUILayout.Label($"Objects: {GameObject.FindObjectsOfType<GameObject>().Length}");
        GUILayout.Label($"Pool Count: {PoolMgr.Instance.GetPoolCount()}");
    }
}
```

### 性能分析

#### 需要关注的数据

```csharp
// 帧率监控
float fps = 1f / Time.deltaTime;
if (fps < 30)
    Debug.LogWarning("Low FPS detected");

// 内存监控
long memory = System.GC.GetTotalMemory(false);
Debug.Log($"Memory: {memory / 1024 / 1024:F2} MB");

// 对象数量
int objCount = FindObjectsOfType<GameObject>().Length;
Debug.Log($"Active Objects: {objCount}");

// 事件数量
int eventCount = EventCenter.Instance.GetEventCount();
Debug.Log($"Event Listeners: {eventCount}");
```

#### 优化优先级

1. **高频对象**：优先优化Update中频繁执行的内容
2. **大内存分配**：优先优化大容量的内存分配
3. **复杂计算**：优先优化复杂度高的计算逻辑
4. **资源加载**：优化资源加载策略，减少卡顿

## 版本兼容性

### Unity版本

| 框架版本 | Unity版本 |
|----------|-----------|
| 1.0+ | 2019.3+ |
| 1.5+ | 2020.1+ |
| 2.0+ | 2021.1+ |

### .NET版本

推荐使用 .NET Standard 2.0 或 .NET 4.x。

### 平台支持

框架支持以下平台：

- Windows/Mac/Linux (编辑器)
- Windows Standalone
- Mac Standalone
- iOS
- Android
- WebGL

### 已知限制

1. EditorResMgr仅在编辑器环境下工作
2. UWQResMgr需要网络连接
3. 某些功能在WebGL平台可能有兼容性问题

## 总结

本故障排除指南涵盖了使用框架时最常见的问题。如果遇到未列出的问题，可以：

1. 检查Unity控制台错误信息
2. 查看对应模块的源码注释
3. 在框架源码中添加调试日志
4. 使用Unity Profiler进行性能分析
5. 简化问题代码，创建最小复现示例

框架源码是最好的文档，遇到问题时可以直接阅读源码理解实现细节。