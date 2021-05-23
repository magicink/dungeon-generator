using UnityEngine;

public class RoomDetector : MonoBehaviour
{
    private bool _buildComplete;

    private void Start()
    {
        TileGenerator.HandleBuildComplete += HandleBuildComplete;
    }

    private void HandleBuildComplete(TileGenerator tileGenerator)
    {
        _buildComplete = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!_buildComplete) return;
        var tileController = hit.collider.GetComponent<TileController>();
        if (!tileController) return;
        if (tileController != GameController.Instance.Current)
        {
            GameController.Instance.Current = tileController;
        }
    }
}
