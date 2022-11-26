using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Manager {
    public class ResourceManager : MonoBehaviour {
        public List<MonoResource> resources;

        public T GetResource<T>(int id) where T : MonoResource {
            var resource = resources[id];
            var requiredType = typeof(T);
            if (resource.GetType().IsInstanceOfType(requiredType)) {
                throw new Exception("Required type " + requiredType.Name + " but resource with rId = " + id +
                                    " is " + resource.GetType().Name);
            }
            return (T) resource;
        }

        #region Singleton

        public static ResourceManager Instance { get; private set; }

        void Awake() {
            Instance = this;
        }

        #endregion
    }
}