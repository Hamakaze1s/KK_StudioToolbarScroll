# KK_StudioToolbarScroll

**English** | [简体中文](docs/zh-CN/README.md) | [日本語](docs/ja/README.md)

A CharaStudio plugin for Koikatu that keeps the studio's left toolbar usable when too many plugins have added buttons to it. Buttons that no longer fit on the screen are reached with ▲▼ buttons instead of being lost off the top edge.

> **Status:** in development (0.0.x), not released yet. Tested in game with KKAPI 1.45.1 and 1.48.1.

## What it does

- Does nothing while all toolbar buttons fit.
- When they don't, the top row of the scrolling area becomes a ▲ and a ▼ button. Click to scroll one row; hold to keep scrolling. Hovering shows how many rows are left in that direction.
- The game's own buttons (Undo, Redo, Camera, ...) stay put. A thin line separates them from the buttons that scroll.
- Rows hidden behind the studio's main menu (add, anim, sound, ...) are not counted as usable space.
- Buttons you drag out of the two toolbar columns stay where you put them.
- No mouse-wheel scrolling: the wheel is used by the 3D camera and by many windows. Optional hotkeys exist (unbound by default).

## Requirements

- Koikatu **CharaStudio**, BepInEx 5.4.x
- KKAPI (IllusionModdingAPI) 1.45 or later

## Installation

1. Download the zip from the latest release and extract it into your Koikatu folder so that `KK_StudioToolbarScroll.dll` ends up in `BepInEx/plugins`.
2. Start CharaStudio. No other setup is needed.

## Settings (F1 → KK_StudioToolbarScroll)

| Setting | Default | Meaning |
|---|---|---|
| General / Language | Auto | UI language: Auto, English, Simplified Chinese, Japanese |
| Hotkeys / Scroll up, Scroll down | unbound | Scrolls the toolbar by one row |
| Debug / Fake buttons | 0 | Adds dummy buttons to test overflow. Keep at 0 for normal use |

## Known issues

- With KKAPI 1.46 or later, entering the toolbar's *Edit Mode* (Option button, right-click) can push a vanilla button such as Undo one column to the right, because KKAPI also places buttons that are normally hidden. This happens without this plugin too. *Rearrange buttons* in the same menu restores the layout.

## Build from source

Needs the .NET SDK. `KKAPI.dll` is referenced from your game folder.

```bash
cd src
dotnet build -c Release -p:GameDir="C:\path\to\Koikatu"
```

You can also put the path into `GameDir.props` in the repository root. After building, the DLL is copied to `GameDir\BepInEx\plugins` automatically; pass `-p:CopyToGame=false` to skip that.

## License

This software is provided "as is", without warranty of any kind. Use it at your own risk.

Copyright (C) 2026 Hamakaze. Released under the [GNU General Public License v3.0 or later](LICENSE) (GPL-3.0-or-later). The plugin links against KKAPI (LGPL-3.0) and BepInEx (LGPL-2.1) but does not bundle or redistribute them.
