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
	}

    // Update is called once per frame
    void FixedUpdate() {
        if (player != null) {
            if(player.GetComponent<PlayerController>().playerState == PlayerState.Play) {
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

    public void SetPlayer(GameObject player) {
        this.player = player;
    }
}
