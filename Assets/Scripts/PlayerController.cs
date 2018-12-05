using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public GameObject bombPrefab;

    private Player player;
    private Side currentSide;
    private Side previousSide;
    private Map map;

    // Use this for initialization
    void Start () {
        currentSide = Side.Other;
        previousSide = currentSide;
        if(isServer) {
            map = GameObject.FindWithTag("Map").GetComponent<Map>();
            player = map.GetNewPlayer(this.gameObject);
        }
    }

    public override void OnStartLocalPlayer() {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer) {
            return;
        }

        if (Input.GetKey(KeyCode.DownArrow)) {
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
            CmdSetRotation(currentSide);
            previousSide = currentSide;
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            CmdPutBomb();
        }
    }

    [Command]
    void CmdPutBomb() {
        if (player.IsPossiblePutBombHere()) {
            Vector3 spawnPosition = new Vector3(player.cell.position.x, 0.5f, player.cell.position.z);
            var bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
            player.SetBomb(bomb);
            player.cell.bomb = bomb;
            bomb.GetComponent<BombController>().cell = player.cell;
            NetworkServer.Spawn(bomb);
        }
    }

    void FixedUpdate() {
        if(currentSide != Side.Other) {
            CmdMove(currentSide);
        }
    }

    [Command]
    void CmdMove(Side side) {
        player.Move(side);
    }

    [ClientRpc]
    public void RpcPosition(Vector3 pos, bool absolute) {
        if(absolute) {
            transform.position = pos;
        } else {
            transform.position += pos;
        }
    }

    [Command]
    private void CmdSetRotation(Side side) {
        RpcSetRotation(side);
    }

    [ClientRpc]
    private void RpcSetRotation(Side side) {
        float angle = 0;
        switch (side) {
            case Side.Up:       angle = 0; break;
            case Side.Right:    angle = 90; break;
            case Side.Down:     angle = 180; break;
            case Side.Left:     angle = 270; break;
        }
        transform.eulerAngles = new Vector3(0, angle, 0);
    }

}
