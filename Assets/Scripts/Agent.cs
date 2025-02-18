using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Agent : MonoBehaviour

{

	[SerializeField] float visionRange = 10f; // Max distance villain can see
    [SerializeField] float visionAngle = 90f; // Field of view in degrees

    [SerializeField] float detectionRange = 15f; 
    [SerializeField] Transform player; // Reference to player

	[SerializeField]
	Color color = Color.white;

	[SerializeField, Min(0f)]
	float speed = 1f;

	[SerializeField]
	 string triggerMessage;

	[SerializeField]
	bool isGoal;

	Maze maze;

	int targetIndex;

	Vector3 targetPosition;
	bool alarmPlaying;
	bool nearPlaying;

	public string TriggerMessage => triggerMessage;



	void Awake ()
	{
		GetComponent<Light>().color = color;
		GetComponent<MeshRenderer>().material.color = color;
		ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
		main.startColor = color;
		gameObject.SetActive(false);
	}

	public void StartNewGame (Maze maze, int2 coordinates)
	{
		this.maze = maze;
		targetIndex = maze.CoordinatesToIndex(coordinates);
		targetPosition = transform.localPosition =
			maze.CoordinatesToWorldPosition(coordinates, transform.localPosition.y);
		gameObject.SetActive(true);
	}
	

	void Update()
    {
		if (CanSeePlayer() && IsVeryNearPlayer())
        {
			playAlarm();
        } else {
			stopAlarm();
		}
		if (IsNearPlayer()){
			Debug.Log("Player near you!");
			playNear();
		} else {
			stopNear();
		}
        
    }


	void playNear(){
		if (!nearPlaying){
			AudioManager.Instance.PlaySoundOn(gameObject, "Blue Box v2");
			nearPlaying = true;
		}
	}

	void stopNear(){
		AudioManager.Instance.StopSound("Blue Box v2");
		nearPlaying = false;
	}


	void playAlarm(){
		if (!alarmPlaying) {
			AudioManager.Instance.PlaySoundOn(gameObject, "Alarm");
			alarmPlaying = true;
		}
	}

	void stopAlarm(){
		AudioManager.Instance.StopSound("Alarm");
		alarmPlaying = false;
	}



	private bool IsNearPlayer()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

	private bool IsVeryNearPlayer()
	{
		if (player == null) return false;
		return Vector3.Distance(transform.position, player.position) <= detectionRange/2;
	}

	bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check distance
        if (distanceToPlayer > visionRange) return false;

        // Check FOV
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > visionAngle / 2) return false;

		LayerMask obstacleMask =  LayerMask.GetMask("Obstacles");

        // Raycast to check if there's an obstacle blocking view
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, visionRange, obstacleMask))
        {
            if (hit.collider.transform != player) return false; // Something is blocking the view
        }

        return true; // Player is visible
    }

	public void EndGame () => gameObject.SetActive(false);

	public Vector3 Move (NativeArray<float> scent)
	{
		Vector3 position = transform.localPosition;
		Vector3 targetVector = targetPosition - position;
		float targetDistance = targetVector.magnitude;
		float movement = speed * Time.deltaTime;

		while (movement > targetDistance)
		{
			position = targetPosition;
			if (TryFindNewTarget(scent))
			{
				movement -= targetDistance;
				targetVector = targetPosition - position;
				targetDistance = targetVector.magnitude;
			}
			else
			{
				return transform.localPosition = position;
			}
		}

		return transform.localPosition =
			position + targetVector * (movement / targetDistance);
	}

	bool TryFindNewTarget (NativeArray<float> scent)
	{
		MazeFlags cell = maze[targetIndex];
		(int, float) trail = (0, isGoal ? float.MaxValue : 0f);

		if (cell.Has(MazeFlags.PassageNE))
		{
			Sniff(ref trail, scent, maze.StepN + maze.StepE);
		}
		if (cell.Has(MazeFlags.PassageNW))
		{
			Sniff(ref trail, scent, maze.StepN + maze.StepW);
		}
		if (cell.Has(MazeFlags.PassageSE))
		{
			Sniff(ref trail, scent, maze.StepS + maze.StepE);
		}
		if (cell.Has(MazeFlags.PassageSW))
		{
			Sniff(ref trail, scent, maze.StepS + maze.StepW);
		}
		if (cell.Has(MazeFlags.PassageE))
		{
			Sniff(ref trail, scent, maze.StepE);
		}
		if (cell.Has(MazeFlags.PassageW))
		{
			Sniff(ref trail, scent, maze.StepW);
		}
		if (cell.Has(MazeFlags.PassageN))
		{
			Sniff(ref trail, scent, maze.StepN);
		}
		if (cell.Has(MazeFlags.PassageS))
		{
			Sniff(ref trail, scent, maze.StepS);
		}

		if (trail.Item2 > 0f)
		{
			targetIndex = trail.Item1;
			targetPosition = maze.IndexToWorldPosition(trail.Item1, targetPosition.y);
			return true;
		}
		return false;
	}

	void Sniff (ref (int, float) trail, NativeArray<float> scent, int indexOffset)
	{
		int sniffIndex = targetIndex + indexOffset;
		float detectedScent = scent[sniffIndex];
		if (isGoal ? detectedScent < trail.Item2 : detectedScent > trail.Item2)
		{
			trail = (sniffIndex, detectedScent);
		}
	}
}
