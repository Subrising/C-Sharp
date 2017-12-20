using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : MonoBehaviour {

    public Text timeText;
    public Text scoreText;
    public Text fpsText;
    public Image healthBar;
    public Image energyBar;
    public Canvas Canvas;
    public Canvas CanvasGameOver;
    public Canvas CanvasPause;
    public GameObject spawner;
    public GameObject powerupSpawner;
    public Camera GameOverCam;
    public float timeTaken = 0.0f;
    public float health = 100.0f;
    public int score = 0;

    private int frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;
    private float updateRate = 4.0f;  // 4 updates per sec.

    private float originalEnergyBarWidth = 0.0f;
    private float originalHealthBarWidth = 0.0f;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool GodMode = false;
    private bool GameOver = false;
    private bool Paused = false;
    private Vector3 PauseVelocity;

    // Use this for initialization
    void Start () {
        timeTaken = 0.0f;
        score = 0;
        health = 100.0f;
        originalEnergyBarWidth = energyBar.rectTransform.rect.width;
        originalHealthBarWidth = healthBar.rectTransform.rect.width;
        originalPosition = this.transform.position;
        originalRotation = this.transform.rotation;
        updateTime();
        UpdateScore(score);
    }
	
	// Update is called once per frame
	void Update () {
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0 / updateRate)
        {
            fps = frameCount / dt;
            fpsText.text = "FPS: " + (int)fps;
            frameCount = 0;
            dt -= 1.0f / updateRate;
        }

        if (!GameOver)
        {
            if (!Paused)
            {
                timeTaken += Time.deltaTime;
                updateTime();
            }

            if (Input.GetKeyDown("g"))
            {
                GodMode = !GodMode;
                if (GodMode)
                {
                    health = 100.0f;
                    healthBar.color = energyBar.color = new Color(0f, 1f, 1f, 1f);
                    UpdateHealthBar();
                }
                else
                {
                    healthBar.color = energyBar.color = new Color(1f, 1f, 1f, 1f);
                }
            }

            if (Input.GetKeyDown("t"))
                ApplyDamage(100);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Paused) PauseGame();
                else ResumeGame();
            }
        }
    }

    void EndGame()
    {
        if (!GameOver)
        {
            GameOver = true;

            spawner.gameObject.SendMessage("stopSpawn");
            powerupSpawner.gameObject.SendMessage("stopSpawn");

            // Stop all agents
            foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                enemy.SendMessage("Stop");
            foreach (GameObject bystander in GameObject.FindGameObjectsWithTag("Bystander"))
                bystander.SendMessage("Stop");

            Text temp = CanvasGameOver.transform.FindChild("TimeTaken").GetComponent<Text>();
            if ((timeTaken / 60) > 1)
                temp.text = "Time Survived: " + (int)(timeTaken / 60) + " Minutes " + (int)(timeTaken % 60) + " Seconds";
            else
                temp.text = "Time Survived: " + (int)(timeTaken % 60) + " Seconds";

            temp = CanvasGameOver.transform.Find("Score").GetComponent<Text>();
            temp.text = "Score: " + score;

            Canvas.enabled = false;
            CanvasGameOver.gameObject.SetActive(true);
            this.transform.GetChild(0).gameObject.SetActive(false);
            GameOverCam.gameObject.SetActive(true);

            GetComponent<RigidbodyFirstPersonController>().enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartGame()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);

        CanvasGameOver.gameObject.SetActive(false);
        Canvas.enabled = true;
        GameOverCam.gameObject.SetActive(false);
        this.transform.GetChild(0).gameObject.SetActive(true);
        this.transform.GetChild(0).GetChild(0).gameObject.SendMessage("Start");

        this.transform.localPosition = originalPosition;
        this.transform.rotation = originalRotation;
        this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        health = 100.0f;
        timeTaken = 0;
        score = 0;
        updateTime();
        UpdateScore(score);
        UpdateHealthBar();

        spawner.gameObject.SendMessage("Start");
        powerupSpawner.gameObject.SendMessage("Start");

        if (GameOver) GameOver = false;

        GetComponent<RigidbodyFirstPersonController>().enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void updateTime()
    {
        if ((timeTaken / 60) > 1) 
            timeText.text = "Time Survived: " + (int)(timeTaken / 60) + " Minutes " + (int)(timeTaken % 60) + " Seconds";
        else
            timeText.text = "Time Survived: " + (int)(timeTaken % 60) + " Seconds";
    }

    void UpdateScore(int add)
    {
        score += add;
        scoreText.text = "Score: " + score;
    }

    void UpdateEnergy(float energy)
    {
        if (originalEnergyBarWidth <= 0) originalEnergyBarWidth = energyBar.rectTransform.rect.width;

        float width = energy / 100.0f * originalEnergyBarWidth, height = energyBar.rectTransform.rect.height;
        energyBar.rectTransform.sizeDelta = new Vector2(width, height);
    }

    void UpdateHealthBar()
    {
        if (originalHealthBarWidth <= 0) originalHealthBarWidth = healthBar.rectTransform.rect.width;

        float width = health / 100.0f * originalHealthBarWidth, height = healthBar.rectTransform.rect.height;
        healthBar.rectTransform.sizeDelta = new Vector2(width, height);
    }

    void PauseGame()
    {
        Paused = true;

        spawner.gameObject.SendMessage("stopSpawn");
        powerupSpawner.gameObject.SendMessage("stopSpawn");

        // Stop all agents
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            enemy.SendMessage("Stop");
        foreach (GameObject bystander in GameObject.FindGameObjectsWithTag("Bystander"))
            bystander.SendMessage("Stop");

        GetComponent<RigidbodyFirstPersonController>().enabled = false;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        PauseVelocity = rigidbody.velocity;
        rigidbody.isKinematic = true;

        CanvasPause.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //Time.timeScale = 0;
    }

    void ResumeGame()
    {
        Paused = false;

        CanvasPause.gameObject.SetActive(false);

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            enemy.SendMessage("Resume");
        foreach (GameObject bystander in GameObject.FindGameObjectsWithTag("Bystander"))
            bystander.SendMessage("Resume");

        GetComponent<RigidbodyFirstPersonController>().enabled = true;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = PauseVelocity;
        rigidbody.isKinematic = false;

        //Time.timeScale = 1;

        spawner.gameObject.SendMessage("Start");
        powerupSpawner.gameObject.SendMessage("Start");

        Cursor.lockState = CursorLockMode.Locked;
    }

    void ApplyDamage(float subtract)
    {
        if (!GodMode && health > 0) health -= subtract;
        if (health < 0) health = 0.0f;
        if (health == 0) EndGame();
        else UpdateHealthBar();
    }

    void Heal(float add)
    {
        health += add;
        if (health > 100) health = 100.0f;
        UpdateHealthBar();
    }

    void Heal(Health h)
    {
        if (health == 100)
        {
            h.isFullHealth = true;
            return;
        }

        health += h.addHealth;
        if (health > 100) health = 100.0f;
        UpdateHealthBar();
    }

    public bool isPaused()
    {
        return Paused;
    }
}
