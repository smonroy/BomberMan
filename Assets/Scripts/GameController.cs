using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

    public GameObject mapPrefab;

    public override void OnStartServer() {
        var map = Instantiate(mapPrefab);
        map.transform.name = "Map";
        NetworkServer.Spawn(map);
        //NetworkServer.SpawnWithClientAuthority(map, connectionToClient);
    }
}
