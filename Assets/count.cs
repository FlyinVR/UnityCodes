using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class count : MonoBehaviour {

    public TextMesh countText;
    float timeRemain = 6.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        timeRemain -= Time.deltaTime;
        countText.text = ((int)(timeRemain)).ToString();
        if (timeRemain < 2)
        {
            SceneManager.LoadScene(0);
        }

    }
}
