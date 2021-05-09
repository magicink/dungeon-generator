using UnityEngine;

public class Tile
{
    public Transform Transform;
    public Transform Origin;
    public Connector Connector;

    public Tile(Transform transform, Transform origin)
    {
        Transform = transform;
        Origin = origin;
    }
}