using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {

	[System.Serializable]
	public class Enemy
	{
		public GameObject spawningPrefab;
		public float spawnInterval;
		public int spawnChance;

		public float elapsedTime;
	}

	public float spawnInterval = 7f;
    public Transform[] spawnLocations;
	public Enemy[] enemies;
	private Enemy[] enemyChance = new Enemy[10];

    protected Transform playerTransform;


	static void RandomizeBuiltInArray(Enemy[] arr)
	{
		for (int i = 0; i < arr.Length; i++) {
			int r = (int)Random.Range(0, i);
			Enemy tmp = arr[i];
			arr[i] = arr[r];
			arr[r] = tmp;
		}
	}

    void Start()
    {
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;

		foreach (Enemy enemy in enemies) {
			enemy.elapsedTime = 0;
		}

		int k = 0;
		for (int i = 0; i < enemies.Length; i++) {
			for (int j = 0; j < enemies[i].spawnChance; j++, k++)
				enemyChance[k] = enemies[i];
		}

		RandomizeBuiltInArray(enemyChance);

		InvokeRepeating("spawnEnemy", spawnInterval, spawnInterval);
    }

    void Update()
    {
		if (GameObject.Find ("RigidBodyFPSController").GetComponent<GameManager> ().isPaused ())
			return;

		float dt = Time.deltaTime;

		foreach (Enemy enemy in enemies) {
			enemy.elapsedTime += dt;
		}
    }

    void spawnEnemy()
    { 
		int randSpawn = (int)Random.Range(0, spawnLocations.Length);
		int enemySpawnType = (int)Random.Range(0, 9);

		if (enemyChance[enemySpawnType].elapsedTime >= enemyChance[enemySpawnType].spawnInterval) {
			Instantiate(enemyChance[enemySpawnType].spawningPrefab, spawnLocations[randSpawn].transform.position, playerTransform.rotation);
			enemyChance[enemySpawnType].elapsedTime = 0;
		}

		// Old Code
		/*int randSpawn = (int)Random.Range(0, 3);
        int enemySpawnType = (int)Random.Range(0, 9);

		if (enemySpawnType >= 6 && enemies[0].elapsedTime >= enemies[0].spawnInterval)
        {
			Instantiate(enemies[0].spawningPrefab, spawnLocations[randSpawn].transform.position, playerTransform.rotation);
			enemies[0].elapsedTime = 0;
        }
		else if (enemySpawnType < 6 && enemies[1].elapsedTime >= enemies[0].spawnInterval)
        {
			Instantiate(enemies[1].spawningPrefab, spawnLocations[randSpawn].transform.position, playerTransform.rotation);
			enemies[0].elapsedTime = 0;
        }*/
    }

    void stopSpawn()
    {
        CancelInvoke("spawnEnemy");
    }
}
