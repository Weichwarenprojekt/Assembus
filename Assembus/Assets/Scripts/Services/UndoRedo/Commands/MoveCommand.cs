﻿using System.Collections.Generic;
using System.Linq;
using MainScreen.Sidebar.HierarchyView;
using Services.UndoRedo.Models;
using Shared;
using UnityEngine;

namespace Services.UndoRedo.Commands
{
    public class MoveCommand : Command
    {
        /// <summary>
        ///     The item states
        /// </summary>
        private readonly List<ItemState> _oldStates, _newStates;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="oldStates">The old item states</param>
        /// <param name="newStates">The new item states</param>
        public MoveCommand(List<ItemState> oldStates, List<ItemState> newStates)
        {
            _oldStates = oldStates;
            _newStates = newStates;
        }

        /// <summary>
        ///     Undo the move
        /// </summary>
        public override void Undo()
        {
            foreach (var state in _oldStates) Move(state);
        }

        /// <summary>
        ///     Redo the move
        /// </summary>
        public override void Redo()
        {
            foreach (var state in _newStates) Move(state);
        }

        /// <summary>
        ///     Move an item
        /// </summary>
        /// <param name="state">The item state</param>
        public static void Move(ItemState state)
        {
            if (state.ID == state.NeighbourID) return;

            // Get the item and the parent
            var hierarchyItem = Utility.FindChild(HierarchyView.transform, state.ID);
            var container = HierarchyView.transform;
            var hierarchyParent = Utility.FindChild(HierarchyView.transform, state.ParentID);
            if (hierarchyParent != null)
                container = hierarchyParent.GetComponent<HierarchyItemController>().childrenContainer.transform;

            // Get the sibling index
            var siblingIndex = GetSiblingIndex(state, hierarchyItem, container);

            // Move the item
            var oldParent = hierarchyItem.parent.parent;
            hierarchyItem.SetParent(container);
            hierarchyItem.SetSiblingIndex(siblingIndex);

            // Move the item in the actual object
            var modelItem = Utility.FindChild(Model.transform, state.ID);
            var modelParent = Utility.FindChild(Model.transform, state.ParentID);
            if (modelParent == null) modelParent = Model.transform;
            modelItem.SetParent(modelParent);
            modelItem.SetSiblingIndex(siblingIndex);

            // Check if the parent needs to be collapsed
            var oldParentItem = oldParent.GetComponent<HierarchyItemController>();
            if (oldParentItem != null) oldParentItem.ExpandItem(true);

            // Expand the parent and indent the item
            var indentionDepth = 0f;
            if (hierarchyParent != null)
            {
                var parentItem = hierarchyParent.GetComponent<HierarchyItemController>();
                indentionDepth = parentItem.GetIndention() + 16f;
                parentItem.ExpandItem(true);
            }

            IndentItems(hierarchyItem, indentionDepth);

            // Update the button visuals
            hierarchyItem.GetComponent<HierarchyItemController>().UpdateVisuals();
        }

        /// <summary>
        ///     Calculate the sibling index of an item
        /// </summary>
        /// <param name="state">The item state</param>
        /// <param name="item">The actual item in the list view</param>
        /// <param name="parent">The parent</param>
        /// <returns>The sibling index</returns>
        private static int GetSiblingIndex(ItemState state, Transform item, Transform parent)
        {
            // Check if item should be last
            var siblingIndex = parent.childCount;
            if (state.NeighbourID == ItemState.Last) return siblingIndex;

            // Check if item should be first
            var sibling = Utility.FindChild(
                HierarchyView.transform,
                state.NeighbourID
            );
            siblingIndex = 0;
            if (sibling == null) return siblingIndex;

            // Calculate the sibling index
            var sameParent = item.parent == sibling.parent;
            var lowerSibling = item.GetSiblingIndex() < sibling.transform.GetSiblingIndex();
            var offset = sameParent && lowerSibling ? 0 : 1;
            siblingIndex = sibling.GetSiblingIndex() + offset;

            return siblingIndex;
        }

        /// <summary>
        ///     Recursively indent the items
        /// </summary>
        /// <param name="item">The item to be corrected</param>
        /// <param name="indentionDepth">The indention depth</param>
        private static void IndentItems(Transform item, float indentionDepth)
        {
            // Indent the children
            var listItem = item.GetComponent<HierarchyItemController>();
            var childrenContainer = listItem.childrenContainer.transform;
            for (var i = 0; i < childrenContainer.childCount; i++)
                IndentItems(childrenContainer.GetChild(i), indentionDepth + HierarchyViewController.Indention);

            // Indent the item
            listItem.IndentItem(indentionDepth);
        }

        /// <summary>
        ///     Check if the GameObject with the given ID has been part of the MoveCommand
        /// </summary>
        /// <param name="id">The id of the GameObject</param>
        /// <returns>Returns if the given GameObject has been part of the MoveCommand</returns>
        public bool ContainsItem(string id)
        {
            return _oldStates.Any(state => state.ID == id);
        }
    }
}