using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Map : NetworkBehaviour {

    public Vector2 mapCenter;
    public GameObject[] cellPrefabs;
    public GameObject playerPrefab;

    private Vector2 cellSize;
    private bool irregular;
    private int mapRadio;
    private int minTouchPoints;
    private Vector2 mapSize;
    private Vector2 spawnPointsMargen; // from the corner;
    private Cell[,] map;
    private List<Cell> cells;
    private Cell[] spawnCells;
    private Player[] players;
    private int numPlayers;

    public override void OnStartServer() {
        cellSize = new Vector2(1f, 1f);
        mapRadio = 5;
        minTouchPoints = 2;
        irregular = true;

        mapSize = new Vector2(mapRadio * 4 + 3, mapRadio * 4 + 3);
        spawnPointsMargen = new Vector2(1, 1);

        MapInit();
        LinkCells();
        SetSpawnPoints();

        if (irregular) {
            BuildMap(minTouchPoints);
        }

        DetectBorder();
        DetectIndestructibleCells();

        KeyValuePair<ItemType, int>[] items = new KeyValuePair<ItemType, int>[3];
        items[0] = new KeyValuePair<ItemType, int>(ItemType.bombUp, 20);
        items[1] = new KeyValuePair<ItemType, int>(ItemType.speedUp, 20);
        items[2] = new KeyValuePair<ItemType, int>(ItemType.scopeUp, 20);


        SetDestructibleCells(70, items);

        ShowMap();

        numPlayers = 0;
        players = new Player[4];

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren) {
            if (child.tag != "StartPosition") {
                NetworkServer.Spawn(child.gameObject);
            }
        }
    }


    private void MapInit() {
        map = new Cell[(int)mapSize.x, (int)mapSize.y];
        cells = new List<Cell>();
        for (int xi = 0; xi < mapSize.x; xi++) {
            for (int yi = 0; yi < mapSize.y; yi++) {
                float left = mapCenter.x - (((mapSize.x - 1) * cellSize.x) / 2);
                float top = mapCenter.y - (((mapSize.y - 1) * cellSize.y) / 2);
                Cell cell = new Cell(new Vector2(xi, yi), new Vector2(left + (xi * cellSize.x), top + (yi * cellSize.y)));
                cell.SetCellType(irregular ? CellType.Outside : CellType.Walkable);
                map[xi, yi] = cell;
                cells.Add(cell);
            }
        }
    }

    private void ShowMap() {
        foreach (Cell cell in cells) {
            switch(cell.GetCellType()) {
                case CellType.Indestructible:
                    var cellI = Instantiate(cellPrefabs[(int)cell.GetCellType()], cell.position, Quaternion.identity, this.transform);
                    cell.go = cellI;
                    break;
                case CellType.Destructible:
                    var cellD = Instantiate(cellPrefabs[(int)cell.GetCellType()], cell.position, Quaternion.identity, this.transform);
                    cell.go = cellD;
                    break;
            }
        }
    }

    public Player GetNewPlayer(GameObject go) {
        Player player = new Player(spawnCells[numPlayers]);
        players[numPlayers] = player;
        Debug.Log("numPlayer: " + numPlayers);
        numPlayers++;
        player.SetGO(go);
        return player;
    }

    private void LinkCells() {
        for (int xi = 0; xi < mapSize.x; xi++) {
            for (int yi = 0; yi < mapSize.y; yi++) {
                if (yi > 0)                 { map[xi, yi].AddLink(map[xi, yi - 1], Side.Down); }
                if (xi > 0)                 { map[xi, yi].AddLink(map[xi - 1, yi], Side.Left); }
                if (yi < mapSize.y - 1)     { map[xi, yi].AddLink(map[xi, yi + 1], Side.Up); }
                if (xi < mapSize.x - 1)     { map[xi, yi].AddLink(map[xi + 1, yi], Side.Right); }

                if (yi > 0 && xi > 0)                           { map[xi, yi].AddLink(map[xi - 1, yi - 1]); }
                if (yi > 0 && xi < mapSize.x - 1)               { map[xi, yi].AddLink(map[xi + 1, yi - 1]); }
                if (yi < mapSize.y - 1 && xi > 0)               { map[xi, yi].AddLink(map[xi - 1, yi + 1]); }
                if (yi < mapSize.y - 1 && xi < mapSize.x - 1)   { map[xi, yi].AddLink(map[xi + 1, yi + 1]); }
            }
        }
    }

    private void DetectBorder() {
        foreach(Cell cell in cells) {
            if(cell.GetCellType() == CellType.Walkable) {
                if (cell.links.Count < 8) {
                    cell.SetCellType(CellType.Indestructible);
                } else {
                    if(cell.CountLinksOfType(CellType.Outside) > 0) {
                        cell.SetCellType(CellType.Indestructible);
                    }
                }
            }
        }
    }

    private void DetectIndestructibleCells() {
        foreach (Cell cell in cells) {
            if (cell.GetCellType() == CellType.Walkable) {
                if(cell.CountLinksOfType(CellType.Walkable) == 8) {
                    cell.SetCellType(CellType.Indestructible);
                }
            }
        }
    }

    private void SetSpawnPoints() {
        spawnCells = new Cell[4];
        spawnCells[0] = map[(int)spawnPointsMargen.x, (int)spawnPointsMargen.y];
        spawnCells[1] = map[(int)(mapSize.x - spawnPointsMargen.x - 1), (int)spawnPointsMargen.y];
        spawnCells[2] = map[(int)(mapSize.x - spawnPointsMargen.x - 1), (int)(mapSize.y - spawnPointsMargen.y - 1)];
        spawnCells[3] = map[(int)spawnPointsMargen.x, (int)(mapSize.y - spawnPointsMargen.y - 1)];
        foreach (Cell cell in spawnCells) {
            cell.SetSpawnCell();
        }
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren) {
            if (child.tag == "StartPosition") {
                int i = int.Parse(child.name.Substring(child.name.Length - 1));
                child.position = spawnCells[i].position + (Vector3.up * playerPrefab.transform.localScale.y);
            }
        }
    }

    private void SetDestructibleCells(float probability, KeyValuePair<ItemType, int>[] itemsNumbers) {
        int i = 0;
        int v = 0;
        foreach(Cell cell in cells) {
            if (cell.GetCellType() == CellType.Walkable) {
                if (!cell.spawnPoint && !cell.IsNextToSpawnPoint()) {
                    if (Random.Range(0f, 100f) <= probability) {
                        cell.SetCellType(CellType.Destructible);
                        if (i < itemsNumbers.Length) {
                            if (v < itemsNumbers[i].Value) {
                                cell.itemType = itemsNumbers[i].Key;
                                v++;
                                if (v >= itemsNumbers[i].Value) { // next item type
                                    i++;
                                    v = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void BuildMap(int touchPoints = 1) {
        List<Cell>[] boundary = new List<Cell>[2] { new List<Cell>(), new List<Cell>() };
        boundary[0].AddRange(spawnCells);

        Cell center = map[(int)(mapSize.x / 2), (int)(mapSize.y / 2)];
        center.SetWalkableArea(1);
        boundary[1].Add(center);
        Cell explorationPoint;

        int limit = 100; // maximun number of cycles;
        while (limit > 0) {
            int b = limit % 2;
            limit--;
            if (boundary[b].Count > 0) {
                explorationPoint = boundary[b][Random.Range(0, boundary[b].Count)]; // get a random exploration cell from one of the boundaries groups
                Cell neighbour = GetNeighbour(explorationPoint); // get a random neighbour from outside, is going to be null is there is no one from outside
                if (neighbour != null) {
                    boundary[b].Add(neighbour);
                    foreach (Cell mirror in GetMirrorCells(neighbour)) {
                        mirror.SetWalkableArea(b);
                    }
                    if (neighbour.IsNextToAnotherAreaIndex()) { // if the neighbour is next to the other group, the loop finish.
                        touchPoints--;
                        if(touchPoints <= 0) {
                            return;
                        }
                    }
                } else {
                    boundary[b].Remove(explorationPoint);
                    limit++; // return the cycle back to try another exploration cell
                }
            }
        }
        
    }

    private Cell GetNeighbour(Cell cell) {
        int side = Random.Range(0, 5) + 4;
        int sideInc = (Random.Range(0, 2) * 2) - 1;
        for (int i = 0; i <= 4; i++) {
            int s = (side + (i * sideInc)) % 4;
            if(cell.sides[s] != null) {
                if (cell.sides[s].sides[s] != null) {
                    if (cell.sides[s].sides[s].GetCellType() == CellType.Outside) {
                        return cell.sides[s].sides[s];
                    }
                }
            }
        }
        return null;
    }


    private List<Cell> GetMirrorCells(Cell cell) {
        List<Cell> result = new List<Cell>();
        int x = (int)cell.mapPosition.x;
        int y = (int)cell.mapPosition.y;
        result.Add(map[x, y]);
        result.Add(map[y, x]);
        result.Add(map[x, (int)mapSize.y - y - 1]);
        result.Add(map[y, (int)mapSize.x - x - 1]);
        result.Add(map[(int)mapSize.y - y - 1, x]);
        result.Add(map[(int)mapSize.x - x - 1, y]);
        result.Add(map[(int)mapSize.y - y - 1, (int)mapSize.x - x - 1]);
        result.Add(map[(int)mapSize.x - x - 1, (int)mapSize.y - y - 1]);
        return result;
    }

}
