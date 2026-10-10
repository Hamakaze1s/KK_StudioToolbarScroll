// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;
using BepInEx;
using KKAPI.Studio.UI.Toolbars;
using UnityEngine;

namespace KK_StudioToolbarScroll.Core
{
    /// <summary>
    /// Registers dummy toolbar buttons through KKAPI so overflow can be reproduced without many plugins installed.
    /// </summary>
    internal sealed class DebugButtons
    {
        private readonly BaseUnityPlugin _owner;
        private readonly List<SimpleToolbarButton> _buttons = new List<SimpleToolbarButton>();

        public DebugButtons(BaseUnityPlugin owner)
        {
            _owner = owner;
        }

        /// <summary>Adds or removes dummy buttons until exactly <paramref name="count"/> exist.</summary>
        public void SetCount(int count)
        {
            if (count < 0) count = 0;
            Plugin.Trace("fake buttons " + _buttons.Count + " -> " + count);
            try
            {
                while (_buttons.Count < count)
                {
                    int n = _buttons.Count + 1;
                    var btn = new SimpleToolbarButton(
                        "Fake " + n.ToString("00"),
                        "Fake button " + n + " (KK_StudioToolbarScroll debug)",
                        () => MakeIcon(n),
                        _owner,
                        b => Plugin.Log.LogInfo("Fake button " + n + " clicked"));
                    if (!ToolbarManager.AddLeftToolbarControl(btn))
                    {
                        btn.Dispose();
                        break;
                    }
                    _buttons.Add(btn);
                }
                while (_buttons.Count > count)
                {
                    int last = _buttons.Count - 1;
                    _buttons[last].Dispose();
                    _buttons.RemoveAt(last);
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("Fake button update failed: " + e);
            }
        }

        /// <summary>32x32 icon: a colour per button, with n binary ticks along the bottom so buttons are tellable apart.</summary>
        private static Texture2D MakeIcon(int n)
        {
            var tex = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            Color body = Color.HSVToRGB((n * 0.137f) % 1f, 0.55f, 0.85f);
            var px = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    bool border = x < 2 || y < 2 || x > 29 || y > 29;
                    px[y * 32 + x] = border ? Color.black : body;
                }
            }
            for (int bit = 0; bit < 6; bit++)
            {
                if ((n & (1 << bit)) == 0) continue;
                for (int y = 6; y < 14; y++)
                    for (int x = 4 + bit * 4; x < 7 + bit * 4; x++)
                        px[y * 32 + x] = Color.black;
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
