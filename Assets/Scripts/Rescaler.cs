using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rescaler : MonoBehaviour
{
    public Rigidbody2D target;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            target.linearVelocity += (Vector2)((target.transform.position - transform.position).normalized * 10f);
    }
}
