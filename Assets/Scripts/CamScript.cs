using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class CamScript : MonoBehaviour
{
    // MAKE SURE TO SET THE PLAYER OBJECT IN THE EDITOR
    [SerializeField] private Transform player;
    [SerializeField] private float cameraSpeed = 2f;
    private Vector3 offset = new(1f, 0.5f, -5f);

    private void Start() {
        transform.position = player.position + offset;
    }

    // Follow player
    void Update () {
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, cameraSpeed*2 * Time.deltaTime);
        transform.position = smoothedPosition;
        CheckPlayerDir();
    }
    
    // Make the camera always be offset so it is showing more space in front of the player.
    void CheckPlayerDir() {
        Vector3 targetOffset;
        if (player.transform.localScale.x == 1)
            targetOffset = new Vector3 (1f, 0.5f, -5f);
        else
            targetOffset = new Vector3 (-1f, 0.5f, -5f);
        offset = Vector3.Lerp(offset, targetOffset, cameraSpeed * Time.deltaTime);
    }
}
