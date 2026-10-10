# KK_StudioToolbarScroll

A CharaStudio plugin for Koikatu that keeps the studio's left toolbar usable when too many plugins have added buttons to it. Buttons that no longer fit on the screen are reached with ▲▼ buttons instead of being lost off the top edge.

> **Status:** in development, nothing to use yet. The current build only loads and writes one line to the log.

## Planned behaviour

- Does nothing while all toolbar buttons fit.
- When they don't, shows ▲▼ buttons that page through the third-party buttons. The game's own buttons (Undo, Redo, Camera, ...) stay put.
- No mouse-wheel scrolling: the wheel is used by the 3D camera and by many windows.

## Requirements

- Koikatu **CharaStudio**, BepInEx 5.4.x
- KKAPI (IllusionModdingAPI) 1.43 or later

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
