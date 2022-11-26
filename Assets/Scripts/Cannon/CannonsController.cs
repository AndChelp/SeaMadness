using System;
using System.Collections.Generic;
using Common;
using Manager;
using UnityEngine;

namespace Cannon {
    public class CannonsController {
        readonly GameObject _aim;
        readonly List<Cannon> _leftCannons;
        readonly List<Cannon> _rightCannons;
        int _leftActiveCannon;
        int _rightActiveCannon;


        public CannonsController(List<Cannon> cannons, GameObject aim) {
            _leftCannons = cannons.FindAll(it => it.side == Side.Left);
            _rightCannons = cannons.FindAll(it => it.side == Side.Right);
            _aim = aim;
        }
        public Side CurrentSide { get; set; }

        List<Cannon> CannonsBySide(Side side) => Side.Left == side ? _leftCannons : _rightCannons;

        int CannonIndexBySide(Side side) => Side.Left == side ? _leftActiveCannon : _rightActiveCannon;

        public void SetCannonForRecharge(int delta) {
            if (Side.Left == CurrentSide && (delta == 1 && _leftActiveCannon == 0 || delta == -1 && _leftActiveCannon == 1)) {
                _leftActiveCannon += delta;
                UIManager.Instance.SelectCannon(CurrentSide, _leftActiveCannon);
                Debug.Log(CurrentSide + " " + _leftActiveCannon + " " + _rightActiveCannon);
            } else if (Side.Right == CurrentSide && delta == 1 && _rightActiveCannon == 0 || delta == -1 && _rightActiveCannon == 1) {
                _rightActiveCannon += delta;
                UIManager.Instance.SelectCannon(CurrentSide, _rightActiveCannon);
                Debug.Log(CurrentSide + " " + _leftActiveCannon + " " + _rightActiveCannon);
            }
        }

        public void RotateTo(Vector3 point) {
            var anyCanAim = false;
            foreach (var cannon in CannonsBySide(CurrentSide)) {
                if (cannon.CanAim(point)) {
                    anyCanAim = true;
                    cannon.Aim(point);
                    cannon.ShowPredicateLine(true);
                } else {
                    cannon.ShowPredicateLine(false);
                }
            }

            if (anyCanAim) {
                _aim.SetActive(true);
                _aim.transform.position = point;
            } else {
                _aim.SetActive(false);
            }
        }

        public bool Fire(uint netId, int cannonIndex, int cbRId) => CannonsBySide(CurrentSide)[cannonIndex].LaunchCannonball(netId, cbRId);

        public Cannon GetCannonForRecharge() => CannonsBySide(CurrentSide)[CannonIndexBySide(CurrentSide)];

        public void ForEachActive(Action<int, Cannon> action) {
            var cannons = CannonsBySide(CurrentSide);
            for (var cannonIndex = 0; cannonIndex < cannons.Count; cannonIndex++) {
                var cannon = cannons[cannonIndex];
                if (!cannon.IsShowPredicateLine()) continue;
                action.Invoke(cannonIndex, cannon);
            }
        }
    }
}