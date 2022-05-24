using System;
using UnityEngine;

public class CameraFollowing : MonoBehaviour {
    public int cameraDistance = 60;
    public int cameraHeight = 60;
    public float rotationSpeed = 2;

    private Transform _player;

    private void Update() {
        if (_player == null){
            return;
        }
        var newPosition = _player.transform.position + _player.transform.right * cameraDistance;
        newPosition.y += cameraHeight;
        transform.position = Vector3.Slerp(transform.position, newPosition, Time.deltaTime * rotationSpeed);
        transform.LookAt(_player.transform.position);
    }

    public void SetPlayer(Transform player) {
        _player = player;
    }
}