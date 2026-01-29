// SimpleGame.cs - 完整示例：使用框架构建简单游戏
// 将此脚本放在Game/Scripts目录下，创建一个空场景运行

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame
{
    // ==================== 游戏数据 ====================
    
    [System.Serializable]
    public class GameData
    {
        public int level = 1;
        public int score = 0;
        public int hp = 100;
        public int maxHp = 100;
        public float gameTime = 0;
        public bool isPaused = false;
    }
    
    // ==================== 游戏管理器 ====================
    
    /// <summary>
    /// 游戏主管理器 - 使用单例模式
    /// </summary>
    public class GameManager : SingletonAutoMono<GameManager>
    {
        public GameData data = new GameData();
        
        private bool isGameOver = false;
        private float spawnTimer = 0;
        private float spawnInterval = 2f;
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            // 初始化系统
            InitGame();
        }
        
        private void Update()
        {
            if (isGameOver || data.isPaused) return;
            
            // 更新游戏时间
            data.gameTime += Time.deltaTime;
            
            // 生成敌人
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0;
                SpawnEnemy();
            }
            
            // 更新UI
            UpdateGameUI();
        }
        
        private void InitGame()
        {
            data = new GameData();
            isGameOver = false;
            spawnTimer = 0;
            
            // 注册事件
            EventCenter.Instance.AddEventListener(E_GameEvent.E_Enemy_Dead, OnEnemyDead);
            EventCenter.Instance.AddEventListener(E_GameEvent.E_Player_Damaged, OnPlayerDamaged);
            EventCenter.Instance.AddEventListener(E_GameEvent.E_Player_Dead, OnPlayerDead);
            EventCenter.Instance.AddEventListener(E_GameEvent.E_Score_Changed, OnScoreChanged);
            
            // 注册帧更新
            MonoMgr.Instance.AddUpdateListener(OnUpdate);
            
            Debug.Log("游戏初始化完成");
        }
        
        private void OnUpdate()
        {
            // 每帧执行的逻辑
        }
        
        // 生成敌人
        private void SpawnEnemy()
        {
            if (data.level > 5) spawnInterval = 1.5f;
            if (data.level > 10) spawnInterval = 1f;
            
            Vector3 spawnPos = new Vector3(Random.Range(-8f, 8f), 0, 10f);
            EnemyManager.Instance.SpawnEnemy(spawnPos);
        }
        
        // 敌人死亡
        private void OnEnemyDead(int score)
        {
            data.score += score * data.level;
            
            // 升级逻辑
            if (data.score >= data.level * 500)
            {
                data.level++;
                MusicMgr.Instance.PlaySound("levelup");
                EventCenter.Instance.EventTrigger(E_GameEvent.E_Level_Up, data.level);
            }
        }
        
        // 玩家受伤
        private void OnPlayerDamaged(int damage)
        {
            data.hp -= damage;
            if (data.hp < 0) data.hp = 0;
            
            if (data.hp <= 0)
            {
                EventCenter.Instance.EventTrigger(E_GameEvent.E_Player_Dead);
            }
        }
        
        // 玩家死亡
        private void OnPlayerDead()
        {
            if (isGameOver) return;
            isGameOver = true;
            
            Debug.Log($"游戏结束！最终得分: {data.score}, 等级: {data.level}");
            
            // 显示游戏结束面板
            UIMgr.Instance.ShowPanel<GameOverPanel>(E_UILayer.Top, (panel) =>
            {
                panel.ShowResult(data.score, data.level);
            });
            
            // 停止生成敌人
            spawnTimer = float.MaxValue;
        }
        
        // 分数变化
        private void OnScoreChanged(int newScore)
        {
            data.score = newScore;
        }
        
        // 暂停游戏
        public void TogglePause()
        {
            data.isPaused = !data.isPaused;
            Time.timeScale = data.isPaused ? 0 : 1;
            MusicMgr.Instance.PlayOrPauseSound(!data.isPaused);
            
            if (data.isPaused)
            {
                UIMgr.Instance.ShowPanel<PausePanel>(E_UILayer.System);
            }
        }
        
        // 清理游戏
        public void Cleanup()
        {
            MonoMgr.Instance.RemoveUpdateListener(OnUpdate);
            EventCenter.Instance.Clear();
            PoolMgr.Instance.ClearPool();
        }
    }
    
    // ==================== 事件类型 ====================
    
    public enum E_GameEvent
    {
        // 玩家事件
        E_Player_Damaged,
        E_Player_Dead,
        E_Player_Jump,
        
        // 敌人事件
        E_Enemy_Spawn,
        E_Enemy_Dead,
        E_Enemy_Damaged,
        
        // 游戏事件
        E_Score_Changed,
        E_Level_Up,
        E_Game_Start,
        E_Game_Pause,
        E_Game_Over,
    }
    
    // ==================== 玩家控制器 ====================
    
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 8f;
        public float jumpForce = 10f;
        public int damage = 10;
        
        private Rigidbody rb;
        private bool isGrounded = false;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        private void OnEnable()
        {
            EventCenter.Instance.AddEventListener(E_GameEvent.E_Player_Damaged, OnDamaged);
        }
        
        private void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener(E_GameEvent.E_Player_Damaged, OnDamaged);
        }
        
        private void Update()
        {
            if (GameManager.Instance.data.isPaused) return;
            
            // 移动
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            
            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
            
            // 跳跃
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
                MusicMgr.Instance.PlaySound("jump");
            }
            
            // 攻击
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
            }
            
            // 暂停
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.TogglePause();
            }
        }
        
        private void Attack()
        {
            MusicMgr.Instance.PlaySound("attack");
            
            // 攻击检测
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 2f);
            foreach (var col in hitEnemies)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
        
        private void OnDamaged(int damage)
        {
            // 受伤逻辑
            MusicMgr.Instance.PlaySound("player_hurt");
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }
    }
    
    // ==================== 敌人管理器 ====================
    
    public class EnemyManager : BaseManager<EnemyManager>
    {
        private EnemyManager() { }
        
        public Enemy SpawnEnemy(Vector3 position)
        {
            Enemy enemy = PoolMgr.Instance.GetObj<Enemy>("Enemy");
            enemy.transform.position = position;
            enemy.gameObject.SetActive(true);
            enemy.Init(GameManager.Instance.data.level);
            return enemy;
        }
        
        public void DespawnEnemy(Enemy enemy)
        {
            PoolMgr.Instance.PushObj(enemy);
        }
    }
    
    // ==================== 敌人 ====================
    
    public class Enemy : MonoBehaviour, IPoolObject
    {
        public int hp = 50;
        public int damage = 10;
        public float moveSpeed = 3f;
        public int scoreValue = 100;
        
        private Transform playerTransform;
        private bool isDead = false;
        
        private void Awake()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        public void Init(int level)
        {
            hp = 50 + level * 10;
            damage = 10 + level * 2;
            moveSpeed = 3f + level * 0.2f;
            scoreValue = 100 + level * 20;
            isDead = false;
            gameObject.SetActive(true);
        }
        
        private void Update()
        {
            if (GameManager.Instance.data.isPaused || isDead) return;
            
            // 追踪玩家
            if (playerTransform != null)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    playerTransform.position, 
                    moveSpeed * Time.deltaTime
                );
                transform.LookAt(playerTransform);
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (isDead) return;
            
            hp -= damage;
            
            if (hp <= 0)
            {
                Die();
            }
            else
            {
                // 受伤动画
                MusicMgr.Instance.PlaySound("enemy_hurt");
            }
        }
        
        private void Die()
        {
            if (isDead) return;
            isDead = true;
            
            MusicMgr.Instance.PlaySound("enemy_death");
            EventCenter.Instance.EventTrigger(E_GameEvent.E_Enemy_Dead, scoreValue);
            
            // 延迟回收
            StartCoroutine(回收Delay());
        }
        
        private IEnumerator 回收Delay()
        {
            yield return new WaitForSeconds(1f);
            EnemyManager.Instance.DespawnEnemy(this);
        }
        
        // IPoolObject接口实现
        public void ResetInfo()
        {
            hp = 0;
            damage = 0;
            isDead = true;
            gameObject.SetActive(false);
        }
    }
    
    // ==================== UI面板 ====================
    
    public class MainPanel : BasePanel
    {
        private Text scoreText;
        private Text levelText;
        private Text hpText;
        private Slider hpSlider;
        
        protected override void Awake()
        {
            base.Awake();
            
            scoreText = GetControl<Text>("ScoreText");
            levelText = GetControl<Text>("LevelText");
            hpText = GetControl<Text>("HpText");
            hpSlider = GetControl<Slider>("HpSlider");
        }
        
        public override void ShowMe()
        {
            gameObject.SetActive(true);
            UpdateUI();
        }
        
        public override void HideMe()
        {
            gameObject.SetActive(false);
        }
        
        public void UpdateUI()
        {
            var data = GameManager.Instance.data;
            scoreText.text = $"分数: {data.score}";
            levelText.text = $"等级: {data.level}";
            hpText.text = $"{data.hp}/{data.maxHp}";
            hpSlider.value = (float)data.hp / data.maxHp;
        }
    }
    
    public class GameOverPanel : BasePanel
    {
        private Text finalScoreText;
        private Text finalLevelText;
        private Button restartButton;
        private Button menuButton;
        
        protected override void Awake()
        {
            base.Awake();
            
            finalScoreText = GetControl<Text>("FinalScoreText");
            finalLevelText = GetControl<Text>("FinalLevelText");
            restartButton = GetControl<Button>("RestartButton");
            menuButton = GetControl<Button>("MenuButton");
            
            restartButton.onClick.AddListener(OnRestart);
            menuButton.onClick.AddListener(OnMenu);
        }
        
        public override void ShowMe()
        {
            gameObject.SetActive(true);
        }
        
        public override void HideMe()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowResult(int score, int level)
        {
            finalScoreText.text = $"最终得分: {score}";
            finalLevelText.text = $"最高等级: {level}";
        }
        
        private void OnRestart()
        {
            GameManager.Instance.Cleanup();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        private void OnMenu()
        {
            GameManager.Instance.Cleanup();
            // 返回主菜单
        }
    }
    
    public class PausePanel : BasePanel
    {
        private Button resumeButton;
        private Button quitButton;
        
        protected override void Awake()
        {
            base.Awake();
            
            resumeButton = GetControl<Button>("ResumeButton");
            quitButton = GetControl<Button>("QuitButton");
            
            resumeButton.onClick.AddListener(OnResume);
            quitButton.onClick.AddListener(OnQuit);
        }
        
        public override void ShowMe()
        {
            gameObject.SetActive(true);
        }
        
        public override void HideMe()
        {
            gameObject.SetActive(false);
        }
        
        private void OnResume()
        {
            GameManager.Instance.TogglePause();
            HideMe();
        }
        
        private void OnQuit()
        {
            GameManager.Instance.Cleanup();
            Application.Quit();
        }
    }
}

// ==================== 游戏启动器 ====================

public class GameLauncher : MonoBehaviour
{
    private void Awake()
    {
        // 预加载资源
        PreloadResources();
        
        // 播放背景音乐
        MusicMgr.Instance.PlayBKMusic("main_bgm");
        MusicMgr.Instance.ChangeBKMusicValue(0.5f);
        
        Debug.Log("游戏启动完成！");
    }
    
    private void PreloadResources()
    {
        // 预热对象池
        for (int i = 0; i < 10; i++)
        {
            var enemy = PoolMgr.Instance.GetObj<SimpleGame.Enemy>("Enemy");
            PoolMgr.Instance.PushObj(enemy);
        }
    }
}

// 使用说明：
// 1. 在Unity中创建一个空场景
// 2. 创建空物体，挂载GameLauncher脚本
// 3. 创建Player预制体（Tag设为Player，挂载Rigidbody和PlayerController）
// 4. 创建Enemy预制体（挂载Enemy脚本和PoolObj脚本）
// 5. 创建UI面板（MainPanel、GameOverPanel、PausePanel）
// 6. 运行游戏