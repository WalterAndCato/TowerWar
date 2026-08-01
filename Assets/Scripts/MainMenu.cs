using TMPro;
using TowerWar.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject titleScreen;
    public GameObject joinScreen;
    public GameObject pl1Cam;
    public GameObject pl2Cam;
    public TextMeshProUGUI joinText;
    
    private PlayerInput _player1Input;
    private PlayerInput _player2Input;
    private bool _subscribedToPlayerJoined;
    private bool _loadingGame;

    private void Start()
    {
        CleanupPlayersFromPreviousMatch();
        joinScreen.SetActive(false);
        PlayerInputManager.instance.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        PlayerInputManager.instance.DisableJoining();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Exit();
        }

        if (_player1Input && !_player2Input && Input.GetKeyDown(KeyCode.C))
            StartSoloGame();
    }

    public void StartGame()
    {
        titleScreen.SetActive(false);
        joinScreen.SetActive(true);
        SetJoinTextFor(1);
        PlayerInputManager.instance.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenJoinActionIsTriggered;
        PlayerInputManager.instance.EnableJoining();
        SubscribeToPlayerJoined();
        pl1Cam.SetActive(true);
    }

    private void PlayerJoined(PlayerInput playerInput)
    {
        DontDestroyOnLoad(playerInput.gameObject);
        
        if (!_player1Input)
        {
            _player1Input = playerInput;
            playerInput.GetComponent<InputControl>().isPlayerOne = true;
            joinText.text = "Player 2 - Press START or Space | Press C for computer";
            pl2Cam.SetActive(true);
        }
        else
        {
            _player2Input = playerInput;
            playerInput.GetComponent<InputControl>().isPlayerOne = false;
            PlayerInputManager.instance.DisableJoining();
            LoadGame(MatchMode.HumanVsHuman);
        }
    }

    public void StartSoloGame()
    {
        if (_loadingGame)
            return;

        PlayerInputManager.instance.DisableJoining();

        if (_player2Input)
            Destroy(_player2Input.gameObject);

        LoadGame(MatchMode.HumanVsComputer);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadGame()
    {
        LoadGame(MatchMode.HumanVsHuman);
    }

    private void LoadGame(MatchMode mode)
    {
        if (_loadingGame)
            return;

        _loadingGame = true;
        MatchSession.Configure(mode);
        SceneManager.LoadScene("Level1");
    }

    private void SetJoinTextFor(int playerNumber)
    {
        joinText.text = "Player " + playerNumber + " - Press START or Space";
    }

    private void SubscribeToPlayerJoined()
    {
        if (_subscribedToPlayerJoined)
            return;

        PlayerInputManager.instance.onPlayerJoined += PlayerJoined;
        _subscribedToPlayerJoined = true;
    }

    private void CleanupPlayersFromPreviousMatch()
    {
        foreach (var playerInput in PlayerInput.all)
        {
            if (playerInput)
                Destroy(playerInput.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_subscribedToPlayerJoined && PlayerInputManager.instance)
            PlayerInputManager.instance.onPlayerJoined -= PlayerJoined;

        _subscribedToPlayerJoined = false;
    }
}
