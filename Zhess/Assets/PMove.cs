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
    private float raycastDistance = 0.1f;
    [SerializeField] private GameObject m_GroundCheck;
    private float m_JumpForce = 600f;
    float hopt;
    float fhTimer;
    bool firstHop = true;
    bool wRelease = true;
    float GCsub;

    bool moveStop;
    int mode;

    float CoyoteTime = 0.15f;
    float CoyoteTimeTimer;

    // Start is called before the first frame update
    void Start()
    {
        mode = 1;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PhysicMat2D = m_Rigidbody2D.sharedMaterial;
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
        fhTimer -= Time.deltaTime;
        if (fhTimer < 0)
        {
            firstHop = false;
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
        if (Input.GetKey(KeyCode.S) == true)
        {
            moveY -= 1;
        }
        //lr movement normal
        if (mode == 1)
        {
            m_PhysicMat2D.friction = 0;
            m_Rigidbody2D.gravityScale = 7;
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_velocity, 0.05f);
        }
        //lr movement special
        if (mode == 3)
        {
            m_PhysicMat2D.friction = 0;
            m_Rigidbody2D.gravityScale = 7;

            if(Mathf.Abs(move) + Mathf.Abs(moveY) > 0 && (m_Grounded == true || moveStop == false))
            {
                m_Rigidbody2D.gravityScale = 0;
                m_PhysicMat2D.friction = 1000;
                m_Rigidbody2D.velocity += new Vector2(move, moveY);
                if (Mathf.Abs(m_Rigidbody2D.velocity.x) > 20)
                {
                    m_Rigidbody2D.velocity = new Vector2(move * 20, moveY * 20);
                }
            }
            
        }
        m_Rigidbody2D.sharedMaterial = m_Rigidbody2D.sharedMaterial;

        // jumping normal
        if (mode == 1)
        {
            if (Input.GetKey(KeyCode.W))
            {
                if (CoyoteTimeTimer > 0 && firstHop == false && wRelease == true)
                {
                    m_Rigidbody2D.velocity = new Vector3(m_Rigidbody2D.velocity.x, 0);
                    fhTimer = 0.3f;
                    firstHop = true;
                    hopt = 0;
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    CoyoteTimeTimer = 0;
                    wRelease = false;
                }
                // delay for short hop/big jumps
                hopt -= Time.deltaTime;
                if (hopt < -0.03f && hopt > -0.18f)
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

        Transform T = m_GroundCheck.transform;
        RaycastHit2D middleRay = Physics2D.Raycast(T.position, Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D leftRay = Physics2D.Raycast(new Vector2(T.position.x - (T.localScale.x / 2) + GCsub, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D rightRay = Physics2D.Raycast(new Vector2(T.position.x + (T.localScale.x / 2) - GCsub, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        GCsub = 0f;
        if(Mathf.Abs(m_Rigidbody2D.velocity.x) + Mathf.Abs(m_Rigidbody2D.velocity.y) > 0)
        {
            GCsub = 0.25f;
        }
        if (middleRay.collider != null || leftRay.collider != null || rightRay.collider != null)
        {
            m_Grounded = true;
            CoyoteTimeTimer = CoyoteTime;
            
            moveStop = false;
            //if (!wasGrounded)
            //  OnLandEvent.Invoke();
        }

        //no chain jumping
        if (Input.GetKeyUp(KeyCode.W) == true)
        {
            hopt = 100;
            wRelease = true;

            if(mode == 3 && moveStop == false)
            {
                moveStop = true;
                m_Rigidbody2D.velocity = new Vector2(0, 0);
            }
            
        }
        if (Input.GetKeyUp(KeyCode.S) == true)
        {
            if (mode == 3 && moveStop == false)
            {
                moveStop = true;
                m_Rigidbody2D.velocity = new Vector2(0, 0);
            }
        }
        //lr movement special
        if (Input.GetKeyUp(KeyCode.A) == true || Input.GetKeyUp(KeyCode.D) == true)
        {
            if (mode == 3 && moveStop == false)
            {
                moveStop = true;
                m_Rigidbody2D.velocity = new Vector2(0, 0);
            }
        }
    }

}
