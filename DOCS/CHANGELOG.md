# 更新日志

## 版本 2.0.0 (2024)

### 新增功能

- **对象池系统增强**
  - 支持泛型对象池
  - 添加IPoolObject接口
  - 支持对象预热
  - 池状态监控

- **事件系统升级**
  - 支持泛型事件参数
  - 添加事件类型枚举
  - 支持批量清理

- **资源管理优化**
  - AB包异步加载
  - 引用计数系统
  - 资源卸载管理

### 改进

- 单例模式线程安全增强
- 定时器精度提升
- UI控件自动查找
- 输入系统统一管理

### 修复

- 修复场景切换时的内存泄漏
- 修复对象池重复归还问题
- 修复事件监听未移除导致的bug

---

## 版本 1.5.0 (2023)

### 新增功能

- **音乐管理系统**
  - 背景音乐播放控制
  - 音效管理
  - 音量分组控制

- **输入管理系统**
  - 键盘输入映射
  - 鼠标输入支持
  - 事件驱动输入

- **工具类扩展**
  - MathUtil数学工具
  - TextUtil字符串工具
  - EncryptionUtil加密工具

---

## 版本 1.0.0 (2022)

### 首次发布

核心模块发布：

- BaseManager<T> 单例基类
- SingletonMono 单例类
- SingletonAutoMono 自动单例类
- MonoMgr 帧更新管理
- TimerMgr 定时器管理
- PoolMgr 对象池管理
- EventCenter 事件中心
- UIMgr UI管理
- ResMgr 资源管理
- ABMgr AssetBundle管理
- SceneMgr 场景管理
- MusicMgr 音频管理
- InputMgr 输入管理

---

## 升级指南

### 从 1.0 升级到 1.5

1. 更新using语句
2. 重构输入处理逻辑
3. 使用新的音乐管理API

### 从 1.5 升级到 2.0

1. 实现IPoolObject接口
2. 更新事件监听代码
3. 使用新的对象池API

---

## 路线图

### 计划功能

- **2.1.0**: 网络模块增强
- **2.2.0**: 存档系统
- **2.3.0**: AI行为树
- **3.0.0**: 完整的游戏框架

---

## 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork本仓库
2. 创建特性分支
3. 提交更改
4. 发起Pull Request

### 代码规范

- 遵循框架现有编码风格
- 添加必要的注释
- 编写测试用例
- 更新相关文档

---

## 许可证

本框架基于MIT许可证开源。

---

## 致谢

感谢所有为框架贡献代码和反馈问题的开发者。

---

# 附录

## 文件结构

```
Framework/
├── AB/                    # Asset Bundle管理
│   ├── ABMgr.cs
│   └── ABResMgr.cs
├── EditorRes/            # 编辑器资源
│   └── EditorResMgr.cs
├── EventCenter/          # 事件系统
│   ├── E_EventType.cs
│   └── EventCenter.cs
├── Input/                # 输入管理
│   ├── InputInfo.cs
│   └── InputMgr.cs
├── Mono/                 # MonoBehaviour工具
│   └── MonoMgr.cs
├── Music/                # 音频管理
│   └── MusicMgr.cs
├── Pool/                 # 对象池
│   ├── PoolMgr.cs
│   └── PoolObj.cs
├── Res/                  # 资源管理
│   └── ResMgr.cs
├── Scene/                # 场景管理
│   └── SceneMgr.cs
├── Singleton/            # 单例模式
│   ├── BaseManager.cs
│   ├── SingletonAutoMono.cs
│   └── SingletonMono.cs
├── Timer/                # 定时器
│   ├── TimerItem.cs
│   └── TimerMgr.cs
├── UI/                   # UI管理
│   ├── BasePanel.cs
│   └── UIMgr.cs
├── UWQ/                  # 网络请求
│   └── UWQResMgr.cs
├── Util/                 # 工具类
│   ├── EncryptionUtil.cs
│   ├── MathUtil.cs
│   └── TextUtil.cs
└── DOCS/                 # 文档
    ├── README.md
    ├── API_REFERENCE.md
    ├── PATTERNS_EXAMPLES.md
    ├── TROUBLESHOOTING.md
    └── CHANGELOG.md
```

## 快速开始检查清单

- [ ] Unity项目已创建
- [ ] Framework文件夹已导入
- [ ] 启动脚本已创建
- [ ] 单例管理器可正常访问
- [ ] 资源可正常加载
- [ ] 对象池已预热
- [ ] 事件系统可正常通信
- [ ] 定时器可正常工作
- [ ] UI面板可正常显示

## 推荐学习路径

1. **入门 (1-2天)**
   - 阅读README.md快速开始部分
   - 运行示例代码
   - 理解单例模式

2. **进阶 (1周)**
   - 学习所有模块的API
   - 完成PATTERNS_EXAMPLES.md中的示例
   - 开始实际项目开发

3. **精通 (2-4周)**
   - 阅读全部API文档
   - 学习架构设计模式
   - 根据项目需求扩展框架
   - 贡献代码到框架

## 常见问题索引

| 问题 | 章节 |
|------|------|
| 单例为null | 故障排除 - 问题1 |
| 事件不触发 | 故障排除 - 问题2 |
| 对象池不工作 | 故障排除 - 问题3 |
| 资源加载失败 | 故障排除 - 问题4 |
| 定时器不执行 | 故障排除 - 问题5 |
| UI控件为null | 故障排除 - 问题6 |
| 内存泄漏 | 故障排除 - 问题7 |

## 相关资源

- Unity官方文档: https://docs.unity3d.com/
- C#编程指南: https://docs.microsoft.com/zh-cn/dotnet/csharp/
- 设计模式: https://refactoring.guru/design-patterns

---

**最后更新**: 2024年
**框架版本**: 2.0.0