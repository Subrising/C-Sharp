using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Play()
    {
        SceneManager.LoadScene("kkkkk");
    }

    public void Instructions()
    {

    }

    public void Tutorial()
    {

    }

    public void HighScores()
    {

    }

    public void Settings()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }

}
