using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TileGenerator : MonoBehaviour
{
    [Header("Settings")]
    
    [Range(2, 50)][SerializeField] private int mainLength = 10;
    [Range(0, 10)] [SerializeField] private int branches;
    [Range(2, 50)][SerializeField] private int branchLength = 5;
    [Range(0f, 1f)][SerializeField] private float buildDelay = 0.1f;
    
    [Header("Assets")]
    
    [SerializeField] private GameObject[] capTiles;
    [SerializeField] private GameObject[] tiles;

    [Header("Debugging")]
    
    [SerializeField] private KeyCode refreshKey = KeyCode.Backspace;

    private readonly int[] _startRotation = {0, 90, 180, 270};
    private readonly List<Tile> _tiles = new List<Tile>();
    private readonly List<Connector> _unconnected = new List<Connector>();

    private Transform _from, _to, _root, _container;

    private void Start()
    {
        var branch = new GameObject("Main Branch");
        branch.transform.position = Vector3.zero;
        _container = branch.transform;
        _container.SetParent(transform);
        StartCoroutine(BuildDungeon());
    }

    private IEnumerator BuildDungeon()
    {
        _root = CreateStartTile();
        _to = _root;
        for (var i = 0; i < mainLength; i++)
        {
            yield return new WaitForSeconds(buildDelay);
            _from = _to;
            _to = CreateTile();
            ConnectTiles();
        }

        if (branches <= 0) yield break;

        for (var h = 0; h < branches; h++)
        {
            var branch = new GameObject($"Branch {h}");
            _container = branch.transform;
            _container.SetParent(transform);
            var connector = _unconnected[Random.Range(0, _unconnected.Count)];
            var tile = connector.transform.parent;
            _root = tile;
            _to = _root;
            for (var j = 0; j < branchLength; j++)
            {
                yield return new WaitForSeconds(buildDelay);
                _from = _to;
                _to = CreateTile();
                ConnectTiles();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(refreshKey))
        {
            SceneManager.LoadScene("Scenes/Game");
        }
    }

    private Transform CreateStartTile()
    {
        if (capTiles.Length < 1) return null;
        var capIndex = Random.Range(0, capTiles.Length);
        var rotationIndex = Random.Range(0, _startRotation.Length);
        var startCap = Instantiate(capTiles[capIndex], Vector3.zero, Quaternion.identity, _container);
        startCap.name = "Starting Tile";
        var rotation = _startRotation[rotationIndex];
        startCap.transform.Rotate(0, rotation, 0);
        _tiles.Add(new Tile(startCap.transform, null));
        return startCap.transform;
    }

    private Transform CreateTile()
    {
        if (tiles.Length < 1) return null;
        var tileIndex = Random.Range(0, tiles.Length);
        var tile = Instantiate(tiles[tileIndex], Vector3.zero, Quaternion.identity, _container);
        // var origin = _tiles[_tiles.FindIndex(x => x.Transform == _from)].Transform;
        var origin = _from.transform;
        _tiles.Add(new Tile(tile.transform, origin));
        return tile.transform;
    }

    /**
     * Generates a list of connectors per tile. It returns a random one
     * and sends the remainder to the available connector collection.
     */
    private Transform GetConnector(Component tile)
    {
        var connectors = tile.GetComponentsInChildren<Connector>().Where(c => !c.Connected).ToList();
        var index = Random.Range(0, connectors.Count);
        var connector = connectors[index];
        // Remove the selected connector
        connectors.RemoveAt(index);
        connector.Connected = true;

        _unconnected.RemoveAll(u => u == connector);
        _unconnected.AddRange(connectors);

        return connector.transform;
    }

    private void ConnectTiles()
    {
        if (!_from) return;
        var fromConnector = GetConnector(_from);
        if (!fromConnector) return;
        if (!_to) return;
        var toConnector = GetConnector(_to);
        if (!toConnector) return;
        toConnector.SetParent(fromConnector);
        _to.SetParent(toConnector);
        toConnector.localPosition = Vector3.zero;
        toConnector.localRotation = Quaternion.identity;
        toConnector.Rotate(0, 180f, 0);
        _to.SetParent(_container);
        toConnector.SetParent(_to);
    }
}
