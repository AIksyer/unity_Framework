# 架构模式与实战示例

## 目录

- [架构设计原则](#架构设计原则)
- [常见开发模式](#常见开发模式)
- [完整功能示例](#完整功能示例)
- [性能优化技巧](#性能优化技巧)
- [扩展框架](#扩展框架)

## 架构设计原则

在游戏开发中，良好的架构设计可以显著提升代码的可维护性和可扩展性。本框架遵循以下设计原则，帮助你构建高质量的游戏项目。

### 单一职责原则

每个类应该只有一个职责，只做一件事。在框架中，每个管理器都有明确的职责范围：MonoMgr负责帧更新管理，EventCenter负责事件通信，PoolMgr负责对象池管理。这种设计使得每个模块都可以独立测试和维护。

```csharp
// 好的设计：一个类只负责一件事
public class AudioManager : BaseManager<AudioManager>
{
    // 只负责音频相关功能
    public void PlaySound(string name) { }
    public void SetVolume(float volume) { }
}

public class InputManager : BaseManager<InputManager>
{
    // 只负责输入处理
    public bool GetKeyDown(KeyCode key) { }
    public Vector2 GetAxis() { }
}

// 不好的设计：一个类负责太多事情
public class GameManager : BaseManager<GameManager>
{
    public void PlaySound(string name) { }  // 音频
    public bool GetKeyDown(KeyCode key) { } // 输入
    public void SpawnEnemy() { }            // 生成
    public void SaveGame() { }              // 存档
}
```

### 开闭原则

对扩展开放，对修改关闭。框架通过接口和事件系统实现了这一点：要添加新功能，不需要修改现有代码，只需要注册新的事件监听器或扩展现有管理器。

```csharp
// 框架提供的接口
public interface IPoolObject
{
    void ResetInfo();
}

// 用户扩展：自定义池化对象
public class Enemy : MonoBehaviour, IPoolObject
{
    public int hp;
    
    public void ResetInfo()
    {
        hp = 100;
        gameObject.SetActive(false);
    }
}

// 不需要修改PoolMgr，直接使用
Enemy enemy = PoolMgr.Instance.GetObj<Enemy>("Enemy");
```

### 依赖倒置原则

高层模块不依赖低层模块，都依赖于抽象。框架使用接口和事件进行解耦，使得各模块可以独立变化。

```csharp
// 好的设计：通过接口和事件解耦
public class DamageSystem
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<E_EventType, int>(E_EventType.E_Player_Damaged, ApplyDamage);
    }
    
    private void ApplyDamage(int damage)
    {
        player.TakeDamage(damage);
    }
}

public class UISystem
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<E_EventType, int>(E_EventType.E_Player_Damaged, UpdateHpBar);
    }
    
    private void UpdateHpBar(int damage)
    {
        hpBar.UpdateValue();
    }
}

public class AudioSystem
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<E_EventType, int>(E_EventType.E_Player_Damaged, PlayHurtSound);
    }
    
    private void PlayHurtSound(int damage)
    {
        MusicMgr.Instance.PlaySound("hurt");
    }
}

// 三个系统通过事件解耦，不需要相互引用
// 当添加新的受伤响应时，只需添加新的监听器，无需修改现有代码
```

## 常见开发模式

### 观察者模式

事件系统是观察者模式的实现，适用于一对多的通知场景。

```csharp
// 被观察者：玩家
public class Player : MonoBehaviour
{
    public int HP { get; private set; }
    public int MaxHP { get; } = 100;
    
    public void TakeDamage(int damage)
    {
        HP -= damage;
        HP = Mathf.Max(0, HP);
        
        // 通知所有观察者
        EventCenter.Instance.EventTrigger(E_EventType.E_Player_Damaged, damage);
        
        if (HP <= 0)
            EventCenter.Instance.EventTrigger(E_EventType.E_Player_Dead);
    }
    
    public void Heal(int amount)
    {
        HP += amount;
        HP = Mathf.Min(MaxHP, HP);
        EventCenter.Instance.EventTrigger(E_EventType.E_Player_Healed, amount);
    }
}

// 观察者1：血条UI
public class HealthBar : MonoBehaviour
{
    private Slider slider;
    private Text hpText;
    
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<int>(E_EventType.E_Player_Damaged, OnDamaged);
        EventCenter.Instance.AddEventListener<int>(E_EventType.E_Player_Healed, OnHealed);
    }
    
    private void OnDamaged(int damage)
    {
        UpdateDisplay();
    }
    
    private void OnHealed(int amount)
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        int hp = Player.Instance.HP;
        int maxHp = Player.Instance.MaxHP;
        slider.value = (float)hp / maxHp;
        hpText.text = $"{hp}/{maxHp}";
    }
}

// 观察者2：伤害数字
public class DamageNumber : MonoBehaviour
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<int>(E_EventType.E_Player_Damaged, ShowDamage);
    }
    
    private void ShowDamage(int damage)
    {
        // 显示飘字伤害数字
        SpawnDamageNumber(damage);
    }
}

// 观察者3：音效
public class PlayerAudio : MonoBehaviour
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<int>(E_EventType.E_Player_Damaged, PlayHurtSound);
        EventCenter.Instance.AddEventListener(E_EventType.E_Player_Dead, PlayDeathSound);
    }
    
    private void PlayHurtSound(int damage)
    {
        MusicMgr.Instance.PlaySound("player_hurt");
    }
}
```

### 对象池模式

对象池模式用于复用对象，减少创建和销毁的开销。

```csharp
// 子弹系统完整示例
public class BulletPool
{
    // 预热：游戏开始时创建一定数量的子弹
    public void Prewarm(int count = 10)
    {
        for (int i = 0; i < count; i++)
        {
            Bullet bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");
            PoolMgr.Instance.PushObj(bullet);
        }
    }
    
    // 发射子弹
    public void Fire(Bullet bulletPrefab, Vector3 position, Vector3 direction)
    {
        Bullet bullet = PoolMgr.Instance.GetObj<Bullet>("Bullet");
        bullet.transform.position = position;
        bullet.transform.rotation = Quaternion.LookRotation(direction);
        bullet.gameObject.SetActive(true);
        bullet.Launch(direction);
    }
    
    // 回收子弹
    public void回收(Bullet bullet)
    {
        PoolMgr.Instance.PushObj(bullet);
    }
}

// 粒子特效池
public class EffectPool
{
    public void PlayEffect(string effectName, Vector3 position)
    {
        GameObject effect = PoolMgr.Instance.GetObj(effectName);
        effect.transform.position = position;
        effect.SetActive(true);
        
        // 获取粒子组件
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            
            // 播放完成后自动回收
            StartCoroutine(回收AfterPlay(ps, effect));
        }
    }
    
    private IEnumerator 回收AfterPlay(ParticleSystem ps, GameObject effect)
    {
        yield return new WaitWhile(() => ps.isPlaying);
        PoolMgr.Instance.PushObj(effect);
    }
}
```

### 状态机模式

状态机用于管理游戏对象的多种状态。

```csharp
// 简单的状态机实现
public class StateMachine
{
    private Dictionary<Type, IState> states = new Dictionary<Type, IState>();
    private IState currentState;
    
    public void AddState<T>(T state) where T : IState
    {
        states[typeof(T)] = state;
    }
    
    public void ChangeState<T>() where T : IState
    {
        Type stateType = typeof(T);
        
        if (currentState != null)
        {
            currentState.OnExit();
        }
        
        if (states.TryGetValue(stateType, out IState newState))
        {
            currentState = newState;
            currentState.OnEnter();
        }
    }
    
    public void Update()
    {
        currentState?.OnUpdate();
    }
}

// 状态接口
public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}

// 具体状态
public class IdleState : IState
{
    private Enemy enemy;
    
    public IdleState(Enemy enemy)
    {
        this.enemy = enemy;
    }
    
    public void OnEnter()
    {
        enemy.animator.Play("Idle");
    }
    
    public void OnUpdate()
    {
        // 检测是否应该进入巡逻状态
        if (enemy.DetectPlayer())
        {
            enemy.ChangeState<ChaseState>();
        }
        else if (enemy.ShouldPatrol())
        {
            enemy.ChangeState<PatrolState>();
        }
    }
    
    public void OnExit()
    {
        // 清理状态
    }
}

public class ChaseState : IState
{
    private Enemy enemy;
    
    public void OnEnter()
    {
        enemy.animator.Play("Run");
    }
    
    public void OnUpdate()
    {
        enemy.MoveToPlayer();
        
        if (enemy.CanAttack())
        {
            enemy.ChangeState<AttackState>();
        }
        else if (!enemy.DetectPlayer())
        {
            enemy.ChangeState<IdleState>();
        }
    }
    
    public void OnExit() { }
}

// 敌人使用状态机
public class Enemy : MonoBehaviour
{
    public StateMachine stateMachine;
    public Animator animator;
    public float attackRange = 2f;
    
    private void Awake()
    {
        stateMachine = new StateMachine();
        stateMachine.AddState(new IdleState(this));
        stateMachine.AddState(new ChaseState(this));
        stateMachine.AddState(new AttackState(this));
    }
    
    private void Update()
    {
        stateMachine.Update();
    }
    
    public void ChangeState<T>() where T : IState
    {
        stateMachine.ChangeState<T>();
    }
    
    public bool DetectPlayer()
    {
        // 检测玩家
        return Vector3.Distance(transform.position, Player.Instance.transform.position) < 10f;
    }
}
```

## 完整功能示例

### 敌人AI系统

```csharp
// 敌人管理器 - 管理所有敌人
public class EnemyManager : BaseManager<EnemyManager>
{
    private List<Enemy> activeEnemies = new List<Enemy>();
    private List<Enemy> deadEnemies = new List<Enemy>();
    
    public void RegisterEnemy(Enemy enemy)
    {
        activeEnemies.Add(enemy);
    }
    
    public void UnregisterEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        deadEnemies.Add(enemy);
    }
    
    public void OnEnemyDead(Enemy enemy)
    {
        // 增加击杀计数
        EventCenter.Instance.EventTrigger(E_EventType.E_Enemy_Dead, enemy.xpValue);
        
        // 延迟回收
        PoolMgr.Instance.PushObj(enemy);
    }
}

// 敌人基类
public abstract class Enemy : MonoBehaviour
{
    public int hp = 100;
    public int maxHp = 100;
    public int xpValue = 10;
    public float moveSpeed = 3f;
    
    protected StateMachine stateMachine;
    protected Animator animator;
    protected Transform playerTransform;
    
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        playerTransform = Player.Instance.transform;
        stateMachine = new StateMachine();
        InitStates();
    }
    
    protected abstract void InitStates();
    
    protected void Update()
    {
        stateMachine?.Update();
    }
    
    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        animator?.SetTrigger("Hit");
        
        EventCenter.Instance.EventTrigger(E_EventType.E_Enemy_Damaged, damage);
        
        if (hp <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        animator?.SetTrigger("Dead");
        EventCenter.Instance.EventTrigger(E_EventType.E_Enemy_Dead, this);
        
        // 延迟回收
        Invoke(nameof(回收), 3f);
    }
    
    private void 回收()
    {
        PoolMgr.Instance.PushObj(this);
    }
    
    public bool CanAttack()
    {
        return Vector3.Distance(transform.position, playerTransform.position) < 2f;
    }
}

// 巡逻敌人
public class PatrolEnemy : Enemy
{
    private Vector3[] patrolPoints;
    private int currentPointIndex;
    
    protected override void InitStates()
    {
        stateMachine.AddState(new PatrolEnemy.IdleState(this));
        stateMachine.AddState(new PatrolEnemy.ChaseState(this));
        stateMachine.AddState(new PatrolEnemy.AttackState(this));
        stateMachine.ChangeState<IdleState>();
    }
    
    public void SetPatrolPoints(Vector3[] points)
    {
        patrolPoints = points;
    }
    
    // 内部状态类
    public class IdleState : IState
    {
        private PatrolEnemy enemy;
        
        public IdleState(PatrolEnemy enemy) { this.enemy = enemy; }
        
        public void OnEnter()
        {
            enemy.animator.Play("Idle");
        }
        
        public void OnUpdate()
        {
            if (enemy.DetectPlayer())
                enemy.ChangeState<ChaseState>();
            else if (enemy.CanReachNextPoint())
                enemy.ChangeState<MoveState>();
        }
        
        public void OnExit() { }
    }
    
    public class MoveState : IState
    {
        private PatrolEnemy enemy;
        
        public MoveState(PatrolEnemy enemy) { this.enemy = enemy; }
        
        public void OnEnter()
        {
            enemy.animator.Play("Run");
        }
        
        public void OnUpdate()
        {
            enemy.MoveTo(enemy.patrolPoints[enemy.currentPointIndex]);
            
            if (enemy.HasReachedDestination())
            {
                enemy.currentPointIndex = (enemy.currentPointIndex + 1) % enemy.patrolPoints.Length;
                enemy.ChangeState<IdleState>();
            }
        }
        
        public void OnExit() { }
    }
    
    public class ChaseState : IState
    {
        private PatrolEnemy enemy;
        
        public ChaseState(PatrolEnemy enemy) { this.enemy = enemy; }
        
        public void OnEnter()
        {
            enemy.animator.Play("Run");
        }
        
        public void OnUpdate()
        {
            enemy.MoveTo(enemy.playerTransform.position);
            
            if (enemy.CanAttack())
                enemy.ChangeState<AttackState>();
            else if (!enemy.DetectPlayer())
                enemy.ChangeState<IdleState>();
        }
        
        public void OnExit() { }
    }
    
    public class AttackState : IState
    {
        private PatrolEnemy enemy;
        
        public AttackState(PatrolEnemy enemy) { this.enemy = enemy; }
        
        public void OnEnter()
        {
            enemy.animator.Play("Attack");
        }
        
        public void OnUpdate()
        {
            // 攻击逻辑
        }
        
        public void OnExit() { }
    }
    
    // 辅助方法
    private bool DetectPlayer()
    {
        return Vector3.Distance(transform.position, playerTransform.position) < 8f;
    }
    
    private void MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
    
    private bool CanReachNextPoint()
    {
        return patrolPoints != null && patrolPoints.Length > 0;
    }
    
    private bool HasReachedDestination()
    {
        return Vector3.Distance(transform.position, patrolPoints[currentPointIndex]) < 0.5f;
    }
}
```

### 任务系统

```csharp
// 任务数据
[System.Serializable]
public class Quest
{
    public int id;
    public string title;
    public string description;
    public QuestType type;
    public int targetId;
    public int targetCount;
    public int currentProgress;
    public QuestReward reward;
    public bool isCompleted;
    public bool isRewarded;
}

[System.Serializable]
public class QuestReward
{
    public int exp;
    public int gold;
    public string[] itemIds;
    public int[] itemCounts;
}

// 任务管理器
public class QuestManager : BaseManager<QuestManager>
{
    private Dictionary<int, Quest> activeQuests = new Dictionary<int, Quest>();
    private Dictionary<int, Quest> completedQuests = new Dictionary<int, Quest>();
    
    public void AcceptQuest(int questId)
    {
        Quest quest = QuestDatabase.GetQuest(questId);
        if (quest != null && !activeQuests.ContainsKey(questId))
        {
            quest.currentProgress = 0;
            quest.isCompleted = false;
            activeQuests.Add(questId, quest);
            
            EventCenter.Instance.EventTrigger(E_EventType.E_Quest_Accepted, quest);
            ShowQuestUI(quest);
        }
    }
    
    public void UpdateQuestProgress(QuestType type, int targetId, int count = 1)
    {
        foreach (var quest in activeQuests.Values)
        {
            if (quest.type == type && quest.targetId == targetId && !quest.isCompleted)
            {
                quest.currentProgress += count;
                
                if (quest.currentProgress >= quest.targetCount)
                {
                    quest.currentProgress = quest.targetCount;
                    quest.isCompleted = true;
                    EventCenter.Instance.EventTrigger(E_EventType.E_Quest_Completed, quest);
                    ShowQuestCompleteUI(quest);
                }
                else
                {
                    EventCenter.Instance.EventTrigger(E_EventType.E_Quest_Progress, quest);
                }
                
                UpdateQuestUI(quest);
            }
        }
    }
    
    public void CompleteQuest(int questId)
    {
        if (activeQuests.TryGetValue(questId, out Quest quest))
        {
            if (quest.isCompleted && !quest.isRewarded)
            {
                // 发放奖励
                GrantReward(quest.reward);
                quest.isRewarded = true;
                
                activeQuests.Remove(questId);
                completedQuests.Add(questId, quest);
                
                EventCenter.Instance.EventTrigger(E_EventType.E_Quest_Rewarded, quest);
            }
        }
    }
    
    private void GrantReward(QuestReward reward)
    {
        if (reward.exp > 0)
            Player.Instance.AddExp(reward.exp);
        
        if (reward.gold > 0)
            Player.Instance.AddGold(reward.gold);
        
        for (int i = 0; i < reward.itemIds.Length; i++)
        {
            InventoryMgr.Instance.AddItem(reward.itemIds[i], reward.itemCounts[i]);
        }
    }
    
    private void ShowQuestUI(Quest quest) { /* UI显示 */ }
    private void ShowQuestCompleteUI(Quest quest) { /* 任务完成UI */ }
    private void UpdateQuestUI(Quest quest) { /* 更新UI */ }
}

// 使用示例
public class QuestExample : MonoBehaviour
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener(E_EventType.E_Enemy_Dead, OnEnemyKilled);
        EventCenter.Instance.AddEventListener(E_EventType.E_Item_Collected, OnItemCollected);
    }
    
    private void OnEnemyKilled(E_EventType type, Enemy enemy)
    {
        // 更新击杀任务进度
        QuestManager.Instance.UpdateQuestProgress(QuestType.KillEnemy, enemy.id);
    }
    
    private void OnItemCollected(E_EventType type, string itemId)
    {
        // 更新收集任务进度
        QuestManager.Instance.UpdateQuestProgress(QuestType.CollectItem, int.Parse(itemId));
    }
}
```

## 性能优化技巧

### 对象池优化

对象池是游戏性能优化的关键，正确使用可以显著减少GC。

```csharp
public class ObjectPoolExample : MonoBehaviour
{
    // 预热对象池
    private void WarmupPools()
    {
        int[] warmupCounts = new int[] { 10, 20, 5, 50 }; // 子弹、敌人、特效、粒子
        string[] poolNames = new string[] { "Bullet", "Enemy", "Effect", "Particle" };
        
        for (int i = 0; i < poolNames.Length; i++)
        {
            for (int j = 0; j < warmupCounts[i]; j++)
            {
                var obj = PoolMgr.Instance.GetObj(poolNames[i]);
                PoolMgr.Instance.PushObj(obj);
            }
        }
        
        Debug.Log("对象池预热完成");
    }
    
    // 动态调整池大小
    public void AdjustPoolSize(string poolName, int newSize)
    {
        // 如果当前池中对象少于需要的数量，创建更多
        int currentCount = GetPoolCurrentCount(poolName);
        
        while (currentCount < newSize)
        {
            var obj = PoolMgr.Instance.GetObj(poolName);
            PoolMgr.Instance.PushObj(obj);
            currentCount++;
        }
    }
    
    private int GetPoolCurrentCount(string poolName)
    {
        // 获取池中当前对象数量
        return 0; // 实际实现需要根据PoolMgr的内部逻辑
    }
}
```

### 减少Update开销

```csharp
public class UpdateOptimization : MonoBehaviour
{
    // 问题：每帧执行大量计算
    private void Update()
    {
        // 大量计算
        for (int i = 0; i < 1000; i++)
        {
            // 计算
        }
    }
    
    // 优化1：使用定时器
    private float timer = 0f;
    private float interval = 0.1f; // 每100ms执行一次
    
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            HeavyCalculation();
        }
    }
    
    // 优化2：使用分帧处理
    private int frameIndex = 0;
    private int itemsPerFrame = 10;
    
    private void Update()
    {
        for (int i = 0; i < itemsPerFrame; i++)
        {
            int index = frameIndex * itemsPerFrame + i;
            if (index >= allItems.Count) break;
            ProcessItem(allItems[index]);
        }
        frameIndex++;
    }
    
    // 优化3：使用事件驱动代替Update轮询
    private void OnEnable()
    {
        // 替代在Update中检查状态
        EventCenter.Instance.AddEventListener(E_EventType.E_Player_Jump, OnPlayerJump);
    }
    
    private void OnPlayerJump()
    {
        // 响应跳跃事件
    }
}
```

### 资源管理优化

```csharp
public class ResourceOptimization : MonoBehaviour
{
    // 批量加载资源
    private void LoadAllResources()
    {
        string[] prefabs = new string[] { "Player", "Enemy", "Bullet", "Effect" };
        
        for (int i = 0; i < prefabs.Length; i++)
        {
            // 预加载到对象池
            for (int j = 0; j < 5; j++)
            {
                var obj = PoolMgr.Instance.GetObj(prefabs[i]);
                PoolMgr.Instance.PushObj(obj);
            }
        }
    }
    
    // 异步加载大型资源
    private IEnumerator LoadLargeAsset()
    {
        UIMgr.Instance.ShowPanel<LoadingPanel>();
        
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync("GameLevel");
        asyncOp.allowSceneActivation = false;
        
        while (!asyncOp.isDone)
        {
            float progress = asyncOp.progress;
            // 更新加载进度UI
            
            if (progress >= 0.9f)
            {
                asyncOp.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        UIMgr.Instance.HidePanel<LoadingPanel>();
    }
}
```

## 扩展框架

### 自定义管理器

```csharp
// 创建新的管理器
public class MyCustomManager : BaseManager<MyCustomManager>
{
    private MyCustomManager() { }
    
    public void MyCustomFunction()
    {
        // 自定义功能
    }
}

// 或者继承SingletonAutoMono
public class MyMonoManager : SingletonAutoMono<MyMonoManager>
{
    private void Update()
    {
        // 每帧更新
    }
}
```

### 自定义事件类型

在E_EventType.cs中添加新的事件类型：

```csharp
public enum E_EventType 
{
    // 现有事件...
    
    // 新增自定义事件
    E_Custom_Event_1,
    E_Custom_Event_2,
    E_Custom_Data_Event,  // 带数据的事件
}
```

### 自定义工具类

```csharp
// 在Util目录下创建新工具类
public static class MyUtil
{
    public static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{minutes:D2}:{secs:D2}";
    }
    
    public static T RandomFromList<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
            return default(T);
        
        return list[Random.Range(0, list.Count)];
    }
}
```

### 扩展现有管理器

```csharp
// 通过继承扩展管理器功能
public class ExtendedPoolMgr : PoolMgr
{
    // 添加新方法
    public void PrewarmAll()
    {
        // 预热所有池
    }
    
    // 重写现有方法
    public new void PushObj(GameObject obj)
    {
        // 添加额外逻辑
        base.PushObj(obj);
    }
}
```

这个框架设计灵活，可以根据项目需求进行各种扩展。记住一个原则：尽量通过组合而非继承来扩展功能，这样代码会更加灵活和易于维护。