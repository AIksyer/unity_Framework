# API 参考手册

## 单例模块 API

### BaseManager<T>

```csharp
public abstract class BaseManager<T> where T:class
{
    // 获取单例实例（线程安全）
    public static T Instance { get; }
    
    // 检查实例是否为null
    protected bool InstanceisNull { get; }
}
```

**使用示例：**
```csharp
public class GameConfig : BaseManager<GameConfig>
{
    private Dictionary<string, object> configs = new Dictionary<string, object>();
    
    public void SetConfig(string key, object value)
    {
        configs[key] = value;
    }
    
    public T GetConfig<T>(string key)
    {
        return configs.TryGetValue(key, out var value) ? (T)value : default(T);
    }
}

// 调用方式
GameConfig.Instance.SetConfig("max_level", 100);
int maxLevel = GameConfig.Instance.GetConfig<int>("max_level");
```

### SingletonMono<T>

```csharp
public class SingletonMono<T> : MonoBehaviour where T:MonoBehaviour
{
    protected static T instance;
    public static T Instance { get; }
    
    protected virtual void Awake();
}
```

**使用示例：**
```csharp
public class GameManager : SingletonMono<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        // 初始化逻辑
    }
    
    public void GameLoop()
    {
        // 游戏主循环
    }
}

// 在场景中创建空物体，挂载GameManager脚本
GameManager.Instance.GameLoop();
```

### SingletonAutoMono<T>

```csharp
public class SingletonAutoMono<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T instance;
    public static T Instance { get; }
}
```

**使用示例：**
```csharp
public class NotificationCenter : SingletonAutoMono<NotificationCenter>
{
    public void ShowMessage(string message)
    {
        Debug.Log(message);
    }
}

// 无需手动创建，框架自动处理
NotificationCenter.Instance.ShowMessage("游戏开始");
```

## 资源管理 API

### ResMgr

```csharp
public class ResMgr : BaseManager<ResMgr>
{
    // 同步加载
    public T Load<T>(string path) where T : UnityEngine.Object;
    
    // 异步加载
    public void LoadAsync<T>(string path, UnityAction<T> callBack) where T : UnityEngine.Object;
    
    // 卸载资源
    public void UnloadAsset<T>(string path, bool isDel = false, UnityAction<T> callBack = null, bool isSub = true);
    
    // 卸载未使用资源
    public void UnloadUnusedAssets(UnityAction callBack);
    
    // 获取引用计数
    public int GetRefCount<T>(string path);
}
```

**完整示例：**
```csharp
public class ResourceExample : MonoBehaviour
{
    private void Start()
    {
        // 同步加载
        GameObject playerPrefab = ResMgr.Instance.Load<GameObject>("Prefabs/Player");
        Instantiate(playerPrefab);
        
        // 异步加载并实例化
        ResMgr.Instance.LoadAsync<GameObject>("Prefabs/Enemy", (enemyPrefab) =>
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject enemy = Instantiate(enemyPrefab);
                enemy.transform.position = new Vector3(i * 2f, 0, 0);
            }
        });
        
        // 引用计数管理
        ResMgr.Instance.Load<GameObject>("Prefabs/Player");  // refCount = 1
        ResMgr.Instance.Load<GameObject>("Prefabs/Player");  // refCount = 2
        
        ResMgr.Instance.UnloadAsset<GameObject>("Prefabs/Player");  // refCount = 1
        ResMgr.Instance.UnloadAsset<GameObject>("Prefabs/Player");  // refCount = 0, 资源卸载
    }
}
```

### ABMgr

```csharp
public class ABMgr : SingletonAutoMono<ABMgr>
{
    // 泛型异步加载
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callBack, bool isSync = false) where T:Object;
    
    // Type异步加载
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack, bool isSync = false);
    
    // 名字异步加载
    public void LoadResAsync(string abName, string resName, UnityAction<Object> callBack, bool isSync = false);
    
    // 卸载AB包
    public void UnLoadAB(string name, UnityAction<bool> callBackResult);
    
    // 清理所有AB包
    public void ClearAB();
}
```

**AB加载完整流程：**
```csharp
public class ABExample : MonoBehaviour
{
    private void LoadABScene()
    {
        // 1. 加载AB包中的预制体
        ABMgr.Instance.LoadResAsync<GameObject>("characters", "hero_prefab", (hero) =>
        {
            Instantiate(hero);
        });
        
        // 2. 加载AB包中的材质
        ABMgr.Instance.LoadResAsync<Material>("materials", "hero_mat", (mat) =>
        {
            GetComponent<Renderer>().material = mat;
        });
        
        // 3. 加载AB包中的音频
        ABMgr.Instance.LoadResAsync<AudioClip>("audio", "bgm_main", (clip) =>
        {
            MusicMgr.Instance.PlayBKMusic(clip.name);
        });
        
        // 4. 批量加载场景资源
        ABMgr.Instance.LoadResAsync<Object>("levels", "level_01", typeof(Object), (obj) =>
        {
            // 处理加载的资源
        });
    }
    
    private void UnloadABExample()
    {
        // 卸载指定AB包
        ABMgr.Instance.UnLoadAB("characters", (success) =>
        {
            if (success)
                Debug.Log("AB包卸载成功");
        });
        
        // 清理所有（切换场景时）
    ABMgr.Instance.ClearAB();
    }
}
```

### UWQResMgr

```csharp
public class UWQResMgr : SingletonAutoMono<UWQResMgr>
{
    // 加载资源
    public void LoadRes<T>(string path, UnityAction<T> callBack, UnityAction failCallBack) where T : class;
}
```

**支持类型：**
- `string`: 文本内容
- `byte[]`: 字节数组
- `Texture`: 图片纹理
- `AssetBundle`: AssetBundle包

**使用示例：**
```csharp
public class NetworkExample : MonoBehaviour
{
    private void DownloadConfig()
    {
        // 下载JSON配置
        UWQResMgr.Instance.LoadRes<string>("http://example.com/config.json", 
            (configJson) =>
            {
                var config = JsonUtility.FromJson<GameConfig>(configJson);
                ApplyConfig(config);
            },
            () => Debug.LogError("配置下载失败")
        );
        
        // 下载图片
        UWQResMgr.Instance.LoadRes<Texture>("http://example.com/image.png",
            (texture) =>
            {
                GetComponent<Renderer>().material.mainTexture = texture;
            },
            () => Debug.LogError("图片下载失败")
        );
        
        // 下载AB包
        UWQResMgr.Instance.LoadRes<AssetBundle>("http://example.com/assets/game_ab",
            (ab) =>
            {
                var prefab = ab.LoadAsset<GameObject>("enemy");
                Instantiate(prefab);
            },
            () => Debug.LogError("AB包下载失败")
        );
    }
}
```

## 对象池 API

### PoolMgr

```csharp
public class PoolMgr : BaseManager<PoolMgr>
{
    // 从池中获取GameObject
    public GameObject GetObj(string name);
    
    // 从池中获取T类型对象（需实现IPoolObject）
    public T GetObj<T>(string nameSpace = "") where T : class, IPoolObject, new();
    
    // 归还GameObject到池
    public void PushObj(GameObject obj);
    
    // 归还T类型对象到池
    public void PushObj<T>(T obj, string nameSpace = "") where T : class, IPoolObject;
    
    // 清空所有池
    public void ClearPool();
}
```

### IPoolObject

```csharp
public interface IPoolObject
{
    // 对象回收到池时调用，用于重置状态
    void ResetInfo();
}
```

**完整示例：**
```csharp
// 1. 定义可池化的对象
public class Bullet : MonoBehaviour, IPoolObject
{
    public float speed = 20f;
    public int damage = 10;
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void Fire(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction);
        rb.velocity = direction * speed;
        gameObject.SetActive(true);
    }
    
    // 实现IPoolObject接口
    public void ResetInfo()
    {
        rb.velocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}

// 2. 在预制体上挂载脚本
// 预制体结构：
// - Bullet (挂载Bullet脚本和PoolObj脚本)
//   - Model
//   - Collider

// 3. 使用对象池
public class BulletSystem : MonoBehaviour
{
    public Transform firePoint;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireBullet();
        }
    }
    
    void FireBullet()
    {
        // 从池中获取
        Bullet bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");
        bullet.Fire(firePoint.position, firePoint.forward);
    }
    
    void OnBulletHit(Bullet bullet)
    {
        // 延迟归还到池
        Invoke(() => PoolMgr.Instance.PushObj(bullet), 2f);
    }
}

// 数据结构对象池
public class MessageData : IPoolObject
{
    public int id;
    public string content;
    public float timestamp;
    
    public void ResetInfo()
    {
        id = 0;
        content = null;
        timestamp = 0;
    }
}

// 使用
MessageData msg = PoolMgr.Instance.GetObj<MessageData>();
msg.id = 1;
msg.content = "Hello";
msg.timestamp = Time.time;

// 用完归还
PoolMgr.Instance.PushObj(msg);
```

## 事件系统 API

### EventCenter

```csharp
public class EventCenter : BaseManager<EventCenter>
{
    // 触发带参数的事件
    public void EventTrigger<T>(E_EventType eventName, T info);
    
    // 触发无参数的事件
    public void EventTrigger(E_EventType eventName);
    
    // 添加带参数的事件监听
    public void AddEventListener<T>(E_EventType eventName, UnityAction<T> func);
    
    // 添加无参数的事件监听
    public void AddEventListener(E_EventType eventName, UnityAction func);
    
    // 移除带参数的事件监听
    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> func);
    
    // 移除无参数的事件监听
    public void RemoveEventListener(E_EventType eventName, UnityAction func);
    
    // 清空所有事件
    public void Clear();
    
    // 清空指定事件
    public void Claer(E_EventType eventName);
}
```

**完整示例：**
```csharp
// 1. 定义事件类型（在E_EventType.cs中添加）
public enum E_EventType 
{
    // 现有事件...
    E_Game_Init,           // 游戏初始化完成
    E_Player_Hp_Change,    // 玩家血量变化
    E_Enemy_Spawn,         // 敌人生成
    E_Score_Add,           // 分数增加
    E_Game_Pause,          // 游戏暂停
    E_Game_Resume,         // 游戏恢复
}

// 2. 监听事件
public class GameUI : MonoBehaviour
{
    private int score = 0;
    
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(E_EventType.E_Score_Add, OnScoreAdd);
        EventCenter.Instance.AddEventListener(E_EventType.E_Player_Hp_Change, OnHpChange);
        EventCenter.Instance.AddEventListener(E_EventType.E_Game_Pause, OnGamePause);
    }
    
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener(E_EventType.E_Score_Add, OnScoreAdd);
        EventCenter.Instance.RemoveEventListener(E_EventType.E_Player_Hp_Change, OnHpChange);
        EventCenter.Instance.RemoveEventListener(E_EventType.E_Game_Pause, OnGamePause);
    }
    
    private void OnScoreAdd(int addScore)
    {
        score += addScore;
        UpdateScoreUI();
    }
    
    private void OnHpChange(float hpPercent)
    {
        UpdateHpUI(hpPercent);
    }
    
    private void OnGamePause()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }
}

// 3. 触发事件
public class GameLogic : MonoBehaviour
{
    void AddScore(int amount)
    {
        score += amount;
        // 通知UI更新
        EventCenter.Instance.EventTrigger(E_EventType.E_Score_Add, amount);
    }
    
    void PlayerHurt(float currentHp, float maxHp)
    {
        float percent = currentHp / maxHp;
        EventCenter.Instance.EventTrigger(E_EventType.E_Player_Hp_Change, percent);
    }
}

// 4. 场景切换时清理
public class SceneTransition : MonoBehaviour
{
    void LoadNextScene()
    {
        // 清理事件，防止内存泄漏
        EventCenter.Instance.Clear();
        
        SceneMgr.Instance.LoadSceneAsyn("NextLevel", () =>
        {
            // 新场景会重新注册需要的事件
        });
    }
}
```

## 定时器 API

### TimerMgr

```csharp
public class TimerMgr : BaseManager<TimerMgr>
{
    // 创建定时器
    public int CreateTimer(bool isRealTime, int allTime, UnityAction overCallBack, 
        int intervalTime = 0, UnityAction callBack = null);
    
    // 移除定时器
    public void RemoveTimer(int keyID);
    
    // 启动定时器
    public void StartTimer(int keyID);
    
    // 暂停定时器
    public void StopTimer(int keyID);
    
    // 重置定时器
    public void ResetTimer(int keyID);
    
    // 启动所有定时器
    public void Start();
    
    // 停止所有定时器
    public void Stop();
}
```

**完整示例：**
```csharp
public class TimerExample : MonoBehaviour
{
    private int skillCooldownTimer;
    private int buffTimer;
    private int gameTimer;
    
    void Start()
    {
        // 技能冷却定时器（5秒）
        skillCooldownTimer = TimerMgr.Instance.CreateTimer(
            isRealTime: false,
            allTime: 5000,
            overCallBack: () => 
            {
                Debug.Log("技能冷却完成");
                skillReady = true;
            }
        );
        
        // BUFF持续定时器（10秒，每秒触发一次）
        buffTimer = TimerMgr.Instance.CreateTimer(
            isRealTime: true,
            allTime: 10000,
            overCallBack: () => Debug.Log("BUFF结束"),
            intervalTime: 1000,
            callBack: () => 
            {
                ApplyBuffEffect();
            }
        );
        
        // 游戏倒计时（3分钟）
        gameTimer = TimerMgr.Instance.CreateTimer(
            isRealTime: true,
            allTime: 180000,
            overCallBack: () => 
            {
                Debug.Log("游戏时间到");
                GameOver();
            },
            intervalTime: 1000,
            callBack: () => 
            {
                UpdateGameTimerUI();
            }
        );
        
        // 初始暂停定时器
        TimerMgr.Instance.StopTimer(skillCooldownTimer);
    }
    
    void UseSkill()
    {
        if (skillReady)
        {
            skillReady = false;
            // 启动技能冷却
            TimerMgr.Instance.StartTimer(skillCooldownTimer);
            ExecuteSkill();
        }
    }
    
    void ApplyBuffEffect()
    {
        player.AddBuffPower(10f);
    }
    
    void OnDestroy()
    {
        // 清理定时器
        TimerMgr.Instance.RemoveTimer(skillCooldownTimer);
        TimerMgr.Instance.RemoveTimer(buffTimer);
        TimerMgr.Instance.RemoveTimer(gameTimer);
    }
}
```

## UI管理 API

### UIMgr

```csharp
public class UIMgr : BaseManager<UIMgr>
{
    // 获取层级父物体
    public Transform GetLayerFather(E_UILayer layer);
    
    // 显示面板
    public void ShowPanel<T>(E_UILayer layer = E_UILayer.Middle, 
        UnityAction<T> callBack = null, bool isSync = false) where T:BasePanel;
    
    // 隐藏面板
    public void HidePanel<T>(bool isDestory = false) where T : BasePanel;
    
    // 获取已显示的面板
    public void GetPanel<T>(UnityAction<T> callBack) where T:BasePanel;
    
    // 添加UI事件监听
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, 
        UnityAction<BaseEventData> callBack);
}
```

### BasePanel

```csharp
public abstract class BasePanel : MonoBehaviour
{
    // 显示面板时调用的逻辑
    public abstract void ShowMe();
    
    // 隐藏面板时调用的逻辑
    public abstract void HideMe();
    
    // 获取指定名称和类型的控件
    public T GetControl<T>(string name) where T:UIBehaviour;
    
    // 按钮点击回调（子类可重写）
    protected virtual void ClickBtn(string btnName) { }
    
    // 滑条值变化回调（子类可重写）
    protected virtual void SliderValueChange(string sliderName, float value) { }
    
    // 开关值变化回调（子类可重写）
    protected virtual void ToggleValueChange(string sliderName, bool value) { }
}
```

**完整UI面板示例：**
```csharp
// 1. 定义面板脚本
public class SettingsPanel : BasePanel
{
    private Slider bgmSlider;
    private Slider sfxSlider;
    private Toggle fullscreenToggle;
    private Button confirmButton;
    private Button cancelButton;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 获取控件引用
        bgmSlider = GetControl<Slider>("BGMSlider");
        sfxSlider = GetControl<Slider>("SFXSlider");
        fullscreenToggle = GetControl<Toggle>("FullscreenToggle");
        confirmButton = GetControl<Button>("ConfirmButton");
        cancelButton = GetControl<Button>("CancelButton");
        
        // 添加事件监听
        confirmButton.onClick.AddListener(OnConfirmClick);
        cancelButton.onClick.AddListener(OnCancelClick);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChange);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChange);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
    }
    
    public override void ShowMe()
    {
        gameObject.SetActive(true);
        // 恢复设置值到UI
        bgmSlider.value = AudioMgr.Instance.BGMVolume;
        sfxSlider.value = AudioMgr.Instance.SFXVolume;
        fullscreenToggle.isOn = Screen.fullScreen;
    }
    
    public override void HideMe()
    {
        gameObject.SetActive(false);
    }
    
    private void OnConfirmClick()
    {
        // 保存设置
        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetInt("FullScreen", fullscreenToggle.isOn ? 1 : 0);
        
        // 应用设置
        AudioMgr.Instance.ChangeBKMusicValue(bgmSlider.value);
        AudioMgr.Instance.ChangeSoundValue(sfxSlider.value);
        Screen.fullScreen = fullscreenToggle.isOn;
        
        // 隐藏面板
        UIMgr.Instance.HidePanel<SettingsPanel>();
    }
    
    private void OnCancelClick()
    {
        // 恢复原始设置
        UIMgr.Instance.HidePanel<SettingsPanel>();
    }
    
    private void OnBGMSliderChange(float value)
    {
        AudioMgr.Instance.ChangeBKMusicValue(value);
    }
    
    private void OnSFXSliderChange(float value)
    {
        AudioMgr.Instance.ChangeSoundValue(value);
    }
    
    private void OnFullscreenToggle(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}

// 2. 使用面板
public class UIController : MonoBehaviour
{
    void ShowSettings()
    {
        UIMgr.Instance.ShowPanel<SettingsPanel>(E_UILayer.Top, (panel) =>
        {
            Debug.Log("设置面板显示完成");
        });
    }
    
    void HideSettings()
    {
        UIMgr.Instance.HidePanel<SettingsPanel>();
        // 或销毁面板
        // UIMgr.Instance.HidePanel<SettingsPanel>(true);
    }
}
```

## 音频管理 API

### MusicMgr

```csharp
public class MusicMgr : BaseManager<MusicMgr>
{
    // 背景音乐
    public void PlayBKMusic(string name);
    public void StopBKMusic();
    public void PauseBKMusic();
    public void ChangeBKMusicValue(float v);
    
    // 音效
    public void PlaySound(string name, bool isLoop = false, bool isSync = false, 
        UnityAction<AudioSource> callBack = null);
    public void StopSound(AudioSource source);
    public void ChangeSoundValue(float v);
    public void PlayOrPauseSound(bool isPlay);
    public void ClearSound();
}
```

**完整示例：**
```csharp
public class AudioSystemExample : MonoBehaviour
{
    void Start()
    {
        // 播放背景音乐
        MusicMgr.Instance.PlayBKMusic("main_theme");
        MusicMgr.Instance.ChangeBKMusicValue(0.5f);
        
        // 播放音效
        PlayButtonClick();
    }
    
    void PlayButtonClick()
    {
        MusicMgr.Instance.PlaySound("click", isLoop: false, (source) =>
        {
            Debug.Log("点击音效开始播放");
        });
    }
    
    void EnableSound()
    {
        MusicMgr.Instance.PlayOrPauseSound(true);
    }
    
    void DisableSound()
    {
        MusicMgr.Instance.PlayOrPauseSound(false);
    }
    
    void MuteAll()
    {
        MusicMgr.Instance.ChangeBKMusicValue(0);
        MusicMgr.Instance.ChangeSoundValue(0);
    }
    
    void OnDestroy()
    {
        MusicMgr.Instance.ClearSound();
    }
}
```

## 场景管理 API

### SceneMgr

```csharp
public class SceneMgr : BaseManager<SceneMgr>
{
    // 同步加载
    public void LoadScene(string name, UnityAction callBack = null);
    
    // 异步加载
    public void LoadSceneAsyn(string name, UnityAction callBack = null);
}
```

**完整示例：**
```csharp
public class SceneSystemExample : MonoBehaviour
{
    public Slider loadingSlider;
    public Text loadingText;
    public GameObject loadingPanel;
    
    void Start()
    {
        // 监听加载进度
        EventCenter.Instance.AddEventListener<float>(E_EventType.E_SceneLoadChange, OnLoadProgress);
    }
    
    void OnLoadProgress(float progress)
    {
        loadingSlider.value = progress;
        loadingText.text = $"{(progress * 100):F0}%";
    }
    
    void LoadLevel1()
    {
        loadingPanel.SetActive(true);
        SceneMgr.Instance.LoadSceneAsyn("Level1", () =>
        {
            loadingPanel.SetActive(false);
            OnLevelLoaded();
        });
    }
    
    void OnLevelLoaded()
    {
        // 初始化新场景
        EventCenter.Instance.EventTrigger(E_EventType.E_Game_Init);
    }
    
    void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<float>(E_EventType.E_SceneLoadChange, OnLoadProgress);
    }
}
```

## 输入管理 API

### InputMgr

```csharp
public class InputMgr : BaseManager<InputMgr>
{
    // 开启/关闭输入系统
    public void StartOrCloseInputMgr(bool isStart);
    
    // 配置键盘输入
    public void ChangeKeyboardInfo(E_EventType eventType, KeyCode key, InputInfo.E_InputType inputType);
    
    // 配置鼠标输入
    public void ChangeMouseInfo(E_EventType eventType, int mouseID, InputInfo.E_InputType inputType);
    
    // 移除输入配置
    public void RemoveInputInfo(E_EventType eventType);
    
    // 获取当前输入信息
    public void GetInputInfo(UnityAction<InputInfo> callBack);
}
```

**完整示例：**
```csharp
public class InputSystemExample : MonoBehaviour
{
    void SetupControls()
    {
        // 开启输入系统
        InputMgr.Instance.StartOrCloseInputMgr(true);
        
        // 配置按键 - 键盘
        InputMgr.Instance.ChangeKeyboardInfo(E_EventType.E_Jump, KeyCode.Space, InputInfo.E_InputType.Down);
        InputMgr.Instance.ChangeKeyboardInfo(E_EventType.E_Attack, KeyCode.J, InputInfo.E_InputType.Down);
        InputMgr.Instance.ChangeKeyboardInfo(E_EventType.E_Dash, KeyCode.K, InputInfo.E_InputType.Down);
        InputMgr.Instance.ChangeKeyboardInfo(E_EventType.E_Sprint, KeyCode.LeftShift, InputInfo.E_InputType.Always);
        
        // 配置按键 - 鼠标
        InputMgr.Instance.ChangeMouseInfo(E_EventType.E_Fire, 0, InputInfo.E_InputType.Down);       // 左键
        InputMgr.Instance.ChangeMouseInfo(E_EventType.E_Aim, 1, InputInfo.E_InputType.Down);        // 右键
        InputMgr.Instance.ChangeMouseInfo(E_EventType.E_Special, 2, InputInfo.E_InputType.Down);    // 中键
        
        // 监听输入事件
        EventCenter.Instance.AddEventListener(E_EventType.E_Jump, OnJump);
        EventCenter.Instance.AddEventListener(E_EventType.E_Fire, OnFire);
        EventCenter.Instance.AddEventListener<float>(E_EventType.E_Input_Horizontal, OnHorizontal);
        EventCenter.Instance.AddEventListener<float>(E_EventType.E_Input_Vertical, OnVertical);
    }
    
    private void OnJump()
    {
        if (player.IsGrounded())
            player.Jump();
    }
    
    private void OnFire()
    {
        player.Attack();
    }
    
    private void OnHorizontal(float value)
    {
        player.MoveHorizontal(value);
    }
    
    private void OnVertical(float value)
    {
        player.MoveVertical(value);
    }
    
    void OnDestroy()
    {
        // 清理
        InputMgr.Instance.StartOrCloseInputMgr(false);
    }
}
```

## 工具类 API

### MathUtil

```csharp
public static class MathUtil
{
    // 角度转换
    public static float Deg2Rad(float deg);
    public static float Rad2Deg(float rad);
    
    // 距离计算
    public static float GetObjDistanceXZ(Vector3 srcPos, Vector3 targetPos);
    public static bool CheckObjDistanceXZ(Vector3 srcPos, Vector3 targetPos, float dis);
    public static float GetObjDistanceXY(Vector3 srcPos, Vector3 targetPos);
    public static bool CheckObjDistanceXY(Vector3 srcPos, Vector3 targetPos, float dis);
    
    // 位置判断
    public static bool IsWorldPosOutScreen(Vector3 pos);
    public static bool IsInSectorRangeXZ(Vector3 pos, Vector3 forward, Vector3 targetPos, float radius, float angle);
    
    // 射线检测
    public static void RayCast(Ray ray, UnityAction<RaycastHit> callBack, float maxDistance, int layerMask);
    public static void RayCast(Ray ray, UnityAction<GameObject> callBack, float maxDistance, int layerMask);
    public static void RayCast<T>(Ray ray, UnityAction<T> callBack, float maxDistance, int layerMask);
    public static void RayCastAll(Ray ray, UnityAction<RaycastHit> callBack, float maxDistance, int layerMask);
    public static void RayCastAll(Ray ray, UnityAction<GameObject> callBack, float maxDistance, int layerMask);
    public static void RayCastAll<T>(Ray ray, UnityAction<T> callBack, float maxDistance, int layerMask);
    
    // 范围检测
    public static void OverlapBox<T>(Vector3 center, Quaternion rotation, Vector3 halfExtents, int layerMask, UnityAction<T> callBack) where T : class;
    public static void OverlapSphere<T>(Vector3 center, float radius, int layerMask, UnityAction<T> callBack) where T : class;
}
```

### TextUtil

```csharp
public static class TextUtil
{
    // 字符串分割
    public static string[] SplitStr(string str, int type = 1);
    public static int[] SplitStrToIntArr(string str, int type = 1);
    public static void SplitStrToIntArrTwice(string str, int typeOne, int typeTwo, UnityAction<int, int> callBack);
    public static void SplitStrTwice(string str, int typeOne, int typeTwo, UnityAction<string, string> callBack);
    
    // 数值转字符串
    public static string GetNumStr(int value, int len);
    public static string GetDecimalStr(float value, int len);
    public static string GetBigDataToString(int num);
    
    // 时长格式化
    public static string SecondToHMS(int s, bool egZero = false, bool isKeepLen = false, 
        string hourStr = "时", string minuteStr = "分", string secondStr = "秒");
    public static string SecondToHMS2(int s, bool egZero = false);
}
```

**分割类型说明：**
```csharp
// type参数说明
type = 1  // 分号 ;
type = 2  // 逗号 ,
type = 3  // 百分号 %
type = 4  // 冒号 :
type = 5  // 空格
type = 6  // 竖线 |
type = 7  // 下划线 _
```

### EncryptionUtil

```csharp
public static class EncryptionUtil
{
    // 获取随机密钥
    public static int GetRandomKey();
    
    // 加密数值
    public static int LockValue(int value, int key);
    public static long LockValue(long value, int key);
    
    // 解密数值
    public static int UnLoackValue(int value, int key);
    public static long UnLoackValue(long value, int key);
}
```