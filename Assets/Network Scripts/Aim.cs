using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Aim : NetworkBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo)) return;
        transform.position = hitInfo.point; 
    }
}