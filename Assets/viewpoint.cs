using UnityEngine;
using System.Collections;

public class viewpoint : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * Time.deltaTime * 50.0f;

        transform.Rotate(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0.0f);

        float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
        if (terrainHeightWhereWeAre > transform.position.y)
        {
            reset();
        }
	}

    void OnCollisionEnter(Collision col)
    {
        reset();
    }

    void reset()
    {
        transform.position = new Vector3(813.0f, 330.0f, 874.0f);
        Quaternion rotation = Quaternion.Euler(0.0f, -139.346f, 0.0f);
        transform.rotation = rotation;
    }

}
