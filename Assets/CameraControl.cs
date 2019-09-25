using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");

        this.transform.Translate(new Vector3(x * 0.5f, y * 0.5f, 0f));

        float z = 0f;
        if (Input.GetKey(KeyCode.Z)) {
            z = 0.5f;
        } else if (Input.GetKey(KeyCode.X)) {
            z = -0.5f;
        }

        this.transform.Translate(new Vector3(0f, 0f, z));
    }
}
