using UnityEngine;

public class Connector : MonoBehaviour
{
    [SerializeField] private Vector2 size = Vector2.one * 4.0f;
    [SerializeField] private bool connected;
    
    public bool Connected { get => connected; set => connected = value; }

    private Vector2 _halfSize;
    private TileController _parent;

    private void Start()
    {
        _parent = transform.parent.gameObject.GetComponent<TileController>();
    }

    private void OnDrawGizmos()
    {
        _halfSize = size * 0.5f;
        Gizmos.color = Color.cyan;
        var t = transform;
        var position = t.position;
        var offset = position + Vector3.up * _halfSize.y;
        var forward = Quaternion.Euler(t.eulerAngles) * Vector3.forward;
        var normal = offset + forward;
        Gizmos.DrawSphere(normal, 0.10f);
        Gizmos.DrawLine(offset, normal);
        var top = t.up * size.y;
        var halfWidth = t.right * _halfSize.x;
        var topRight = position + top + halfWidth;
        var topLeft = position + top - halfWidth;
        var bottomRight = position + halfWidth;
        var bottomLeft = position - halfWidth;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(topLeft, offset);
        Gizmos.DrawLine(topRight, offset);
        Gizmos.DrawLine(bottomRight, offset);
        Gizmos.DrawLine(bottomLeft, offset);
    }

    public TileController GetParentTile()
    {
        return _parent;
    }
}
