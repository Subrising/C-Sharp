using UnityEngine;
using System.Collections;

public class DubstepGun : MonoBehaviour {

    [System.Serializable]
    public class Powerup
    {
        public float AnimationSpeed;
        public float Scale;
        public float MaxLength;
        public float Damage;
        public float DamageInterval;
        public float EnergyCost;
        public float EnergyRegen;
        public float EnergyRegenCooldown;
        public bool UnlimitedEnergy;
        public int TrackNo;
        public float Duration;
    }

    // Shots and wave input
    [Header("General")]
    public GameObject[] ShotTypes;
	public GameObject Wave;
    public float ColourChangeInterval = 0.25f;

    // Parameters for the beams/shots
    [Header("Beam Parameters")]
    public float AnimationSpeed = 0.1f;
	public float Scale = 1.0f;
	public float MaxLength = 32.0f;
	public float Damage = 1.0f;
	public float DamageInterval = 1.0f;
    public float Energy = 100.0f;
    public float EnergyCost = 1.0f;
    public float EnergyRegen = 2.0f;
    public float EnergyRegenCooldown = 3.0f;
    public bool UnlimitedEnergy = false;

    // Powerup variables
    [Header("Powerups")]
    public Powerup[] powerups;
    public int activePowerup = -1;

    // Song list
    [Header("Audio")]
    public int CurrentTrack = -1;
	public AudioClip[] Songs;
    private float[] trackTimes;

    // Array of gameobjects that want energy level updates
    [Header("Callbacks")]
    public GameObject[] EnergyListeners;

    // Active/Primary/Secondary Beams/Shots
    private GameObject Primary;
	private GameObject Secondary;
	private GameObject CurrentShot;

	// Required Components
	private AudioSource AS;
	private BeamParam BP;

	// Time passed
	private float ColourTimeTracker;
    private float EnergyTimeTracker;
    private float EnergyCooldownTracker;
    private float PowerupDurationTracker;

    // Use this for initialization
    void Start () {

		// Initialise time trackers and current shot
		ColourTimeTracker = EnergyTimeTracker = EnergyCooldownTracker = PowerupDurationTracker = 0.0f;

        // If this is called before an existing beam is ended
        if (CurrentShot != null && CurrentShot.GetComponent<BeamParam>().bGero)
            CurrentShot.GetComponent<BeamParam>().bEnd = true;
        CurrentShot = null;

		// Initialise Audio Component
		AS = gameObject.AddComponent <AudioSource>();
        if (Songs.Length > 0)
        {
            CurrentTrack = 0;
            AS.clip = Songs[CurrentTrack];
            trackTimes = new float[Songs.Length];
            for (int i = 0; i < trackTimes.Length; i++)
                trackTimes[i] = 0;
        }
		AS.playOnAwake = false;
		AS.loop = true;

		// Initialise BeamParam Component
		BP = gameObject.AddComponent <BeamParam>();
		BP.AnimationSpd = AnimationSpeed;
		BP.Scale = Scale;
		BP.MaxLength = MaxLength;
		BP.Damage = Damage;
		BP.DamageInterval = DamageInterval;

		// Set initial primary shot/beam
		if (ShotTypes.Length > 0) Primary = ShotTypes[0];

        // Update all energy listeners
        Energy = 100;
        UpdateListeners();

    }
	
	// Update is called once per frame
	void Update () {

		GameObject Bullet;

        if (GameObject.Find("RigidBodyFPSController").GetComponent<GameManager>().isPaused())
        {
            // Pause the current song
            if (AS.isPlaying) AS.Pause();

            return;
        }

        // Update the time
        float dt = Time.deltaTime; ColourTimeTracker += dt; EnergyTimeTracker += dt; EnergyCooldownTracker += dt;

        // Track powerups
        if (activePowerup > -1)
        {
            PowerupDurationTracker += dt;
            if (PowerupDurationTracker >= powerups[activePowerup].Duration)
            {
                powerupEnd();
                PowerupDurationTracker = 0.0f;
            }
        }

        // Update beam param for any editor changes or powerups
        if (activePowerup > -1)
        {
            BP.AnimationSpd = powerups[activePowerup].AnimationSpeed;
            BP.Scale = powerups[activePowerup].Scale;
            BP.MaxLength = powerups[activePowerup].MaxLength;
            BP.Damage = powerups[activePowerup].Damage;
            BP.DamageInterval = powerups[activePowerup].DamageInterval;
        }
        else
        {
            BP.AnimationSpd = AnimationSpeed;
            BP.Scale = Scale;
            BP.MaxLength = MaxLength;
            BP.Damage = Damage;
            BP.DamageInterval = DamageInterval;
        }

        // Fire primary shot type
        if ((UnlimitedEnergy || (activePowerup > -1 && powerups[activePowerup].UnlimitedEnergy) || Energy > 0) && Input.GetButtonDown("Fire1"))
		{
            // Reset the energy cooldown tracker
            if (!UnlimitedEnergy && !(activePowerup > -1 && powerups[activePowerup].UnlimitedEnergy)) EnergyCooldownTracker = 0.0f;

            // If this is called before an existing beam is ended
            if (CurrentShot != null && CurrentShot.GetComponent<BeamParam>().bGero)
                CurrentShot.GetComponent<BeamParam>().bEnd = true;

            // Calculate offset for the beam from the prefabs centre
            Vector3 StartPointOffset = this.transform.position + transform.forward * 0.85f + transform.up * 0.1f;

			// Instantiate the wave
			GameObject wav = (GameObject)Instantiate(Wave, StartPointOffset, this.transform.rotation);
			wav.transform.Rotate(Vector3.left, 90.0f);
			wav.GetComponent<BeamWave>().col = BP.BeamColor;

			// Set Bullet to primary shot type
			Bullet = Primary;
			
			// Instantiate the bullet
			CurrentShot = (GameObject)Instantiate(Bullet, StartPointOffset, this.transform.rotation);

            // Energy Cost
            EnergyTimeTracker = 0.0f;
            float ec = 0;
            if (activePowerup > -1) { ec = powerups[activePowerup].EnergyCost; } else { ec = EnergyCost; }
            UseEnergy(ec);

            // Play the currently selected song
            if (AS.clip) AS.Play();
		}
		// Continue fire if it's not "GetButtonDown"
		if ((UnlimitedEnergy || (activePowerup > -1 && powerups[activePowerup].UnlimitedEnergy) || Energy > 0) && Input.GetButton("Fire1"))
		{
            if (!AS.isPlaying) AS.Play();

            // Reset the energy cooldown tracker
            if (!UnlimitedEnergy && !(activePowerup > -1 && powerups[activePowerup].UnlimitedEnergy)) EnergyCooldownTracker = 0.0f;

            if (CurrentShot)
            {
                if (CurrentShot.GetComponent<BeamParam>().bGero)
                    CurrentShot.transform.parent = transform;

                Vector3 s = new Vector3(BP.Scale, BP.Scale, BP.Scale);

                // Change beam colour if enough time has passed
                if (ColourTimeTracker >= ColourChangeInterval)
                {
                    BP.BeamColor = new Color(Random.Range(0F, 0.15F), Random.Range(0F, 0.15F), Random.Range(0F, 0.15F), 1F);
                    ColourTimeTracker -= 0.25f;
                }

                // Energy Cost
                float ec = 0;
                if (activePowerup > -1) { ec = powerups[activePowerup].EnergyCost; } else { ec = EnergyCost; }
                UseEnergy(ec * EnergyTimeTracker);
                EnergyTimeTracker = 0.0f;

                CurrentShot.transform.localScale = s;
                CurrentShot.GetComponent<BeamParam>().SetBeamParam(BP);
            }
		}
        else
        {
            if (CurrentShot != null && CurrentShot.GetComponent<BeamParam>().bGero)
            {
                CurrentShot.GetComponent<BeamParam>().bEnd = true;
            }
        }
        // Stop fire on mouse up
        if (Input.GetButtonUp("Fire1") || (Energy == 0 && !UnlimitedEnergy && !(activePowerup > -1 && powerups[activePowerup].UnlimitedEnergy)))
		{
			if (CurrentShot != null && CurrentShot.GetComponent<BeamParam>().bGero)
			{
				CurrentShot.GetComponent<BeamParam>().bEnd = true;

				// Pause the current song
				if (AS.isPlaying) AS.Pause();
			}
		}

        // God Mode
        if (Input.GetKeyDown("g"))
        {
            UnlimitedEnergy = !UnlimitedEnergy;
            if (UnlimitedEnergy) Energy = 100.0f;
            UpdateListeners();
        }

        // Regen energy if unused for set cooldown period
        float rg = 0, cd = 0;
        if (activePowerup > -1) { rg = powerups[activePowerup].EnergyRegen; cd = powerups[activePowerup].EnergyRegenCooldown; } else { rg = EnergyRegen; cd = EnergyRegenCooldown; }
        if (EnergyCooldownTracker >= cd)
        {
            RestoreEnergy(rg * (EnergyCooldownTracker - cd));
            EnergyCooldownTracker = cd;
        }

    }

    // Update all energy listeners
    void UpdateListeners()
    {
        for (int i = 0; i < EnergyListeners.Length; i++)
        {
            EnergyListeners[i].SendMessage("UpdateEnergy", Energy);
        }
    }

    // Restore energy by add amount
    void RestoreEnergy(float add)
    {
        Energy += add;
        if (Energy > 100) Energy = 100.0f;
        UpdateListeners();
    }

    // Use energy by subtract amount
    void UseEnergy(float subtract)
    {
        if ((!UnlimitedEnergy && !(activePowerup > -1 && powerups[activePowerup].UnlimitedEnergy)) && Energy > 0) Energy -= subtract;
        if (Energy < 0) Energy = 0.0f;
        UpdateListeners();
    }

    // Change music tracks forward
    void nextTrack()
    {
        if (Songs.Length > 0)
        {
            bool wasPlaying = (AS.isPlaying ? true : false);

            if (wasPlaying) AS.Pause();

            trackTimes[CurrentTrack] = AS.time;
            CurrentTrack = (CurrentTrack + 1) % Songs.Length;
            AS.clip = Songs[CurrentTrack];
            AS.time = trackTimes[CurrentTrack];

            if (wasPlaying) AS.Play();
        }
    }

    // Change music tracks backward
    void prevTrack()
    {
        if (Songs.Length > 0)
        {
            bool wasPlaying = (AS.isPlaying ? true : false);

            if (wasPlaying) AS.Pause();

            trackTimes[CurrentTrack] = AS.time;
            CurrentTrack--;
            if (CurrentTrack < 0) CurrentTrack += Songs.Length;
            AS.clip = Songs[CurrentTrack];
            AS.time = trackTimes[CurrentTrack];

            if (wasPlaying) AS.Play();
        }
    }

    // Change music tracks to specified
    void playTrack(int trackNo)
    {
        if (Songs.Length > 0 && trackNo >= 0 && trackNo < Songs.Length)
        {
            bool wasPlaying = (AS.isPlaying ? true : false);

            if (wasPlaying) AS.Pause();

            trackTimes[CurrentTrack] = AS.time;
            CurrentTrack = trackNo;
            AS.clip = Songs[CurrentTrack];
            AS.time = trackTimes[CurrentTrack];

            if (wasPlaying) AS.Play();
        }
        else
        {
            Debug.Log("Invalid Track Number");
        }
    }

    // Activate a powerup
    void activatePowerup(int index)
    {
        activePowerup = index;
        playTrack(powerups[activePowerup].TrackNo);
    }

    // End all powerups
    void powerupEnd()
    {
        activePowerup = -1;
        playTrack(0);
    }
}
