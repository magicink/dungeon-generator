using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileGenerator : MonoBehaviour
{
    [Header("Settings")]
    
    [Range(2, 50)][SerializeField] private int mainLength = 10;
    [Range(0, 10)] [SerializeField] private int branches;
    [Range(2, 50)][SerializeField] private int branchLength = 5;
    [Range(0f, 1f)][SerializeField] private float buildDelay = 0.1f;
    [Range(1, 100)] [SerializeField] private int maxAttempts = 20;
    
    [Header("Assets")]
    
    [SerializeField] private GameObject[] capTiles;
    [SerializeField] private GameObject[] tiles;

    [Header("Debugging")]
    
    [SerializeField] private KeyCode refreshKey = KeyCode.Backspace;

    private readonly int[] _startRotation = {0, 90, 180, 270};
    private readonly List<Tile> _tiles = new List<Tile>();
    private readonly List<Connector> _connectors = new List<Connector>();
    private int _remainingRooms;
    private int _remainingBranches;
    private int _availableAttempts;

    private Transform _root, _container;
    private TileController _from, _to;

    private void Awake()
    {
        _availableAttempts = maxAttempts;
        _remainingRooms = mainLength;
        _remainingBranches = branches;
    }

    private void Start()
    {
        var branch = new GameObject("Main Branch");
        branch.transform.position = Vector3.zero;
        _container = branch.transform;
        _container.SetParent(transform);
        // StartCoroutine(BuildDungeon());
        if (tiles.Length > 1 && capTiles.Length > 1)
        {
            // BuildDungeon();
            StartCoroutine(BuildDungeon());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(refreshKey))
        {
            SceneManager.LoadScene("Scenes/Game");
        }
    }

    private TileController CreateStartTile()
    {
        var capIndex = Random.Range(0, capTiles.Length);
        var rotationIndex = Random.Range(0, _startRotation.Length);
        var startCap = Instantiate(capTiles[capIndex], Vector3.zero, Quaternion.identity, _container);
        startCap.name = "Starting Tile";
        var rotation = _startRotation[rotationIndex];
        startCap.transform.Rotate(0, rotation, 0);
        _tiles.Add(new Tile(startCap.transform, null));
        return startCap.GetComponent<TileController>();
    }

    private TileController CreateTile()
    {
        if (tiles.Length < 1) return null;
        var tileIndex = Random.Range(0, tiles.Length);
        var tile = Instantiate(tiles[tileIndex], Vector3.zero, Quaternion.identity, _container);
        // var origin = _tiles[_tiles.FindIndex(x => x.Transform == _from)].Transform;
        var origin = _from.transform;
        _tiles.Add(new Tile(tile.transform, origin));
        return tile.GetComponent<TileController>();
    }

    private IEnumerator BuildDungeon()
    {
        while (true)
        {
            // If remaining rooms > 0
            if (_remainingRooms > 0 && _availableAttempts > 0)
            {
                // If _from is empty this is the first room
                if (_from == null)
                {
                    // Instantiate room
                    // Assign room to _from
                    _from = CreateStartTile();
                    _from.transform.SetParent(_container);
                    // Get connectors
                    _connectors.AddRange(_from.GetConnectors());
                    // Reduce remaining rooms
                    _remainingRooms--;
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                // Get available unused connectors
                var availableConnectors = _from.GetConnectors().Where(c => !c.Connected).ToList();
                // If there are available connectors select one at random
                if (availableConnectors.Count > 0)
                {
                    var index = Random.Range(0, availableConnectors.Count);
                    var fromConnector = availableConnectors[index];
                    // Instantiate a new room
                    var nextRoom = CreateTile();
                    nextRoom.Origin = _from;
                    // Get available connectors
                    var targetConnectors = nextRoom.GetConnectors();
                    if (targetConnectors.Count > 0)
                    {
                        // Pick a random target connector
                        var targetIndex = Random.Range(0, targetConnectors.Count);
                        var targetConnector = targetConnectors[targetIndex];
                        // Re-parent next room to connector
                        var targetTransform = targetConnector.transform;
                        targetTransform.SetParent(fromConnector.transform);
                        nextRoom.transform.SetParent(targetTransform);
                        // Re-position next room
                        targetTransform.localPosition = Vector3.zero;
                        targetTransform.localRotation = Quaternion.identity;
                        targetConnector.transform.Rotate(0, 180f, 0);
                        nextRoom.transform.SetParent(_container);
                        targetConnector.transform.SetParent(nextRoom.transform);
                        // Detect collision
                        yield return new WaitForSeconds(buildDelay);
                        nextRoom.DetectCollisions();
                        yield return new WaitForEndOfFrame();
                        // If collision is detected
                        if (!nextRoom.CollisionDetected)
                        {
                            // Mark connector as used
                            fromConnector.Connected = true;
                            targetConnector.Connected = true;
                            _from = nextRoom;
                            _connectors.AddRange(targetConnectors);
                            _availableAttempts = maxAttempts;
                            _remainingRooms--;
                            continue;
                        }

                        _availableAttempts--;
                        var nextConnector = GetRandomConnector();
                        var nextFrom = nextConnector.GetParentTile();
                        _from = nextFrom;
                        var o = nextRoom.gameObject;
                        o.name = $"{_container.name} - Deactivated {o.name}";
                        nextRoom.gameObject.SetActive(false);
                        nextRoom.transform.SetParent(_container);
                        continue;
                    }
                }
            }
            else
            {
                _remainingBranches--;
                if (_remainingBranches > 0)
                {
                    // Pick new _from
                    var branchName = $"Branch {branches - _remainingBranches}";
                    var branch = new GameObject(branchName);
                    branch.transform.position = Vector3.zero;
                    branch.transform.SetParent(gameObject.transform);
                    _container = branch.transform;
                    var connector = GetRandomConnector();
                    var connectorParent = connector.GetParentTile();
                    if (connectorParent)
                    {
                        _availableAttempts = maxAttempts;
                        _remainingRooms = branchLength;
                        _from = connectorParent;
                        continue;
                    }
                }
            }

            break;
        }
    }

    private Connector GetRandomConnector()
    {
        var connectors = _connectors.Where(c => !c.Connected).ToList();
        var connectorIndex = Random.Range(0, connectors.Count);
        var connector = connectors[connectorIndex];
        return connector;
    }

    private TileController GetConnectorParent(Connector connector)
    {
        return connector.transform.parent.GetComponent<TileController>();
    }
}
