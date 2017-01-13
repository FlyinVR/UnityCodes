using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonGUI : MonoBehaviour {

    private float timer = 5.0f;
    public TextMesh countText;
    public string textContent = "Game Over";

    // Use this for initialization
    void Start () {
        timer = 5.0f;
    }
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            SceneManager.LoadScene(0);
        }
	}

    void OnGUI()
    {
        //GUI.Button(new Rect(), "");
    }
}
