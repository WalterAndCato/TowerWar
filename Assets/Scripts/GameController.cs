using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public enum MatchState { Playing, Finished }
    public MatchState State { get; private set; } = MatchState.Playing;
    public Player Winner { get; private set; }
    public  TMP_Text winText;
    public  TMP_Text restartText;
    private Player   _player1;
    private Player   _player2;
    public bool     gameFinished;
    public bool     blockSelected;
    public InputControl singlePlayerControl;
    public CinemachineTargetGroup targetGroup;
    public  Music        music;

    private Player _currentPlayer;

    void Start()
    {
        _player1 = GameObject.Find("Player1").GetComponent<Player>();
        _player2 = GameObject.Find("Player2").GetComponent<Player>();

        _player1.OnDeath += () => GameOver(_player2);
        _player2.OnDeath += () => GameOver(_player1);

        winText.gameObject.SetActive(false);
        restartText.gameObject.SetActive(false);

        _currentPlayer = _player1;
        _currentPlayer.Activate(true);
        
        if (singlePlayerControl)
        {
            singlePlayerControl.attachedPlayer = _currentPlayer;
            singlePlayerControl.isPlayerOne = _currentPlayer == _player1;
            singlePlayerControl.directionImage = _currentPlayer.directionImage;
            singlePlayerControl.powerSlider = _currentPlayer.powerSlider;
        }
        
        gameFinished = false;
        blockSelected = false;
    }
    
    public Player Player1 => _player1;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");
    }

    public int GetCurrentPlayer()
    {
        return _currentPlayer == _player1 ? 0 : 1;
    }

    public Player GetCurrentPlayerAsRef()
    {
        return _currentPlayer;
    }

    public void Fire(Vector2 v)
    {
        if (State == MatchState.Finished)
            return;
        _currentPlayer.Shoot(v);
        SwitchPlayer();
    }

    public void StartTrackingStone(Brick stone)
    {
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
        if (gameFinished)
            return;

        _currentPlayer.ChangeBlockSelection(direction);
    }

    public void StopTrackingStone(Transform t)
    {
        targetGroup.m_Targets = targetGroup.m_Targets.Where(a => a.target != t).ToArray();
    }

    void SwitchPlayer()
    {
        _currentPlayer.Activate(false);
        _currentPlayer = _currentPlayer == _player1 ? _player2 : _player1;
        _currentPlayer.Activate(true);

        for (var i = 0; i < targetGroup.m_Targets.Length; i++)
        {
            var target = targetGroup.m_Targets[i];
            
            if (target.target == _currentPlayer.transform)
            {
                target.weight = 1.2f;
                targetGroup.m_Targets[i] = target;
            }
            else if (target.target == _player1.transform || target.target == _player2.transform)
            {
                target.weight = 1f;
                targetGroup.m_Targets[i] = target;
            }
        }

        if (singlePlayerControl)
        {
            singlePlayerControl.attachedPlayer = _currentPlayer;
            singlePlayerControl.isPlayerOne = _currentPlayer == _player1;
            singlePlayerControl.directionImage = _currentPlayer.directionImage;
            singlePlayerControl.powerSlider = _currentPlayer.powerSlider;
        }
        
        blockSelected = false;
    }

    void GameOver(Player winner)
    {
        if (State == MatchState.Finished)
            return;

        Winner = winner;
        State = MatchState.Finished;
        gameFinished = true;
        winText.text = (winner == _player1 ? "Player 1" : "Player 2") + " wins!";
        winText.gameObject.SetActive(true);
        restartText.gameObject.SetActive(true);
        _currentPlayer.Activate(false);
        if(AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("death1");
        
        if (music != null)
            music.SetMode(Music.Mode.End);
    }
}
