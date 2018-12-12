using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public float speed = 0.01f;
    public Vector3 offset;
    public Vector3 startPosition;

    private GameObject player;
    private PlayerController pc;
    private bool isInStartPosition;

	// Use this for initialization
	void Start () {
        transform.position = startPosition;
        transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        isInStartPosition = true;
	}

    // Update is called once per frame
    void FixedUpdate() {
        if (player != null) {
            if(pc == null) {
                pc = player.GetComponent<PlayerController>();
            }
            switch (pc.playerState) {
                case PlayerState.Start:
                    if (!isInStartPosition) {
                        isInStartPosition = true;
                        transform.position = startPosition;
                        transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    }
                    break;
                case PlayerState.Over:
                    if(!isInStartPosition) {
                        isInStartPosition = true;
                        transform.position = startPosition;
                        transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    }
                    break;
                case PlayerState.Play:
                    if(isInStartPosition) {
                        isInStartPosition = false;
                        transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                    }
                    Vector3 direction = (player.transform.position + offset) - transform.position;
                    Vector3 directionSpeed = direction * speed;
                    if (direction.magnitude > directionSpeed.magnitude) {
                        transform.position += directionSpeed;
                    } else {
                        transform.position += direction;
                    }
                    break;
            }
        }
    }

    public void SetPlayer(GameObject player) {
        this.player = player;
    }
}
