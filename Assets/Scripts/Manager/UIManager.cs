using System.Collections.Generic;
using Cannon.Cannonball;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manager {
    public class UIManager : MonoBehaviour {
        const int ItemSlotWidth = 75;
        const int InventorySize = 5;

        public Text healthText;
        public Transform inventoryContainer;
        public Transform inventoryItemSlot;

        public List<Transform> leftCannons;
        public List<Transform> rightCannons;

        #region Singleton

        public static UIManager Instance { get; private set; }

        #endregion


        void Awake() {
            Instance = this;

            for (var i = 1; i < InventorySize; i++) {
                var itemTransform = Instantiate(inventoryItemSlot, inventoryContainer).GetComponent<RectTransform>();
                itemTransform.gameObject.SetActive(true);
                itemTransform.Find("Selected fg").GetComponent<Image>().enabled = false;
                itemTransform.anchoredPosition = new Vector2(i * ItemSlotWidth, 0);
            }

        }

        public void SetHealth(int healthValue) {
            healthText.text = healthValue + "/100";
        }

        public void RefreshInventory(List<InventoryItem> items) {
            using var itemEnumerator = items.GetEnumerator();
            foreach (Transform slot in inventoryContainer) {
                var slotItem = slot.Find("Item");
                if (itemEnumerator.MoveNext()) {
                    var item = itemEnumerator.Current;
                    if (item == null) {
                        Debug.Log("Item is null");
                        break;
                    }
                    var cannonball = ResourceManager.Instance.GetResource<AbstractCannonball>(item.rId);
                    slotItem.gameObject.SetActive(true);
                    slotItem.Find("Sprite").GetComponent<Image>().sprite = cannonball.sprite;
                    slotItem.Find("Count").GetComponent<TextMeshProUGUI>().SetText(item.count.ToString());
                } else {
                    slotItem.gameObject.SetActive(false);
                    slotItem.Find("Sprite").GetComponent<Image>().sprite = null;
                    slotItem.Find("Count").GetComponent<TextMeshProUGUI>().SetText("0");
                }
            }
        }

        public void SelectSlot(int index) {
            var slotEnumerator = inventoryContainer.GetEnumerator();
            for (var i = 0; i < InventorySize; i++) {
                if (!slotEnumerator.MoveNext()) break;
                var current = (Transform) slotEnumerator.Current;
                current!.Find("Selected fg").GetComponent<Image>().enabled = i == index;
            }
        }

        public void SelectCannon(Side side, int index) {
            var cannons = Side.Left == side ? leftCannons : rightCannons;
            for (var i = 0; i < cannons.Count; i++) {
                cannons[i].Find("Selected fg").GetComponent<Image>().enabled = i == index;
            }
        }
    }
}