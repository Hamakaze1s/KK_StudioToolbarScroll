// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KK_StudioToolbarScroll.Core;
using KK_StudioToolbarScroll.UI;
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

        private static string _lastState;

        /// <summary>Info-level line with a wall-clock time, so it can be matched against a crash report's timestamp.</summary>
        internal static void Trace(string message)
        {
            Log.LogInfo("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message);
        }

        private ConfigEntry<int> _fakeButtons;
        private ConfigEntry<UiLanguage> _language;
        private ConfigEntry<KeyboardShortcut> _scrollUp;
        private ConfigEntry<KeyboardShortcut> _scrollDown;
        private DebugButtons _debugButtons;
        private ScrollDivider _divider;
        private ScrollArrows _arrows;

        void Awake()
        {
            Log = Logger;
            if (!StudioAPI.InsideStudio) return;

            _fakeButtons = Config.Bind("Debug", "Fake buttons", 0,
                new ConfigDescription("Adds this many dummy buttons to the left toolbar to test overflow. Keep at 0 for normal use.",
                    new AcceptableValueRange<int>(0, 60)));
            _language = Config.Bind("General", "Language", UiLanguage.Auto, "UI language / 界面语言");
            _scrollUp = Config.Bind("Hotkeys", "Scroll up", KeyboardShortcut.Empty,
                "Scrolls the toolbar up by one row. Unbound by default / 工具栏向上滚动一行，默认未绑定");
            _scrollDown = Config.Bind("Hotkeys", "Scroll down", KeyboardShortcut.Empty,
                "Scrolls the toolbar down by one row. Unbound by default / 工具栏向下滚动一行，默认未绑定");
            L.SetLanguage(_language.Value);

            var scroller = new ToolbarScroller();
            if (Patches.Install(scroller))
            {
                Scroller = scroller;
                scroller.Changed += LogState;
                _divider = new ScrollDivider(scroller);
                _arrows = new ScrollArrows(scroller);
                _language.SettingChanged += (s, e) =>
                {
                    L.SetLanguage(_language.Value);
                    _arrows.Refresh();
                };
                Trace("KKAPI toolbar hooks installed");
            }

            _debugButtons = new DebugButtons(this);
            _fakeButtons.SettingChanged += (s, e) => _debugButtons.SetCount(_fakeButtons.Value);
            _debugButtons.SetCount(_fakeButtons.Value);

            Trace(Name + " " + Version + " loaded, fake buttons " + _fakeButtons.Value);
        }

        void Update()
        {
            if (Scroller == null) return;
            Scroller.Tick();
            if (_scrollUp.Value.IsDown()) Scroller.ScrollBy(1);
            if (_scrollDown.Value.IsDown()) Scroller.ScrollBy(-1);
        }

        private static void LogState()
        {
            ToolbarScroller s = Scroller;
            string state = "scroll " + s.Scroll + "/" + s.MaxScroll + ", window " + s.WindowRows + " rows, overflow " + s.Overflowing + " (rows: pinned<=" + s.PinnedMax + ", usable<=" + s.TopRow + ", content<=" + s.MaxRow + "; toolbar canvas " + s.OwnCanvas + ", covered by " + s.CoveredBy + ")";
            if (state == _lastState) return;
            _lastState = state;
            Trace(state);
        }
    }
}
