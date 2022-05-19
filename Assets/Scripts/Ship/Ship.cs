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

        public List<Cannon.Cannon> cannons;
        public GameObject aim;

        private void Awake() {
            _camera = Camera.main;
            _rb = transform.GetComponent<Rigidbody>();
        }

        public override void OnStartLocalPlayer() {
            if (!hasAuthority) return;
            UIManager.Instance.SetHealth(100);
            _camera.GetComponent<CameraFollowing>().SetPlayer(transform);
        }

        private void Update() {
            if (!hasAuthority) return;
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo)) return;
            RotateCannons(hitInfo.point);
            ShotInput();
            SelectCannonballInput(hitInfo.point);
        }

        private void SelectCannonballInput(Vector3 point) {
            var activeCannonIndex = FindActiveCannonIndex(point);
            if (activeCannonIndex < 0){
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)){
                cannons[activeCannonIndex].RechargeCannonball(0);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)){
                cannons[activeCannonIndex].RechargeCannonball(1);
            } else if (Input.GetKeyDown(KeyCode.Alpha3)){
                cannons[activeCannonIndex].RechargeCannonball(2);
            } else if (Input.GetKeyDown(KeyCode.Alpha4)){
                cannons[activeCannonIndex].RechargeCannonball(3);
            } else if (Input.GetKeyDown(KeyCode.Alpha5)){
                cannons[activeCannonIndex].RechargeCannonball(4);
            }
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

        private void FixedUpdate() {
            if (!hasAuthority) return;
            var vertical = Input.GetAxis("Vertical");
            _rb.AddForce(-transform.right * (vertical * acceleration * Time.fixedDeltaTime), ForceMode.Acceleration);
            var horizontal = Input.GetAxis("Horizontal");
            _rb.AddTorque(new Vector3(0, horizontal * _rb.velocity.magnitude, 0), ForceMode.Acceleration);
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
            if (!other.collider.CompareTag("shell")) return;
            var cannonball = other.collider.GetComponent<AbstractCannonball>();
            if (cannonball.ownerNetId == netId){
                return;
            }

            TakeDamage(cannonball.damageAmount);
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