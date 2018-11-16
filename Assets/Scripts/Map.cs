using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{

    public Vector2 mapCenter;
    public Vector2 cellSize;
    public bool irregular;
    public int mapRadio;
    public int touchPoints;
    public GameObject[] cellPrefabs;

    private Vector2 mapSize;
    private Vector2 spawnPointsMargen; // from the corner;
    private Cell[,] map;
    private List<Cell> cells;
    private Cell[] spawnCells;

    // Use this for initialization
    void Start()
    {
        mapSize = new Vector2(mapRadio * 4 + 3, mapRadio * 4 + 3);
        spawnPointsMargen = new Vector2(1, 1);

        MapInit();
        LinkCells();
        SetSpawnPoints();

        if(irregular) {
            BuildMap(touchPoints);
        }

        DetectBorder();
//        DetectIndestructibleCells();
//        SetDestructibleCells(70);

        ShowMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void MapInit()
    {
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
            if(cell.GetCellType() != CellType.Outside) {
                Instantiate(cellPrefabs[(int)cell.GetCellType()], cell.position, Quaternion.identity, this.transform);
            }
        }
    }

    private void LinkCells() {
        for (int xi = 0; xi < mapSize.x; xi++) {
            for (int yi = 0; yi < mapSize.y; yi++) {
                if (yi > 0)                 { map[xi, yi].AddLink(map[xi, yi - 1], Side.Up); }
                if (xi > 0)                 { map[xi, yi].AddLink(map[xi - 1, yi], Side.Left); }
                if (yi < mapSize.y - 1)     { map[xi, yi].AddLink(map[xi, yi + 1], Side.Down); }
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
    }

    private void SetDestructibleCells(float probability) {
        foreach(Cell cell in cells) {
            if (cell.GetCellType() == CellType.Walkable) {
                if (!cell.spawnPoint && !cell.IsNextToSpawnPoint()) {
                    if (Random.Range(0f, 100f) <= probability) {
                        cell.SetCellType(CellType.Destructible);
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
