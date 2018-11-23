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
    private GameObject go;
    private Cell nextCell;
    private PlayerController pc;
    private Side currentSide;
    private float currentSpeed;
    private float incrementsSpeed;
    private float maximunSpeed;

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
        this.go = go;
        pc = go.GetComponent<PlayerController>();
    }

    public void Move(Side sideTry) {
        if (nextCell == null) { // is in the center of a cell
            if (CellIsWalkable(cell.sides[(int)sideTry])) {
                nextCell = cell.sides[(int)sideTry];
                currentSide = sideTry;
            }
        }

        if (nextCell != null) {
            Vector3 direction = nextCell.position - go.transform.position;
            direction.y = 0;
            Vector3 directionSpeed = direction.normalized * currentSpeed / 10;
            if (currentSide == sideTry) { // go away from the center
                go.transform.position += directionSpeed;
            } else {
                if (CellIsWalkable(cell.sides[(int)sideTry]) || sideTry == GetOppositeSide(currentSide)) { // closing to the center to change of direction or walking directly to the center
                    if (directionSpeed.magnitude < direction.magnitude || sideTry == GetOppositeSide(currentSide)) {
                        go.transform.position -= directionSpeed;
                        if(directionSpeed.magnitude > direction.magnitude && sideTry == GetOppositeSide(currentSide)) {
                            nextCell = cell.sides[(int)sideTry];
                            currentSide = sideTry;
                        }
                    } else {
                        go.transform.position -= direction;
                    }
                } else {
                    if (CellIsWalkable(nextCell.sides[(int)sideTry])) { // go back to the previous cell to change direction
                        go.transform.position += directionSpeed;
                    }
                }
            }

            // switch cell and nextCell
            if (Vector3.Magnitude(nextCell.position - go.transform.position) < Vector3.Magnitude(cell.position - go.transform.position)) {
                var tmpCell = cell;
                cell = nextCell;
                nextCell = tmpCell;
                currentSide = GetOppositeSide(currentSide);
                if(cell.item != null) {
                    TakeItem();
                }
            }

            Vector3 distance = go.transform.position - cell.position;
            distance.y = 0;
            if (distance.magnitude <= centerMargin && currentSide != sideTry) {
                go.transform.position = new Vector3(cell.position.x, go.transform.position.y, cell.position.z);
                nextCell = null;
            }
        }

    }

    public Side GetSide(Vector2 movement)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            if (movement.x > 0) {
                return Side.Right;
            } else {
                return Side.Left;
            }
        } else {
            if (movement.y < 0) {
                return Side.Up;
            } else {
                return Side.Down;
            }
        }
    }

    public float GetMagnitud(Vector2 movement) {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y)) {
            return movement.x;
        } else {
            return movement.y;
        }
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
