using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public GameObject bombPrefab;

    private Player player;
    private Side currentSide;
    private Side previousSide;

    // Use this for initialization
    void Start () {
        currentSide = Side.Other;
        previousSide = currentSide;
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKey(KeyCode.DownArrow)) {
            currentSide = Side.Down;
        } else if (Input.GetKey(KeyCode.UpArrow)) {
            currentSide = Side.Up;
        } else if (Input.GetKey(KeyCode.LeftArrow)) {
            currentSide = Side.Left;
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            currentSide = Side.Right;
        } else {
            currentSide = Side.Other;
        }
        if(currentSide != Side.Other && currentSide != previousSide) {
            SetRotation(currentSide);
            previousSide = currentSide;
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            if(player.IsPossiblePutBombHere()) {
                Vector3 spawnPosition = new Vector3(player.cell.position.x, 0.5f, player.cell.position.z);
                var bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
                player.SetBomb(bomb);
                player.cell.bomb = bomb;
                bomb.GetComponent<BombController>().cell = player.cell;
            }
        }
    }

    void FixedUpdate() {
        if(currentSide != Side.Other) {
            player.Move(currentSide);
        }
    }

    private void SetRotation(Side side) {
        float angle = 0;
        switch (side) {
            case Side.Up:       angle = 0; break;
            case Side.Right:    angle = 90; break;
            case Side.Down:     angle = 180; break;
            case Side.Left:     angle = 270; break;
        }
        transform.eulerAngles = new Vector3(0, angle, 0);
    }

    public void SetPlayer(Player player) {
        this.player = player;
    }
    
}
