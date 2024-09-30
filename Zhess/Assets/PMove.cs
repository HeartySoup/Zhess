using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMove : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;
    Vector3 m_velocity = Vector3.zero;

    private bool m_Grounded;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private Transform m_GroundCheck;
    const float k_GroundedRadius = .1f;
    private float m_JumpForce = 400f;
    float hopt;
    int mode;

    // Start is called before the first frame update
    void Start()
    {
        mode = 1;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //lr movement
        float move = 0;
        if (Input.GetKey(KeyCode.A) == true)
        {
            move -= 1;
        }
            
        if (Input.GetKey(KeyCode.D) == true)
        {
            move += 1;
        }
        Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_velocity, 0.05f);

        //groundcheck logic
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                hopt = 0.1f;
                //if (!wasGrounded)
                  //  OnLandEvent.Invoke();
            }
        }


        if(Input.GetKey(KeyCode.W) == true && hopt > 0)
        {
            if (m_Grounded)
            {
                m_Grounded = false;
            }
            // Add a vertical force to the player.
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce * hopt * 10));
            hopt -= Time.deltaTime;
        }
        if (Input.GetKeyUp(KeyCode.W) == true)
        {
            hopt = 0;
        }
    }
}
