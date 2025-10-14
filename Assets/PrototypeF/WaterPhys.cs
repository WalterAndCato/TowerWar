using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPhys : MonoBehaviour
{
    public float waterLevel = -1;
    public float airDrag = 0;
    public float waterDrag = 0.05f;
    public float updraftForce = 2f;
    
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (transform.position.y <= waterLevel)
        {
            _rb.linearDamping = waterDrag;
            _rb.AddForce(Vector2.up * (updraftForce * (waterLevel - transform.position.y)), ForceMode2D.Force);
        }
        else
        {
            _rb.linearDamping = airDrag;
        }
    }
}
