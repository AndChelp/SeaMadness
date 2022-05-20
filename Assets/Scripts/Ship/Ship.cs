using System;
using System.Collections.Generic;
using Cannon.Cannonball;
using Common;
using Manager;
using Mirror;
using UnityEngine;

namespace Ship {
    public class Ship : NetworkBehaviour, IDamageable {
        private Camera _camera;
        private Rigidbody _rb;
        [SyncVar] private int _currentHealth = 100;

        public float acceleration = 1500f;
        public int inventorySize = 5;

        public List<Cannon.Cannon> cannons;
        public GameObject aim;

        // todo не хочу, чтобы все клиенты знали об инвентарях других клиентов
        private readonly List<InventoryItem> _inventory = new();

        private void Awake() {
            _camera = Camera.main;
            _rb = transform.GetComponent<Rigidbody>();
        }

        public override void OnStartLocalPlayer() {
            base.OnStartLocalPlayer();
            if (!hasAuthority) return;
            UIManager.Instance.SetHealth(100);
            _camera.GetComponent<CameraFollowing>().SetPlayer(transform);
        }

        private void FixedUpdate() {
            if (!hasAuthority) return;
            var vertical = Input.GetAxis("Vertical");
            _rb.AddForce(-transform.right * (vertical * acceleration * Time.fixedDeltaTime), ForceMode.Acceleration);
            var horizontal = Input.GetAxis("Horizontal");
            _rb.AddTorque(new Vector3(0, horizontal * _rb.velocity.magnitude, 0), ForceMode.Acceleration);
        }

        private void Update() {
            if (!hasAuthority) return;
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo)) return;
            RotateCannons(hitInfo.point);
            ShotInput();
            SelectCannonballInput(hitInfo.point);
        }

        [Command]
        private void CmdAddToInventory(int rId, int count) {
            Debug.Log("CmdAddToInventory rId = " + rId + " count = " + count);
            if (AddToInventory(rId, count)){
                TargetAddToInventory(rId, count);
            }
        }

        [TargetRpc]
        private void TargetAddToInventory(int rId, int count) {
            Debug.Log("TargetAddToInventory rId = " + rId + " count = " + count);
            AddToInventory(rId, count);
        }

        private bool AddToInventory(int rId, int count) {
            var item = _inventory.Find(it => it.rId == rId);
            if (item != null){
                item.count += count;
                return true;
            }
            if (_inventory.Count <= inventorySize){
                _inventory.Add(new InventoryItem(rId, count));
                return true;
            }
            return false;
        }

        private void SelectCannonballInput(Vector3 point) {
            var activeCannonIndex = FindActiveCannonIndex(point);
            if (activeCannonIndex == -1) return;
            var inventoryIndex = InputInventoryIndex();
            if (inventoryIndex == -1 || _inventory.Count <= inventoryIndex) return;
            var cannon = cannons[activeCannonIndex];
            if (cannon.isCharged){
                Debug.Log("Cannon was charged, call discharge");
                CmdAddToInventory(cannon.DischargeCannonball(), 1);
            }
            CmdUseItem(cannon.RechargeCannonball(_inventory[inventoryIndex].rId));
        }

        private static int InputInventoryIndex() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha4)) return 3;
            if (Input.GetKeyDown(KeyCode.Alpha5)) return 4;
            return -1;
        }

        private int FindActiveCannonIndex(Vector3 point) {
            for (var cannonIndex = 0; cannonIndex < cannons.Count; cannonIndex++){
                var cannon = cannons[cannonIndex];
                if (!cannon.IsInDiapason(point)) continue;
                return cannonIndex;
            }

            return -1;
        }

        private void ShotInput() {
            if (!Input.GetMouseButtonDown(0)) return;
            for (var cannonIndex = 0; cannonIndex < cannons.Count; cannonIndex++){
                var cannon = cannons[cannonIndex];
                if (!cannon.IsShowPredicateLine()) continue;
                CmdCannonFire(cannonIndex, cannon.currentCbRId);
            }
        }

        [Command]
        private void CmdUseItem(int rId) {
            if (UseItem(rId)){
                TargetUseItem(rId);
            }
        }

        [TargetRpc]
        private void TargetUseItem(int rId) {
            UseItem(rId);
        }

        private bool UseItem(int rId) {
            Debug.Log("UseItem rId = " + rId);
            var item = _inventory.Find(it => it.rId == rId);
            if (item == null) throw new Exception("Item with rId = " + rId + " not found in inventory");
            if (--item.count <= 0){
                _inventory.Remove(item);
            }
            return true;
        }

        [Command]
        private void CmdCannonFire(int cannonIndex, int cbRId) {
            cannons[cannonIndex].LaunchCannonball(netId, cbRId);
            RpcCannonFire(cannonIndex, cbRId);
        }

        [ClientRpc]
        private void RpcCannonFire(int cannonIndex, int cbRId) {
            if (!isServer){
                cannons[cannonIndex].LaunchCannonball(netId, cbRId);
            }
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

        [ServerCallback]
        private void OnCollisionEnter(Collision other) {
            switch (other.collider.tag){
                case "barrel":
                    Debug.Log("OnCollisionEnter barrel");
                    if (AddToInventory(0, 1)){
                        TargetAddToInventory(0, 1);
                    }
                    break;
                case "shell":
                    Debug.Log("OnCollisionEnter shell");
                    var cannonball = other.collider.GetComponent<AbstractCannonball>();
                    if (cannonball.ownerNetId == netId) return;
                    TakeDamage(cannonball.damageAmount);
                    break;
            }
        }

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
    }
}