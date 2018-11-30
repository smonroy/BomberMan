using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public float speed = 0.01f;
    public Vector3 offset;
    private GameObject player;
    //private bool lookat;

	// Use this for initialization
	void Start () {
//        lookat = false;
	}

    // Update is called once per frame
    void FixedUpdate() {
        if (player == null) {
            if(GameObject.FindWithTag("Player")) {
                player = GameObject.FindWithTag("Player").gameObject;
                Vector3 position = player.transform.position + offset;
                transform.position = position;
                transform.LookAt(player.transform);
            }
        }

        if (player != null) {
            Vector3 direction = (player.transform.position + offset) - transform.position;
            Vector3 directionSpeed = direction * speed;
            if (direction.magnitude > directionSpeed.magnitude) {
                transform.position += directionSpeed;
            } else {
                transform.position += direction;
            }
        }
    }
}
