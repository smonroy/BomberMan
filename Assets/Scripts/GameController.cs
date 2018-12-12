using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

    public GameObject mapPrefab;

    private Map map;

    public override void OnStartServer() {
        var mapGO = Instantiate(mapPrefab);
        mapGO.transform.name = "Map";
        map = mapGO.GetComponent<Map>();
        NetworkServer.Spawn(mapGO);
    }

    public void StartGame(int mapNumber) {
        if (isServer) {
            switch (mapNumber) {
                case 1:
                    map.BuildMap(3, false, 2);
                    break;
                case 2:
                    map.BuildMap(7, true, 2);
                    break;
            }
        }
    }
}
