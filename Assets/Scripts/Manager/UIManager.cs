using System.Collections.Generic;
using Cannon.Cannonball;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manager {
    public class UIManager : MonoBehaviour {
        #region Singleton

        private static UIManager _uiManager;
        public static UIManager Instance => _uiManager;

        #endregion

        public Text healthText;
        public Transform inventoryContainer;
        public Transform inventoryItemSlot;

        private void Awake() {
            _uiManager = this;
        }

        public void SetHealth(int healthValue) {
            healthText.text = healthValue + "/100";
        }

        public void RefreshInventory(List<InventoryItem> items) {
            var x = 0;
            const int itemSlotWidth = 75;

            foreach (Transform child in inventoryContainer){
                if (child == inventoryItemSlot) continue;
                Destroy(child.gameObject);
            }

            foreach (var item in items){
                var itemTransform = Instantiate(inventoryItemSlot, inventoryContainer).GetComponent<RectTransform>();
                itemTransform.gameObject.SetActive(true);
                itemTransform.anchoredPosition = new Vector2(x * itemSlotWidth, 0);
                var cannonball = ResourceManager.Instance.GetResource<AbstractCannonball>(item.rId);
                itemTransform.Find("Sprite").GetComponent<Image>().sprite = cannonball.sprite;
                itemTransform.Find("Count").GetComponent<TextMeshProUGUI>().SetText(item.count.ToString());
                x++;
            }
        }
    }
}