using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {

    public GameObject bombPrefab;
    public UIController uIController;
    public Color[] playerColor;
    public PlayerState playerState;
    public Sprite[] faces;

    private Player player;
    private Side currentSide;
    private Side previousSide;
    private Map map;

    [SyncVar]
    private int playerIndex;

    private GameObject canvas;
    private Text bombsText;
    private Text speedText;
    private Text fireText;
    private Image face;
    private int faceNum;

    // Use this for initialization
    void Start () {
        faceNum = -1;
        currentSide = Side.Other;
        previousSide = currentSide;
        playerState = PlayerState.Start;
        uIController = GameObject.FindWithTag("UIController").GetComponent<UIController>();
        canvas = transform.GetChild(2).gameObject;
        bombsText = canvas.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>();
        speedText = canvas.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Text>();
        fireText = canvas.transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<Text>();
        face = canvas.transform.GetChild(1).GetComponent<Image>();

        if (isServer) {
            map = GameObject.FindWithTag("Map").GetComponent<Map>();
            player = map.GetNewPlayer(this.gameObject);
        } 
        if(isLocalPlayer) {
            GameObject.FindWithTag("MainCamera").gameObject.GetComponent<CameraScript>().SetPlayer(gameObject);
        } else {
            canvas.SetActive(false);
        }
        GetComponent<MeshRenderer>().material.color = playerColor[playerIndex];
    }

    public override void OnStartServer() {
        FindObjectOfType<NetworkManagerHUD>().GetComponent<NetworkManagerHUD>().showGUI = false;
        base.OnStartServer();
    }

    public override void OnStartClient() {
        FindObjectOfType<NetworkManagerHUD>().GetComponent<NetworkManagerHUD>().showGUI = false;
        base.OnStartClient();
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer) {
            return;
        }

        if(face) {
            if(faceNum != playerIndex) {
                faceNum = playerIndex;
                face.sprite = faces[faceNum];
            }
        }

        switch (playerState) {
            case PlayerState.Play:
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

                if (isNew) {
                    if (isCurrent) {
                        previousSide = currentSide;
                    }
                    currentSide = newSide;
                    CmdSetRotation(currentSide);
                } else {
                    if (!isCurrent) {
                        currentSide = previousSide;
                        if (currentSide != Side.Other) {
                            CmdSetRotation(currentSide);
                        }
                    }
                    if (!isPrevious) {
                        previousSide = Side.Other;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space)) {
                    CmdPutBomb();
                }
                break;
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
        switch (playerState) {
            case PlayerState.Play:
                if (currentSide != Side.Other) {
                    CmdMove(currentSide);
                }
                break;
            case PlayerState.Start:
                transform.localEulerAngles += new Vector3(0f, 1f, 0f);
                break;
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

    [ClientRpc]
    public void RpcSetState(PlayerState state) {
        playerState = state;
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

    [ClientRpc]
    public void RpcUpdateUI(int bombAvailable, int bombTotal, int bombScope, int speedCount) {
        bombsText.text = "= " + bombAvailable + "/" + bombTotal;
        speedText.text = "= " + speedCount;
        fireText.text = "= " + bombScope;
    }

    [ClientRpc]
    public void RpcSetPlayerIndex(int index) {
        playerIndex = index;
        GetComponent<MeshRenderer>().material.color = playerColor[playerIndex];
    }
}
