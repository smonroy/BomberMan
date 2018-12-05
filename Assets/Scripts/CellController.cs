using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CellController : NetworkBehaviour {
    public GameObject[] itemPrefabs;

    public void Destroy() {
        Destroy(this.gameObject);
    }

    public GameObject PutItem(int itemIdex) {
        GameObject item = Instantiate(itemPrefabs[itemIdex], transform.position, Quaternion.identity);
        NetworkServer.Spawn(item);
        return item;
    }
}
