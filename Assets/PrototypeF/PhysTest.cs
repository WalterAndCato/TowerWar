using UnityEngine;

public class PhysTest : MonoBehaviour
{
    private bool _hasBeenDragged;
    private Vector3 _startPosition;
    private GameObject _dragObject;

    private void OnMouseDown()
    {
        _hasBeenDragged = false;
        _startPosition = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        var currentLocation = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        
        if (!_hasBeenDragged && Vector3.Distance(_startPosition, currentLocation) > 0.05f)
            _hasBeenDragged = true;
    }

    private void OnMouseUp()
    {
        //if (!_hasBeenDragged)
        //    Destroy(gameObject);

        if (_dragObject)
        {
            Destroy(_dragObject);
            Destroy(GetComponent<SpringJoint2D>());
        }

        _hasBeenDragged = false;
    }
    
    
    
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector2 mouseOffset;
    private float dragSpeed = 10f;

    void Start()
    {
        // Get the Rigidbody2D component attached to the GameObject
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Check for mouse input and handle dragging
        HandleDrag();
    }

    void HandleDrag()
    {
        // If the mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the mouse is over this GameObject
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collider = Physics2D.OverlapPoint(mousePosition);

            // If the collider belongs to this object, start dragging
            if (collider != null && collider.gameObject == gameObject)
            {
                isDragging = true;
                mouseOffset = mousePosition - rb.position;
            }
        }

        // If the mouse button is released, stop dragging
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            // Get the current mouse position in world space
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Calculate the target position by subtracting the offset
            Vector2 targetPosition = mousePosition - mouseOffset;
            
            // Move the rigidbody by adding force to the object (this keeps the physics system in play)
            Vector2 force = (targetPosition - rb.position) * dragSpeed;
            rb.linearVelocity = force;
        }
    }
}
