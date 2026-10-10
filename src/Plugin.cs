// SPDX-License-Identifier: GPL-3.0-or-later
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KK_StudioToolbarScroll.Core;
using KKAPI.Studio;
using UnityEngine;

namespace KK_StudioToolbarScroll
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, "1.43")]
    [BepInProcess("CharaStudio")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "Hamakaze.KK_StudioToolbarScroll";
        public const string Name = "KK_StudioToolbarScroll";
        public const string Version = "0.0.1";

        internal static ManualLogSource Log;
        internal static ToolbarScroller Scroller;

        private ConfigEntry<int> _fakeButtons;
        private ConfigEntry<KeyboardShortcut> _debugUp;
        private ConfigEntry<KeyboardShortcut> _debugDown;
        private DebugButtons _debugButtons;

        void Awake()
        {
            Log = Logger;
            if (!StudioAPI.InsideStudio) return;

            _fakeButtons = Config.Bind("Debug", "Fake buttons", 0,
                new ConfigDescription("Adds this many dummy buttons to the left toolbar to test overflow. Keep at 0 for normal use.",
                    new AcceptableValueRange<int>(0, 60)));
            _debugUp = Config.Bind("Debug", "Scroll up key", new KeyboardShortcut(KeyCode.Keypad8),
                "Temporary: scrolls the toolbar one row until the on-screen arrows exist");
            _debugDown = Config.Bind("Debug", "Scroll down key", new KeyboardShortcut(KeyCode.Keypad2),
                "Temporary: scrolls the toolbar one row until the on-screen arrows exist");

            var scroller = new ToolbarScroller();
            if (Patches.Install(scroller))
            {
                Scroller = scroller;
                scroller.Changed += LogState;
            }

            _debugButtons = new DebugButtons(this);
            _fakeButtons.SettingChanged += (s, e) => _debugButtons.SetCount(_fakeButtons.Value);
            _debugButtons.SetCount(_fakeButtons.Value);

            Log.LogInfo(Name + " " + Version + " loaded");
        }

        void Update()
        {
            if (Scroller == null) return;
            Scroller.Tick();
            if (_debugUp.Value.IsDown()) Scroller.ScrollBy(1);
            if (_debugDown.Value.IsDown()) Scroller.ScrollBy(-1);
        }

        private static void LogState()
        {
            ToolbarScroller s = Scroller;
            Log.LogDebug("scroll " + s.Scroll + "/" + s.MaxScroll + ", window " + s.WindowRows + " rows, overflow " + s.Overflowing);
        }
    }
}
