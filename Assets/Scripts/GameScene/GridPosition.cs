// GridPosition.cs
using UnityEngine;

[System.Serializable]
public struct GridPosition
{
    public int x;
    public int y;

    public GridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, 0);
    }
}
