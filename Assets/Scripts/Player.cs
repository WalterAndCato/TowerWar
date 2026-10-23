using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public  Animator                  animator;
    public  Transform                 shootPoint;
    public  GameObject                tower;
    private Brick                     selectedBrick;
    private GameObject                shotBullets;
    private List<ReactivateCollision> _reactivateCollisions = new();
    public  float                     waterLevel            = -15.25f;
    public Material highlightMaterial;
    public Material defaultMaterial;
    public bool isPlayerOne;
    public GameObject directionImage;
    public Image powerSlider;
    public GameObject backupInputControl;
    public AudioSource powerAudioSource;

    public event Action OnDeath;
    private bool _dead;

    public void Start()
    {
        shotBullets = new GameObject("shotBullets");

        var inputs = FindObjectsOfType<InputControl>();
        var gameController = FindObjectOfType<GameController>();
        var input = inputs.FirstOrDefault(a => a.isPlayerOne == isPlayerOne);

        if (input == null)
        {
            var go = Instantiate(backupInputControl);
            input = go.GetComponent<InputControl>();
            input.isPlayerOne = !isPlayerOne;
            gameController.singlePlayerControl = input;
        }
        else if (input == gameController.singlePlayerControl)
        {
            input.isPlayerOne = true;
        }

        input.directionImage   = directionImage;
        input.gameController   = gameController;
        input.powerSlider      = powerSlider;
        input.powerAudioSource = powerAudioSource;
        input.attachedPlayer   = this;
    }

    private void Update()
    {
        var removeList = new List<ReactivateCollision>();

        foreach (var reactivateCollision in _reactivateCollisions)
        {
            reactivateCollision.remainingTime -= Time.deltaTime;
            if (reactivateCollision.remainingTime <= 0)
            {
                if(reactivateCollision.collider != null)
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), reactivateCollision.collider, false);
                removeList.Add(reactivateCollision);
            }
        }

        foreach (var remove in removeList)
        {
            _reactivateCollisions.Remove(remove);
        }

        var collider = GetComponent<Collider2D>();
        var lowestPoint = collider != null && collider.enabled
            ? collider.bounds.min.y
            : transform.position.y;
        if (transform.position is { x: > 50 or < -50 } or { y: < -50 } || lowestPoint <= waterLevel)
            Die();
    }

    private void Die()
    {
        if (_dead)
            return;
        _dead = true;
        OnDeath?.Invoke();
    }

    void findInitialBlock()
    {
        float maxY = -Mathf.Infinity;
        Brick brick = null;
        foreach (Brick block in tower.GetComponentsInChildren<Brick>())
        {
            var y = block.transform.position.y;
            if (y > maxY)
            {
                maxY  = y;
                brick = block;
            }
        }

        SelectBlock(brick);
    }

    public void Activate(bool activate)
    {
        foreach (Brick block in tower.GetComponentsInChildren<Brick>())
            block.OnClick = activate? SelectBlock : null;

        if (activate)
        {
            foreach (Brick block in tower.GetComponentsInChildren<Brick>())
                block.OnRemoveFromTower += () => RemoveBlockFromTower(block.gameObject);

            findInitialBlock();
        }
    }

    void RemoveBlockFromTower(GameObject block)
    {
        block.transform.parent = shotBullets.transform;

        if (FindObjectOfType<GameController>().GetCurrentPlayer() == (isPlayerOne ? 0 : 1))
            findInitialBlock();
    }

    public void ChangeBlockSelection(Vector2 direction)
    {
        if (selectedBrick == null)
        {
            findInitialBlock();
            return;
        }

        var size     = selectedBrick.GetComponent<Collider2D>().bounds.size;
        var position = selectedBrick.transform.position;
        position.x += size.x / 2 * direction.x;
        position.y += size.y / 2 * direction.y;

        float   closest           = Mathf.Infinity;
        Brick   newBrick          = null;
        Vector3 direction3        = new Vector3(direction.x, direction.y, 0);
        Vector3 directionRotated3 = new Vector3(-1 * direction.y, direction.x, 0);

        foreach (Brick block in tower.GetComponentsInChildren<Brick>())
        {
            if (GameObject.ReferenceEquals(block.gameObject, selectedBrick.gameObject))
                continue;

            var dist     = block.transform.position - position;
            var distDir1 = Vector3.Dot(dist, direction3);
            if (distDir1 < 0)
                continue;

            var distDir2 = Vector3.Dot(dist, directionRotated3);
            var fullDist = new Vector2(distDir1, distDir2).sqrMagnitude;
            if (fullDist < closest)
            {
                closest  = fullDist;
                newBrick = block;
            }
        }

        SelectBlock(newBrick);
    }


    public void Shoot(Vector2 direction)
    {
        if (selectedBrick == null)
            return;

        var bullet = selectedBrick.gameObject;
        var brick = selectedBrick;
        bullet.GetComponent<SpriteRenderer>().material = defaultMaterial;
        bullet.transform.position                   = shootPoint.position;
        bullet.transform.parent                     = shotBullets.transform;
        bullet.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bullet.GetComponent<Rigidbody2D>().AddForce(direction);
        animator.SetTrigger("Shot");
        selectedBrick = null;

        var joints = bullet.GetComponent<JointRef>();
        if (joints != null)
            foreach (var joint in joints.Joints)
                if (joint != null)
                    Destroy(joint);

        var bulletCol = bullet.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), bulletCol);
        _reactivateCollisions.Add(new ReactivateCollision
        {
            collider      = bulletCol,
            remainingTime = 1f
        });

        FindObjectOfType<GameController>().StartTrackingStone(brick);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("shoot1");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            Die();
    }

    private void SelectBlock(Brick block)
    {
        if (block == null)
            return;

        if(!block.transform.IsChildOf(tower.transform))
            return;

        if (selectedBrick != null)
            selectedBrick.GetComponent<SpriteRenderer>().material = defaultMaterial;

        selectedBrick = block;
        if (selectedBrick != null)
            selectedBrick.GetComponent<SpriteRenderer>().material = highlightMaterial;
    }

    private class ReactivateCollision
    {
        public Collider2D collider;
        public float      remainingTime = 1;
    }
}
