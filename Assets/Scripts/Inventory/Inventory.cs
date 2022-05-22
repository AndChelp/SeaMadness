using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Inventory {
    public class Inventory {
        private const int InventorySize = 5;

        private readonly List<InventoryItem> _inventory = new();

        public InventoryItem SelectedItemInput() {
            var inventoryIndex = InventoryInput.SelectedIndex();
            if (inventoryIndex == -1 || _inventory.Count <= inventoryIndex) return null;
            return _inventory[inventoryIndex];
        }

        public bool AddItem(int rId, int count) {
            Debug.Log("AddItem rId = " + rId + " count = " + count);
            var item = _inventory.Find(it => it.rId == rId);
            if (item != null){
                item.count += count;
                return true;
            }
            if (_inventory.Count <= InventorySize){
                _inventory.Add(new InventoryItem(rId, count));
                return true;
            }
            return false;
        }

        public bool UseItem(int rId) {
            Debug.Log("UseItem rId = " + rId);
            var item = _inventory.Find(it => it.rId == rId);
            if (item == null) throw new Exception("Item with rId = " + rId + " not found in inventory");
            if (--item.count <= 0){
                _inventory.Remove(item);
            }
            return true;
        }
    }
}