using System.Collections;
using System.Linq;
using NUnit.Framework;
using TowerWar.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MatchLifecycleTests
{
    [UnityTest]
    public IEnumerator SoloLevel_CreatesComputerTurnController()
    {
        MatchSession.Configure(MatchMode.HumanVsComputer);

        yield return SceneManager.LoadSceneAsync("Level1");
        yield return null;

        var gameController = FindBehaviour("GameController");
        Assert.That(gameController, Is.Not.Null);

        var hasComputerController = gameController.GetComponents<MonoBehaviour>()
            .Any(component => component && component.GetType().Name == "ComputerPlayerController");
        Assert.That(hasComputerController, Is.True,
            "A solo match must attach a ComputerPlayerController to the GameController object.");
    }

    [UnityTest]
    public IEnumerator ReloadingMainMenu_DoesNotDuplicatePlayerInputManager()
    {
        yield return SceneManager.LoadSceneAsync("MainMenu");
        yield return null;
        Assert.That(CountBehaviours("PlayerInputManager"), Is.EqualTo(1));

        yield return SceneManager.LoadSceneAsync("MainMenu");
        yield return null;

        Assert.That(CountBehaviours("PlayerInputManager"), Is.EqualTo(1));
    }

    private static MonoBehaviour FindBehaviour(string typeName)
    {
        return Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .FirstOrDefault(component => component && component.GetType().Name == typeName);
    }

    private static int CountBehaviours(string typeName)
    {
        return Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .Count(component => component && component.GetType().Name == typeName);
    }
}
