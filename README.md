# ModenjoASM

《青天井》（Aotenjo）的 7nnkmj 迁移工程。

## 目标

把七子麻将的**役种表**迁移到 Aotenjo，并用一个不可移除的 C# 遗物接管七子的赋位计分。

当前范围：

- 只迁移役种判定与计分
- 先做静态役（最终手牌/副露可判断的役）
- 状态役、四人结算、AI 不在首版
- 兼容目标：`0.7.1.1` 到 `0.8.0.0`

## 结构

```text
ModenjoASM/
├─ src/
│  ├─ Bootstrap.cs               C# 入口，Lua 加载后调用
│  ├─ SevenScoringArtifact.cs    不可移除遗物 + 额外 1 槽
│  └─ SevenYaku/
│     └─ SevenYakuRegistry.cs    七子役种注册与判定
├─ mod/
│  ├─ modinfo.json
│  └─ script/init.lua           Lua 入口，加载 C# DLL
└─ tools/
   └─ install.ps1               构建后覆盖旧模组目录（带备份）
```

## 加载方式

不修改 `Assembly-CSharp.dll`：

```text
Aotenjo ModManager
→ 加载 mods/7nnkmj_0.001/script/init.lua
→ Lua 调用 Assembly.LoadFile 加载 ModenjoASM.dll
→ Bootstrap.Init() 注册遗物与役种
```

这样避免直接改游戏原始程序集；如果 Lua 侧无法调用 `System.Reflection`，再单独评估注入器方案。

## 构建

```powershell
dotnet build ModenjoASM.csproj -c Release
```

项目引用的是本机 Aotenjo 的 `Aotenjo_Data/Managed` 程序集，不随仓库分发游戏 DLL。

## 安装

```powershell
powershell -ExecutionPolicy Bypass -File tools/install.ps1
```

脚本会：

1. 备份 `mods/7nnkmj_0.001`
2. 复制 `mod/` 文件
3. 复制 `ModenjoASM.dll` 到同一目录

不修改 `Assembly-CSharp.dll`。

## 当前状态

- C# 工程可编译：`netstandard2.1`
- 已在 Aotenjo `0.7.1.2-EA` 实机加载：43 个 custom yaku 注册成功
- 遗物：不可出售，开局授予，额外 1 格
- 计分算法仍在补齐，未声称已完成全表迁移
- 已加入首批静态役、赋位表、本地中英文案
- 状态役、四暗刻/明杠暗杠区分、完整高役特例仍需继续

By DSH
