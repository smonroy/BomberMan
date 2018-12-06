using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {
    public Cell cell;
    public int bombNumber;
    public int bombUsed;
    public int bombScope;
    public float initialSpeed;

    private const float centerMargin = 0.05f;
    private Cell nextCell;
    private PlayerController pc;
    private Side currentSide;
    private float currentSpeed;
    private float incrementsSpeed;
    private float maximunSpeed;
    private Vector3 position;

    public Player(Cell cell) {
        this.cell = cell;
        nextCell = null;
        bombUsed = 0;
        bombNumber = 1;
        bombScope = 1;
        initialSpeed = 0.6f;
        incrementsSpeed = 0.1f;
        maximunSpeed = 1.4f;
        currentSpeed = initialSpeed;

    }

    public void SetGO(GameObject go) {
        pc = go.GetComponent<PlayerController>();
        position = go.transform.position;
    }

    public void Move(Side sideTry) {

        if (nextCell == null) { // is in the center of a cell
            if (CellIsWalkable(cell.sides[(int)sideTry])) {
                nextCell = cell.sides[(int)sideTry];
                currentSide = sideTry;
            }
        }

        if (nextCell != null) {
            Vector3 forward = nextCell.position - position;
            forward.y = 0;
            forward = forward.normalized * currentSpeed / 10f;
            Vector3 backward = cell.position - position;
            backward.y = 0;

            if (currentSide == sideTry) { // go away from the center
                ChangePosition(forward, false);
            } else {
                if (CellIsWalkable(cell.sides[(int)sideTry]) || sideTry == GetOppositeSide(currentSide)) { // closing to the center to change of direction or walking directly to the center
                    if (forward.magnitude < backward.magnitude || (sideTry == GetOppositeSide(currentSide) && CellIsWalkable(cell.sides[(int)sideTry]))) {
                        ChangePosition(-forward, false); // full movement
                        if(forward.magnitude > backward.magnitude) {
                            nextCell = cell.sides[(int)sideTry];
                            currentSide = sideTry;
                        }
                    } else {
                        ChangePosition(backward, false); // limited movement
                    }
                } else {
                    if (CellIsWalkable(nextCell.sides[(int)sideTry])) { // go to the nextcell cell to change direction
                        ChangePosition(forward, false);
                    }
                }
            }

            // switch cell and nextCell
            if (Vector3.Magnitude(nextCell.position - position) < Vector3.Magnitude(cell.position - position)) {
                var tmpCell = cell;
                cell = nextCell;
                nextCell = tmpCell;
                currentSide = GetOppositeSide(currentSide);
                if (cell.item != null) {
                    TakeItem();
                }
            }

            Vector3 distance = position - cell.position;
            distance.y = 0;
            if (distance.magnitude <= centerMargin && currentSide != sideTry) {
                Vector3 pos = new Vector3(cell.position.x, position.y, cell.position.z);
                ChangePosition(pos, true);
                nextCell = null;
            }

        }

    }

    public void ChangePosition(Vector3 pos, bool absolute) {
        if (absolute) {
            position = pos;
        } else {
            position += pos;
        }
        pc.RpcPosition(pos, absolute);
    }

    Side GetOppositeSide(Side side) {
        switch (side) {
            case Side.Down:
                return Side.Up;
            case Side.Up:
                return Side.Down;
            case Side.Left:
                return Side.Right;
            case Side.Right:
                return Side.Left;
        }
        return Side.Other;
    }

    bool CellIsWalkable(Cell thisCell) {
        return thisCell.GetCellType() == CellType.Walkable && thisCell.bomb == null;
    }

    public bool IsPossiblePutBombHere() {
        if (cell.bomb == null) {
            if(bombUsed < bombNumber) {
                return true;
            }
        }
        return false;
    }

    public void SetBomb(GameObject bomb) {
        cell.bomb = bomb;
        BombController bc = bomb.GetComponent<BombController>();
        bc.cell = cell;
        bc.player = this;
        bc.maxScope = bombScope;
        bombUsed++;
    }

    public void TakeItem() {
        switch (cell.itemType) {
            case ItemType.bombUp:
                bombNumber++;
                break;
            case ItemType.scopeUp:
                bombScope++;
                break;
            case ItemType.speedUp:
                if(currentSpeed + incrementsSpeed <= maximunSpeed) {
                    currentSpeed += incrementsSpeed;
                    Debug.Log(currentSpeed);
                }
                break;
        }
        cell.itemType = ItemType.nothing;
        cell.item.GetComponent<ItemController>().Destroy();
        cell.item = null;
    }
}
