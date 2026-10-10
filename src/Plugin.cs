// SPDX-License-Identifier: GPL-3.0-or-later
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KK_StudioToolbarScroll.Core;
using KKAPI.Studio;

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

        private ConfigEntry<int> _fakeButtons;
        private DebugButtons _debugButtons;

        void Awake()
        {
            Log = Logger;
            if (!StudioAPI.InsideStudio) return;

            _fakeButtons = Config.Bind("Debug", "Fake buttons", 0,
                new ConfigDescription("Adds this many dummy buttons to the left toolbar to test overflow. Keep at 0 for normal use.",
                    new AcceptableValueRange<int>(0, 60)));
            _debugButtons = new DebugButtons(this);
            _fakeButtons.SettingChanged += (s, e) => _debugButtons.SetCount(_fakeButtons.Value);
            _debugButtons.SetCount(_fakeButtons.Value);

            Log.LogInfo(Name + " " + Version + " loaded");
        }
    }
}
