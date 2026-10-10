// SPDX-License-Identifier: GPL-3.0-or-later
using BepInEx;
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

        void Awake()
        {
            if (!StudioAPI.InsideStudio) return;
            Logger.LogInfo(Name + " " + Version + " loaded (skeleton, no behaviour yet)");
        }
    }
}
