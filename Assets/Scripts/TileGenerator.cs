using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileGenerator : MonoBehaviour
{
    #region Editor API
    [Header("Settings")]
    
    [Range(2, 50)][SerializeField] private int mainLength = 10;
    [Range(0, 10)] [SerializeField] private int branches;
    [Range(2, 50)][SerializeField] private int branchLength = 5;
    [Range(0f, 1f)][SerializeField] private float buildDelay = 0.1f;
    [Range(1, 100)] [SerializeField] private int maxAttempts = 20;
    
    [Header("Assets")]
    
    [SerializeField] private GameObject[] capTiles;
    [SerializeField] private GameObject[] tiles;
    [SerializeField] private GameObject[] walls;

    [Header("Debugging")]

    [SerializeField] private KeyCode refreshKey = KeyCode.Backspace;
    #endregion
    
    public GameObject[] Walls => walls;

    #region Events
    public delegate void OnBuildComplete(TileGenerator tileGenerator);
    public static OnBuildComplete HandleBuildComplete;
    #endregion

    #region Internal
    private readonly int[] _startRotation = {0, 90, 180, 270};
    private readonly List<Connector> _connectors = new List<Connector>();
    private int _remainingRooms;
    private int _remainingBranches;
    private int _availableAttempts;
    private bool _dirty;
    private bool _buildComplete;
    private bool _sealed;

    private Transform _container;
    private TileController _from;
    #endregion

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
        if (tiles.Length > 1 && capTiles.Length > 1)
        {
            StartCoroutine(BuildDungeon());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(refreshKey))
        {
            SceneManager.LoadScene("Scenes/Game");
        }

        if (!_buildComplete || _sealed) return;
        HandleBuildComplete?.Invoke(this);
        _sealed = true;
    }

    private TileController CreateStartTile()
    {
        var capIndex = Random.Range(0, capTiles.Length);
        var rotationIndex = Random.Range(0, _startRotation.Length);
        var startTile = Instantiate(capTiles[capIndex], Vector3.zero, Quaternion.identity, _container);
        startTile.name = "Starting Tile";
        var rotation = _startRotation[rotationIndex];
        startTile.transform.Rotate(0, rotation, 0);
        var startTileController = startTile.GetComponent<TileController>();
        return startTileController;
    }

    private TileController CreateTile()
    {
        if (tiles.Length < 1) return null;
        var tileIndex = Random.Range(0, tiles.Length);
        var tile = Instantiate(tiles[tileIndex], Vector3.zero, Quaternion.identity, _container);
        var tileController = tile.GetComponent<TileController>();
        return tileController;
    }

    private IEnumerator BuildDungeon()
    {
        while (true)
        {
            if (_remainingRooms > 0 && _availableAttempts > 0)
            {
                if (!_dirty)
                {
                    _dirty = true;
                    _from = CreateStartTile();
                    _from.transform.SetParent(_container);
                    _connectors.AddRange(_from.GetUnconnected());
                    _remainingRooms--;
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                var availableConnectors = _from.GetUnconnected().Where(c => !c.Connected).ToList();
                if (availableConnectors.Count > 0)
                {
                    var index = Random.Range(0, availableConnectors.Count);
                    var fromConnector = availableConnectors[index];
                    var nextRoom = CreateTile();
                    var targetConnectors = nextRoom.GetUnconnected();
                    
                    nextRoom.Origin = _from;

                    if (targetConnectors.Count > 0)
                    {
                        var targetIndex = Random.Range(0, targetConnectors.Count);
                        var targetConnector = targetConnectors[targetIndex];
                        var targetTransform = targetConnector.transform;
                        var nextRoomTransform = nextRoom.transform;

                        targetTransform.SetParent(fromConnector.transform);
                        nextRoom.transform.SetParent(targetTransform);
                        targetTransform.localPosition = Vector3.zero;
                        targetTransform.localRotation = Quaternion.identity;
                        targetConnector.transform.Rotate(0, 180f, 0);
                        nextRoomTransform.SetParent(_container);
                        targetConnector.transform.SetParent(nextRoomTransform);

                        yield return new WaitForSeconds(buildDelay);
                        nextRoom.DetectCollisions();
                        yield return new WaitForEndOfFrame();
                        
                        if (!nextRoom.CollisionDetected)
                        {
                            fromConnector.Connected = true;
                            targetConnector.Connected = true;
                            _from.AddNeighbor(nextRoom);
                            _from = nextRoom;
                            _connectors.AddRange(targetConnectors);
                            _availableAttempts = maxAttempts;
                            _remainingRooms--;
                            continue;
                        }

                        _availableAttempts--;
                        var nextRoomGameObject = nextRoom.gameObject;
                        nextRoomGameObject.SetActive(false);
                        nextRoom.transform.SetParent(_container);
                        var nextConnector = GetAvailableConnector();
                        if (nextConnector)
                        {
                            var nextFrom = nextConnector.GetParentTile();
                            _from = nextFrom;
                        }
                        continue;
                    }
                }
            }
            else
            {
                _remainingBranches--;
                if (_remainingBranches > 0)
                {
                    var branchName = $"Branch {branches - _remainingBranches}";
                    var branch = new GameObject(branchName);
                    branch.transform.position = Vector3.zero;
                    branch.transform.SetParent(gameObject.transform);
                    _container = branch.transform;
                    var connector = GetAvailableConnector();
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
            
            // Seal the dungeon
            _buildComplete = true;
            break;
        }
    }

    private Connector GetAvailableConnector()
    {
        var connectors = _connectors.Where(c => !c.Connected).ToList();
        var connectorIndex = Random.Range(0, connectors.Count);
        var connector = connectors[connectorIndex];
        return connector;
    }
}
