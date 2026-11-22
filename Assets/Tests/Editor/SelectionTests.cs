using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class SelectionTests
{
    // Runtime scripts live in Assembly-CSharp; keep its assembly layout unchanged.
    private static Type RuntimeType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private readonly List<GameObject> objects = new List<GameObject>();
    private Component player;
    private GameObject tower;
    private static readonly Color Original = new Color(.2f, .4f, .7f, .35f);
    private static readonly Color Highlight = new Color(1f, 1f, 0f, 1f);

    private GameObject Create(string name)
    {
        var go = new GameObject(name);
        objects.Add(go);
        return go;
    }

    private void Set(string name, object value) => player.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(player, value);

    private void Call(string name, params object[] args) => player.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(player, args);

    private void Setup(bool playerOne)
    {
        player = Create("Player").AddComponent(RuntimeType("Player"));
        tower = Create("Tower");
        tower.transform.localScale = new Vector3(playerOne ? -1 : 1, 1, 1);
        Set("tower", tower);
        Set("isPlayerOne", playerOne);
    }

    private Component Brick(string name, float y)
    {
        var go = Create(name);
        go.transform.SetParent(tower.transform);
        go.transform.position = new Vector3(0, y, 0);
        go.AddComponent<SpriteRenderer>().color = Original;
        go.AddComponent<BoxCollider2D>();
        go.AddComponent<Rigidbody2D>();
        return go.AddComponent(RuntimeType("Brick"));
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var go in objects)
            if (go != null) Object.DestroyImmediate(go);
        objects.Clear();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void InitialClickAndDirectionalSelectionRestoreOriginalColor(bool playerOne)
    {
        Setup(playerOne);
        var upper = Brick("Upper", 2);
        var lower = Brick("Lower", 0);
        var renderer = upper.GetComponent<SpriteRenderer>();
        renderer.flipX = true;
        var material = renderer.sharedMaterial;
        Call("Activate", true);
        Assert.That(renderer.color, Is.EqualTo(Highlight));
        // Re-selecting the same object must not overwrite the saved original color.
        Call("SelectBlock", upper);
        Call("ChangeBlockSelection", Vector2.down);
        Assert.That(renderer.color, Is.EqualTo(Original));
        Assert.That(lower.GetComponent<SpriteRenderer>().color, Is.EqualTo(Highlight));
        var click = (Delegate)lower.GetType().GetField("OnClick").GetValue(lower);
        var upperClick = (Delegate)upper.GetType().GetField("OnClick").GetValue(upper);
        Assert.That(click, Is.Not.Null);
        upperClick.DynamicInvoke(upper);
        Assert.That(lower.GetComponent<SpriteRenderer>().color, Is.EqualTo(Original));
        Assert.That(renderer.color, Is.EqualTo(Highlight));
        Assert.That(renderer.sharedMaterial, Is.SameAs(material));
        Assert.That(renderer.flipX, Is.True);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShootRestoresColorAndClearsSelection(bool playerOne)
    {
        Setup(playerOne);
        var brick = Brick("Projectile", 1);
        Set("shotBullets", Create("ShotBullets"));
        Set("shootPoint", Create("ShootPoint").transform);
        player.gameObject.AddComponent<BoxCollider2D>();
        var animator = player.gameObject.AddComponent<Animator>();
        var controller = new UnityEditor.Animations.AnimatorController();
        controller.AddLayer("Base Layer");
        controller.AddParameter("Shot", AnimatorControllerParameterType.Trigger);
        animator.runtimeAnimatorController = controller;
        Set("animator", animator);
        var gameController = Create("Controller").AddComponent(RuntimeType("GameController"));
        var groupType = Type.GetType("Cinemachine.CinemachineTargetGroup, Cinemachine", true);
        gameController.GetType().GetField("targetGroup").SetValue(gameController, Create("Targets").AddComponent(groupType));
        try
        {
            Call("SelectBlock", brick);
            Call("Shoot", Vector2.right);
            Assert.That(brick.GetComponent<SpriteRenderer>().color, Is.EqualTo(Original));
            Assert.That(player.GetType().GetField("selectedBrick", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player), Is.Null);
            Call("Shoot", Vector2.right);
            Assert.That(brick.GetComponent<SpriteRenderer>().color, Is.EqualTo(Original));
        }
        finally { Object.DestroyImmediate(controller); }
    }

    [Test]
    public void InvalidSelectionPreservesCurrentHighlightAndSavedColor()
    {
        Setup(true);
        var current = Brick("Current", 1);
        var foreign = Brick("Foreign", 0);
        foreign.transform.SetParent(null);
        Call("SelectBlock", current);
        Call("SelectBlock", new object[] { null });
        Call("SelectBlock", foreign);
        Assert.That(current.GetComponent<SpriteRenderer>().color, Is.EqualTo(Highlight));
        Assert.That(foreign.GetComponent<SpriteRenderer>().color, Is.EqualTo(Original));
        Call("SelectBlock", Brick("Next", 0));
        Assert.That(current.GetComponent<SpriteRenderer>().color, Is.EqualTo(Original));
    }

    [Test]
    public void EverySelectablePrefabSupportsTintAndRestoration()
    {
        Setup(true);
        var count = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            foreach (var source in prefab.GetComponentsInChildren(RuntimeType("Brick"), true))
            {
                var instance = Object.Instantiate(source.gameObject, tower.transform);
                objects.Add(instance);
                var renderer = instance.GetComponent<SpriteRenderer>();
                Assert.That(renderer, Is.Not.Null, prefab.name);
                var original = renderer.color;
                var material = renderer.sharedMaterial;
                Call("SelectBlock", instance.GetComponent(RuntimeType("Brick")));
                Assert.That(renderer.color, Is.EqualTo(Highlight), prefab.name);
                Call("SelectBlock", Brick("Next", 0));
                Assert.That(renderer.color, Is.EqualTo(original), prefab.name);
                Assert.That(renderer.sharedMaterial, Is.SameAs(material), prefab.name);
                count++;
            }
        }
        Assert.That(count, Is.GreaterThan(0));
    }
}
