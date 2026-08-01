public interface IPlayerTurnController
{
    void BeginTurn(Player player, GameController gameController);
    void EndTurn();
}
