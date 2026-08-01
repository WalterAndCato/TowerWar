using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneConfigurationTests
{
    private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
    private const string LevelOneScene = "Assets/Scenes/Level1.unity";

    [Test]
    public void BuildSettings_StartWithMainMenu_AndContainLevelOne()
    {
        var enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        Assert.That(enabledScenes, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(enabledScenes[0], Is.EqualTo(MainMenuScene));
        Assert.That(enabledScenes, Does.Contain(LevelOneScene));
    }

    [TestCase(MainMenuScene)]
    [TestCase(LevelOneScene)]
    public void Scene_HasNoMissingScripts(string scenePath)
    {
        var scene = OpenScene(scenePath);

        var missingScriptCount = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Sum(transform => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject));

        Assert.That(missingScriptCount, Is.Zero,
            $"Scene '{scenePath}' contains {missingScriptCount} missing script reference(s).");
    }

    [Test]
    public void MainMenu_HasRequiredObjectsAndReferences()
    {
        var scene = OpenScene(MainMenuScene);

        Assert.That(FindGameObject(scene, "PlayerInputManager"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "Main Camera"), Is.Not.Null);

        var mainMenu = FindBehaviour(scene, "MainMenu");
        Assert.That(mainMenu, Is.Not.Null, "MainMenu behaviour is missing.");

        AssertObjectReferenceIsAssigned(mainMenu, "titleScreen");
        AssertObjectReferenceIsAssigned(mainMenu, "joinScreen");
        AssertObjectReferenceIsAssigned(mainMenu, "pl1Cam");
        AssertObjectReferenceIsAssigned(mainMenu, "pl2Cam");
        AssertObjectReferenceIsAssigned(mainMenu, "joinText");
    }

    [Test]
    public void LevelOne_HasRequiredPlayersControllerAndCamera()
    {
        var scene = OpenScene(LevelOneScene);

        Assert.That(FindGameObject(scene, "Player1"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "Player2"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "Main Camera"), Is.Not.Null);

        var gameController = FindBehaviour(scene, "GameController");
        Assert.That(gameController, Is.Not.Null, "GameController behaviour is missing.");

        AssertObjectReferenceIsAssigned(gameController, "winText");
        AssertObjectReferenceIsAssigned(gameController, "restartText");
        AssertObjectReferenceIsAssigned(gameController, "targetGroup");
        AssertObjectReferenceIsAssigned(gameController, "music");
    }

    private static Scene OpenScene(string scenePath)
    {
        Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), Is.Not.Null,
            $"Scene asset '{scenePath}' does not exist.");

        return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }

    private static GameObject FindGameObject(Scene scene, string objectName)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(transform => transform.gameObject)
            .FirstOrDefault(gameObject => gameObject.name == objectName);
    }

    private static MonoBehaviour FindBehaviour(Scene scene, string typeName)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
            .FirstOrDefault(behaviour => behaviour != null && behaviour.GetType().Name == typeName);
    }

    private static void AssertObjectReferenceIsAssigned(MonoBehaviour behaviour, string propertyName)
    {
        var property = new SerializedObject(behaviour).FindProperty(propertyName);

        Assert.That(property, Is.Not.Null,
            $"Serialized property '{propertyName}' no longer exists on {behaviour.GetType().Name}.");
        Assert.That(property.objectReferenceValue, Is.Not.Null,
            $"Reference '{propertyName}' is not assigned on {behaviour.GetType().Name}.");
    }
}
