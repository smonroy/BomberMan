using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour {
    public GameObject[] itemPrefabs;

    public void Destroy() {
        Destroy(this.gameObject);
    }

    public GameObject PutItem(int itemIdex) {
        GameObject item = Instantiate(itemPrefabs[itemIdex], transform.position, Quaternion.identity);
        return item;
    }
}
