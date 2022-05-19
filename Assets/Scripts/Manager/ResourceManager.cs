using System.Collections.Generic;
using Cannon.Cannonball;
using UnityEngine;

namespace Manager {
    public class ResourceManager : MonoBehaviour {
        #region Singleton

        private static ResourceManager _resourceManager;
        public static ResourceManager Instance => _resourceManager;

        #endregion

        public List<AbstractCannonball> cannonballs;

        private void Awake() {
            _resourceManager = this;
        }

        public AbstractCannonball GetCannonball(int id) {
            return cannonballs[id];
        }
    }
}