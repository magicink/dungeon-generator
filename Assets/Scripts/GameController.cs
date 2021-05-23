using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    
    
    [SerializeField] private TileController current;

    public delegate void OnCurrentChanged(TileController tileController);
    public delegate void OnReady();
    public static OnCurrentChanged HandleCurrentChanged;
    public static OnReady HandleGameReady;
    
    public bool ready;


    public TileController Current
    {
        get => current;
        set
        {
            current = value;
            HandleCurrentChanged?.Invoke(current);
            if (ready) return;
            ready = true;
            HandleGameReady?.Invoke();
        }
    }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
