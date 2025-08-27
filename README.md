# 繁星Demo项目

## 项目概述

繁星Demo是一款开放世界RPG游戏的Demo版本，采用写实3D国风美术风格，核心特色是双职业自由选择系统和开放式任务交互。

### 基本信息
- **项目名称**：繁星Demo
- **开发周期**：10周（2.5个月）
- **团队规模**：10人程序组
- **主程序**：黄畅修
- **技术栈**：Unity 2022.3.62f1 LTS + C#
- **目标平台**：PC端游（Windows/Mac/Linux）
- **架构状态**：完整架构100%完成，编辑器工具100%完成
- **代码统计**：33个核心文件，12000+行代码

### 核心功能

#### 架构系统（100%完成）
- Global统一入口系统[已完成] - 完全解耦合的统一访问架构
- 事件驱动通信系统[已完成] - 类型安全的事件系统
- 8个核心管理器[已完成] - 游戏、UI、数据、事件、系统、职业、技能、属性
- 9个游戏系统架构[已完成] - 玩家、NPC、任务、战斗、经济、种田、商店等

#### 编辑器工具（100%完成）
- 策划配置工具[已完成] - 可视化NPC、任务、商店、作物、技能配置编辑器
- 资源命名检查工具[已完成] - 自动化资源命名规范检查和质量保证
- 自动化构建工具[已完成] - 一键多平台构建和发布管理
- 编辑器基础框架[已完成] - 统一的编辑器工具开发基础架构

#### 游戏功能（架构完成，具体实现进行中）
- 双职业自由选择系统（商人、修士）[架构完成]
- 完整的玩家属性和技能系统[架构完成]
- 种田系统（种植、收获、销售）[架构完成]
- 战斗系统（野外敌对生物、AI）[架构完成]
- 城镇交互系统（7个功能NPC）[架构完成]
- 任务系统（接取、发布、完成）[架构完成]
- 经济系统（金钱、经验、等级）[架构完成]
- 商铺租赁系统[架构完成]

## 快速开始

### 环境要求
- Unity 2022.3.62f1 LTS
- Visual Studio 2022 Community
- Git + Git LFS
- Windows 10/11 或 macOS 12+

### 开发工具配置
- **Unity插件**：DOTween、TextMeshPro、Unity Analytics
- **推荐插件**：Odin Inspector、Cinemachine
- **代码编辑器**：Visual Studio 2022 Community

### 编辑器工具快速上手
```csharp
// 在Unity编辑器菜单栏中找到"FanXing"菜单
// 配置编辑器/策划配置工具 - 可视化配置NPC、任务等
// 质量工具/资源命名检查 - 自动检查资源命名规范
// 构建工具/构建管理器 - 一键构建和发布管理

// 策划配置工具使用示例
// 1. 打开策划配置工具
// 2. 选择NPC配置标签页
// 3. 点击"新建NPC"创建配置
// 4. 编辑NPC属性和对话
// 5. 点击"保存配置"和"导出JSON"
```

### Global架构快速上手
```csharp
// 管理器访问 - 使用Global统一入口
Global.Data.SaveGameData();
Global.UI.ShowPanel(UIPanelType.Inventory);
Global.Game.ChangeGameState(GameState.Playing);

// 系统访问 - 类型安全的系统访问
Global.Player.AddExperience(100);
Global.Quest.CompleteQuest(questId);
Global.NPC.StartDialogue(npcId);

// 事件系统 - 类型安全的事件通信
Global.TriggerEvent(Global.Events.Player.LEVEL_UP, args);
Global.RegisterEvent<PlayerLevelUpEventArgs>(
    Global.Events.Player.LEVEL_UP,
    OnPlayerLevelUp
);

// 禁止的用法 - 避免直接访问
// DataManager.Instance.SaveGameData(); // ❌ 禁止
// EventManager.Instance.TriggerEvent("OnPlayerLevelUp", args); // ❌ 禁止
```

## 项目结构

```
FX-Demo/
├── Assets/
│   ├── Scripts/              # 脚本文件（33个核心文件，12000+行代码，100%完成）
│   │   ├── Data/             # 数据类和配置（7个文件，100%完成）
│   │   │   ├── ConfigClasses.cs      # 配置类集合（586行）
│   │   │   ├── Enums.cs              # 25个枚举定义（400行）
│   │   │   ├── EventArgs.cs          # 事件参数类（600行）
│   │   │   ├── EventConstants.cs     # 事件常量定义（300行）
│   │   │   ├── ItemData.cs           # 物品数据类（500行）
│   │   │   ├── PlayerAttributes.cs   # 属性系统（300行）
│   │   │   └── PlayerData.cs         # 玩家数据类（300行）
│   │   ├── Editor/           # 编辑器工具（4个文件，100%完成）
│   │   │   ├── AssetNamingValidator.cs # 资源命名检查（300行）
│   │   │   ├── BuildAutomation.cs    # 自动化构建（378行）
│   │   │   ├── ConfigEditorWindow.cs # 策划配置工具（304行）
│   │   │   └── FXEditorBase.cs       # 编辑器基类（200行）
│   │   ├── Examples/         # 示例代码（1个文件，100%完成）
│   │   │   └── EventSystemExample.cs # 事件系统使用示例（400行）
│   │   ├── Managers/         # 管理器类（8个文件，100%完成）
│   │   │   ├── AttributeManager.cs   # 属性管理器（500行）
│   │   │   ├── DataManager.cs        # 数据管理器（395行）
│   │   │   ├── EventManager.cs       # 事件管理器（300行）
│   │   │   ├── GameManager.cs        # 游戏总控制器（320行）
│   │   │   ├── ProfessionManager.cs  # 职业管理器（500行）
│   │   │   ├── SkillManager.cs       # 技能管理器（600行）
│   │   │   ├── SystemManager.cs      # 系统管理器（300行）
│   │   │   └── UIManager.cs          # UI界面管理器（363行）
│   │   ├── Player/           # 玩家组件（1个文件，100%完成）
│   │   │   └── Player.cs             # 玩家组件（400行）
│   │   ├── Systems/          # 系统逻辑（9个文件，100%完成）
│   │   │   ├── BaseGameSystem.cs     # 系统基类（360行）
│   │   │   ├── CombatSystem.cs       # 战斗系统（700行）
│   │   │   ├── EconomySystem.cs      # 经济系统（600行）
│   │   │   ├── FarmingSystem.cs      # 种田系统（800行）
│   │   │   ├── IGameSystem.cs        # 系统接口（50行）
│   │   │   ├── NPCSystem.cs          # NPC系统（800行）
│   │   │   ├── PlayerSystem.cs       # 玩家系统（1100行）
│   │   │   ├── QuestSystem.cs        # 任务系统（900行）
│   │   │   └── ShopSystem.cs         # 商店系统（700行）
│   │   ├── Global.cs         # Global统一入口（390行，100%完成）
│   │   └── Scripts.meta      # Unity元数据文件
│   └── Scripts.meta          # Unity元数据文件
├── Library/                  # Unity库文件（自动生成，不纳入版本控制）
│   ├── AnnotationManager     # 注释管理器
│   ├── ArtifactDB           # 构件数据库
│   ├── Artifacts/           # 构件缓存
│   ├── Bee/                 # Unity构建系统
│   ├── PackageCache/        # 包缓存
│   ├── PackageManager/      # 包管理器
│   ├── ScriptAssemblies/    # 脚本程序集
│   ├── ShaderCache/         # 着色器缓存
│   ├── StateCache/          # 状态缓存
│   └── ...                  # 其他Unity生成文件
├── Logs/                     # Unity日志文件（自动生成）
│   ├── AssetImportWorker0.log # 资源导入日志
│   ├── Packages-Update.log   # 包更新日志
│   └── shadercompiler-*.log  # 着色器编译日志
├── Packages/                 # Unity包管理
│   ├── manifest.json         # 包依赖清单
│   └── packages-lock.json    # 包版本锁定文件
├── ProjectSettings/          # Unity项目设置
│   ├── AudioManager.asset    # 音频管理器设置
│   ├── DynamicsManager.asset # 物理管理器设置
│   ├── EditorBuildSettings.asset # 编辑器构建设置
│   ├── GraphicsSettings.asset # 图形设置
│   ├── InputManager.asset    # 输入管理器设置
│   ├── ProjectSettings.asset # 项目设置
│   ├── ProjectVersion.txt    # Unity版本信息
│   ├── QualitySettings.asset # 质量设置
│   └── ...                   # 其他项目设置文件
├── Temp/                     # Unity临时文件（自动生成，不纳入版本控制）
│   ├── ProcessJobs/          # 进程作业
│   ├── UnityLockfile         # Unity锁文件
│   └── ...                   # 其他临时文件
├── UserSettings/             # 用户设置（不纳入版本控制）
│   ├── EditorUserSettings.asset # 编辑器用户设置
│   └── Search.settings       # 搜索设置
├── docs/                     # 技术文档
│   ├── api-reference.md      # API参考文档
│   ├── architecture.md       # 架构设计文档
│   ├── coding-standards.md   # 代码规范
│   ├── deployment.md         # 部署说明
│   ├── git-commit-standards.md # GIT提交规范
│   └── global-api-reference.md # Global API参考文档
└── README.md                 # 项目说明文档
```

## 开发规范

### 代码规范
- 类名：PascalCase（如：PlayerManager）
- 方法名：PascalCase（如：GetPlayerData）
- 变量名：camelCase（如：currentHealth）
- 常量：UPPER_CASE（如：MAX_LEVEL）
- 私有字段：下划线前缀（如：_playerData）

### Git工作流程
1. 创建功能分支：`git checkout -b feature/功能名称`
2. 开发和提交：`git add . && git commit -m "feat: 功能描述"`
3. 推送分支：`git push origin feature/功能名称`
4. 创建Pull Request进行代码审查
5. 合并到主分支

### 提交信息规范
- `feat`: 新功能
- `fix`: Bug修复
- `docs`: 文档更新
- `style`: 代码格式调整
- `refactor`: 代码重构
- `test`: 测试相关
- `chore`: 构建过程或辅助工具变动

## 团队协作

### 工作时间
- 主要工作时间：晚上19:00-23:00
- 每日签到：19:00-19:15（飞书群）
- 每日总结：23:00-23:15（飞书群）

### 沟通渠道
- **飞书群**：日常沟通、问题求助、进度同步
- **GitHub Issues**：任务管理和Bug追踪
- **GitHub Projects**：项目看板管理
- **GitHub Wiki**：技术文档和开发规范

### 会议安排
- 周日晚20:00-21:00：周进度同步会议
- 周三晚21:00-21:30：技术讨论会
- 重要节点：里程碑验收会议

## 里程碑

### 里程碑1（第2周末）：架构验证 [100%完成]
- [x] 基础框架可运行
- [x] 核心管理器功能正常
- [x] 团队开发环境统一

**完成情况**：完整游戏架构已100%实现，包括Global统一入口、8个核心管理器、9个游戏系统架构、完整的事件驱动通信系统。29个核心文件，10000+行代码。

### 里程碑2（第4周末）：系统架构 [100%完成]
- [x] 职业系统架构完整可用
- [x] 所有游戏系统架构完成
- [x] Global事件系统统一完成

**完成情况**：所有游戏系统架构已100%完成，包括玩家系统、NPC系统、任务系统、战斗系统、经济系统、种田系统、商店系统等。Global事件系统统一架构完成，实现完全解耦合设计。

### 里程碑3（第6周末）：主要功能
- [ ] 种田和战斗系统完成
- [ ] 经济系统运行正常
- [ ] 核心玩法体验完整

### 里程碑4（第8周末）：集成完成
- [ ] 所有系统集成稳定
- [ ] UI交互体验良好
- [ ] 存档功能正常

### 里程碑5（第10周末）：Demo发布
- [ ] 完整Demo可发布
- [ ] 文档齐全
- [ ] 演示准备就绪

## 当前开发状态

### 已完成 (100%完成)
- **核心架构**：完整的管理器体系和系统架构（33个文件，12000+行代码）
- **Global统一入口**：完整的Global事件系统统一架构（390行代码，100%完成）
- **事件系统**：强大的事件通信机制（事件常量、类型安全、完整事件参数支持）
- **玩家系统**：双职业、技能、属性系统（完整实现）
- **游戏系统**：9个核心游戏系统架构（PlayerSystem、NPCSystem、QuestSystem等）
- **数据管理**：完整的数据结构和序列化（25个枚举，7个数据类）
- **编辑器工具**：完整的开发工具链（策划配置、资源检查、自动构建、基础框架）
- **代码规范**：代码质量和文档（100%遵循规范）
- **解耦合架构**：完全消除模块间直接依赖，实现松耦合设计

### 进行中
- **Unity项目**：实际Unity项目的创建和配置
- **UI实现**：基于UIManager的具体界面实现
- **系统具体实现**：各游戏系统的具体功能实现

### 待开始
- **场景设计**：游戏场景和环境搭建
- **资源制作**：美术资源和音频资源
- **集成测试**：系统间的完整集成测试

## 技术文档

### 项目文档
- [架构设计文档](docs/architecture.md) - 完整的系统架构设计
- [代码规范](docs/coding-standards.md) - 严格的编码标准
- [API参考文档](docs/api-reference.md) - 详细的API说明
- [GlobalAPI参考文档](docs/Global-api-reference.md) - Global统一入口详细的API说明
- [GIT提交规范](docs/git-commit-standards.md) - 标准的GIT提交规范
- [部署说明](docs/deployment.md) - 项目部署指南
  
### 技术特点
- **Global统一入口**：完整的Global事件系统统一架构，消除模块间直接依赖
- **完全解耦合**：通过Global统一入口实现完全的模块解耦合设计
- **事件驱动**：基于事件常量的类型安全事件通信机制
- **编辑器工具链**：完整的开发工具支持，包括策划配置、资源检查、自动构建
- **模块化设计**：清晰的模块划分，支持10人团队并行协作开发
- **数据驱动**：配置文件和数据分离设计，支持策划灵活调整
- **高质量代码**：严格的编码规范和100%完整文档覆盖
- **自动化流程**：资源命名检查、构建发布、质量保证全自动化

## 联系方式

- **项目总负责人**：[黄畅修]
- **技术支持**：繁星技术讨论群
- **问题反馈**：GitHub Issues

## 许可证

本项目采用 [MIT] 许可证，详情请查看 LICENSE 文件。


ddd