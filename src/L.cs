// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KK_StudioToolbarScroll
{
    public enum UiLanguage
    {
        Auto,
        English,
        SimplifiedChinese,
        Japanese,
    }

    /// <summary>UI strings in en, zh-CN and ja. All dictionaries must keep the same keys and {n} placeholders.</summary>
    internal static class L
    {
        /// <summary>
        /// XUnity.AutoTranslator skips text starting with U+180E (IgnoreTextStartingWith). Tooltips are drawn with
        /// IMGUI, which it would otherwise translate again.
        /// </summary>
        private const string NoTranslate = "᠎";

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "arrow.up", "Scroll the toolbar up\n{0} more rows above" },
            { "arrow.down", "Scroll the toolbar down\n{0} more rows below" },
        };

        private static readonly Dictionary<string, string> ZhCN = new Dictionary<string, string>
        {
            { "arrow.up", "向上滚动工具栏\n上方还有 {0} 行" },
            { "arrow.down", "向下滚动工具栏\n下方还有 {0} 行" },
        };

        private static readonly Dictionary<string, string> Ja = new Dictionary<string, string>
        {
            { "arrow.up", "ツールバーを上へスクロール\n上にあと {0} 行" },
            { "arrow.down", "ツールバーを下へスクロール\n下にあと {0} 行" },
        };

        private static Dictionary<string, string> _active = En;

        public static void SetLanguage(UiLanguage language)
        {
            if (language == UiLanguage.Auto)
            {
                SystemLanguage sys = Application.systemLanguage;
                if (sys == SystemLanguage.Chinese || sys == SystemLanguage.ChineseSimplified || sys == SystemLanguage.ChineseTraditional)
                    language = UiLanguage.SimplifiedChinese;
                else if (sys == SystemLanguage.Japanese)
                    language = UiLanguage.Japanese;
                else
                    language = UiLanguage.English;
            }

            switch (language)
            {
                case UiLanguage.SimplifiedChinese: _active = ZhCN; break;
                case UiLanguage.Japanese: _active = Ja; break;
                default: _active = En; break;
            }
        }

        /// <summary>Localized, formatted UI text, protected from machine translation.</summary>
        public static string Get(string key, params object[] args)
        {
            string text;
            if (!_active.TryGetValue(key, out text) && !En.TryGetValue(key, out text))
                text = key;
            return NoTranslate + (args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, text, args) : text);
        }
    }
}
