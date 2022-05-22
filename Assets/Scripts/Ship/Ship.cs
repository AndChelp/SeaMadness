using System;
using System.Collections.Generic;
using Cannon.Cannonball;
using Common;
using Manager;
using Mirror;
using UnityEngine;

namespace Ship {
    public class Ship : NetworkBehaviour, IDamageable {
        private Inventory.Inventory _inventory;
        private Camera _camera;
        private Rigidbody _rb;
        [SyncVar] private int _currentHealth = 100;

        public float acceleration = 1500f;

        public List<Cannon.Cannon> cannons;
        public GameObject aim;

        #region Initialization

        private void Awake() {
            _camera = Camera.main;
            _rb = transform.GetComponent<Rigidbody>();
            _inventory = new Inventory.Inventory();
        }

        public override void OnStartLocalPlayer() {
            base.OnStartLocalPlayer();
            if (!hasAuthority) return;
            UIManager.Instance.SetHealth(100);
            _camera.GetComponent<CameraFollowing>().SetPlayer(transform);
        }

        #endregion

        private void FixedUpdate() {
            if (!hasAuthority) return;
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");
            _rb.AddForce(-transform.right * (vertical * acceleration * Time.fixedDeltaTime), ForceMode.Acceleration);
            var torqueVelocity = Math.Sign(vertical) * horizontal * _rb.velocity.magnitude;
            _rb.AddTorque(new Vector3(0, torqueVelocity, 0), ForceMode.Acceleration);
        }

        private void Update() {
            if (!hasAuthority) return;
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo)) return;
            RotateCannons(hitInfo.point);
            ShotInput();
            ChargeCannonballInput(hitInfo.point);
        }

        private void RotateCannons(Vector3 point) {
            var anyCanAim = false;
            foreach (var cannon in cannons){
                if (cannon.CanAim(point)){
                    anyCanAim = true;
                    cannon.Aim(point);
                    cannon.ShowPredicateLine(true);
                } else{
                    cannon.ShowPredicateLine(false);
                }
            }

            if (anyCanAim){
                aim.SetActive(true);
                aim.transform.position = point;
            } else{
                aim.SetActive(false);
            }
        }

        #region Shooting

        private void ShotInput() {
            if (!Input.GetMouseButtonDown(0)) return;
            for (var cannonIndex = 0; cannonIndex < cannons.Count; cannonIndex++){
                var cannon = cannons[cannonIndex];
                if (!cannon.IsShowPredicateLine()) continue;
                CmdCannonFire(cannonIndex, cannon.currentCbRId);
            }
        }

        [Command]
        private void CmdCannonFire(int cannonIndex, int cbRId) {
            RpcCannonFire(cannonIndex, cbRId);
        }

        [ClientRpc]
        private void RpcCannonFire(int cannonIndex, int cbRId) {
            cannons[cannonIndex].LaunchCannonball(netId, cbRId);
        }

        #endregion

        #region Charging

        private void ChargeCannonballInput(Vector3 point) {
            var activeCannonIndex = FindActiveCannonIndex(point);
            if (activeCannonIndex == -1) return;
            var item = _inventory.SelectedItemInput();
            if (item == null) return;
            var cannon = cannons[activeCannonIndex];
            if (cannon.isCharged){
                Debug.Log("Cannon was charged, calling discharge");
                CmdAddToInventory(cannon.DischargeCannonball(), 1);
            }
            CmdUseItem(cannon.RechargeCannonball(item.rId));
        }

        [Command]
        private void CmdAddToInventory(int rId, int count) {
            Debug.Log("CmdAddToInventory rId = " + rId + " count = " + count);
            if (_inventory.AddItem(rId, count)) TargetAddToInventory(rId, count);
        }

        [TargetRpc]
        private void TargetAddToInventory(int rId, int count) {
            Debug.LogAssertion("TargetAddToInventory rId = " + rId + " count = " + count);
            _inventory.AddItem(rId, count);
        }

        private int FindActiveCannonIndex(Vector3 point) {
            for (var cannonIndex = 0; cannonIndex < cannons.Count; cannonIndex++){
                var cannon = cannons[cannonIndex];
                if (!cannon.IsInDiapason(point)) continue;
                return cannonIndex;
            }
            return -1;
        }

        #endregion

        #region Inventory

        [Command]
        private void CmdUseItem(int rId) {
            if (_inventory.UseItem(rId)) TargetUseItem(rId);
        }

        [TargetRpc]
        private void TargetUseItem(int rId) {
            _inventory.UseItem(rId);
        }

        #endregion

        #region Collisions

        [ServerCallback]
        private void OnTriggerEnter(Collider other) {
            switch (other.tag){
                case "barrel":
                    Debug.Log("OnCollisionEnter barrel");
                    if (_inventory.AddItem(0, 1)) TargetAddToInventory(0, 1);
                    break;
            }
        }

        [ServerCallback]
        private void OnCollisionEnter(Collision other) {
            switch (other.collider.tag){
                case "shell":
                    Debug.Log("OnCollisionEnter shell");
                    var cannonball = other.collider.GetComponent<AbstractCannonball>();
                    if (cannonball.ownerNetId == netId) return;
                    TakeDamage(cannonball.damageAmount);
                    break;
            }
        }

        #endregion

        #region Health

        public void TakeDamage(int amount) {
            var currentHealth = _currentHealth -= amount;
            TargetUpdateHealth(currentHealth);
            if (amount >= 0 && currentHealth <= 0){
                Die();
            }
        }

        [TargetRpc]
        private void TargetUpdateHealth(int healthValue) {
            UIManager.Instance.SetHealth(healthValue);
        }

        public void Die() {
            NetworkServer.Destroy(gameObject);
        }

        #endregion
    }
}