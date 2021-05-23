using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileController : MonoBehaviour
{
    [SerializeField] private TileController origin;
    [SerializeField] private List<Connector> connectors;
    [SerializeField] private List<TileController> neighbors = new List<TileController>();

    public TileController Origin
    {
        get => origin;
        set => origin = value;
    }

    private List<Connector> Connectors
    {
        get => connectors;
        set => connectors = value;
    }

    public bool CollisionDetected { get; private set; }

    private BoxCollider _boxCollider;
    private Rigidbody _rigidbody;
    private bool isOriginNull;

    private void Awake()
    {
        _boxCollider = gameObject.AddComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        Connectors = GetComponentsInChildren<Connector>().ToList();
    }

    public void Start()
    {
        isOriginNull = Origin == null;
        TileGenerator.HandleBuildComplete += HandleBuildComplete;
        GameController.HandleCurrentChanged += HandleCurrentChanged;
        GameController.HandleGameReady += HandleReady;
    }

    private void HandleReady()
    {
    }

    private void HandleCurrentChanged(TileController tileController)
    {
    }

    public List<Connector> GetUnconnected()
    {
        return Connectors.Where(c => !c.Connected).ToList();
    }

    public void DetectCollisions()
    {
        var collisions = Physics.OverlapBox(transform.position, _boxCollider.size, Quaternion.identity, LayerMask.GetMask("Tile"));
        if (collisions.Length <= 0) return;
        foreach (var collision in collisions)
        {
            if (collision.gameObject == gameObject) continue;
            if (isOriginNull)
            {
                CollisionDetected = false;
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

    private void HandleBuildComplete(TileGenerator tileGenerator)
    {
        if (!gameObject.activeSelf)
        {
            Destroy(gameObject);
        }
        else
        {
            _boxCollider.enabled = false;
            if (tileGenerator.Walls.Length <= 0) return;
            var unused = GetUnconnected();
            foreach (var connector in unused)
            {
                var wallIndex = Random.Range(0, tileGenerator.Walls.Length);
                var wallPrefab = tileGenerator.Walls[wallIndex];
                var connectorTransform = connector.transform;
                Instantiate(wallPrefab, connectorTransform.position, connectorTransform.rotation, connectorTransform);
            }
        }
    }

    public void AddNeighbor(TileController tileController)
    {
        neighbors.Add(tileController);
    }
}
