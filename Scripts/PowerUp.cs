using UnityEngine;
using System;
using System.Collections;

public class PowerUp : MonoBehaviour 
{
	float originalY;
	public int rotationSpeed = 5;
	public float floatStrength = 0.2f;

	void Start()
	{
		this.originalY = this.transform.position.y; // Saves the original Y coordinates of the object
	}

	void Update()
	{
		transform.position = new Vector3(transform.position.x,
			originalY + ((float)Math.Sin(Time.time) * floatStrength),
			transform.position.z);
		
		// Rotate the object around its local Y axis at a pre defined degree/s per second
		transform.Rotate (0, rotationSpeed, 0 * Time.deltaTime);
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Player")
        {
            switch (gameObject.tag)
            {
                case "healthUp":
                    Health h = new Health(); h.addHealth = 25.0f;
                    col.gameObject.SendMessage("Heal", h);
                    if (h.isFullHealth) return;
                    break;
                case "basicPowerUp":
                    col.transform.GetChild(0).GetChild(0).gameObject.SendMessage("activatePowerup", 0, SendMessageOptions.RequireReceiver);
                    break;
                case "elitePowerUp":
                    col.transform.GetChild(0).GetChild(0).gameObject.SendMessage("activatePowerup", 1);
                    break;
            }
			
			Destroy(gameObject);
		}
	}
}