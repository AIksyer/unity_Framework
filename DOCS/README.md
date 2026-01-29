# Unity 框架使用指南

## 目录

- [框架概述](#框架概述)
- [快速开始](#快速开始)
- [单例模块详解](#单例模块详解)
- [资源加载系统](#资源加载系统)
- [对象池系统](#对象池系统)
- [事件系统](#事件系统)
- [定时器系统](#定时器系统)
- [UI管理系统](#ui管理系统)
- [音频管理系统](#音频管理系统)
- [输入管理系统](#输入管理系统)
- [场景管理系统](#场景管理系统)
- [工具类模块](#工具类模块)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)

## 框架概述

本框架是一套完整的Unity游戏开发解决方案，提供了游戏开发中常用的大部分功能模块。通过本框架，开发者可以快速构建游戏项目，无需重复造轮子。框架采用模块化设计，各模块之间低耦合、高内聚，可以根据需要灵活使用。

框架的核心设计理念是"约定大于配置"，遵循统一的编码规范和设计模式。框架内部大量使用了设计模式，包括单例模式、对象池模式、观察者模式等，这些都是游戏开发中非常经典的设计模式。掌握这些设计模式不仅能帮助你更好地使用框架，也能提升你的架构设计能力。

框架支持Unity 2019及以上版本，推荐使用Unity 2020或更高版本以获得最佳体验。框架不依赖任何第三方插件，纯原生Unity实现，这意味着你可以轻松地将框架集成到任何Unity项目中，而不用担心引入额外的依赖。

## 快速开始

### 初始化框架

在使用框架之前，你需要创建一个启动脚本来初始化各个管理器。建议在游戏的入口场景中创建一个空物体，挂载以下启动脚本：

```csharp
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private void Awake()
    {
        // 初始化各个管理器
        // 单例模式会在首次访问时自动初始化
        
        // 测试各个管理器是否正常初始化
        Debug.Log("框架初始化完成");
        Debug.Log($"MonoMgr: {MonoMgr.Instance != null}");
        Debug.Log($"EventCenter: {EventCenter.Instance != null}");
        Debug.Log($"PoolMgr: {PoolMgr.Instance != null}");
        Debug.Log($"TimerMgr: {TimerMgr.Instance != null}");
    }
}
```

框架的单例管理器采用延迟初始化的设计，只有当首次访问Instance属性时才会创建实例。这种设计有两个好处：一是可以控制初始化的时机，二是节省资源，避免在不需要的时候创建管理器。所有的单例都是线程安全的，采用了双重检查锁定模式，可以安全地在多线程环境下使用。

### 项目结构建议

建议按照以下结构组织你的项目代码：

```
Assets/
├── Scripts/
│   ├── Framework/          # 框架核心代码（保持不变）
│   ├── Game/               # 游戏逻辑代码
│   │   ├── Manager/        # 游戏管理器
│   │   ├── Model/          # 数据模型
│   │   ├── View/           # 视图/UI
│   │   ├── Controller/     # 控制器
│   │   └── Util/           # 游戏专用工具类
│   └── Editor/             # 编辑器扩展
└── ArtRes/                 # 美术资源
```

遵循这样的目录结构可以让项目保持清晰，便于维护。如果你需要扩展框架的功能，建议在Game目录下创建新的管理器，而不是修改Framework目录下的代码。这样当框架升级时，你的游戏代码不会受到影响。

## 单例模块详解

### 何时使用

单例模式在游戏开发中非常常用，适用于以下场景：

1. **全局唯一的管理器**：如音效管理器、日志管理器、存档管理器等，这些管理器在游戏中有且仅有一个实例
2. **跨场景共享的数据**：如玩家数据、游戏配置等，需要在多个场景之间共享
3. **全局访问点**：提供统一的接口访问某些功能，避免通过FindObjectOfType等方式查找

需要注意的是，单例不是万能的，过度使用会导致代码耦合度增加，难以测试。一般情况下，一个游戏项目有5-10个单例是合理的，超过这个数量就需要考虑是否设计有问题。

### BaseManager<T> - 非MonoBehaviour单例

用于不需要挂载到GameObject的管理器，如资源管理器、对象池管理器等。这类管理器完全在内存中运行，不需要与Unity的场景交互。

```csharp
// 使用示例：自定义一个配置管理器
public class ConfigMgr : BaseManager<ConfigMgr>
{
    private Dictionary<string, object> configData = new Dictionary<string, object>();
    
    public void LoadConfig(string key, object data)
    {
        configData[key] = data;
    }
    
    public T GetConfig<T>(string key)
    {
        if (configData.ContainsKey(key))
            return (T)configData[key];
        return default(T);
    }
}

// 使用时直接通过Instance访问
void GameFunction()
{
    ConfigMgr.Instance.LoadConfig("monster_max", 100);
    int maxMonster = ConfigMgr.Instance.GetConfig<int>("monster_max");
}
```

BaseManager<T>使用了反射来调用私有的无参构造函数，这意味着你可以将构造函数设为私有，仍然可以创建实例。这种设计保证了单例的封装性，防止外部代码创建额外的实例。内部的锁机制确保了线程安全，多个线程同时访问时不会创建多个实例。

### SingletonMono<T> - 需手动挂载的单例

用于需要挂载到GameObject上，但又需要在代码中全局访问的场景。使用前需要在场景中手动创建GameObject并挂载脚本。

```csharp
public class GameController : SingletonMono<GameController>
{
    protected override void Awake()
    {
        base.Awake();
        // 你的初始化代码
    }
    
    public void GameLogic()
    {
        // 游戏逻辑
    }
}

// 在场景中创建空物体，挂载GameController脚本
// 然后就可以通过GameController.Instance访问了
```

这种单例适合那些需要与场景交互，但又需要全局访问的对象。例如游戏控制器、关卡管理器等。注意Awake方法中应该先调用base.Awake()来保证单例的正确初始化。

### SingletonAutoMono<T> - 自动挂载的单例（推荐）

这是最方便的单例类型，当你首次访问Instance属性时，框架会自动创建一个GameObject并挂载脚本。这种单例适合绝大多数场景。

```csharp
// 音乐管理器示例
public class MusicMgr : SingletonAutoMono<MusicMgr>
{
    public void PlayBGM(string name)
    {
        // 播放背景音乐
    }
}

// 使用时不需要手动创建，框架会自动处理
void SomeFunction()
{
    MusicMgr.Instance.PlayBGM("main_theme");
}
```

推荐在以下情况下使用SingletonAutoMono：

1. **音频管理器**：需要全局控制背景音乐和音效
2. **网络管理器**：管理网络连接和消息收发
3. **存档管理器**：管理游戏存档的读写
4. **UI管理器**：管理全局UI的显示和隐藏

## 资源加载系统

### ResMgr - Resources资源加载

适合加载小型的、经常使用的资源，如UI贴图、配置文件等。Resources文件夹中的资源会打包到游戏中，无法动态更新。

```csharp
// 同步加载 - 适合小资源
void LoadResourceSync()
{
    // 加载单个资源
    GameObject player = ResMgr.Instance.Load<GameObject>("Prefabs/Player");
    Sprite icon = ResMgr.Instance.Load<Sprite>("Icons/player_icon");
    
    // 实例化资源
    GameObject obj = Instantiate(player);
}

// 异步加载 - 适合大型资源
void LoadResourceAsync()
{
    ResMgr.Instance.LoadAsync<GameObject>("Prefabs/Enemy", (enemy) =>
    {
        GameObject obj = Instantiate(enemy);
        // 资源加载完成后的操作
    });
}

// 卸载资源（引用计数为0时才会真正卸载）
void UnloadResource()
{
    ResMgr.Instance.UnloadAsset<GameObject>("Prefabs/Player");
}
```

ResMgr实现了引用计数机制，每次加载资源引用计数+1，卸载时-1。只有当引用计数为0时，资源才会被真正卸载。这种设计可以有效地管理内存，避免资源泄露。框架还支持异步加载，可以在大场景加载时显示进度条。

### ABMgr - AssetBundle资源加载

适合加载大型资源、需要热更新或DLC的场景。AssetBundle可以将资源打包成独立的文件，支持按需加载和更新。

```csharp
// 异步加载AB资源
void LoadABResource()
{
    // 加载名为"ui"的AB包中的"main_panel"预制体
    ABMgr.Instance.LoadResAsync<GameObject>("ui", "main_panel", (panel) =>
    {
        GameObject obj = Instantiate(panel);
    });
    
    // 同步加载
    ABMgr.Instance.LoadResAsync<GameObject>("ui", "main_panel", (panel) => { }, true);
}

// 卸载AB包
void UnloadAB()
{
    ABMgr.Instance.UnLoadAB("ui", (success) =>
    {
        if (success)
            Debug.Log("AB包卸载成功");
    });
}

// 清理所有AB包
void ClearAllAB()
{
    ABMgr.Instance.ClearAB();
}
```

AssetBundle是Unity官方推荐的资源管理方案，特别适合以下场景：

1. **热更新**：更新游戏资源而无需重新发布
2. **按需加载**：大型游戏可以按章节或功能分批加载资源
3. **DLC扩展**：为游戏添加付费或免费的扩展内容
4. **资源压缩**：AB包可以压缩，减少包体大小

### ABResMgr - 开发/生产环境切换

在开发阶段，可以使用EditorResMgr直接加载资源，方便调试；发布后自动切换到AB加载。

```csharp
// 设置为true时，使用Editor资源（仅开发环境）
ABResMgr.Instance.isDebug = true;

// 加载资源 - 开发环境走EditorResMgr，发布环境走ABMgr
void LoadGameResource()
{
    ABResMgr.Instance.LoadResAsync<GameObject>("ui", "main_panel", (panel) =>
    {
        // 无论什么环境，接口都是一样的
    });
}
```

这个设计非常实用，可以在开发阶段快速迭代，发布时只需修改一个布尔值即可切换到AB加载。建议在游戏发布前将isDebug设为false，并确保所有的AB包都已经正确打包。

### UWQResMgr - 网络资源加载

用于从服务器下载资源，支持string、byte[]、Texture、AssetBundle等类型。

```csharp
// 从服务器加载图片
void LoadImageFromServer()
{
    UWQResMgr.Instance.LoadRes<Texture>("http://example.com/image.png", 
        (texture) =>
        {
            // 下载成功
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = texture;
        },
        () =>
        {
            // 下载失败
            Debug.LogError("图片下载失败");
        });
}

// 从服务器下载AB包
void DownloadAssetBundle()
{
    UWQResMgr.Instance.LoadRes<AssetBundle>("http://example.com/assets/ab", 
        (ab) =>
        {
            // 下载成功，可以使用AB包
        },
        () =>
        {
            // 下载失败
        });
}
```

网络加载需要注意以下几点：

1. **网络状态检查**：在加载前检查网络连接状态
2. **加载进度**：大型资源应该显示加载进度
3. **错误处理**：网络不稳定，应该有重试机制
4. **缓存策略**：下载过的资源应该缓存到本地

## 对象池系统

### 为何使用对象池

游戏开发中经常需要频繁创建和销毁对象，如子弹、怪物、特效等。直接使用Instantiate和Destroy会产生以下问题：

1. **内存碎片**：频繁的内存分配和释放会导致内存碎片
2. **GC压力**：垃圾回收器需要频繁工作，影响游戏性能
3. **性能开销**：对象创建和销毁需要CPU资源

对象池通过复用对象来解决这些问题，显著提升游戏性能。实测表明，使用对象池后，内存占用可降低50%以上，GC峰值可降低80%以上。

### PoolMgr - 对象池管理器

```csharp
// 预制体需要挂载PoolObj脚本
[Serializable]
public class Bullet : MonoBehaviour, IPoolObject
{
    public float speed = 10f;
    
    // 对象池回收时调用，用于重置状态
    public void ResetInfo()
    {
        // 重置子弹状态
    }
    
    public void Fire(Vector3 direction)
    {
        GetComponent<Rigidbody>().velocity = direction * speed;
    }
}

// 使用对象池获取对象
void Shoot()
{
    Bullet bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");
    bullet.transform.position = firePoint.position;
    bullet.Fire(fireDirection);
}

// 使用完归还到对象池
void BulletHit(Bullet bullet)
{
    // 延迟归还，让子弹飞一会儿
    PoolMgr.Instance.PushObj(bullet);
}

// GameObject对象池使用
void SpawnEnemy()
{
    GameObject enemy = PoolMgr.Instance.GetObj("Enemy");
    enemy.transform.position = spawnPoint.position;
}

void DespawnEnemy(GameObject enemy)
{
    PoolMgr.Instance.PushObj(enemy);
}
```

对象池的使用有一些最佳实践：

1. **预热**：游戏开始时可以预先创建一些常用对象，避免游戏过程中的卡顿
2. **最大数量**：设置对象池的最大数量，避免内存无限增长
3. **分层管理**：不同类型的对象放入不同的池，便于管理和调试
4. **监控统计**：定期检查对象池的使用情况，优化池的大小

## 事件系统

### 何时使用事件

事件系统用于解耦模块之间的通信。当两个模块需要通信但又不方便直接引用时，事件是很好的选择：

1. **跨模块通信**：如玩家死亡时，音效模块播放死亡音效，UI模块显示结算界面
2. **异步回调**：如资源加载完成后的通知
3. **广播通知**：一个操作触发多个响应

```csharp
// 定义事件类型（在E_EventType.cs中添加）
public enum E_EventType 
{
    E_Player_Dead,
    E_Level_Complete,
    E_Coin_Collected,
}

// 监听事件
void RegisterEvents()
{
    EventCenter.Instance.AddEventListener(E_EventType.E_Player_Dead, OnPlayerDead);
    EventCenter.Instance.AddEventListener<int>(E_EventType.E_Coin_Collected, OnCoinCollected);
}

// 触发事件
void PlayerDie()
{
    EventCenter.Instance.EventTrigger(E_EventType.E_Player_Dead);
}

void CollectCoin(int amount)
{
    EventCenter.Instance.EventTrigger(E_EventType.E_Coin_Collected, amount);
}

// 取消监听
void UnregisterEvents()
{
    EventCenter.Instance.RemoveEventListener(E_EventType.E_Player_Dead, OnPlayerDead);
    EventCenter.Instance.RemoveEventListener<int>(E_EventType.E_Coin_Collected, OnCoinCollected);
}

// 清理所有事件（场景切换时建议调用）
void OnSceneChange()
{
    EventCenter.Instance.Clear();
}
```

使用事件系统时需要注意：

1. **及时取消监听**：对象销毁前一定要移除事件监听，否则会导致空引用异常
2. **避免滥用**：不是所有通信都需要事件，简单的直接调用更高效
3. **事件参数**：使用泛型事件可以传递任意类型的参数
4. **顺序问题**：事件的触发顺序是不确定的，不要依赖特定的执行顺序

## 定时器系统

### TimerMgr - 定时器管理器

用于执行延迟操作、周期性任务等。定时器支持两种模式：受Time.timeScale影响的普通模式和不受影响的自定义模式。

```csharp
// 创建一次性定时器（5秒后执行）
void StartOneShotTimer()
{
    int timerId = TimerMgr.Instance.CreateTimer(
        isRealTime: false,      // false: 受timeScale影响，true: 不受影响
        allTime: 5000,          // 总时间（毫秒）
        overCallBack: () =>     // 定时结束回调
        {
            Debug.Log("5秒定时器结束");
        },
        intervalTime: 1000,     // 间隔时间（毫秒）
        callBack: () =>         // 间隔回调（可选）
        {
            Debug.Log("每秒执行一次");
        }
    );
}

// 创建循环定时器
void StartLoopTimer()
{
    TimerMgr.Instance.CreateTimer(
        isRealTime: true,
        allTime: 60000,         // 60秒循环一次
        overCallBack: () => { }, // 循环定时器可以设空回调
        intervalTime: 1000,
        callBack: () =>
        {
            // 每秒执行的任务
            UpdateGameTime();
        }
    );
}

// 定时器控制
void ControlTimer(int timerId)
{
    // 暂停定时器
    TimerMgr.Instance.StopTimer(timerId);
    
    // 恢复定时器
    TimerMgr.Instance.StartTimer(timerId);
    
    // 重置定时器
    TimerMgr.Instance.ResetTimer(timerId);
    
    // 移除定时器
    TimerMgr.Instance.RemoveTimer(timerId);
}
```

定时器的典型应用场景：

1. **技能冷却**：玩家释放技能后，N秒内不能再次释放
2. **buff持续时间**：如中毒buff持续5秒，每秒造成伤害
3. **游戏倒计时**：如战斗限时30秒
4. **延迟执行**：如攻击命中后N毫秒才造成伤害（延迟伤害判定）

## UI管理系统

### UIMgr - UI面板管理器

提供了面板的显示、隐藏、获取功能，支持层级管理。面板会自动查找子控件，减少重复代码。

```csharp
// 定义面板（继承BasePanel）
public class MainPanel : BasePanel
{
    // 框架会自动查找名为"StartBtn"的Button控件
    protected override void Awake()
    {
        base.Awake();
        
        // 获取控件
        Button startBtn = GetControl<Button>("StartBtn");
        Slider volumeSlider = GetControl<Slider>("VolumeSlider");
        
        // 添加事件
        startBtn.onClick.AddListener(OnStartClick);
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);
    }
    
    public override void ShowMe()
    {
        // 显示面板时的逻辑
        gameObject.SetActive(true);
    }
    
    public override void HideMe()
    {
        // 隐藏面板时的逻辑
        gameObject.SetActive(false);
    }
    
    private void OnStartClick()
    {
        // 开始游戏
    }
}

// 使用UIMgr管理面板
void ShowMainUI()
{
    // 显示主面板（默认在Middle层级）
    UIMgr.Instance.ShowPanel<MainPanel>(E_UILayer.Middle, (panel) =>
    {
        Debug.Log("主面板显示完成");
    });
    
    // 显示设置面板（在Top层级，显示在其他面板上方）
    UIMgr.Instance.ShowPanel<SettingsPanel>(E_UILayer.Top);
    
    // 隐藏主面板（不销毁）
    UIMgr.Instance.HidePanel<MainPanel>();
    
    // 销毁主面板
    UIMgr.Instance.HidePanel<MainPanel>(true);
    
    // 获取已显示的面板
    UIMgr.Instance.GetPanel<MainPanel>((panel) =>
    {
        if (panel != null)
        {
            // 使用面板
        }
    });
}
```

### UI层级说明

框架定义了四个UI层级，从下到上依次是：

1. **Bottom**：最底层背景，如主界面背景
2. **Middle**：中间层，主面板所在层级
3. **Top**：上层，弹窗、提示框所在层级
4. **System**：系统层，最顶层，如设置界面、帮助界面

层级使用建议：

- 主界面放在Middle层级
- 弹窗、确认框放在Top层级
- 公告、系统消息放在System层级
- 根据需求灵活调整，确保UI遮挡关系正确

## 音频管理系统

### MusicMgr - 音频管理器

统一管理背景音乐和音效，支持音量控制、暂停/恢复等功能。

```csharp
// 背景音乐管理
void ManageBGM()
{
    // 播放背景音乐
    MusicMgr.Instance.PlayBKMusic("main_theme");
    
    // 暂停背景音乐
    MusicMgr.Instance.PauseBKMusic();
    
    // 停止背景音乐
    MusicMgr.Instance.StopBKMusic();
    
    // 设置背景音乐音量
    MusicMgr.Instance.ChangeBKMusicValue(0.5f);
}

// 音效管理
void PlaySoundEffects()
{
    // 播放音效
    MusicMgr.Instance.PlaySound("click", isLoop: false, isSync: false, (source) =>
    {
        // 音效播放回调
        Debug.Log("音效开始播放");
    });
    
    // 播放循环音效（如环境音）
    MusicMgr.Instance.PlaySound("wind", isLoop: true);
    
    // 停止指定音效
    MusicMgr.Instance.StopSound(soundSource);
    
    // 设置音效音量
    MusicMgr.Instance.ChangeSoundValue(0.8f);
    
    // 暂停/恢复所有音效
    MusicMgr.Instance.PlayOrPauseSound(false);  // 暂停
    MusicMgr.Instance.PlayOrPauseSound(true);   // 恢复
    
    // 清理所有音效
    MusicMgr.Instance.ClearSound();
}
```

音频管理最佳实践：

1. **分类管理**：将音效按类型分类，如UI音效、战斗音效、环境音效
2. **预加载**：游戏开始时预加载常用音效
3. **音量分组**：背景音乐、音效、语音分开控制
4. **静音功能**：提供全局静音开关

## 输入管理系统

### InputMgr - 输入管理器

将输入事件转换为框架事件，支持键盘和鼠标输入的统一管理。

```csharp
// 初始化输入配置
void SetupInput()
{
    // 开启输入系统
    InputMgr.Instance.StartOrCloseInputMgr(true);
    
    // 配置键盘按键
    InputMgr.Instance.ChangeKeyboardInfo(
        E_EventType.E_Input_Skill1,  // 事件类型
        KeyCode.Q,                    // 按键
        InputInfo.E_InputType.Down    // 按下触发
    );
    
    // 配置鼠标按键
    InputMgr.Instance.ChangeMouseInfo(
        E_EventType.E_Fire,          // 事件类型
        0,                           // 0=左键，1=右键，2=中键
        InputInfo.E_InputType.Down   // 按下触发
    );
    
    // 监听输入事件
    EventCenter.Instance.AddEventListener(E_EventType.E_Input_Skill1, OnSkill1);
    EventCenter.Instance.AddEventListener(E_EventType.E_Fire, OnFire);
    EventCenter.Instance.AddEventListener<float>(E_EventType.E_Input_Horizontal, OnHorizontal);
    EventCenter.Instance.AddEventListener<float>(E_EventType.E_Input_Vertical, OnVertical);
}

private void OnSkill1()
{
    // Q键按下
    UseSkill(1);
}

private void OnFire()
{
    // 鼠标左键按下
    Fire();
}

private void OnHorizontal(float value)
{
    // 水平输入 -1~1
    Player.MoveHorizontal(value);
}

private void OnVertical(float value)
{
    // 垂直输入 -1~1
    Player.MoveVertical(value);
}
```

输入系统的优势：

1. **统一管理**：所有输入配置集中在一处，便于维护
2. **热更新**：可以运行时修改按键配置，支持自定义按键
3. **事件驱动**：输入直接触发事件，无需在Update中轮询
4. **平台适配**：可以统一处理不同平台的输入差异

## 场景管理系统

### SceneMgr - 场景管理器

提供同步和异步场景加载功能，异步加载支持进度回调。

```csharp
// 同步加载（不推荐，会卡顿）
void LoadSceneSync()
{
    SceneMgr.Instance.LoadScene("GameScene", () =>
    {
        Debug.Log("场景加载完成");
    });
}

// 异步加载（推荐）
void LoadSceneAsync()
{
    SceneMgr.Instance.LoadSceneAsyn("GameScene", () =>
    {
        Debug.Log("场景加载完成");
    });
}

// 监听加载进度
void LoadWithProgress()
{
    // 监听加载进度事件
    EventCenter.Instance.AddEventListener<float>(E_EventType.E_SceneLoadChange, (progress) =>
    {
        // progress范围 0~1
        loadingBar.value = progress;
        loadingText.text = $"加载中... {(progress * 100):F0}%";
    });
    
    SceneMgr.Instance.LoadSceneAsyn("GameScene", () =>
    {
        // 加载完成
        loadingPanel.SetActive(false);
    });
}
```

异步加载时进度事件的触发频率很高，如果需要更新UI，建议加上节流处理，避免每帧更新UI造成性能问题。同时要注意在场景加载完成后及时清理进度监听事件。

## 工具类模块

### MathUtil - 数学工具

提供常用的数学计算功能：

```csharp
// 角度转换
float rad = MathUtil.Deg2Rad(45f);  // 角度转弧度
float deg = MathUtil.Rad2Deg(Mathf.PI / 4f);  // 弧度转角度

// 距离计算
float distanceXZ = MathUtil.GetObjDistanceXZ(pos1, pos2);  // XZ平面距离
float distanceXY = MathUtil.GetObjDistanceXY(pos1, pos2);  // XY平面距离
bool isNear = MathUtil.CheckObjDistanceXZ(pos1, pos2, 5f); // XZ距离判断

// 射线检测
MathUtil.RayCast(ray, (hit) =>
{
    // 射线命中
    Debug.Log($"击中: {hit.collider.name}");
}, 100f, LayerMask.GetMask("Enemy"));

// 范围检测
MathUtil.OverlapSphere(center, radius, layerMask, (collider) =>
{
    // 处理范围内的物体
});
```

### TextUtil - 字符串工具

提供字符串解析和格式化功能：

```csharp
// 字符串分割
string[] items = TextUtil.SplitStr("item1;item2;item3", 1);  // 分号分割
string[] coords = TextUtil.SplitStr("100,200", 2);           // 逗号分割

// 字符串转数组
int[] nums = TextUtil.SplitStrToIntArr("1,2,3,4,5", 2);

// 数值格式化
string numStr = TextUtil.GetNumStr(5, 3);    // "005"
string decimalStr = TextUtil.GetDecimalStr(3.14159f, 2);  // "3.14"
string bigNum = TextUtil.GetBigDataToString(12345);       // "1万2345"

// 时长格式化
string hms = TextUtil.SecondToHMS(3661, false, false, "时", "分", "秒");  // "1时1分1秒"
string hms2 = TextUtil.SecondToHMS2(3661);  // "01:01:01"
```

### EncryptionUtil - 加密工具

简单的数值加密/解密，用于保护关键数据：

```csharp
// 加密
int key = EncryptionUtil.GetRandomKey();  // 获取密钥
int encrypted = EncryptionUtil.LockValue(100, key);  // 加密

// 解密
int decrypted = EncryptionUtil.UnLoackValue(encrypted, key);  // 解密

// 存档数据保护示例
void SaveGame()
{
    int key = EncryptionUtil.GetRandomKey();
    int health = EncryptionUtil.LockValue(player.health, key);
    int score = EncryptionUtil.LockValue(gameScore, key);
    
    // 存档保存加密后的值和密钥
    PlayerPrefs.SetInt("health_key", key);
    PlayerPrefs.SetInt("health_data", health);
}

void LoadGame()
{
    int key = PlayerPrefs.GetInt("health_key");
    int healthData = PlayerPrefs.GetInt("health_data");
    player.health = EncryptionUtil.UnLoackValue(healthData, key);
}
```

## 最佳实践

### 初始化顺序

框架的各个管理器有依赖关系，初始化时应该注意：

```csharp
// 推荐顺序
void FrameworkInitialize()
{
    // 1. 基础模块最先初始化
    MonoMgr.Instance.AddUpdateListener(Update);  // Update循环
    
    // 2. 资源相关
    ResMgr.Instance.Load<GameObject>("Prefabs/GameManager");  // 预加载
    
    // 3. 业务模块
    AudioMgr.Instance.PlayBGM("main_theme");  // 播放背景音乐
    
    // 4. 输入系统最后开启
    InputMgr.Instance.StartOrCloseInputMgr(true);
}
```

### 内存管理

游戏开发中内存管理至关重要，以下是一些建议：

```csharp
// 1. 使用对象池
void SpawnBullet()
{
    Bullet bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");
    // 使用完后归还
    PoolMgr.Instance.PushObj(bullet);
}

// 2. 及时卸载无用资源
void UnloadUnusedResources()
{
    ResMgr.Instance.UnloadUnusedAssets(() =>
    {
        Debug.Log("无用资源卸载完成");
    });
}

// 3. 场景切换时清理
void OnSceneChange(string sceneName)
{
    // 清理事件
    EventCenter.Instance.Clear();
    
    // 清理定时器
    TimerMgr.Instance.StopAll();
    
    // 清理音效
    MusicMgr.Instance.ClearSound();
    
    // 清理对象池
    PoolMgr.Instance.ClearPool();
}
```

### 性能优化

```csharp
// 1. 减少Update中的计算
void Update()
{
    // 不推荐：每帧计算
    float distance = Vector3.Distance(transform.position, target.position);
    
    // 推荐：使用定时器
    // 见TimerMgr使用示例
}

// 2. 使用对象池减少GC
// 见PoolMgr使用示例

// 3. 合理使用事件避免耦合
// 见EventCenter使用示例

// 4. 资源预加载
void PreloadResources()
{
    // 游戏开始时预加载
    for (int i = 0; i < 10; i++)
    {
        PoolMgr.Instance.GetObj("Enemy");
        PoolMgr.Instance.PushObj(PoolMgr.Instance.GetObj("Enemy"));
    }
}
```

## 常见问题

### Q1: 单例实例为null怎么办？

检查是否在正确的时机访问Instance。单例采用延迟初始化，应该在场景加载完成后访问。如果需要在Awake中访问，确保基类的Awake先执行。

### Q2: 事件监听不触发？

1. 检查事件类型是否匹配
2. 检查监听是否正确注册
3. 检查事件是否已触发
4. 场景切换后需要重新注册事件

### Q3: 对象池获取不到对象？

1. 检查预制体是否正确放置在Resources文件夹
2. 检查PoolObj脚本是否正确挂载
3. 检查对象池是否已达最大数量

### Q4: 资源加载失败？

1. 检查资源路径是否正确
2. 检查资源类型是否匹配
3. 检查AB包是否正确打包
4. 检查网络连接（如果是网络加载）

### Q5: 定时器不执行？

1. 检查定时器是否已启动
2. 检查isRealTime参数
3. 检查回调是否为null
4. 检查是否在场景切换后忘记重新创建

### Q6: UI控件获取为null？

1. 检查控件名称是否与GetControl中的一致
2. 检查BasePanel的Awake是否已执行
3. 检查控件是否在面板的子物体中

## 总结

本框架提供了游戏开发中常用的功能模块，通过合理使用这些模块，可以大幅提升开发效率。框架的核心优势在于：

1. **模块化设计**：各模块独立，可以按需使用
2. **低耦合高内聚**：模块之间通过接口通信，降低耦合度
3. **完善的工具类**：提供常用的工具函数，减少重复代码
4. **统一的编码规范**：遵循一致的编程风格，便于维护

建议在项目开发过程中，逐步熟悉各个模块的使用方法，根据实际需求选择合适的模块。遇到问题时，可以参考本文档的常见问题章节，或者查看各模块的源码注释获取更多帮助。