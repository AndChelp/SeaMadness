using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cannon {
    public class CannonsController {
        private readonly List<Cannon> _cannons;
        private readonly GameObject _aim;

        public CannonsController(List<Cannon> cannons, GameObject aim) {
            _cannons = cannons;
            _aim = aim;
        }

        public void RotateTo(Vector3 point) {
            var anyCanAim = false;
            foreach (var cannon in _cannons){
                if (cannon.CanAim(point)){
                    anyCanAim = true;
                    cannon.Aim(point);
                    cannon.ShowPredicateLine(true);
                } else{
                    cannon.ShowPredicateLine(false);
                }
            }

            if (anyCanAim){
                _aim.SetActive(true);
                _aim.transform.position = point;
            } else{
                _aim.SetActive(false);
            }
        }

        public void Fire(uint netId, int cannonIndex, int cbRId) {
            _cannons[cannonIndex].LaunchCannonball(netId, cbRId);
        }

        public Cannon AccessibleCannon(Vector3 point) {
            return _cannons.FirstOrDefault(it => it.IsInDiapason(point));
        }

        public void ForEachActive(Action<int, Cannon> action) {
            for (var cannonIndex = 0; cannonIndex < _cannons.Count; cannonIndex++){
                var cannon = _cannons[cannonIndex];
                if (!cannon.IsShowPredicateLine()) continue;
                action.Invoke(cannonIndex, cannon);
            }
        }
    }
}