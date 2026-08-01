using System.Linq;
using Cinemachine;
using TMPro;
using TowerWar.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private const float DefaultMaximumShotForce = 10000f;
    private const float DefaultMinimumShotCharge = 0.2f;
    private const float DefaultRotationSpeed = 90f;
    private const float DefaultComputerThinkTime = 0.8f;
    private const float DefaultComputerMinimumCharge = 0.55f;
    private const float DefaultComputerMaximumCharge = 0.9f;
    private const float DefaultComputerAimVariation = 8f;

    public TMP_Text winText;
    public TMP_Text restartText;
    public bool gameFinished;
    public bool blockSelected;

    // Kept for backwards compatibility with the existing Level1 scene.
    public InputControl singlePlayerControl;
    public CinemachineTargetGroup targetGroup;
    public Music music;
    public MatchConfiguration matchConfiguration;

    private readonly IPlayerTurnController[] _turnControllers = new IPlayerTurnController[2];
    private Player _player1;
    private Player _player2;
    private Player _currentPlayer;
    private MatchState _matchState;

    public Player Player1 => _player1;
    public MatchState MatchState => _matchState;
    public float MaximumShotForce => matchConfiguration ? matchConfiguration.maximumShotForce : DefaultMaximumShotForce;
    public float MinimumShotCharge => matchConfiguration ? matchConfiguration.minimumShotCharge : DefaultMinimumShotCharge;
    public float RotationSpeed => matchConfiguration ? matchConfiguration.rotationSpeed : DefaultRotationSpeed;
    public float ComputerThinkTime => matchConfiguration ? matchConfiguration.computerThinkTime : DefaultComputerThinkTime;
    public float ComputerMinimumCharge => matchConfiguration ? matchConfiguration.computerMinimumCharge : DefaultComputerMinimumCharge;
    public float ComputerMaximumCharge => matchConfiguration ? matchConfiguration.computerMaximumCharge : DefaultComputerMaximumCharge;
    public float ComputerAimVariation => matchConfiguration ? matchConfiguration.computerAimVariation : DefaultComputerAimVariation;

    private void Start()
    {
        _player1 = FindRequiredPlayer("Player1", 0);
        _player2 = FindRequiredPlayer("Player2", 1);

        if (!_player1 || !_player2)
        {
            enabled = false;
            return;
        }

        _player1.Initialize(this);
        _player2.Initialize(this);
        _player1.OnDeath += OnPlayerOneDied;
        _player2.OnDeath += OnPlayerTwoDied;

        winText.gameObject.SetActive(false);
        restartText.gameObject.SetActive(false);

        ConfigureTurnControllers();

        _matchState = new MatchState();
        gameFinished = false;
        blockSelected = false;
        _currentPlayer = _player1;
        BeginCurrentTurn();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");
    }

    public int GetCurrentPlayer()
    {
        return _matchState?.CurrentPlayerIndex ?? 0;
    }

    public Player GetCurrentPlayerAsRef()
    {
        return _currentPlayer;
    }

    public Player GetOpponent(int playerIndex)
    {
        return playerIndex == 0 ? _player2 : _player1;
    }

    public bool CanPlayerAct(int playerIndex)
    {
        return _matchState != null && _matchState.CanPlayerAct(playerIndex);
    }

    public void SelectCurrentBlock()
    {
        if (_matchState == null || _matchState.IsFinished)
            return;

        _matchState.SelectBlock();
        blockSelected = true;
    }

    public void Fire(Vector2 velocity)
    {
        if (_matchState == null || _matchState.Phase != MatchPhase.Aiming)
            return;

        _matchState.BeginShot();

        if (!_currentPlayer.Shoot(velocity))
        {
            // The selected brick disappeared between aiming and firing.
            _matchState.CompleteShot();
            SwitchPlayerVisualsAndController();
            return;
        }

        _matchState.CompleteShot();
        SwitchPlayerVisualsAndController();
    }

    public void StartTrackingStone(Brick stone)
    {
        if (!targetGroup || !stone)
            return;

        stone.stopTrackingStone = StopTrackingStone;
        var oldTargets = targetGroup.m_Targets;
        var newTargets = new CinemachineTargetGroup.Target[oldTargets.Length + 1];
        oldTargets.CopyTo(newTargets, 0);
        newTargets[^1] = new CinemachineTargetGroup.Target
        {
            target = stone.transform,
            weight = 1f,
            radius = 1f
        };
        targetGroup.m_Targets = newTargets;
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void BlockSelection(Vector2 direction)
    {
        if (_matchState == null || _matchState.Phase != MatchPhase.SelectingBlock)
            return;

        _currentPlayer.ChangeBlockSelection(direction);
    }

    public void StopTrackingStone(Transform trackedTransform)
    {
        if (!targetGroup)
            return;

        targetGroup.m_Targets = targetGroup.m_Targets
            .Where(target => target.target != trackedTransform)
            .ToArray();
    }

    private void ConfigureTurnControllers()
    {
        var inputs = FindObjectsByType<InputControl>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var playerOneInput = inputs.FirstOrDefault(input => input.isPlayerOne);

        if (!playerOneInput)
            playerOneInput = CreateFallbackInput(_player1, true);

        singlePlayerControl = playerOneInput;
        _turnControllers[0] = playerOneInput;

        if (MatchSession.Mode == MatchMode.HumanVsHuman)
        {
            var playerTwoInput = inputs.FirstOrDefault(input => !input.isPlayerOne);
            if (!playerTwoInput)
                playerTwoInput = CreateFallbackInput(_player2, false);

            _turnControllers[1] = playerTwoInput;
        }
        else
        {
            _turnControllers[1] = gameObject.AddComponent<ComputerPlayerController>();
        }
    }

    private InputControl CreateFallbackInput(Player player, bool isPlayerOne)
    {
        var inputObject = Instantiate(player.backupInputControl);
        var input = inputObject.GetComponent<InputControl>();
        input.isPlayerOne = isPlayerOne;
        return input;
    }

    private void BeginCurrentTurn()
    {
        _currentPlayer.Activate(true);
        _turnControllers[_matchState.CurrentPlayerIndex]?.BeginTurn(_currentPlayer, this);
    }

    private void SwitchPlayerVisualsAndController()
    {
        var previousPlayerIndex = _matchState.CurrentPlayerIndex == 0 ? 1 : 0;
        _turnControllers[previousPlayerIndex]?.EndTurn();
        _currentPlayer.Activate(false);

        _currentPlayer = _matchState.CurrentPlayerIndex == 0 ? _player1 : _player2;
        BeginCurrentTurn();
        UpdateCameraTargetWeights();
        blockSelected = false;
    }

    private void UpdateCameraTargetWeights()
    {
        if (!targetGroup)
            return;

        for (var i = 0; i < targetGroup.m_Targets.Length; i++)
        {
            var target = targetGroup.m_Targets[i];

            if (target.target == _currentPlayer.transform)
                target.weight = 1.2f;
            else if (target.target == _player1.transform || target.target == _player2.transform)
                target.weight = 1f;
            else
                continue;

            targetGroup.m_Targets[i] = target;
        }
    }

    private Player FindRequiredPlayer(string objectName, int playerIndex)
    {
        var playerObject = GameObject.Find(objectName);
        var player = playerObject ? playerObject.GetComponent<Player>() : null;

        if (!player)
        {
            Debug.LogError($"Required player object '{objectName}' with a Player component is missing.", this);
            return null;
        }

        player.isPlayerOne = playerIndex == 0;
        return player;
    }

    private void OnPlayerOneDied()
    {
        FinishMatch(1);
    }

    private void OnPlayerTwoDied()
    {
        FinishMatch(0);
    }

    private void FinishMatch(int winnerIndex)
    {
        if (_matchState == null || _matchState.IsFinished)
            return;

        _matchState.Finish(winnerIndex);
        gameFinished = true;
        blockSelected = false;

        foreach (var controller in _turnControllers)
            controller?.EndTurn();

        _currentPlayer.Activate(false);
        winText.text = $"Player {winnerIndex + 1} wins!";
        winText.gameObject.SetActive(true);
        restartText.gameObject.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("death1");

        if (music)
            music.SetMode(Music.Mode.End);
    }

    private void OnDestroy()
    {
        foreach (var controller in _turnControllers)
            controller?.EndTurn();

        if (_player1)
            _player1.OnDeath -= OnPlayerOneDied;
        if (_player2)
            _player2.OnDeath -= OnPlayerTwoDied;
    }
}
