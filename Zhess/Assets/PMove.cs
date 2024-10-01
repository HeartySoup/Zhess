using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMove : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;
    PhysicsMaterial2D m_PhysicMat2D;
    Vector3 m_velocity = Vector3.zero;

    private bool m_Grounded;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private Transform m_GroundCheck;
    const float k_GroundedRadius = .3f;
    private float m_JumpForce = 600f;
    float hopt;
    bool firstHop = true;
    bool wRelease = true;

    bool moveStop;
    int mode;

    float CoyoteTime = 0.15f;
    float CoyoteTimeTimer;

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
        //velocity limiter
        if(m_Rigidbody2D.velocity.y < -40)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -40);
        }

        //groundcheck logic
        bool wasGrounded = m_Grounded;
        m_Grounded = false;
        CoyoteTimeTimer -= Time.deltaTime;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                firstHop = false;
                m_Grounded = true;
                CoyoteTimeTimer = CoyoteTime;

                moveStop = false;
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
            if (Input.GetKey(KeyCode.W))
            {
                if (CoyoteTimeTimer > 0 && firstHop == false && wRelease == true)
                {
                    m_Rigidbody2D.velocity = new Vector3(m_Rigidbody2D.velocity.x, 0);
                    firstHop = true;
                    hopt = 0;
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    CoyoteTimeTimer = 0;
                    wRelease = false;
                }
                // delay for short hop/big jumps
                hopt -= Time.deltaTime;
                Debug.Log(hopt);
                if (hopt < -0.01f && hopt > -0.2f)
                {
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 10));
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
                wRelease = true;

                moveStop = true;
            }
            //lr movement special
            if (Input.GetKeyUp(KeyCode.A) == true || Input.GetKeyUp(KeyCode.D) == true)
            {
                moveStop = true;
            }
        
    }
}
