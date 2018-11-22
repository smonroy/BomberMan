using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType {Outside, Indestructible, Destructible, Walkable}

public enum Side {Right, Up, Left, Down, Other}

public enum ItemType {speedUp, bombUp, scopeUp, nothing}

public class Cell {
    public Vector3 position;
    public Vector2 mapPosition;
    public Dictionary<CellType, float> cellHeigh = new Dictionary<CellType, float> {
        { CellType.Outside,  -0.05f },
        { CellType.Indestructible, 0.4f},
        { CellType.Destructible, 0.4f},
        { CellType.Walkable, -0.05f},
    };
    public List<Cell> links;
    public Cell[] sides;
    public bool spawnPoint;
    public int walkableAreaIndex;
    public GameObject go;
    public GameObject bomb;
    public ItemType itemType;
    public GameObject item;

    private CellType cellType;

    public Cell(Vector2 mapPosition, Vector2 position) {
        this.mapPosition = mapPosition;
        cellType = CellType.Outside;
        links = new List<Cell>();
        spawnPoint = false;
        sides = new Cell[4] { null, null, null, null };
        bomb = null;
        itemType = ItemType.nothing;
        this.position = new Vector3(position.x, cellHeigh[cellType], position.y);
    }

    public void AddLink(Cell cell, Side side = Side.Other) {
        if(side != Side.Other) {
            sides[(int)side] = cell;
        }
        links.Add(cell);
    }

    public void SetCellType(CellType cellType) {
        this.cellType = cellType;
        position.y = cellHeigh[cellType];
    }

    public CellType GetCellType() {
        return cellType;
    }

    public int CountLinksOfType(CellType cellType) {
        int count = 0;
        foreach(Cell link in links) {
            if(link.GetCellType() == cellType) {
                count++;
            }
        }
        return count;
    }

    public bool IsNextToSpawnPoint() {
        foreach (Cell link in links) {
            if (link.spawnPoint) {
                return true;
            }
        }
        return false;
    }

    public void SetSpawnCell() {
        spawnPoint = true;
        SetWalkableArea();
    }

    public void SetWalkableArea(int index = 0) {
        cellType = CellType.Walkable;
        walkableAreaIndex = index;
        foreach (Cell cell in links) {
            cell.cellType = CellType.Walkable;
            cell.walkableAreaIndex = index;
        }
    }

    public bool IsNextToAnotherAreaIndex() {
        for (int s = 0; s < 4; s++) {
            if(sides[s] != null) {
                if(sides[s].sides[s] != null) {
                    if(sides[s].sides[s].GetCellType() == CellType.Walkable) {
                        if(sides[s].sides[s].walkableAreaIndex != walkableAreaIndex) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void BombImpact() {
        if(cellType == CellType.Destructible) {
            if(go != null) {
                cellType = CellType.Walkable;
                CellController cc = go.GetComponent<CellController>();
                if (itemType != ItemType.nothing) {
                    item = cc.PutItem((int)itemType);
                }
                cc.Destroy();
                go = null;
            }
        }
    }
}
