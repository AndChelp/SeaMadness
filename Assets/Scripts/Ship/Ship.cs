using System;
using System.Collections.Generic;
using Cannon;
using Cannon.Cannonball;
using Common;
using Manager;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ship {
    public class Ship : NetworkBehaviour, IDamageable {
        public float acceleration = 1500f;
        public List<Cannon.Cannon> cannons;
        public GameObject aim;
        Camera _camera;

        CannonsController _cannonsController;
        [SyncVar]
        int _currentHealth = 100;

        Inventory.Inventory _inventory;
        Rigidbody _rb;

        int a;

        void Update() {
            if (!hasAuthority) return;
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo)) return;
            _cannonsController.CurrentSide = ShipSideInput(hitInfo.point);
            _cannonsController.RotateTo(hitInfo.point);
            _inventory.SelectSlotInput();
            ShotInput();
            RechargeInput();
            SelectCannonForRechargeInput();
        }

        void FixedUpdate() {
            if (!hasAuthority) return;
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");
            _rb.AddForce(-transform.right * (vertical * acceleration * Time.fixedDeltaTime), ForceMode.Acceleration);
            var torqueVelocity = Math.Sign(vertical) * horizontal * _rb.velocity.magnitude;
            _rb.AddTorque(new Vector3(0, torqueVelocity, 0), ForceMode.Acceleration);
        }
        void SelectCannonForRechargeInput() {
            var scrollDeltaY = Input.mouseScrollDelta.y;
            if (scrollDeltaY == 0) return;
            _cannonsController.SetCannonForRecharge((int) scrollDeltaY % 2);
        }

        //todo переделать
        Side ShipSideInput(Vector3 point) {
            var direction = (point - transform.position).normalized;
            var angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(direction));
            return angle <= 90 ? Side.Right : Side.Left;
        }

        #region Charging

        void RechargeInput() {
            if (!Input.GetKeyDown(KeyCode.R)) return;

            var cannon = _cannonsController.GetCannonForRecharge();
            var item = _inventory.SelectedItem();
            if (item == null) return;
            if (cannon.isCharged) {
                Debug.Log("Cannon was charged, calling discharge");
                CmdAddToInventory(cannon.DischargeCannonball(), 1);
            }
            CmdUseItem(cannon.RechargeCannonball(item.rId));
        }

        #endregion

        #region Initialization

        void Awake() {
            _camera = Camera.main;
            _rb = transform.GetComponent<Rigidbody>();
            _inventory = new Inventory.Inventory();
            _cannonsController = new CannonsController(cannons, aim);
        }

        public override void OnStartLocalPlayer() {
            base.OnStartLocalPlayer();
            if (!hasAuthority) return;
            UIManager.Instance.SetHealth(100);
            _camera.GetComponent<CameraFollowing>().SetPlayer(transform);
        }

        #endregion

        #region Shooting

        void ShotInput() {
            if (!Input.GetMouseButtonDown(0)) return;
            _cannonsController.ForEachActive((index, cannon) => CmdCannonFire(index, cannon.currentCbRId));
        }

        [Command]
        void CmdCannonFire(int cannonIndex, int cbRId) {
            if (_cannonsController.Fire(netId, cannonIndex, cbRId)) RpcCannonFire(cannonIndex, cbRId);
        }

        [ClientRpc]
        void RpcCannonFire(int cannonIndex, int cbRId) {
            if (!isServer) _cannonsController.Fire(netId, cannonIndex, cbRId);
        }

        #endregion

        #region Inventory

        [Command]
        void CmdAddToInventory(int rId, int count) {
            Debug.Log("CmdAddToInventory rId = " + rId + " count = " + count);
            if (_inventory.AddItem(rId, count)) TargetAddToInventory(rId, count);
        }

        [TargetRpc]
        void TargetAddToInventory(int rId, int count) {
            Debug.Log("TargetAddToInventory rId = " + rId + " count = " + count);
            if (!isServer) _inventory.AddItem(rId, count);
        }

        [Command]
        void CmdUseItem(int rId) {
            if (_inventory.UseItem(rId)) TargetUseItem(rId);
        }

        [TargetRpc]
        void TargetUseItem(int rId) {
            if (!isServer) _inventory.UseItem(rId);
        }

        #endregion

        #region Collisions

        [ServerCallback]
        void OnTriggerEnter(Collider other) {
            switch (other.tag) {
                case "barrel":
                    Debug.Log("OnCollisionEnter barrel");
                    var rId = Random.Range(0, 5);
                    var count = Random.Range(1, 5);
                    if (_inventory.AddItem(rId, count)) TargetAddToInventory(rId, count);
                    break;
            }
        }

        [ServerCallback]
        void OnCollisionEnter(Collision other) {
            switch (other.collider.tag) {
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
            if (amount >= 0 && currentHealth <= 0) {
                Die();
            }
        }

        [TargetRpc]
        void TargetUpdateHealth(int healthValue) {
            UIManager.Instance.SetHealth(healthValue);
        }

        public void Die() {
            NetworkServer.Destroy(gameObject);
        }

        #endregion
    }
}