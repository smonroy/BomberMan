using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum GameState { Start, Play, Over }

public class Map : NetworkBehaviour {

    public Vector2 mapCenter;
    public GameObject[] cellPrefabs;
    public GameObject playerPrefab;

    public GameState gameState;

    private Vector2 cellSize;
    private Vector2 mapSize;
    private Vector2 spawnPointsMargen; // from the corner;
    private Cell[,] map;
    private List<Cell> cells;
    private Cell[] spawnCells;
    private Player[] players;
    private int numPlayers;
    private NetworkManager networkManager;
    private int numPlayersPlaying;
    private GameObject canvasServer;

    public override void OnStartServer() {
        gameState = GameState.Start;
        cellSize = new Vector2(1f, 1f);
        networkManager = FindObjectOfType<NetworkManager>().GetComponent<NetworkManager>();
        canvasServer = GameObject.Find("CanvasServer");

        numPlayers = 0;
        players = new Player[] {null, null, null, null};
    }

    public void BuildMap(int mapRadio, bool irregular, int minTouchPoints) {
        mapSize = new Vector2(mapRadio * 4 + 3, mapRadio * 4 + 3);
        spawnPointsMargen = new Vector2(1, 1);

        MapInit(irregular);
        LinkCells();
        SetSpawnPoints();

        if (irregular) {
            BuildMap(minTouchPoints);
        }

        DetectBorder();
        DetectIndestructibleCells();


        KeyValuePair<ItemType, int>[] items = {
            new KeyValuePair<ItemType, int>(ItemType.bombUp, 20),
            new KeyValuePair<ItemType, int>(ItemType.speedUp, 20),
            new KeyValuePair<ItemType, int>(ItemType.scopeUp, 20)
        };
        SetDestructibleCells(70, items);

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        ShowMap();

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren) {
            NetworkServer.Spawn(child.gameObject);
        }

        foreach(Player player in players) {
            if(player != null) {
                player.StartPlayer(spawnCells[player.playerIndex]);
            }
        }
        networkManager.maxConnections = numPlayers;
        numPlayersPlaying = numPlayers;
        canvasServer.SetActive(false);
    }

    public Player GetNewPlayer(GameObject go) {
        if(numPlayers < 3) {
            int rndPlayer = Random.Range(0, 4);
            int i = 0;
            int playerIndex = (rndPlayer + i) % 4;
            while (players[playerIndex] != null) {
                i++;
                playerIndex = (rndPlayer + i) % 4;
            }
            Player player = new Player(playerIndex);
            players[playerIndex] = player;
            numPlayers++;
            player.SetGO(go);
            return player;
        } else {
            return null;
        }
    }

    public void PlayerDisconnected(int index) {
        players[index] = null;
        numPlayers--;
        if(gameState != GameState.Start) {
            networkManager.maxConnections = numPlayers;
        }
    }

    private void MapInit(bool irregular) {
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

    private void SetDestructibleCells(float probability, KeyValuePair<ItemType, int>[] itemsProbability) {
        foreach (Cell cell in cells) {
            if (cell.GetCellType() == CellType.Walkable) {
                if (!cell.spawnPoint && !cell.IsNextToSpawnPoint()) {
                    if (Random.Range(0f, 100f) <= probability) {
                        cell.SetCellType(CellType.Destructible);

                        int itemRandom = Random.Range(0, 100);
                        for (int i = 0; i < itemsProbability.Length; i++) {
                            if(itemRandom <= itemsProbability[i].Value) {
                                cell.itemType = itemsProbability[i].Key;
                                break;
                            }
                            itemRandom -= itemsProbability[i].Value;
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

    public Player[] GetPlayersInCell(Cell cell) {
        List<Player> result = new List<Player>();
        foreach(Player player in players) {
            if(player != null) {
                if(player.cell == cell) {
                    result.Add(player);
                }
            }
        }
        return result.ToArray();
    }

    public void PlayerDead(int index) {
        numPlayersPlaying--;
        if(numPlayersPlaying <= 0 || (numPlayersPlaying <= 1 && numPlayers > 1)) {
            gameState = GameState.Over;
            canvasServer.SetActive(true);
            if(numPlayersPlaying == 1) {
                for (int i = 0; i < 4; i++) {
                    if(players[i] != null) {
                        if(players[i].playerState == PlayerState.Play) {
                            Debug.Log("player win: " + i);
                            players[i].score++;
                            canvasServer.transform.GetChild(0).GetChild(i).GetComponent<Text>().text = players[i].score.ToString();
                            players[i].ReturnPlayer();
                        }
                    }
                }
            }
        }
    }

}
