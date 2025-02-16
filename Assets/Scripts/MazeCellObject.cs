using System.Collections.Generic;
using UnityEngine;

public class MazeCellObject : MonoBehaviour


{
#if UNITY_EDITOR
    private static List<Stack<MazeCellObject>> pools = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ClearPools()
    {
        foreach (var pool in pools)
        {
            pool.Clear();
        }
    }
#endif

	[System.NonSerialized]
	private Stack<MazeCellObject> pool;

	public MazeCellObject GetInstance()
	{
		// Initialize the pool only once
		pool ??= new Stack<MazeCellObject>();

#if UNITY_EDITOR
        if (!pools.Contains(pool))
        {
            pools.Add(pool);
        }
#endif

		if (pool.TryPop(out MazeCellObject instance))
		{
			instance.gameObject.SetActive(true);
		}
		else
		{
			instance = Instantiate(this);
			instance.pool = pool;
		}

		return instance;
	}

	public void Recycle()
	{
		pool.Push(this);
		gameObject.SetActive(false);
		Invoke(nameof(ReactivateWall), 0.1f); // Reactivate slightly later to ensure physics updates
	}

	private void ReactivateWall()
	{
		gameObject.SetActive(true);
		if (GetComponent<Collider>() == null)
		{
			gameObject.AddComponent<BoxCollider>(); // Ensure collider exists after reactivation
		}
	}


}
