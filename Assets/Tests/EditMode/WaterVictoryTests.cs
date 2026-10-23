using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Runtime scripts remain in Assembly-CSharp so existing scene references are unchanged.
public class WaterVictoryTests
{
    private readonly List<GameObject> objects = new();
    private static Type RuntimeType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private GameObject Create(string name)
    {
        var go = new GameObject(name);
        objects.Add(go);
        return go;
    }
    private Component CreatePlayer(string name)
    {
        var go = Create(name);
        go.transform.position = Vector3.up * 5;
        go.AddComponent<BoxCollider2D>().size = Vector2.one * 2;
        var player = go.AddComponent(RuntimeType("Player"));
        Set(player, "waterLevel", 0f);
        Set(player, "tower", Create(name + "Tower"));
        return player;
    }
    private static void Set(Component target, string name, object value) =>
        target.GetType().GetField(name).SetValue(target, value);
    private static object Get(Component target, string name) =>
        target.GetType().GetProperty(name).GetValue(target);
    private static void Invoke(Component target, string name, params object[] args) =>
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(target, args);

    [TearDown]
    public void TearDown()
    {
        foreach (var go in objects)
            UnityEngine.Object.DestroyImmediate(go);
        objects.Clear();
    }

    [TestCase(1.01f, false)]
    [TestCase(1f, true)]
    [TestCase(0.5f, true)]
    [TestCase(-10f, true)]
    public void ColliderContactOrSubmersionRaisesDeathExactlyOnce(float height, bool dies)
    {
        var player = CreatePlayer("Player1");
        var deaths = 0;
        player.GetType().GetEvent("OnDeath").AddEventHandler(player, (Action)(() => deaths++));
        player.transform.position = Vector3.up * height;
        Physics2D.SyncTransforms();
        Invoke(player, "Update");
        Invoke(player, "Update");
        Assert.That(deaths, Is.EqualTo(dies ? 1 : 0));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void EitherPlayerLosingFinishesMatchInSharedOrSeparateInputMode(bool sharedInput, bool secondLoses)
    {
        var first = CreatePlayer("Player1");
        var second = CreatePlayer("Player2");
        var controller = Create("GameController").AddComponent(RuntimeType("GameController"));
        // These are the two input configurations used by the existing controller.
        if (sharedInput)
            Set(controller, "singlePlayerControl", Create("Input").AddComponent(RuntimeType("InputControl")));
        foreach (var field in new[] { "winText", "restartText" })
        {
            var ui = new GameObject(field, typeof(RectTransform));
            objects.Add(ui);
            Set(controller, field, ui.AddComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true)));
        }
        Invoke(controller, "Start");
        Assert.That(Get(controller, "State").ToString(), Is.EqualTo("Playing"));
        Assert.That(Get(controller, "Winner"), Is.Null);
        var loser = secondLoses ? second : first;
        var winner = secondLoses ? first : second;
        loser.transform.position = Vector3.up;
        Physics2D.SyncTransforms();
        Invoke(loser, "Update");
        Assert.That(Get(controller, "State").ToString(), Is.EqualTo("Finished"));
        Assert.That(Get(controller, "Winner"), Is.SameAs(winner));
        Assert.That(controller.GetType().GetField("gameFinished").GetValue(controller), Is.True);
        var current = controller.GetType().GetMethod("GetCurrentPlayerAsRef").Invoke(controller, null);
        Invoke(controller, "Fire", Vector2.one);
        Assert.That(controller.GetType().GetMethod("GetCurrentPlayerAsRef").Invoke(controller, null), Is.SameAs(current));
        winner.transform.position = Vector3.down;
        Physics2D.SyncTransforms();
        Invoke(winner, "Update");
        Assert.That(Get(controller, "Winner"), Is.SameAs(winner), "Later deaths cannot replace the result.");
    }
}
