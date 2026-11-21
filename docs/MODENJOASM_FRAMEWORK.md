# ModenjoASM 扩展框架

> 目标游戏：Aotenjo《青天井》单人 Roguelike，兼容版本 `0.7.1.1`–`0.8.0.0`。
> 本文对应 commit `ecf73b4`（宿主机 / 示例模块拆分之后）。

## 1. 定位：扩展宿主，不是七子专用工具

ModenjoASM 是 Aotenjo 的**不修改原游戏的 C# 扩展宿主**，由三部分组成：

| 组成 | 作用 |
| --- | --- |
| 扩展 API | `ModenjoASM.IModenjoModule` 模块接口 |
| 加载器 | `ModenjoASM.ModenjoModuleLoader`，发现并初始化模块 DLL |
| 宿主入口 | `ModenjoASM.Bootstrap`，由 Lua 加载后调用 `Init` |

**七子（7nnkmj / `ModenjoASM.Seven`）只是第一个示例模块，不是框架目标。** 宿主不引用任何模块程序集；删掉 `modules/ModenjoASM.Seven.dll` 后宿主照常启动，只留下“没有模块”的警告。仓库里的 56 役、赋位计分、七子遗物全部属于示例模块目录 `src/ModenjoASM.Seven/`，不属于宿主。

因此读本项目的顺序应是：先看宿主如何提供“可被加载的扩展位”，再看七子如何作为第一个消费者使用它。

## 2. 非侵入原则

### 2.1 不 patch `Assembly-CSharp.dll`

- 仓库不分发、不修改、不重写游戏程序集。
- 两个 C# 工程只**引用**本机 `Aotenjo_Data/Managed` 下的 `Assembly-CSharp.dll`、`UnityEngine*.dll`、`Unity.TextMeshPro.dll` 等，且全部 `Private=false`（不复制进输出目录）。
- 仓库里没有 Harmony / MonoMod / Doorstop / 任何 IL 补丁代码，也没有构建后改写 DLL 的步骤。

### 2.2 不修改 prefab、场景和 Unity 资源

- 仓库不含 `.prefab`、`.unity`、AssetBundle 或 `StreamingAssets` 资源改动。
- 界面上唯一的“改动”是运行时行为：`ModenjoAsmVersionTag` 每秒扫描一次 `TextMeshProUGUI`，把 `Modded!! By-ModenjoASM` 追加到短版本号文本后。它只改内存中的 UI 文本，不碰 prefab/场景文件。
- 遗物、役种、文案都通过 C# 注册与 `mod/lang/*.json` 提供，不注入资源包。

### 2.3 唯一入口：Lua → 宿主 → 模块

```text
Aotenjo ModManager
→ mods/7nnkmj_0.001/script/init.lua
→ CS.System.Reflection.Assembly.LoadFile(<mod>/ModenjoASM.dll)
→ asm:CreateInstance("ModenjoASM.Bootstrap")  →  runner:Init(modDir)
→ ModenjoModuleLoader.Load(<mod>)
→ Assembly.LoadFrom(<mod>/modules/ModenjoASM.Seven.dll)
→ SevenModule.Initialize()
```

Lua 侧只做三件事：定位宿主 DLL、用反射加载并实例化 `Bootstrap`、调用 `Init(modDir)`。所有规则逻辑都在 C# 里，Lua 文件保持很小。

## 3. 当前可用的扩展点

### 3.1 Aotenjo `EventBus`

宿主本身不订阅任何游戏事件；事件订阅由模块自己完成。示例模块用到的公开事件：

| 事件 | 用途 |
| --- | --- |
| `PlayerRoundEvent.Start.Prepare` / `.Pre` / `.Post` | 保证遗物在回合开始时已授予 |
| `PlayerEvents.PreAddTileEvent` / `PostAddTileEvent` | 中途补授遗物（读档、换人等情况） |
| `PlayerEvents.PostSettlePermutationEvent` | 和牌结算后接管计分 |

遗物补授回调使用 `priority: -100`，计分替换回调使用 `priority: 10_000`；优先级数值的排序方向以 Aotenjo `EventBus` 实现为准，本项目只按“补授靠前、结算靠后”的约定使用。

### 3.2 公开 API

当前用到的游戏公开面（全部是 `Aotenjo` 命名空间下的公开类型/成员）：

- `Aotenjo.ArtifactRegistry.Register(id, factory, modId)`：注册遗物。
- `Aotenjo.CustomYakuBuilder.RegisterCustomYaku(...)`：注册自定义役。
- `Player.ObtainArtifact`、`Player.GetArtifacts`、`Player.ChangeArtifactLimit`：遗物授予与槽位。
- `Player.GetSkillSet().SetLevel/GetLevel`：授予役种等级（示例模块用它把 56 役全部置为 1 级）。
- `Player.RoundAccumulatedScore`、`Player.CurrentAccumulatedScore`：替换本局得分。
- `Aotenjo.Utils.GetConfigValue("version")`：读版本号（版本标记用）。
- `Aotenjo.Logger` / `UnityEngine.Debug`：日志。

### 3.3 反射只用于“装配”，不用于 Hook

`System.Reflection` 在项目里只有两类用途：

1. Lua 侧 `Assembly.LoadFile` + `CreateInstance` 加载宿主（xLua 的命名空间缓存看不到启动后加载的程序集，因此还要 `System.Type.GetType` 显式解析）。
2. 宿主侧 `Assembly.LoadFrom` + `Assembly.GetTypes` + `Activator.CreateInstance` 发现模块。

**没有**用反射去查找、调用或替换游戏的私有方法。反射在这里是加载机制，不是拦截机制。

### 3.4 现在不是任意方法 Hook

当前扩展模型的能力边界是公开事件 + 公开 API：

- 可以做：订阅事件、注册遗物/役种、读写公开的玩家状态、在结算回调里改写结果。
- 不能做：拦截游戏原本的方法、修改私有字段、改变方法返回值、给已有方法加前置/后置逻辑。
- 事件覆盖不到的行为（例如某个私有方法内部的判定分支），当前无法扩展。

如果将来确实需要 Hook 私有方法，才评估 Harmony / MonoMod / Doorstop 一类方案；那是独立决策，会改变“非侵入”这一前提，需单独评估版本兼容与反作弊影响。当前代码没有任何相关依赖。

## 4. 模块模型

### 4.1 目录约定与加载流程

```text
mods/7nnkmj_0.001/
├─ modinfo.json
├─ script/init.lua          Lua 入口
├─ ModenjoASM.dll           宿主
└─ modules/
   └─ ModenjoASM.Seven.dll  示例模块
```

加载细节（`src/ModenjoModuleLoader.cs`）：

1. `Bootstrap.Init(modDirectory)`：若传入目录存在则用之，否则回退到宿主 DLL 所在目录。
2. 注册 `AppDomain.CurrentDomain.AssemblyResolve`，按模块简单名从 **mod 根目录**（不是 `modules/`）探测依赖 DLL。
3. 创建 `ModenjoASM.VersionTag` GameObject（`DontDestroyOnLoad`）。
4. 扫描 `<mod 根>/modules/*.dll`，**只扫顶层、不递归**。
5. 每个 DLL 用 `Assembly.LoadFrom` 加载，遍历 `GetTypes()`，跳过抽象类/接口，找实现 `ModenjoASM.IModenjoModule` 的具体类型，每个类型 `Activator.CreateInstance` 后调用一次 `Initialize()`。
6. 加载失败只记 `LogError` 并继续下一个 DLL；整个目录缺失或没有任何实现时只记 `LogWarning`，不影响游戏启动。

### 4.2 `IModenjoModule`

```csharp
public interface IModenjoModule
{
    string Id { get; }
    void Initialize();
}
```

约束：

- 模块必须有公共无参构造函数（`Activator.CreateInstance` 创建）。
- 模块不接收 GameObject、不接收游戏对象引用，只能通过 Aotenjo 公开 API 与 EventBus 工作。
- 一个 DLL 里可以有多个模块类型，会全部初始化；宿主 `_initialized` 保证整个宿主每进程只初始化一次，模块目录因此只扫描一遍。

### 4.3 示例模块：`ModenjoASM.Seven`

- 入口：`src/ModenjoASM.Seven/SevenModule.cs`，`Id = "7nnkmj"`。
- `Initialize()` 做两件事：
  1. `SevenYakuRegistry.RegisterAll()`：用 `CustomYakuBuilder.RegisterCustomYaku` 注册 **56 个**自定义役（前缀 `7nnkmj:`，标签 `standard`/`galaxy`，稀有度 `COMMON`）。
  2. `SevenArtifactRegistration.Register()`：注册遗物 `7nnkmj:scoring_artifact` 并接线事件。
- 遗物：`SevenScoringArtifact`，不可出售（`CanBeSellByPlayer` / `CanBeSold` 均返回 false），占用 1 格并额外提供 1 格（`ChangeArtifactLimit(1)`）。
- 计分：在 `PostSettlePermutationEvent` 里用 `SevenYakuRegistry.Evaluate(permutation, player)` 算出七子赋位点数，替换 `Player.RoundAccumulatedScore` / `CurrentAccumulatedScore` 中游戏原本的贡献。
- 文案：`mod/lang/zh-CN.json`、`mod/lang/en-US.json`。

### 4.4 宿主 / 模块依赖方向

- 模块工程通过 `ProjectReference` 引用宿主工程，因此模块 DLL 在运行时**必须有宿主 DLL**；宿主不依赖模块。
- 安装布局也按这个方向：宿主 DLL 在 mod 根，模块 DLL 在 `modules/` 子目录。
- 因为框架和示例模块打包在同一个 mod 目录里，当前没有“多个 mod 共享一份宿主”的机制：每个 mod 各带一份 `ModenjoASM.dll`。同一进程加载两份宿主会产生两份静态状态（两个版本标记、两套 `_initialized`），这是当前设计的已知限制。

## 5. 范围限制

- **只针对单人 Roguelike 流程**：扩展的目标是 Aotenjo 现有的单人一局制、遗物/赋位/事件流。
- **明确不做**：四人对局、四人结算、AI 对决。框架里没有联网、座位轮转、多人结算的代码，本项目也不计划做。
- Aotenjo 不是传统麻将：它的手牌以 `Permutation`（和牌排列）为单位、计分以“赋位”表为基准，而不是传统麻将的番符体系。七子模块是把役种表和赋位表映射到这套机制上，不能假设它等同于传统麻将规则。

## 6. 构建、安装与运行

### 6.1 构建两个项目

```powershell
# 一次构建宿主 + 示例模块（-Install 可接着安装）
powershell -ExecutionPolicy Bypass -File tools\build.ps1

# 或者分别构建
dotnet build ModenjoASM.csproj -c Release
dotnet build src\ModenjoASM.Seven\ModenjoASM.Seven.csproj -c Release
```

- 两个工程都是 `netstandard2.1`。
- 输出：`bin\Release\netstandard2.1\ModenjoASM.dll`（宿主）、`src\ModenjoASM.Seven\bin\Release\netstandard2.1\ModenjoASM.Seven.dll`（示例模块）。
- 构建依赖本机已安装的 Aotenjo（默认路径 `E:\SteamLibrary\steamapps\common\Aotenjo\Aotenjo_Data\Managed`，写在两个 csproj 的 `GameManagedDir` 里）。仓库不随附游戏 DLL。

### 6.2 安装

```powershell
powershell -ExecutionPolicy Bypass -File tools\install.ps1
```

脚本行为（`tools/install.ps1`，默认 `GameRoot=E:\SteamLibrary\steamapps\common\Aotenjo`、`ModFolder=7nnkmj_0.001`）：

1. 确认 `Aotenjo_Data\StreamingAssets\mods` 存在。
2. 若目标 mod 目录已存在，先备份到游戏根下的 `7nnkmj_backup\7nnkmj_0.001.backup-yyyyMMdd-HHmmss`（备份放在 mods 目录外，避免被 ModManager 当成另一个 mod 扫描）。
3. 覆盖目标 mod 目录：复制 `mod\*`、宿主 `ModenjoASM.dll` 到 mod 根，模块 `ModenjoASM.Seven.dll` 到 `modules\`。
4. **不修改 `Assembly-CSharp.dll`**，脚本最后会打印这一点。

注意：安装是“备份后覆盖”，不会删除目标目录里旧版本遗留的多余文件；需要彻底干净时先手动清空该 mod 目录再安装。

### 6.3 运行与日志

装好后由 Aotenjo ModManager 加载 `script/init.lua`。日志关键字：

- `[ModenjoASM] host assembly: ...`、`[ModenjoASM] host initialized from ...`：宿主加载成功。
- `[ModenjoASM] module loaded: 7nnkmj (...)`：模块发现并初始化成功。
- `[ModenjoASM] no IModenjoModule found ...`、`modules directory not found`：模块缺失或为空（宿主仍可用）。
- `[7nnkmj] registered 56 custom yaku`：示例模块役种注册成功。

## 7. 安全与限制

### 7.1 Lua 加载 DLL 的权限边界

- `init.lua` 使用的是 xLua 暴露的 `CS.System.Reflection` 与 `CS.System.IO`。这类 API 一旦可用，Lua 代码就具备**在当前进程内加载任意本地 DLL 并调用 .NET 代码**的能力，和游戏本身同权限，没有操作系统级沙箱。
- 模块 DLL 同理：它在游戏进程的主线程、同一 AppDomain 里运行，可以读文件、发网络请求、访问游戏对象。
- 结论：**只安装可信来源的 mod**；启用一个 mod 等于允许它执行任意进程内代码。框架不做也不应假装做安全隔离。

### 7.2 版本兼容

- 宿主与模块都直接引用 `Assembly-CSharp` 和 `UnityEngine` 程序集，游戏版本变化可能导致类型/成员缺失。
- `mod/modinfo.json` 声明的兼容范围是 `supportedGameVersions: 0.7.1.1–0.8.0.0`。超出该范围不保证加载，可能直接抛 `TypeLoadException` / `MissingMethodException`。
- `modinfo.json` 里有 `artifactApiVersion: 2` 字段（遗物 API 版本标记）；具体校验行为以游戏 ModManager 为准，改动前需确认对应游戏版本的 API。
- 计分替换依赖 `PostSettlePermutationEvent` 的触发时机和 `RoundAccumulatedScore` 的语义；换游戏版本后这两点必须重新验证。

### 7.3 崩溃与隔离

- 没有进程隔离：宿主、模块、游戏共用一个 Unity 进程。宿主初始化失败会让整个 mod 失效；模块 `Initialize` 抛异常由加载器捕获并 `LogError`，不会中断其它模块的加载，但该模块可能处于半初始化状态。
- 事件回调里抛异常、或计分算法写坏玩家分数，会直接影响当前这局，严重时卡死或需要重启游戏。
- “不 patch 游戏 DLL”只表示不修改磁盘上的游戏文件，**不等于运行时零风险**：模块改的是内存中的玩家状态与结算结果。

## 8. 当前验证状态

### 8.1 已实机确认

在 Aotenjo `0.7.1.2-EA` 上确认过：

- 拆分前版本已实机加载，日志出现 56 个自定义役注册成功（`[7nnkmj] registered 56 custom yaku`）。
- 遗物：不可出售、开局授予、额外提供 1 格遗物槽。
- 版本标记 `Modded!! By-ModenjoASM` 能显示在版本号旁。
- 拆分后（commit `ecf73b4`）宿主工程与示例模块工程可构建，产出两个 DLL；安装脚本已按“宿主在 mod 根、模块在 `modules/`”的方式复制。
- 拆分后的“宿主 + modules 目录”加载路径已实机验证，日志出现 `[ModenjoASM] module loaded: 7nnkmj` 与 `[7nnkmj] registered 56 custom yaku`。

### 8.2 尚未验证 / 未完成

- **可和牌的实际计分尚未验证**：`SevenYakuRegistry.Evaluate` 算出的赋位点数是否与游戏结算面板/预期一致，还没有实机对局确认。README 也写明“计分算法仍在补齐”。
- 状态役（海底、岭上、人和、河底、和绝张）未实现；暗刻/明刻区分未实现；完整高役特例仍需继续补。
- 不能把“注册了 56 个役”等同于“役种判定和计分正确”；当前状态是“能加载、能注册、能授予”，不是“规则已验证”。

## 9. 已知不一致（截至本次文档更新）

以下是仓库里与“框架宿主 + 示例模块”定位不一致、但本次未改动的地方：

- `mod/script/init.lua` 把 mod 目录硬编码为 `mods/7nnkmj_0.001`；换 mod 文件夹名或换 modID 时必须同步改这两行（框架本身不解决“任意 mod 目录”的自动定位）。
- 示例模块内 `SevenYakuRegistry` 的命名空间仍是 `ModenjoASM.SevenYaku`（其余模块代码在 `ModenjoASM.Seven`），属于拆分遗留的命名不一致，不影响构建。
- 仓库没有 `.sln`/`.slnx`，两个工程靠 `tools/build.ps1` 依次构建。
- 示例模块工程构建时会把宿主 `ModenjoASM.dll` 一并复制到自己的输出目录；安装脚本取的是仓库根 `bin` 下的宿主 DLL 和模块输出目录下的模块 DLL，两者不要混用。

By DSH
