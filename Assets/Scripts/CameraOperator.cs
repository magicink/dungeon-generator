using UnityEngine;

public class CameraOperator : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject overheadCamera;

    [Header("Key bindings")]
    [SerializeField] private KeyCode toggleMap = KeyCode.Tab;
    // Start is called before the first frame update
    void Start()
    {
        if (!playerCamera || !overheadCamera)
        {
            if (!playerCamera) Debug.Log("Player camera not found");
            if (!overheadCamera) Debug.Log("Overhead camera not found");
            Application.Quit();
        }
        playerCamera.SetActive(true);
        overheadCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleMap))
        {
            playerCamera.SetActive(!playerCamera.activeSelf);
            overheadCamera.SetActive(!overheadCamera.activeSelf);
        }
    }
}
