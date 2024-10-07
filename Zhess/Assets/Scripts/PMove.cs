using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PMove : BooleanLogic
{
    private Rigidbody2D m_Rigidbody2D;
    private PhysicsMaterial2D m_PhysicMat2D;

    private Vector3 m_velocity = Vector3.zero;
    private Vector2 move;
    float HV;
    private Vector2 savedMov;
    private Vector3 savedHMD;
    private Vector3 savedHMD2;
    float accel;
    float dampSpeed;
    float bufferMove = 0.08f;
    float bufferMoveTime;
    bool valBuffer;
    bool rookStop;
    public GameObject HorseIndic;

    private bool m_Grounded;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private GameObject m_GroundCheck;
    private float raycastDistance;
    private float swapInputTime = 3f;
    private float swapTimer;
    private float airOffset;
    private float m_JumpForce = 600f;
    private float hopt;
    private float fhTimer;

    bool pawnDouble;
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
        Camera.main.transform.position = Vector3.Lerp(new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10), new Vector3(transform.position.x, transform.position.y+ 3, -10), 0.05f);

        if (transform.position.y < -30 || transform.position.y > 40)
        {
            transform.position = Vector3.zero;
        }
        if (m_Rigidbody2D.velocity.y < -35)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -35);
        }
    }
    private void GroundCheck()
    {
        swapTimer -= Time.deltaTime;
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
        RaycastHit2D leftRay = Physics2D.Raycast(new Vector2(T.position.x - (T.lossyScale.x / 2) + airOffset, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D rightRay = Physics2D.Raycast(new Vector2(T.position.x + (T.lossyScale.x / 2) - airOffset, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        
        m_Grounded = false;
        raycastDistance = 0.03f;
        if (middleRay.collider != null || leftRay.collider != null || rightRay.collider != null || swapTimer > 0)
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
                RaycastHit2D LR = Physics2D.Raycast(new Vector2(T.position.x - (T.lossyScale.x / 2) + airOffset, T.position.y), Vector2.down, raycastDistance * 10, m_WhatIsGround);
                RaycastHit2D RR = Physics2D.Raycast(new Vector2(T.position.x + (T.lossyScale.x / 2) - airOffset, T.position.y), Vector2.down, raycastDistance * 10, m_WhatIsGround);
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
        Debug.Log(swapTimer);
        move = Vector2.zero;
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 targetVelocity = m_Rigidbody2D.velocity;
        m_PhysicMat2D.friction = 0f;
        m_Rigidbody2D.gravityScale = 7f;
        if (mode < 4)
        {
            if (mode < 3)
            {
                if (mode == 2) move = new Vector2(0, m_Rigidbody2D.velocity.y);
                targetVelocity = new Vector2(move.x * 10f, m_Rigidbody2D.velocity.y);
            }
            if (mode == 3)
            {
                float HM = 2;
                if(Mathf.Abs(savedHMD.x) + Mathf.Abs(savedHMD.y) == 2)
                {
                    swapTimer = 0;
                    transform.position += savedHMD2 * 2 * HM;
                    transform.position += (savedHMD - savedHMD2) * HM;
                    savedHMD = Vector2.zero;
                }
                HorseIndic.transform.position = transform.position + savedHMD;
                HorseIndic.transform.rotation = Quaternion.LookRotation(Vector3.forward, savedHMD);
                savedHMD2 = savedHMD;
            }

        }
        else if (mode == 4 || (mode == 5 && (HV == 3 || HV == 0)) || (mode == 6))
        {
            if (MoveEqualorO(move.x, savedMov.x) == true && (MoveEqualorO(move.y, savedMov.y) == true) && GroundedOrContinuing(m_Grounded, savedMov, valBuffer))
            {
                //rook prioritzes up, acceleration
                if (HV == 3 && mode == 6) savedMov = new Vector2(0, savedMov.y);
                if (accel < 6 && move != Vector2.zero) accel += (Time.deltaTime * 3);

                targetVelocity = new Vector2(savedMov.x, savedMov.y) * (12 + accel);
                dampSpeed = 0.05f;
                if (savedMov != Vector2.zero)
                {
                    swapTimer = 0;
                    if (m_Grounded == false)
                    {
                        m_Rigidbody2D.gravityScale = 0f;
                        m_PhysicMat2D.friction = 1000f;
                        rookStop = true;
                    }
                }
                if (rookStop == true && move == Vector2.zero)
                {
                    dampSpeed = 0.01f;
                }
            }
            else
            {
                accel = 0;
                if (RookFallCheck(HV, mode))
                {
                    targetVelocity = new Vector2(0, m_Rigidbody2D.velocity.y);
                }
                else
                {
                    targetVelocity = new Vector2(m_Rigidbody2D.velocity.x * 0.9f, m_Rigidbody2D.velocity.y);
                }
                savedMov = new Vector2(1000, 1000);
            }
            if (Mathf.Abs(m_Rigidbody2D.velocity.x) < 1f) rookStop = false;
        }

        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_velocity, dampSpeed);
        m_Rigidbody2D.sharedMaterial = m_Rigidbody2D.sharedMaterial;
    }

    private void Jump()
    {
        if (mode == 1 || mode == 2)
        {
            float JF = m_JumpForce;
            if (Input.GetAxisRaw("Vertical") == 1)
            {
                if (CanJump(CoyoteTimeTimer, firstHop, wRelease))
                {
                    swapTimer = 0;
                    if (JF != m_JumpForce) JF = m_JumpForce;
                    if (PawnDoubleHop(mode, pawnDouble)) JF = m_JumpForce * 1.3f;
                    m_Rigidbody2D.velocity = new Vector3(m_Rigidbody2D.velocity.x, 0);
                    fhTimer = 0.3f;
                    firstHop = true;
                    hopt = 0;
                    m_Rigidbody2D.AddForce(new Vector2(0f, JF));
                    CoyoteTimeTimer = 0;
                    wRelease = false;
                }

                hopt -= Time.deltaTime;
                if (hopt < -0.03f && hopt > -0.18f)
                {
                    m_Rigidbody2D.AddForce(new Vector2(0f, JF / 10));
                }
            }
        }
    }
    private void InputChecks()
    {
        if (mode == 3 && m_Grounded == true)
        {
            //horseyinputs
            if (Input.GetButtonDown("Horizontal")) savedHMD = new Vector2(savedHMD.x + Input.GetAxisRaw("Horizontal"), savedHMD.y);
            if (Input.GetButtonDown("Vertical")) savedHMD = new Vector2(savedHMD.x, savedHMD.y + Input.GetAxisRaw("Vertical"));
            if (Mathf.Abs(savedHMD.x) > 1) savedHMD = new Vector2(0, savedHMD.y);
            if (Mathf.Abs(savedHMD.y) > 1) savedHMD = new Vector2(savedHMD.x, 0);
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            hopt = 100f;
            wRelease = true;
            pawnDouble = true;
        }
    }

    private void CycleModes()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            mode = (mode < 6) ? mode + 1 : 1;

            pawnDouble = false;
            accel = 0;
            savedHMD = Vector2.zero;
            HorseIndic.transform.position = transform.position + savedHMD;
            Debug.Log(mode);
            swapTimer = swapInputTime;
        }
    }

}
