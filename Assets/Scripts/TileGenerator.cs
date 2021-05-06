using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] capTiles;
    [SerializeField] private KeyCode refreshKey = KeyCode.Backspace;

    private readonly int[] _startRotation = {0, 90, 180, 270};

    private void Start()
    {
        CreateStartTile();
    }

    private void Update()
    {
        if (Input.GetKeyDown(refreshKey))
        {
            SceneManager.LoadScene("Scenes/Game");
        }
    }

    private void CreateStartTile()
    {
        if (capTiles.Length <= 0) return;
        var capIndex = Random.Range(0, capTiles.Length);
        var rotationIndex = Random.Range(0, _startRotation.Length);
        var startCap = Instantiate(capTiles[capIndex], Vector3.zero, Quaternion.identity, transform);
        var rotation = _startRotation[rotationIndex];
        startCap.transform.Rotate(0, rotation, 0);
    }
}
