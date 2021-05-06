using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] capTiles;
    [SerializeField] private GameObject[] tiles;
    [SerializeField] private KeyCode refreshKey = KeyCode.Backspace;

    private readonly int[] _startRotation = {0, 90, 180, 270};

    private Transform _from, _to;

    private void Start()
    {
        _from = CreateStartTile();
        _to = CreateTile();
        ConnectTiles();
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
        var startCap = Instantiate(capTiles[capIndex], Vector3.zero, Quaternion.identity, transform);
        var rotation = _startRotation[rotationIndex];
        startCap.transform.Rotate(0, rotation, 0);
        return startCap.transform;
    }

    private Transform CreateTile()
    {
        if (tiles.Length < 1) return null;
        var tileIndex = Random.Range(0, tiles.Length);
        var tile = Instantiate(tiles[tileIndex], Vector3.zero, Quaternion.identity, transform);
        return tile.transform;
    }

    private static Transform GetConnector(Component tile)
    {
        var connectors = tile.GetComponentsInChildren<Connector>().Where(c => !c.Connected).ToList();
        var index = Random.Range(0, connectors.Count);
        return connectors[index].transform;
    }

    private void ConnectTiles()
    {
        if (_from)
        {
            var fromConnector = GetConnector(_from);
        }

        if (_to)
        {
            var toConnector = GetConnector(_to);
        }
    }
}
