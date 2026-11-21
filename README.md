# ModenjoASM

《青天井》（Aotenjo）的**不修改原游戏的 C# 扩展宿主**。  
七子麻将（7nnkmj）只是第一个示例模块，不是框架目标。

- 只针对单人 Roguelike
- 不修改 `Assembly-CSharp.dll`、prefab、场景或 Unity 资源
- 通过 Lua 加载宿主，再由宿主发现 `modules/*.dll`
- 扩展点：Aotenjo `EventBus`、公开 API；当前不是任意方法 Hook

完整说明见 [docs/MODENJOASM_FRAMEWORK.md](docs/MODENJOASM_FRAMEWORK.md)。

## 结构

```text
ModenjoASM/
├─ src/
│  ├─ Bootstrap.cs                 宿主入口
│  ├─ IModenjoModule.cs            模块接口
│  ├─ ModenjoModuleLoader.cs       模块发现/加载
│  ├─ ModenjoAsmVersionTag.cs      版本标记
│  └─ ModenjoASM.Seven/            示例模块（56 役 + 遗物）
├─ mod/
│  ├─ modinfo.json                 示例模块的 mod 清单
│  ├─ script/init.lua              Lua 入口
│  └─ lang/                        文案
├─ docs/
│  └─ MODENJOASM_FRAMEWORK.md      框架文档
└─ tools/
   ├─ build.ps1                    构建宿主 + 示例模块
   └─ install.ps1                  备份并安装到游戏
```

## 加载路径

```text
Aotenjo ModManager
→ mods/7nnkmj_0.001/script/init.lua
→ Assembly.LoadFile(ModenjoASM.dll)
→ Bootstrap.Init(modDir)
→ ModenjoModuleLoader
→ modules/ModenjoASM.Seven.dll
→ SevenModule.Initialize()
```

## 构建与安装

```powershell
powershell -ExecutionPolicy Bypass -File tools\build.ps1
powershell -ExecutionPolicy Bypass -File tools\install.ps1
```

安装只备份并替换 mod 目录，不修改游戏程序集。备份放在游戏根目录 `7nnkmj_backup`，不会留在 `mods` 下造成重复 modID。

## 当前状态

- 宿主 + 示例模块已拆分，Aotenjo `0.7.1.2-EA` 实机加载成功
- 示例模块注册 56 个 custom yaku，并授予不可出售的计分遗物
- 版本号显示 `Modded!! By-ModenjoASM`
- 可和牌的实际计分尚未验证
- 状态役、暗刻/明刻区分不做
- 不做四人对局、四人结算、AI 对决

By DSH
