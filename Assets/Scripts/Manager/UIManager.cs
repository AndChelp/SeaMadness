using UnityEngine;
using UnityEngine.UI;

namespace Manager {
    public class UIManager : MonoBehaviour {
        #region Singleton

        private static UIManager _uiManager;
        public static UIManager Instance => _uiManager;

        #endregion

        public Text healthText;

        private void Awake() {
            _uiManager = this;
        }

        public void SetHealth(int healthValue) {
            healthText.text = healthValue + "/100";
        }
    }
}