using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    private Transform player;
    public Vector3 offset;

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        transform.position = player.position + offset;
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }
}