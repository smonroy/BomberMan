using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

    public GameObject mapPrefab;

    private UIController uIController;
    private Map map;

    public override void OnStartServer() {
        var mapGO = Instantiate(mapPrefab);
        mapGO.transform.name = "Map";
        uIController = GameObject.FindWithTag("UIController").GetComponent<UIController>();
        map = mapGO.GetComponent<Map>();
        NetworkServer.Spawn(mapGO);
    }

    public void StartGame() {
        if(isServer) {
            uIController.startButton.gameObject.SetActive(false);
            map.BuildMap(3, false, 2);
        }
    }
}
