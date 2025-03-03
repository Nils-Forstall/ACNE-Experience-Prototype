using UnityEngine;


public class MazeWall : MonoBehaviour
{
    public float detectionRange = 0.5f; // Distance at which camera is considered "colliding"

    private void Update()
    {
        // DetectCameraCollision();
    }

    private void DetectCameraCollision()
    {
        // Find the main camera
        Camera playerCamera = Camera.main;
        if (playerCamera == null) return;

        // Cast a ray from the camera's position in the forward direction
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionRange))
        {
            // Check if the object hit is a wall
            if (hit.collider.gameObject == gameObject)
            {
                // Debug.Log($"Collision: Player collided {gameObject.name} (Wall)");
                AudioManager.Instance.StopSound("ForwardBackward");
                AudioManager.Instance.PlaySound("Collision 2");
            }
        }
    }
}