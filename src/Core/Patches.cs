// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Reflection;
using HarmonyLib;
using KKAPI.Studio.UI.Toolbars;
using UnityEngine;

namespace KK_StudioToolbarScroll.Core
{
    /// <summary>Harmony hooks into KKAPI's toolbar layout and drag handling.</summary>
    internal static class Patches
    {
        private static ToolbarScroller _scroller;

        /// <returns>false if a hook could not be installed; the caller must then leave scrolling off.</returns>
        public static bool Install(ToolbarScroller scroller)
        {
            _scroller = scroller;
            try
            {
                var harmony = new Harmony(Plugin.Guid);

                MethodInfo update = AccessTools.Method(typeof(ToolbarManager), "UpdateInterface");
                Type dragHelper = AccessTools.Inner(typeof(ToolbarControlBase), "DragHelper");
                MethodInfo begin = FindExplicit(dragHelper, ".OnBeginDrag");
                MethodInfo end = FindExplicit(dragHelper, ".OnEndDrag");
                if (update == null || begin == null || end == null)
                {
                    Plugin.Log.LogWarning("KKAPI toolbar internals not found (UpdateInterface/DragHelper); scrolling stays off");
                    return false;
                }

                harmony.Patch(update,
                    prefix: new HarmonyMethod(typeof(Patches), nameof(BeforeUpdate)),
                    finalizer: new HarmonyMethod(typeof(Patches), nameof(AfterUpdate)));
                harmony.Patch(begin, prefix: new HarmonyMethod(typeof(Patches), nameof(BeforeBeginDrag)));
                harmony.Patch(end, prefix: new HarmonyMethod(typeof(Patches), nameof(BeforeEndDrag)));
                Plugin.Trace("patched " + update.DeclaringType.Name + "." + update.Name + ", " + begin.Name + ", " + end.Name);
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Could not patch KKAPI toolbar; scrolling stays off: " + e);
                return false;
            }
        }

        private static MethodInfo FindExplicit(Type type, string suffix)
        {
            if (type == null) return null;
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (m.Name.EndsWith(suffix, StringComparison.Ordinal)) return m;
            return null;
        }

        private static int _unplacedBefore;

        private static void BeforeUpdate()
        {
            _scroller.Clear();
            _unplacedBefore = CountUnplaced();
        }

        /// <summary>Buttons without a saved grid position. A jump here means KKAPI dropped positions during a relayout.</summary>
        private static int CountUnplaced()
        {
            int n = 0;
            ToolbarControlBase[] all = ToolbarManager.GetAllButtons(true);
            for (int i = 0; i < all.Length; i++)
                if (!all[i].DesiredPosition.HasValue) n++;
            return n;
        }

        // Finalizer so offsets are re-applied even if UpdateInterface throws
        private static void AfterUpdate()
        {
            try
            {
                int after = CountUnplaced();
                if (after != _unplacedBefore)
                    Plugin.Trace("relayout: buttons without saved position " + _unplacedBefore + " -> " + after
                        + ", screen " + Screen.width + "x" + Screen.height);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("relayout diagnostics failed: " + e.Message);
            }
            _scroller.Apply();
        }

        private static void BeforeBeginDrag(Component __instance)
        {
            _scroller.BeginDrag(__instance);
        }

        private static void BeforeEndDrag()
        {
            _scroller.EndDrag();
        }
    }
}
