// SPDX-License-Identifier: GPL-3.0-or-later
using KK_StudioToolbarScroll.Core;
using UnityEngine;
using UnityEngine.UI;

namespace KK_StudioToolbarScroll.UI
{
    /// <summary>
    /// A thin line across both toolbar columns between the pinned (vanilla) rows and the rows that scroll.
    /// Only shown while the toolbar overflows.
    /// </summary>
    internal sealed class ScrollDivider
    {
        private const float ThicknessPx = 2f;

        private readonly ToolbarScroller _scroller;
        private RectTransform _rect;
        private GameObject _go;

        public ScrollDivider(ToolbarScroller scroller)
        {
            _scroller = scroller;
            scroller.Changed += Refresh;
        }

        private void Refresh()
        {
            if (!_scroller.Overflowing)
            {
                if (_go != null) _go.SetActive(false);
                return;
            }
            if (_go == null && !Create()) return;

            // The parent is the overlay canvas (world units = pixels), scaled by the canvas scaler only.
            float scale = _rect.parent.lossyScale.y;
            if (scale <= 0f) return;
            _go.SetActive(true);
            _rect.sizeDelta = new Vector2(_scroller.DividerWidth / scale, ThicknessPx / scale);
            _rect.position = new Vector3(_scroller.DividerLeft, _scroller.DividerY, 0f);
        }

        private bool Create()
        {
            // Beside 01_Button, not inside it: other plugins may enumerate the toolbar's children.
            GameObject toolbar = GameObject.Find("StudioScene/Canvas System Menu/01_Button");
            if (toolbar == null) return false;

            _go = new GameObject("KK_StudioToolbarScroll Divider", typeof(RectTransform), typeof(Image));
            _rect = (RectTransform)_go.transform;
            _rect.SetParent(toolbar.transform.parent, false);
            _rect.anchorMin = _rect.anchorMax = Vector2.zero;
            _rect.pivot = new Vector2(0f, 0.5f);
            _rect.SetSiblingIndex(toolbar.transform.GetSiblingIndex() + 1);

            Image image = _go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.75f);
            image.raycastTarget = false;
            return true;
        }
    }
}
