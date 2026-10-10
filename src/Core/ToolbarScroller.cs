// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Collections.Generic;
using KKAPI.Studio.UI.Toolbars;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KK_StudioToolbarScroll.Core
{
    /// <summary>
    /// Scrolls the third-party toolbar buttons that don't fit on screen by offsetting their anchoredPosition and
    /// hiding the ones outside the window with a CanvasGroup. Vanilla buttons (and anything at or below the highest
    /// vanilla row) stay put. KKAPI owns the layout: <see cref="Clear"/> must run before it relays out and
    /// <see cref="Apply"/> after, otherwise its bounds test and saved positions are corrupted.
    /// </summary>
    internal sealed class ToolbarScroller
    {
        private const float RowStep = 40f;
        private const int ScreenMargin = 5;

        private sealed class Entry
        {
            public RectTransform Rect;
            public CanvasGroup Group;
            public float Offset;
            public float WrittenY;
        }

        private readonly List<Entry> _applied = new List<Entry>();
        private readonly List<RectTransform> _scrollable = new List<RectTransform>();
        private readonly List<int> _scrollableRows = new List<int>();
        private readonly Vector3[] _corners = new Vector3[4];
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();
        private PointerEventData _pointer;

        private RectTransform _dragged;
        private RectTransform _reference;
        private int _lastScreenW, _lastScreenH;
        private float _lastScale;
        private bool _failed;
        private int _scroll;

        /// <summary>Rows at the top of the window kept free for the arrow controls.</summary>
        public int ReserveRows = 1;

        public bool Overflowing { get; private set; }
        public int Scroll { get { return _scroll; } }
        public int MaxScroll { get; private set; }
        /// <summary>Rows of scrollable buttons that are visible at once.</summary>
        public int WindowRows { get; private set; }

        /// <summary>Screen-pixel line between the pinned rows and the scrolling rows; valid while <see cref="Overflowing"/>.</summary>
        public float DividerLeft, DividerY, DividerWidth;

        /// <summary>Screen-pixel centres of the two reserved arrow cells and the on-screen size scale of a button.</summary>
        public float ArrowLeftX, ArrowRightX, ArrowY, ButtonScale;

        /// <summary>Diagnostics: highest usable row, highest pinned row, highest row of any scrollable button.</summary>
        public int TopRow, PinnedMax, MaxRow;

        public event Action Changed;

        public void ScrollBy(int rows)
        {
            ScrollTo(_scroll + rows);
        }

        public void ScrollTo(int row)
        {
            int clamped = Mathf.Clamp(row, 0, MaxScroll);
            if (clamped == _scroll) return;
            Plugin.Trace("scroll " + _scroll + " -> " + clamped);
            _scroll = clamped;
            Apply();
        }

        /// <summary>Called from the drag hook: dragging math needs untouched positions, so go back to the first page.</summary>
        public void BeginDrag(Component dragHelper)
        {
            _dragged = dragHelper != null ? dragHelper.GetComponent<RectTransform>() : null;
            Plugin.Trace("drag begin: " + (dragHelper != null ? dragHelper.name : "?") + ", scroll was " + _scroll);
            if (_applied.Count == 0) return;
            _scroll = 0;
            Apply();
        }

        public void EndDrag()
        {
            Plugin.Trace("drag end");
            _dragged = null;
        }

        /// <summary>Puts every touched button back where KKAPI placed it.</summary>
        public void Clear()
        {
            for (int i = 0; i < _applied.Count; i++)
            {
                Entry e = _applied[i];
                if (e.Rect != null)
                {
                    Vector2 p = e.Rect.anchoredPosition;
                    // Only undo our own write; if something else moved the button since, leave it.
                    if (Mathf.Abs(p.y - e.WrittenY) < 0.01f)
                    {
                        p.y += e.Offset;
                        e.Rect.anchoredPosition = p;
                    }
                }
                if (e.Group != null)
                {
                    e.Group.alpha = 1f;
                    e.Group.blocksRaycasts = true;
                    e.Group.interactable = true;
                }
            }
            _applied.Clear();
        }

        /// <summary>Recomputes the layout from the live button positions and applies the current scroll.</summary>
        public void Apply()
        {
            if (_failed) return;
            try
            {
                Clear();
                Compute();
            }
            catch (Exception e)
            {
                _failed = true;
                Clear();
                Plugin.Log.LogError("Toolbar scrolling disabled after an error: " + e);
            }
            Action changed = Changed;
            if (changed != null) changed();
        }

        /// <summary>Per-frame check for screen size or UI scale changes. Allocation-free.</summary>
        public void Tick()
        {
            if (_failed || _reference == null) return;
            float scale = _reference.lossyScale.y;
            if (Screen.width != _lastScreenW || Screen.height != _lastScreenH || !Mathf.Approximately(scale, _lastScale))
                Apply();
        }

        private void Compute()
        {
            Overflowing = false;
            MaxScroll = 0;
            WindowRows = 0;
            _scrollable.Clear();
            _scrollableRows.Clear();

            ToolbarControlBase[] buttons = ToolbarManager.GetAllButtons(false);
            RectTransform reference = null;
            float refY = 0f;
            for (int i = 0; i < buttons.Length; i++)
            {
                RectTransform rt = ActiveRect(buttons[i]);
                if (rt == null || !IsVanilla(buttons[i])) continue;
                float y = rt.anchoredPosition.y;
                if (reference == null || y < refY) { reference = rt; refY = y; }
            }
            _reference = reference;
            if (reference == null) return;

            int pinnedMax = 0;
            float minX = float.MaxValue;
            for (int i = 0; i < buttons.Length; i++)
            {
                RectTransform rt = ActiveRect(buttons[i]);
                if (rt == null) continue;
                if (rt.anchoredPosition.x < minX) minX = rt.anchoredPosition.x;
                int row = Mathf.RoundToInt((rt.anchoredPosition.y - refY) / RowStep);
                if (IsVanilla(buttons[i]))
                {
                    if (row > pinnedMax) pinnedMax = row;
                }
                else
                {
                    _scrollable.Add(rt);
                    _scrollableRows.Add(row);
                }
            }

            // Third-party buttons at or below the highest vanilla row are pinned too.
            int maxRow = int.MinValue;
            for (int i = _scrollable.Count - 1; i >= 0; i--)
            {
                if (_scrollableRows[i] <= pinnedMax)
                {
                    _scrollable.RemoveAt(i);
                    _scrollableRows.RemoveAt(i);
                }
                else if (_scrollableRows[i] > maxRow)
                {
                    maxRow = _scrollableRows[i];
                }
            }

            _lastScreenW = Screen.width;
            _lastScreenH = Screen.height;
            _lastScale = reference.lossyScale.y;
            if (_scrollable.Count == 0) { _scroll = 0; return; }

            // Highest row whose button still passes KKAPI's own screen test (world corners within Screen +- margin).
            reference.GetWorldCorners(_corners);
            float refTop = _corners[1].y;
            float rowPx = RowStep * _lastScale;
            int topRow = Mathf.FloorToInt((Screen.height + ScreenMargin - refTop) / rowPx);

            // Cell centres in screen pixels, for the occlusion scan and the divider.
            float refW = _corners[2].x - _corners[0].x;
            float refCenterX = (_corners[0].x + _corners[2].x) * 0.5f;
            float refCenterY = (_corners[0].y + _corners[2].y) * 0.5f;
            float col0X = refCenterX + (minX - reference.anchoredPosition.x) * _lastScale;
            float col1X = col0X + RowStep * _lastScale;
            topRow = Math.Min(topRow, FirstCoveredRow(reference, pinnedMax + 1, topRow, col0X, col1X, refCenterY, rowPx) - 1);

            TopRow = topRow;
            PinnedMax = pinnedMax;
            MaxRow = maxRow;
            if (maxRow <= topRow) { _scroll = 0; return; }

            int windowTop = topRow - ReserveRows;
            WindowRows = windowTop - pinnedMax;
            if (WindowRows < 1)
            {
                _failed = true;
                Plugin.Log.LogWarning("Toolbar scrolling disabled: no room above the vanilla buttons");
                return;
            }
            Overflowing = true;
            ArrowLeftX = col0X;
            ArrowRightX = col1X;
            ArrowY = refCenterY + (windowTop + 1) * rowPx;
            ButtonScale = _lastScale;
            DividerLeft = col0X - refW * 0.5f;
            DividerWidth = (col1X - col0X) + refW;
            DividerY = refTop + pinnedMax * rowPx + (RowStep - refW / _lastScale) * 0.5f * _lastScale;
            MaxScroll = Math.Max(0, maxRow - pinnedMax - WindowRows);
            _scroll = Mathf.Clamp(_scroll, 0, MaxScroll);

            for (int i = 0; i < _scrollable.Count; i++)
            {
                RectTransform rt = _scrollable[i];
                int row = _scrollableRows[i] - _scroll;
                bool visible = (row > pinnedMax && row <= windowTop) || rt == _dragged;
                if (_scroll == 0 && visible) continue;

                var entry = new Entry { Rect = rt, Offset = _scroll * RowStep };
                Vector2 p = rt.anchoredPosition;
                p.y -= entry.Offset;
                rt.anchoredPosition = p;
                entry.WrittenY = rt.anchoredPosition.y;
                if (!visible)
                {
                    entry.Group = rt.GetComponent<CanvasGroup>() ?? rt.gameObject.AddComponent<CanvasGroup>();
                    entry.Group.alpha = 0f;
                    entry.Group.blocksRaycasts = false;
                    entry.Group.interactable = false;
                }
                _applied.Add(entry);
            }
        }

        /// <summary>
        /// First row in [from, to] where a cell of either column is covered by UI from another root canvas (the
        /// studio's main menu is drawn above the toolbar). Returns to + 1 if none. Same-canvas windows don't count,
        /// they come and go. If the very first row is covered it is a modal overlay, not the menu: ignore it all.
        /// </summary>
        private int FirstCoveredRow(RectTransform reference, int from, int to, float col0X, float col1X, float refCenterY, float rowPx)
        {
            EventSystem es = EventSystem.current;
            Canvas own = reference.GetComponentInParent<Canvas>();
            if (es == null || own == null) return to + 1;
            Canvas root = own.rootCanvas;
            if (_pointer == null) _pointer = new PointerEventData(es);

            for (int r = from; r <= to; r++)
            {
                float y = refCenterY + r * rowPx;
                if (IsCovered(es, root, col0X, y) || IsCovered(es, root, col1X, y))
                    return r == from ? to + 1 : r;
            }
            return to + 1;
        }

        private bool IsCovered(EventSystem es, Canvas toolbarRoot, float x, float y)
        {
            _pointer.position = new Vector2(x, y);
            _hits.Clear();
            es.RaycastAll(_pointer, _hits);
            for (int i = 0; i < _hits.Count; i++)
            {
                GameObject go = _hits[i].gameObject;
                Canvas c = go != null ? go.GetComponentInParent<Canvas>() : null;
                if (c == null) continue;
                return c.rootCanvas != toolbarRoot;
            }
            return false;
        }

        private static RectTransform ActiveRect(ToolbarControlBase b)
        {
            if (b == null || b.ButtonObject == null) return null;
            GameObject go = b.ButtonObject.gameObject;
            return go.activeSelf ? go.GetComponent<RectTransform>() : null;
        }

        // ToolbarControlAdapter is internal to KKAPI
        private static bool IsVanilla(ToolbarControlBase b)
        {
            return b.GetType().Name == "ToolbarControlAdapter";
        }
    }
}
