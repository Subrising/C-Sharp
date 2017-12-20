using UnityEngine;
using System.Collections;

public class PowerUpSpawner : MonoBehaviour {

    protected Transform playerTransform;

    public Transform[] spawnLocations;
	public GameObject[] spawningPrefab;
    public bool[] previousSpawns;

	private float elapsedTime;
	private float basicPowerUpTimer;
	private float elitePowerUpTimer;
	private float healthUpTimer;
	private int previousRand;

	private float basiclifetime = 0f;
	private float healthlifetime = 0f;
	private float elitelifetime = 0f;
	private bool healthLive = false;
	private bool basicLive = false;
	private bool eliteLive = false;

	private float spawnTime = 30f;
	private float basicPowerUp = 40f;
	private float elitePowerUp = 60f;
	private float healthUp = 30f;

	void Start ()
	{

		GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
		playerTransform = objPlayer.transform;

		elapsedTime = 0.0f;
		basicPowerUpTimer = 30.0f;
		elitePowerUpTimer = 30.0f;
		healthUpTimer = 30.0f;

		InvokeRepeating("spawnPowerUp", spawnTime, spawnTime);

	}

	void Update ()
	{

        float dt = Time.deltaTime;

        elapsedTime += dt;
        basicPowerUpTimer += dt;
        elitePowerUpTimer += dt;
        healthUpTimer += dt;

        if (healthLive) {
            healthlifetime += dt;
        }
		if (basicLive) {
            basiclifetime += dt;
        }
		if (eliteLive) {
            elitelifetime += dt;
        }

		deSpawnPowerUp();

	}

	void deSpawnPowerUp()
	{
		GameObject health = GameObject.FindGameObjectWithTag("healthUp");
		GameObject basic = GameObject.FindGameObjectWithTag("basicPowerUp");
		GameObject elite = GameObject.FindGameObjectWithTag("elitePowerUp");

		if (healthlifetime >= 20.0f) {
			Destroy(health);
			healthLive = false;
			healthlifetime = 0;
			previousSpawns [previousRand] = false;
		}
		if (basiclifetime >= 20.0f) {
			Destroy(basic);
			basicLive = false;
			basiclifetime = 0;
			previousSpawns [previousRand] = false;
		}
		if (elitelifetime >= 15.0f) {
			Destroy(elite);
			eliteLive = false;
			elitelifetime = 0;
			previousSpawns [previousRand] = false;
		}

	}

	void spawnPowerUp()
	{ 
		int randSpawn = (int)Random.Range(0, 6);
		int powerUpSpawnType = (int)Random.Range(0, 16);
		previousRand = randSpawn;

		if (previousSpawns [randSpawn] == false) {
			if (powerUpSpawnType >= 12 && elitePowerUpTimer >= elitePowerUp) {
				Instantiate (spawningPrefab [0], spawnLocations [randSpawn].transform.position, playerTransform.rotation);
				elitePowerUpTimer = 0;
				previousSpawns [randSpawn] = true;
			} else if (powerUpSpawnType < 6 && basicPowerUpTimer >= basicPowerUp) {
				Instantiate (spawningPrefab [1], spawnLocations [randSpawn].transform.position, playerTransform.rotation);
				basicPowerUpTimer = 0;
				previousSpawns [randSpawn] = true;
			} else if (powerUpSpawnType >= 6 && healthUpTimer >= healthUp) {
				Instantiate (spawningPrefab [2], spawnLocations [randSpawn].transform.position, playerTransform.rotation);
				basicPowerUpTimer = 0;
				previousSpawns [randSpawn] = true;
				healthLive = true;
			}

			spawnCountCheck();

		}
	}

	void stopSpawn()
	{
		CancelInvoke("spawnPowerUp");
	}

	void spawnCountCheck()
	{
		int spawnCount = 0;

		for (int i = 0; i < 6; i++) {
			if (previousSpawns [i] == true) {
				spawnCount++;
			}
		}

		for (int i = 0; i < 6; i++) {
			if (spawnCount == 6) {
				previousSpawns [i] = false;
			}
		}
	}
}


