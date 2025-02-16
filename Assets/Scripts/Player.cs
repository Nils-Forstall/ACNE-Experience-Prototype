using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField, Min(0f)]
	float movementSpeed = 4f, rotationSpeed = 180f;


	private bool isMovingForward;
	private bool isMovingBackward;


	[SerializeField]
	float startingVerticalEyeAngle = 10f;

	CharacterController characterController;

	Transform eye;

	Vector2 eyeAngles;

	void Awake()
	{
		characterController = GetComponent<CharacterController>();
		eye = transform.GetChild(0);
	}

	public void StartNewGame(Vector3 position)
	{
		eyeAngles.x = Random.Range(0f, 360f);
		eyeAngles.y = startingVerticalEyeAngle;
		characterController.enabled = false;
		transform.localPosition = position;
		characterController.enabled = true;
	}

	public Vector3 Move()
	{
		UpdateEyeAngles();
		UpdatePosition();
		return transform.localPosition;
	}
	void UpdatePosition()
	{
		float forwardMovement = Input.GetAxis("Vertical") * movementSpeed;
		Vector3 movement = eye.forward * forwardMovement;
		characterController.Move(movement * Time.deltaTime);

		if (forwardMovement > 0)
		{
			if (!isMovingForward)
			{
				PlayForwardAudio();
			}
		}
		else if (forwardMovement < 0)
		{
			if (!isMovingBackward)
			{
				PlayBackwardAudio();
			}
		}
		else
		{
			StopAudio();
		}
	}

	void PlayForwardAudio()
	{
		AudioManager.Instance.PlaySound("Bongo 2");
	}

	void PlayBackwardAudio()
	{
		AudioManager.Instance.PlaySound("Bongo 1");
	}
	void StopAudio()
	{
		AudioManager.Instance.StopSound("Bongo 1");
		AudioManager.Instance.StopSound("Bongo 2");
		isMovingForward = false;
		isMovingBackward = false;
	}

	void UpdateEyeAngles()
	{
		float rotationDelta = rotationSpeed * Time.deltaTime;

		// Use left/right arrow keys or A/D to rotate left/right
		eyeAngles.x += rotationDelta * Input.GetAxis("Horizontal");

		// Clamp vertical angle to prevent unnatural movement
		// eyeAngles.y = Mathf.Clamp(eyeAngles.y, -45f, 45f);

		// Rotate camera (eye) and update player's horizontal rotation
		eye.localRotation = Quaternion.Euler(eyeAngles.y, eyeAngles.x, 0f);
		transform.rotation = Quaternion.Euler(0f, eyeAngles.x, 0f); // Rotate player body
	}

}
