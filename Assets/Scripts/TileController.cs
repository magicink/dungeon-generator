using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField] private TileController origin;
    [SerializeField] private List<Connector> connectors;

    public TileController Origin
    {
        get => origin;
        set => origin = value;
    }
    
    public List<Connector> Connectors
    {
        get => connectors;
        private set => connectors = value;
    }

    public delegate void OnCreation(TileController tileController);
    public OnCreation HandleCreation;
    
    public bool CollisionDetected { get; set; }
    
    private bool _checkedForCollisions;
    private BoxCollider _boxCollider;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _boxCollider = gameObject.AddComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        GetConnectors();
    }

    public void Start()
    {
        var collisions = Physics.OverlapBox(transform.position, _boxCollider.size, Quaternion.identity, LayerMask.GetMask("Tile"));
        if (collisions.Length <= 0) return;
        foreach (var collision in collisions)
        {
            if (collision.gameObject != gameObject)
            {
                if (Origin == null)
                {
                    CollisionDetected = true;
                }
                else
                {
                    if (collision.gameObject != Origin.gameObject)
                    {
                        CollisionDetected = true;
                    }
                }
            }
        }
    }

    public List<Connector> GetConnectors()
    {
        Connectors = GetComponentsInChildren<Connector>().Where(c => !c.Connected).ToList();
        return Connectors;
    }
}
