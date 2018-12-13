using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombController : NetworkBehaviour {

    public float secondsToExploit;
    public Cell cell;
    public Player player;
    public int maxScope;
    public bool exploting;
    public GameObject firePrefab;

    private float timeToExploit;
    private Map map;

	// Use this for initialization
	void Start () {
        timeToExploit = Time.time + secondsToExploit;
        exploting = false;
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
    }
	
	// Update is called once per frame
	void Update () {
        if(Time.time >= timeToExploit) {
            Exploit();
        }
	}

    public void Exploit() {
        if (isServer) {
            exploting = true;
            int hCenter = 0;
            int vCenter = 0;
            int hLen = 1;
            int vLen = 1;
            for (int side = 0; side < 4; side++) {
                Cell c = cell.sides[side];
                for (int scope = 0; scope < maxScope; scope++) {
                    CellType type = c.GetCellType();
                    if (type == CellType.Indestructible) {
                        break;
                    }
                    switch (side) { // Right, Up, Left, Down
                        case 0: hLen++; hCenter++; break;
                        case 1: vLen++; vCenter++; break;
                        case 2: hLen++; hCenter--; break;
                        case 3: vLen++; vCenter--; break;
                    }
                    if (type == CellType.Destructible) {
                        c.BombImpact();
                        break;
                    }
                    foreach(Player p in map.GetPlayersInCell(c)) {
                        p.BombImpact();
                    }
                    if (c.bomb != null) {
                        BombController bc = c.bomb.GetComponent<BombController>();
                        if (!bc.exploting) {
                            bc.Exploit();
                        }
                        break;
                    }
                    if (c.item != null) {
                        c.item.GetComponent<ItemController>().Destroy();
                        c.itemType = ItemType.nothing;
                        c.item = null;
                        break;
                    }
                    c = c.sides[side];
                }
            }
            player.bombUsed--;
            player.UpdateUI();

            var fire1 = Instantiate(firePrefab, transform.position, Quaternion.identity);
            fire1.transform.Rotate(Vector3.right, -90);
            fire1.transform.localScale = new Vector3(0.5f, vLen * 0.5f, 0.5f);
            fire1.transform.position += new Vector3(0, 0, vCenter * 0.5f);
            Destroy(fire1, 0.3f);
            NetworkServer.Spawn(fire1);

            var fire2 = Instantiate(firePrefab, transform.position, Quaternion.identity);
            fire2.transform.Rotate(Vector3.forward, 90);
            fire2.transform.localScale = new Vector3(0.5f, hLen * 0.5f, 0.5f);
            fire2.transform.position += new Vector3(hCenter * 0.5f, 0, 0);
            Destroy(fire2, 0.3f);
            NetworkServer.Spawn(fire2);

            cell.bomb = null;
        }

        Destroy(this.gameObject);
    }
}
