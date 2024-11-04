using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private TMP_InputField seedInput;

    [SerializeField]
    private ArchedPathGenerator pathGenerator;

    public static MenuController Instance;

    private bool paused = false;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        PauseGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            {
                PauseGame();
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            RestartGame();
        }

        if (pathGenerator.paused && !paused)
        {
            PauseGame();
        }
    }

    public void StartGame()
    {
        string input = seedInput.text;
        if (System.Int16.TryParse(input, out System.Int16 seed))
        {
            pathGenerator.StartWithSeed(seed);
        }
        else
        {
            pathGenerator.StartWithSeed(Random.Range(1, 1000000));
        }
        PauseGame();
    }

    public void PauseGame()
    {
        paused = !paused;
        if (paused)
        {
            Time.timeScale = 0f;
            canvas.enabled = true;
        }
        else
        {
            Time.timeScale = 1;
            canvas.enabled = false;
            pathGenerator.paused = false;
        }
    }
    public void RestartGame()
    {
        paused = false;
        Time.timeScale = 1;
        StartGame();
    }
}
