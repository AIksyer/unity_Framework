# Unity 框架文档索引

本文档为Unity框架的完整使用指南，涵盖从入门到精通的所有内容。

## 文档概览

| 文档 | 说明 | 难度 |
|------|------|------|
| [README.md](README.md) | 框架概述、快速开始、各模块详解 | ⭐⭐ |
| [API_REFERENCE.md](API_REFERENCE.md) | 所有公共API的详细说明 | ⭐⭐⭐ |
| [PATTERNS_EXAMPLES.md](PATTERNS_EXAMPLES.md) | 设计模式与实战示例 | ⭐⭐⭐⭐ |
| [TROUBLESHOOTING.md](TROUBLESHOOTING.md) | 常见问题与解决方案 | ⭐ |
| [CHANGELOG.md](CHANGELOG.md) | 更新日志与附录 | ⭐ |

## 如何使用本文档

### 新手 (1-2天)

1. 从README.md开始，了解框架整体架构
2. 按照"快速开始"章节创建第一个示例
3. 学习单例模块的使用方法
4. 完成一个简单的功能模块

### 进阶 (1周)

1. 阅读所有模块的详细说明
2. 完成PATTERNS_EXAMPLES.md中的示例代码
3. 开始实际项目开发
4. 遇到问题查看TROUBLESHOOTING.md

### 精通 (2-4周)

1. 阅读API_REFERENCE.md，了解所有接口
2. 学习设计模式在实际项目中的应用
3. 根据项目需求扩展框架
4. 为框架贡献代码

## 模块对应关系

### 功能需求

| 需求 | 文档章节 | 示例代码 |
|------|----------|----------|
| 全局管理 | README.md - 单例模块 | 单例使用示例 |
| 资源加载 | README.md - 资源加载 | ResMgr/ABMgr示例 |
| 对象复用 | README.md - 对象池 | PoolMgr示例 |
| 模块通信 | README.md - 事件系统 | EventCenter示例 |
| 定时任务 | README.md - 定时器 | TimerMgr示例 |
| UI界面 | README.md - UI管理 | UIMgr示例 |
| 音效播放 | README.md - 音频管理 | MusicMgr示例 |
| 输入处理 | README.md - 输入管理 | InputMgr示例 |
| 场景切换 | README.md - 场景管理 | SceneMgr示例 |
| 工具函数 | README.md - 工具类 | MathUtil/TextUtil示例 |

### 进阶需求

| 需求 | 文档章节 |
|------|----------|
| 架构设计 | PATTERNS_EXAMPLES.md - 架构设计原则 |
| 设计模式 | PATTERNS_EXAMPLES.md - 常见开发模式 |
| 性能优化 | PATTERNS_EXAMPLES.md - 性能优化技巧 |
| 框架扩展 | PATTERNS_EXAMPLES.md - 扩展框架 |
| API详情 | API_REFERENCE.md - 各模块API |

## 快速索引

### API快速查找

```csharp
// 资源加载
ResMgr.Instance.Load<T>(path)                    // 同步加载
ResMgr.Instance.LoadAsync<T>(path, callback)    // 异步加载
ABMgr.Instance.LoadResAsync<T>(ab, res, cb)     // AB加载

// 对象池
PoolMgr.Instance.GetObj<T>(name)                 // 获取
PoolMgr.Instance.PushObj(obj)                    // 归还

// 事件
EventCenter.Instance.EventTrigger<T>(type, data) // 触发
EventCenter.Instance.AddEventListener<T>(type, cb) // 监听

// 定时器
TimerMgr.Instance.CreateTimer(...)               // 创建
TimerMgr.Instance.RemoveTimer(id)                // 移除

// UI
UIMgr.Instance.ShowPanel<T>(layer, callback)     // 显示
UIMgr.Instance.HidePanel<T>(destroy)             // 隐藏
```

### 常见问题速查

| 问题 | 位置 |
|------|------|
| 单例为null | TROUBLESHOOTING.md - 问题1 |
| 事件不触发 | TROUBLESHOOTING.md - 问题2 |
| 对象池问题 | TROUBLESHOOTING.md - 问题3 |
| 资源加载失败 | TROUBLESHOOTING.md - 问题4 |
| 定时器不执行 | TROUBLESHOOTING.md - 问题5 |
| UI控件为null | TROUBLESHOOTING.md - 问题6 |
| 内存泄漏 | TROUBLESHOOTING.md - 问题7 |

## 代码示例索引

### 完整系统示例

| 示例 | 说明 | 位置 |
|------|------|------|
| 资源管理系统 | 同步/异步加载、引用计数 | API_REFERENCE.md |
| 对象池系统 | GameObject和数据对象池 | API_REFERENCE.md |
| 事件系统 | 多模块通信解耦 | API_REFERENCE.md |
| 定时器系统 | 技能冷却、BUFF效果 | API_REFERENCE.md |
| UI面板系统 | 自动控件查找、层级管理 | API_REFERENCE.md |
| 音频系统 | 背景音乐、音效控制 | API_REFERENCE.md |
| 输入系统 | 按键映射、事件触发 | API_REFERENCE.md |
| 场景加载 | 异步加载、进度回调 | API_REFERENCE.md |

### 架构模式示例

| 示例 | 说明 | 位置 |
|------|------|------|
| 观察者模式 | 玩家受伤通知多系统 | PATTERNS_EXAMPLES.md |
| 对象池模式 | 子弹池、特效池 | PATTERNS_EXAMPLES.md |
| 状态机模式 | 敌人AI状态管理 | PATTERNS_EXAMPLES.md |
| 任务系统 | 任务进度追踪 | PATTERNS_EXAMPLES.md |
| 敌人AI系统 | 完整AI实现 | PATTERNS_EXAMPLES.md |

## 学习建议

1. **先跑通，再优化**：先用最简单的方式实现功能，后续再优化性能
2. **多看源码**：框架源码是最好的学习资料
3. **写注释**：开发时多写注释，便于后续维护
4. **用好事件**：模块间通信优先使用事件系统
5. **对象池**：频繁创建的对象一定要用对象池

## 进阶阅读

- [架构设计原则](PATTERNS_EXAMPLES.md#架构设计原则)
- [性能优化技巧](PATTERNS_EXAMPLES.md#性能优化技巧)
- [扩展框架](PATTERNS_EXAMPLES.md#扩展框架)

## 版本信息

- 当前版本: 2.0.0
- 文档更新时间: 2024年
- 框架兼容性: Unity 2019.3+

## 反馈与支持

如有问题或建议，请：

1. 查看[故障排除指南](TROUBLESHOOTING.md)
2. 阅读相关模块的源码注释
3. 在项目Issue中提出

---

**开始你的Unity开发之旅吧！**