using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PMove : BooleanLogic
{
    private Rigidbody2D m_Rigidbody2D;
    private PhysicsMaterial2D m_PhysicMat2D;
    [SerializeField] private Sprite[] pSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector3 m_velocity = Vector3.zero;
    private Vector2 move;
    float HV;
    private Vector2 savedMov;
    private Vector3 savedHMD;
    private Vector3 savedHMD2;
    float accel;
    float fallAccel;
    float dampSpeed;
    float bufferMove = 0.08f;
    float bufferMoveTimer;
    float isR;
    bool validBuffer;
    bool rookStop;
    public GameObject HorseIndic;

    private bool m_Grounded;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private GameObject m_GroundCheck;
    private float raycastDistance;

    private float swapInputTime = 1f;
    private float swapTimer;
    private bool swapGrounded;
    private float airOffset;
    private float m_JumpForce = 600f;
    private float hopt;
    private float fhTimer;

    bool pawnDouble;
    private bool firstHop = true;
    private bool wRelease = true;
    
    private int mode;


    void Start()
    {
        mode = 1;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PhysicMat2D = m_Rigidbody2D.sharedMaterial;
    }

    void FixedUpdate()
    {
        SwapSlow();
        GroundCalc();
        Movement();
        Jump();
    }

    private void Update()
    {
        Misc();
        CycleModes();
        GroundRaycast();
        InputChecks();
    }

    
    private void Misc()
    {
        Camera.main.transform.position = Vector3.Lerp(new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10), new Vector3(transform.position.x, transform.position.y + 3, -10), Time.deltaTime * 5);
        Application.targetFrameRate = 120;
        if (transform.position.y < -100 || transform.position.y > 100)
        {
            transform.position = Vector3.zero;
        }
        
    }
    private void GroundCalc()
    {
        if (swapTimer > 0) swapTimer -= Time.unscaledDeltaTime;
        bufferMoveTimer -= Time.deltaTime;
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
        bool NTW = leftRayW.collider == null && rightRayW.collider == null;
        airOffset = (NTW) ? 0f : 0.25f;

        RaycastHit2D middleRay = Physics2D.Raycast(T.position, Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D leftRay = Physics2D.Raycast(new Vector2(T.position.x - (T.lossyScale.x / 2) + airOffset, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D rightRay = Physics2D.Raycast(new Vector2(T.position.x + (T.lossyScale.x / 2) - airOffset, T.position.y), Vector2.down, raycastDistance, m_WhatIsGround);
        RaycastHit2D[] rays = { middleRay, leftRay, rightRay };
        m_Grounded = false;
        swapGrounded = false;
        raycastDistance = 0.5f;

        bool VB = middleRay.collider != null || leftRay.collider != null || rightRay.collider != null;
        if (isG(rays) || swapTimer > 0 + bufferMove)
        {
            if (rightRay.collider == null && leftRay.collider != null) isR = 1;
            else if (leftRay.collider == null && rightRay.collider != null) isR = -1;
            else isR = 0;
            if (swapTimer > 0 + bufferMove)
            {
                swapGrounded = true;
                if (isG(rays)) swapTimer = 0;
            }
            m_Grounded = true;

            bufferMoveTimer = bufferMove;
            UpdateSavedMov();
            validBuffer = false;
        }
        else
        {
            if (bufferMoveTimer > 0 && ((VB && Mathf.Abs(move.y) == 1) || (isR == move.x && move.x != 0)  || swapTimer > 0))
            {
                UpdateSavedMov();
                validBuffer = true;
            }
        }
    }

    private bool isG(RaycastHit2D[] r)
    {
        foreach (RaycastHit2D target in r)
        {
            if (target.collider != null && target.distance < 0.03f)
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateSavedMov()
    {
        savedMov = move;
        HV = Mathf.Abs(savedMov.x) + Mathf.Abs(savedMov.y * 2);
    }

    private void Movement()
    {
        move = Vector2.zero;
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 targetVelocity;
        targetVelocity = (m_Grounded && !swapGrounded) ? m_Rigidbody2D.velocity / 2 : m_Rigidbody2D.velocity;
        m_PhysicMat2D.friction = 0f;
        m_Rigidbody2D.gravityScale = 7f;
        if (mode < 4)
        {
            if (mode < 3)
            {
                bufferMove = 0.15f;
                if (mode == 2) move = new Vector2(0, m_Rigidbody2D.velocity.y);
                targetVelocity = new Vector2(move.x * 10f, m_Rigidbody2D.velocity.y);
            }
            if (mode == 3)
            {
                float HM = 2;
                bool Active = (savedHMD == Vector3.zero) ? false : true;
                HorseIndic.SetActive(Active);
                if(Mathf.Abs(savedHMD.x) + Mathf.Abs(savedHMD.y) == 2)
                {
                    swapTimer = 0;
                    transform.position += savedHMD2 * 2 * HM;
                    transform.position += (savedHMD - savedHMD2) * HM;
                    m_Rigidbody2D.velocity = Vector2.zero;
                    savedHMD = Vector2.zero;
                }
                HorseIndic.transform.position = transform.position + savedHMD;
                HorseIndic.transform.rotation = Quaternion.LookRotation(Vector3.forward, savedHMD);
                savedHMD2 = savedHMD;
            }

        }
        else if (mode == 4 || (mode == 5 && (HV == 3 || HV == 0)) || (mode == 6))
        {
            bufferMove = 0.2f;
            if (MoveEqualorO(move.x, savedMov.x) == true && (MoveEqualorO(move.y, savedMov.y) == true) && GroundedOrContinuing(m_Grounded, savedMov, validBuffer))
            {
                //rook prioritzes up, acceleration
                if (HV == 3 && mode == 6) savedMov = new Vector2(0, savedMov.y);
                if (accel < 8 && move != Vector2.zero) accel += (Time.deltaTime * (8 - accel));
                targetVelocity = new Vector2(savedMov.x, savedMov.y) * (14 + Mathf.Abs(accel));

                if (savedMov != Vector2.zero && m_Grounded == false)
                {
                    m_Rigidbody2D.gravityScale = 0f;
                    m_PhysicMat2D.friction = 1000f;
                    rookStop = true;
                }

                //swapping timer logic
                if (swapGrounded == true)
                {
                    if (move != Vector2.zero && swapTimer < swapInputTime) swapTimer = bufferMove;
                    else targetVelocity = m_Rigidbody2D.velocity;
                }
                
                dampSpeed = 0.06f;
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
                    targetVelocity = new Vector2(m_Rigidbody2D.velocity.x * 0.95f, m_Rigidbody2D.velocity.y);
                }
                savedMov = new Vector2(1000, 1000);
            }
            if (Mathf.Abs(m_Rigidbody2D.velocity.x) < 1f) rookStop = false;
        }
        if (targetVelocity.y < -10 - fallAccel)
        {
            //targetVelocity = (m_Rigidbody2D.velocity.y > -20) ? new Vector2(targetVelocity.x, -10 - fallAccel) : new Vector2(targetVelocity.x, -20);
            fallAccel += ((Time.deltaTime) + fallAccel);
        }
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_velocity, dampSpeed, Mathf.Infinity, Time.fixedDeltaTime);
        m_Rigidbody2D.sharedMaterial = m_Rigidbody2D.sharedMaterial;
    }

    private void Jump()
    {
        
        if (mode == 1 || mode == 2)
        {
            float JF = m_JumpForce;
            if (Input.GetAxisRaw("Vertical") == 1)
            {
                if (CanJump(bufferMoveTimer, firstHop, wRelease))
                {
                    Debug.Log("s");
                    swapTimer = 0;
                    if (PawnDoubleHop(mode, pawnDouble)) JF = m_JumpForce * 1.6f;
                    pawnDouble = true;
                    m_Rigidbody2D.velocity = new Vector3(m_Rigidbody2D.velocity.x, 0);
                    fhTimer = 0.3f;
                    firstHop = true;
                    hopt = 0;
                    m_Rigidbody2D.AddForce(new Vector2(0f, JF));
                    bufferMoveTimer = 0;
                    wRelease = false;
                }

                hopt -= Time.deltaTime;
                if (hopt < -0.03f && hopt > -0.18f) m_Rigidbody2D.AddForce(new Vector2(0f, JF / 10));
                
            }
        }
        else if (Input.GetAxisRaw("Vertical") == 1) wRelease = false;
    }
    private void InputChecks()
    {

        if (mode == 3 && m_Grounded == true)
        {
            //horseyinputs
            if (Input.GetButtonDown("Horizontal")) savedHMD = new Vector2(savedHMD.x + Input.GetAxisRaw("Horizontal"), savedHMD.y);
            if (Input.GetButtonDown("Vertical")) savedHMD = new Vector2(savedHMD.x, savedHMD.y + Input.GetAxisRaw("Vertical"));
            if (Mathf.Abs(savedHMD.x) > 1 || Mathf.Abs(savedHMD.y) > 1) savedHMD = Vector2.zero;
        }
        if (mode != 3)
        {
            savedHMD = Vector2.zero;
            HorseIndic.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            hopt = 100f;
            wRelease = true;
        }
    }

    private void CycleModes()
    {
        spriteRenderer.sprite = pSprites[mode - 1];
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            mode = 1;
            Debug.Log(mode);
            swapTimer = swapInputTime;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            pawnDouble = false;
            mode = 2;
            Debug.Log(mode);
            swapTimer = swapInputTime;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            mode = 3;
            Debug.Log(mode);
            swapTimer = swapInputTime;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            mode = 4;
            Debug.Log(mode);
            swapTimer = swapInputTime;
            accel = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            mode = 5;
            Debug.Log(mode);
            swapTimer = swapInputTime;
            accel = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            mode = 6;
            Debug.Log(mode);
            swapTimer = swapInputTime;
            accel = 0;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            //mode = (mode < 6) ? mode + 1 : 1;
            //pawnDouble = false;

            //accel = -6;
            //Debug.Log(mode);
            //swapTimer = swapInputTime;
        }
    }

    private void SwapSlow()
    {
        //Time.timeScale = (1 - swapTimer/3.3f);
        //Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

}
