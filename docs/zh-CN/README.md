# KK_StudioToolbarScroll

[English](../../README.md) | **简体中文** | [日本語](../ja/README.md)

恋活 CharaStudio（工作室）插件。插件装多了，工作室左侧工具栏会被塞满，放不下的按钮会跑到屏幕上沿之外。本插件用 ▲▼ 按钮让这些按钮都能点到。

> **状态：** 开发中（0.0.x），尚未发布。已在游戏内用 KKAPI 1.45.1 和 1.48.1 测试。

## 功能

- 所有按钮都放得下时，什么都不做。
- 放不下时，滚动区域最上面一行变成 ▲ 和 ▼ 两个按钮。单击滚动一行，按住连续滚动。鼠标悬停会显示该方向还剩几行。
- 游戏自带的按钮（Undo、Redo、Camera 等）固定不动，并用一条细线和会滚动的按钮分开。
- 被工作室主菜单（add、anim、sound 等）盖住的行不算可用空间。
- 被你拖出两列之外的按钮留在你放的位置。
- 不支持鼠标滚轮：滚轮用于 3D 视角缩放，很多窗口也在用。可以另外绑定热键（默认未绑定）。

## 环境要求

- 恋活 **CharaStudio**，BepInEx 5.4.x
- KKAPI（IllusionModdingAPI）1.45 及以上

## 安装

1. 从最新发布页下载 zip，解压到恋活目录，使 `KK_StudioToolbarScroll.dll` 位于 `BepInEx/plugins` 中。
2. 启动 CharaStudio，不需要其他设置。

## 设置（F1 → KK_StudioToolbarScroll）

| 设置 | 默认值 | 说明 |
|---|---|---|
| General / Language | Auto | 界面语言：自动、英文、简体中文、日语 |
| Hotkeys / Scroll up、Scroll down | 未绑定 | 工具栏滚动一行 |
| Debug / Fake buttons | 0 | 添加假按钮，用来测试溢出。正常使用请保持 0 |

## 已知问题与建议

- **尽量不要使用工具栏的“编辑模式”**（右键 Option 按钮）。我们目前没有发现本插件在该模式下造成问题，但 KKAPI 1.46 及以上版本在编辑模式中会把平时隐藏的按钮也排进去，可能把 Undo 等原版按钮向右挤一列（不装本插件时同样会发生）。我们也不能排除未来版本的 KKAPI 带来新的冲突。如果布局乱了，用同一菜单里的 *Rearrange buttons* 可以恢复。
- **请不要拖动游戏自带的按钮**（Undo、Redo、Camera 等）。插件假设它们位于最下方。它们留下的空格会被 KKAPI 用其他按钮填上，固定区与滚动区之间的分隔线也会随之移动。

## 从源码构建

需要 .NET SDK。`KKAPI.dll` 从你的游戏目录引用。

```bash
cd src
dotnet build -c Release -p:GameDir="C:\path\to\Koikatu"
```

也可以把路径写进仓库根目录的 `GameDir.props`。构建后 DLL 会自动复制到 `GameDir\BepInEx\plugins`；加 `-p:CopyToGame=false` 可以跳过复制。

## 许可

本软件按“现状”提供，不附带任何形式的担保，使用风险自负。

Copyright (C) 2026 Hamakaze。以 [GNU General Public License v3.0 or later](../../LICENSE)（GPL-3.0-or-later）发布。插件链接 KKAPI（LGPL-3.0）和 BepInEx（LGPL-2.1），但不打包也不再分发它们。
