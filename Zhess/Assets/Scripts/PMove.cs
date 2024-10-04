using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PMove : BooleanLogic
{
    private Rigidbody2D m_Rigidbody2D;
    private PhysicsMaterial2D m_PhysicMat2D;

    private Vector3 m_velocity = Vector3.zero;
    private Vector2 move;
    float HV;
    bool rookReset;
    private Vector2 savedMov;
    float bufferMove = 0.08f;
    float bufferMoveTime;
    bool valBuffer;

    private bool m_Grounded;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private GameObject m_GroundCheck;
    private float raycastDistance;
    private float airOffset;
    private float m_JumpForce = 600f;
    private float hopt;
    private float fhTimer;
    
    private bool firstHop = true;
    private bool wRelease = true;
    
    private int mode;

    private float CoyoteTime = 0.15f;
    private float CoyoteTimeTimer;

    void Start()
    {
        mode = 1;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PhysicMat2D = m_Rigidbody2D.sharedMaterial;
    }

    void FixedUpdate()
    {
        Misc();
        GroundCheck();
        Movement();
        Jump();
    }

    private void Update()
    {
        CycleModes();
        GroundRaycast();
        InputChecks();
    }

    private void Misc()
    {
        if (transform.position.y < -30 || transform.position.y > 40)
        {
            transform.position = Vector3.zero;
        }
        if (m_Rigidbody2D.velocity.y < -30)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -30);
        }
    }
    private void GroundCheck()
    {
        CoyoteTimeTimer -= Time.deltaTime;
        bufferMoveTime -= Time.deltaTime;
        fhTimer -= Time.deltaTime;
        if (fhTimer < 0)
        {
            firstHop = false;
        }
    }
    private void GroundRaycast()
    {
        Transform T = m_GroundCheck.transform;
        RaycastHit2D leftRayW = Physics2D.Raycast(new Vector2(T.position.x - (T.localScale.x / 2), T.position.y), Vector2.left, raycastDistance, m_WhatIsGround);
        RaycastHit2D rightRayW = Physics2D.Raycast(new Vector2(T.position.x + (T.localScale.x / 2), T.position.y), Vector2.right, raycastDistance, m_WhatIsGround);
        airOffset = (leftRayW.collider == null && rightRayW.collider == null) ? 0f : 0.25f;

        RaycastHit2D middleRay = Physics2D.Raycast(T.position, Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D leftRay = Physics2D.Raycast(new Vector2(T.position.x - (T.localScale.x / 2) + airOffset, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D rightRay = Physics2D.Raycast(new Vector2(T.position.x + (T.localScale.x / 2) - airOffset, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        
        m_Grounded = false;
        raycastDistance = 0.03f;
        if (middleRay.collider != null || leftRay.collider != null || rightRay.collider != null)
        {
            m_Grounded = true;
            CoyoteTimeTimer = CoyoteTime;

            bufferMoveTime = bufferMove;
            savedMov = move;
            HV = Mathf.Abs(savedMov.x) + Mathf.Abs(savedMov.y * 2);
            valBuffer = false;
        }
        else
        {
            if (bufferMoveTime > 0)
            {
                RaycastHit2D MR = Physics2D.Raycast(T.position, Vector2.down, raycastDistance, m_WhatIsGround);
                RaycastHit2D LR = Physics2D.Raycast(new Vector2(T.position.x - (T.localScale.x / 2) + airOffset, T.position.y), Vector2.down, raycastDistance * 10, m_WhatIsGround);
                RaycastHit2D RR = Physics2D.Raycast(new Vector2(T.position.x + (T.localScale.x / 2) - airOffset, T.position.y), Vector2.down, raycastDistance * 10, m_WhatIsGround);
                if (MR.collider != null || LR.collider != null || RR.collider != null || (move == savedMov))
                {
                    valBuffer = true;
                    savedMov = move;
                    HV = Mathf.Abs(savedMov.x) + (Mathf.Abs(savedMov.y) * 2);
                }
            }
        }
        
       
    }

    private void Movement()
    {
        move = Vector2.zero;
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Vector3 targetVelocity = new Vector2(move.x * 10f, m_Rigidbody2D.velocity.y);
        m_PhysicMat2D.friction = 0f;
        m_Rigidbody2D.gravityScale = 7f;
        if (mode == 1)
        {
            // Normal left-right movement

            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_velocity, 0.05f);

        }
        else if (mode == 3 || (mode == 4 && (HV == 3 || HV == 0)) || (mode == 5))
        {
            if (MoveEqualorO(move.x, savedMov.x) == true && (MoveEqualorO(move.y, savedMov.y) == true) && GroundedOrContinuing(m_Grounded, savedMov, valBuffer))
            {
                if (m_Grounded == false && move != Vector2.zero)
                {
                    m_Rigidbody2D.gravityScale = 0f;
                    m_PhysicMat2D.friction = 1000f;
                }
                if (mode == 5)
                rookReset = true;
                targetVelocity = new Vector2(savedMov.x, savedMov.y) * 15f;
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_velocity, 0.05f);
            }
            else
            {
                savedMov = new Vector2(1000, 1000);
                if ((HV < 3) && rookReset && !m_Grounded)
                {
                    m_Rigidbody2D.velocity = Vector2.zero;
                    rookReset = false;
                }
            }
        }
        Debug.Log(HV);

        m_Rigidbody2D.sharedMaterial = m_Rigidbody2D.sharedMaterial;
    }

    private void Jump()
    {
        if (mode == 1)
        {
            if (Input.GetAxisRaw("Vertical") == 1)
            {
                if (CanJump(CoyoteTimeTimer, firstHop, wRelease))
                {
                    m_Rigidbody2D.velocity = new Vector3(m_Rigidbody2D.velocity.x, 0);
                    fhTimer = 0.3f;
                    firstHop = true;
                    hopt = 0;
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                    CoyoteTimeTimer = 0;
                    wRelease = false;
                }

                hopt -= Time.deltaTime;
                if (hopt < -0.03f && hopt > -0.18f)
                {
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 10));
                }
            }
        }
    }
    private void InputChecks()
    {

        if (Input.GetKeyUp(KeyCode.W))
        {
            hopt = 100f;
            wRelease = true;
        }
    }

    private void CycleModes()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {

            mode = (mode < 5) ? mode + 1 : 1;
            Debug.Log(mode);
        }
    }

}
