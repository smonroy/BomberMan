using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public GameObject bombPrefab;
    public UIController uIController;

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
            uIController = GameObject.FindWithTag("UIController").GetComponent<UIController>();
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

        Side newSide = Side.Other;
        bool isPrevious = false;
        bool isCurrent = false;
        bool isNew = false;

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            CompareKey(Side.Down, ref isNew, ref isCurrent, ref isPrevious, ref newSide);
        } 
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            CompareKey(Side.Up, ref isNew, ref isCurrent, ref isPrevious, ref newSide);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            CompareKey(Side.Left, ref isNew, ref isCurrent, ref isPrevious, ref newSide);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            CompareKey(Side.Right, ref isNew, ref isCurrent, ref isPrevious, ref newSide);
        }

        if(isNew) {
            if(isCurrent) {
                previousSide = currentSide;
            }
            currentSide = newSide;
            CmdSetRotation(currentSide);
        } else {
            if(!isCurrent) {
                currentSide = previousSide;
                if (currentSide != Side.Other) {
                    CmdSetRotation(currentSide);
                }
            }
            if(!isPrevious) {
                previousSide = Side.Other;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            CmdPutBomb();
        }
    }

    void CompareKey(Side anySide, ref bool isNew, ref bool isCurrent, ref bool isPrevious, ref Side newSide) {
        if (anySide == currentSide) {
            isCurrent = true;
        } else if (anySide == previousSide) {
            isPrevious = true;
        } else {
            isNew = true;
            newSide = anySide;
        }
    }

    void FixedUpdate() {
        if (currentSide != Side.Other) {
            CmdMove(currentSide);
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
