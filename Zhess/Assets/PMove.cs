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
    const float k_GroundedRadius = .5f;
    private float m_JumpForce = 300f;
    float hopt;
    bool reJump = true;

    bool moveStop;
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
        if(transform.position.y < -30 || transform.position.y > 40)
        {
            transform.position = Vector3.zero;
        }
        //groundcheck logic
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                moveStop = false;
                hopt = 1f;
                //if (!wasGrounded)
                //  OnLandEvent.Invoke();
            }
        }

        //move logic
        float move = 0;
        float moveY = 0;
        if (Input.GetKey(KeyCode.A) == true)
        {
            move -= 1;
        }

        if (Input.GetKey(KeyCode.D) == true)
        {
            move += 1;
        }

        if (Input.GetKey(KeyCode.W) == true)
        {
            moveY += 1;
        }
        //lr movement normal
        if (mode == 1)
        {
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_velocity, 0.1f);
        }
        //lr movement special
        if (mode == 2)
        {
            if ((m_Grounded == false && moveStop == true) || (move == 0 || moveY == 0))
            {
                m_Rigidbody2D.gravityScale = 7;
            }
            else
            {
                m_Rigidbody2D.gravityScale = 0;
                transform.position += new Vector3(move * 12f, 0) * Time.deltaTime;

                transform.position += new Vector3(0, moveY * 12f) * Time.deltaTime;
            }
        }


        // jumping normal
        if (mode == 1)
        {
            if (Input.GetKey(KeyCode.W) == true && reJump == true)
            {
                // on first click
                if (hopt > 0 && m_Grounded == true)
                {
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    hopt = 0;
                }
                // delay for short hop/big jumps
                hopt -= Time.deltaTime;
                if (hopt < -0.05f && hopt > -0.2f)
                {
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 10));
                }
                else if (hopt < -0.2f)
                {
                    reJump = false;
                }
            }
        }
        if (mode == 2)
        {
            
        }
       
        
    }

    private void Update()
    {
        //cycling through modes
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_Rigidbody2D.velocity = Vector3.zero;
            if(mode < 4)
            {
                mode += 1;
            }
            else
            {
                mode = 1;
            }
            Debug.Log(mode); 
        }
        //no chain jumping
        if (Input.GetKeyUp(KeyCode.W) == true)
        {
            hopt = 100;
            reJump = true;
            moveStop = true;
        }
        //lr movement special
        if (Input.GetKeyUp(KeyCode.A) == true || Input.GetKeyUp(KeyCode.D) == true)
        {
            moveStop = true;
        }
    }
}
