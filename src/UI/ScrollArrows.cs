// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using KK_StudioToolbarScroll.Core;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KK_StudioToolbarScroll.UI
{
    /// <summary>
    /// The ▲▼ buttons in the reserved top row of the scrolling window. They are clones of a vanilla toolbar button
    /// (as KKAPI makes its own), parked beside 01_Button and positioned in screen pixels. Only shown while the
    /// toolbar overflows.
    /// </summary>
    internal sealed class ScrollArrows
    {
        private sealed class Arrow
        {
            public GameObject Object;
            public RectTransform Rect;
            public Button Button;
            public GlobalTooltips.Tooltip Tooltip;
        }

        private readonly ToolbarScroller _scroller;
        private Arrow _up, _down;
        private bool _failed;

        public ScrollArrows(ToolbarScroller scroller)
        {
            _scroller = scroller;
            scroller.Changed += Refresh;
        }

        /// <summary>Re-applies state and localized tooltips (also used on language change).</summary>
        public void Refresh()
        {
            if (_failed) return;
            try
            {
                if (!_scroller.Overflowing)
                {
                    if (_up != null) { _up.Object.SetActive(false); _down.Object.SetActive(false); }
                    return;
                }
                if (_up == null && !Create()) return;

                // Scrolling "up" reveals higher rows: the scroll value counts how far the content moved down.
                Place(_up, _scroller.ArrowLeftX, _scroller.MaxScroll - _scroller.Scroll, "arrow.up");
                Place(_down, _scroller.ArrowRightX, _scroller.Scroll, "arrow.down");
            }
            catch (Exception e)
            {
                _failed = true;
                Plugin.Log.LogError("Scroll arrows disabled after an error: " + e);
                if (_up != null) _up.Object.SetActive(false);
                if (_down != null) _down.Object.SetActive(false);
            }
        }

        private void Place(Arrow arrow, float x, int moreRows, string textKey)
        {
            arrow.Object.SetActive(true);
            float parentScale = arrow.Rect.parent.lossyScale.y;
            float local = parentScale > 0f ? _scroller.ButtonScale / parentScale : 1f;
            arrow.Rect.localScale = new Vector3(local, local, 1f);
            arrow.Rect.position = new Vector3(x, _scroller.ArrowY, 0f);
            arrow.Button.interactable = moreRows > 0;
            arrow.Tooltip.Text = L.Get(textKey, moreRows);
        }

        private bool Create()
        {
            GameObject source = GameObject.Find("StudioScene/Canvas System Menu/01_Button/Button Center");
            if (source == null) return false;

            _up = CreateArrow(source, "KK_StudioToolbarScroll Up", MakeIcon(true), 1);
            _down = CreateArrow(source, "KK_StudioToolbarScroll Down", MakeIcon(false), -1);
            return true;
        }

        private Arrow CreateArrow(GameObject source, string name, Texture2D icon, int direction)
        {
            Transform toolbar = source.transform.parent;
            GameObject go = UnityEngine.Object.Instantiate(source);
            go.name = name;
            go.transform.SetParent(toolbar.parent, false);
            go.transform.SetSiblingIndex(toolbar.GetSiblingIndex() + 1);

            // The clone inherits KKAPI's drag handler (bound to the Center button) and, on 1.46+, its edit-mode overlay.
            foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>())
                if (mb != null && mb.GetType().Name == "DragHelper") UnityEngine.Object.Destroy(mb);
            for (int i = go.transform.childCount - 1; i >= 0; i--)
                if (go.transform.GetChild(i).name == "HiderOverlay") UnityEngine.Object.Destroy(go.transform.GetChild(i).gameObject);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(32f, 32f);

            Button button = go.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent(); // also drops the original's persistent listeners
            Image image = button.image;
            image.sprite = Sprite.Create(icon, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f));
            image.color = Color.white;

            HoldRepeat repeat = go.AddComponent<HoldRepeat>();
            repeat.Button = button;
            repeat.Step = direction;
            repeat.Scroller = _scroller;

            return new Arrow { Object = go, Rect = rect, Button = button, Tooltip = GlobalTooltips.RegisterTooltip(go, "") };
        }

        /// <summary>32x32 button face: dark square with a light border and a white triangle.</summary>
        private static Texture2D MakeIcon(bool up)
        {
            var tex = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            var px = new Color[32 * 32];
            Color back = new Color(0.22f, 0.22f, 0.24f, 0.95f);
            Color edge = new Color(0.75f, 0.75f, 0.78f, 1f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    bool border = x < 2 || y < 2 || x > 29 || y > 29;
                    // texture y grows upward; the apex is at ty = 21 (top for up, bottom for down)
                    int ty = up ? y : 31 - y;
                    bool tri = ty >= 10 && ty <= 21 && Mathf.Abs(x - 15.5f) <= (21 - ty) * 0.75f + 0.5f;
                    px[y * 32 + x] = border ? edge : (tri ? Color.white : back);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        /// <summary>Scrolls on press and keeps scrolling while held.</summary>
        private sealed class HoldRepeat : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
        {
            private const float FirstDelay = 0.4f;
            private const float Interval = 0.08f;

            public Button Button;
            public int Step;
            public ToolbarScroller Scroller;
            private bool _down;
            private float _next;

            public void OnPointerDown(PointerEventData e)
            {
                if (e.button != PointerEventData.InputButton.Left || !Button.interactable) return;
                _down = true;
                _next = Time.unscaledTime + FirstDelay;
                Scroller.ScrollBy(Step);
            }

            public void OnPointerUp(PointerEventData e) { _down = false; }
            public void OnPointerExit(PointerEventData e) { _down = false; }
            private void OnDisable() { _down = false; }

            private void Update()
            {
                if (!_down) return;
                if (!Button.interactable) { _down = false; return; }
                if (Time.unscaledTime >= _next)
                {
                    _next += Interval;
                    Scroller.ScrollBy(Step);
                }
            }
        }
    }
}
