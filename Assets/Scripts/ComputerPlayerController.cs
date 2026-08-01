using System.Collections;
using TowerWar.Core;
using UnityEngine;

public class ComputerPlayerController : MonoBehaviour, IPlayerTurnController
{
    private Coroutine _turnRoutine;
    private Player _player;
    private GameController _gameController;

    public void BeginTurn(Player player, GameController gameController)
    {
        EndTurn();
        _player = player;
        _gameController = gameController;
        _turnRoutine = StartCoroutine(PlayTurn());
    }

    public void EndTurn()
    {
        if (_turnRoutine != null)
            StopCoroutine(_turnRoutine);

        _turnRoutine = null;
    }

    private IEnumerator PlayTurn()
    {
        yield return new WaitForSeconds(_gameController.ComputerThinkTime);

        if (!_gameController.CanPlayerAct(_player.PlayerIndex))
            yield break;

        _gameController.SelectCurrentBlock();
        yield return new WaitForSeconds(_gameController.ComputerThinkTime);

        if (!_gameController.CanPlayerAct(_player.PlayerIndex))
            yield break;

        var opponent = _gameController.GetOpponent(_player.PlayerIndex);
        var direction = (Vector2)(opponent.transform.position - _player.ShootPoint.position);
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += Random.Range(-_gameController.ComputerAimVariation, _gameController.ComputerAimVariation);

        var charge = Random.Range(
            _gameController.ComputerMinimumCharge,
            _gameController.ComputerMaximumCharge);
        var velocity = ShotCalculator.CalculateVelocity(
            angle,
            charge,
            _gameController.MaximumShotForce,
            _gameController.MinimumShotCharge);

        _gameController.Fire(velocity);
        _turnRoutine = null;
    }
}
