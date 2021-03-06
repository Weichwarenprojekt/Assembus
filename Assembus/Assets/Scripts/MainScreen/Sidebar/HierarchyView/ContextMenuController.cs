﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainScreen.Sidebar.HierarchyView
{
    public class ContextMenuController : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        ///     The available icons
        /// </summary>
        public Texture delete, add, show, hide, edit, folder, fuse, defuse, skipTo;

        /// <summary>
        ///     The default item
        /// </summary>
        public GameObject defaultItem;

        /// <summary>
        ///     The list view of the context menu
        /// </summary>
        public Transform listView;

        /// <summary>
        ///     The main canvas
        /// </summary>
        public Canvas mainCanvas;

        /// <summary>
        ///     True if the context menu should be shown
        /// </summary>
        private bool _show;

        /// <summary>
        ///     Position the context menu and show it
        /// </summary>
        private void LateUpdate()
        {
            if (!_show) return;
            var rectListView = listView.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectListView);

            // Position the item
            listView.position = Input.mousePosition;
            var height = (rectListView.rect.height + 16) * mainCanvas.transform.localScale.y;
            var difference = listView.position.y - height;
            if (difference < 0) listView.position += new Vector3(0, -difference, 0);
            _show = false;
        }

        /// <summary>
        ///     Hide the context menu
        /// </summary>
        private void OnMouseDown()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        ///     Hide the context menu if the user clicked outside of the panel
        /// </summary>
        /// <param name="eventData">The event data</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        ///     Show the context menu
        /// </summary>
        /// <param name="items">The items to be shown</param>
        public void Show(List<Item> items)
        {
            // Hide the panel
            gameObject.SetActive(false);

            // Remove old items
            for (var i = 1; i < listView.childCount; i++) Destroy(listView.GetChild(i).gameObject);

            // Show the default item
            defaultItem.SetActive(true);

            // Apply data to listview
            foreach (var item in items)
            {
                // Create new listview item by instantiating a new prefab
                var newListViewItem = Instantiate(defaultItem, listView, true);

                // Set the new values
                var nameText = newListViewItem.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                nameText.text = item.Name;
                var iconImage = newListViewItem.transform.Find("Icon").GetComponent<RawImage>();
                iconImage.texture = item.Icon;
                newListViewItem.GetComponent<Button>().onClick.AddListener(
                    () =>
                    {
                        item.Action();
                        gameObject.SetActive(false);
                    }
                );
            }

            // Hide the default item
            defaultItem.SetActive(false);

            // Show the context menu
            gameObject.SetActive(true);
            _show = true;
        }

        /// <summary>
        ///     The class for one context menu entry
        /// </summary>
        public class Item
        {
            /// <summary>
            ///     The action of the entry
            /// </summary>
            public Action Action;

            /// <summary>
            ///     The icon of the entry
            /// </summary>
            public Texture Icon;

            /// <summary>
            ///     The name of the entry
            /// </summary>
            public string Name;
        }
    }
}