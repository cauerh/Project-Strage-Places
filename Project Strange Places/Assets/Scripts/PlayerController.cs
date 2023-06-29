using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;

    [SerializeField] private int amountOfJumps;
    private int amountOfJumpsLeft;
    [SerializeField] private int facingDirection;
    [SerializeField] private int lastWallJumpDirection;

    private float moveInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;

    private bool isFacinfRight = true;
    private bool isRunning;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isTouchingWall;
    [SerializeField] private bool isWallSliding;
    [SerializeField] private bool canNormalJump;
    [SerializeField] private bool canWallJump;
    [SerializeField] private bool isAttemptingToJump;
    private bool CheckJumpMultiplier;
    [SerializeField] private bool canMove;
    [SerializeField] private bool canFlip;
    [SerializeField] private bool hasWallJumped;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float moveForceInAir;
    [SerializeField] private float airDragMultiplier;
    [SerializeField] private float variableJumpHeightMultiplier;
    [SerializeField] private float wallHopForce;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float jumpTimerSet;
    [SerializeField] private float TurnTimerSet;
    [SerializeField] private float wallJumpTimerSet;


    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private Vector2 wallHopDirection;
    [SerializeField] private Vector2 wallJumpDirection;

    [SerializeField] Transform groundCheck;
    [SerializeField] Transform wallCheck;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        if(isTouchingWall && moveInputDirection == facingDirection && rb.velocity.y < 0)
            isWallSliding = true;
        else
            isWallSliding = false;
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y <= 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps;
        }

        if (isTouchingWall)
            canWallJump = true;

        if (amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }

    private void CheckMovementDirection()
    {
        if (isFacinfRight && moveInputDirection < 0)
            Flip();
        else if (!isFacinfRight && moveInputDirection > 0)
            Flip();

        if (rb.velocity.x != 0)
            isRunning = true;
        else
            isRunning = false;
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void CheckInput()
    {
        moveInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            if(isGrounded || (amountOfJumpsLeft > 0 && isTouchingWall))
                NormalJump();
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }

        if(Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if(!isGrounded && moveInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = TurnTimerSet;
            }
        }

        if (!canMove)
        {
            turnTimer -= Time.deltaTime;

            if(turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (CheckJumpMultiplier && !Input.GetButton("Jump"))
        {
            CheckJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckJump()
    {
        if(jumpTimer > 0)
        {
            //Wall Jump
            if (!isGrounded && isTouchingWall && moveInputDirection != 0 && moveInputDirection != facingDirection)
                WallJump();
            else if (isGrounded)
                NormalJump();
        }
        if(isAttemptingToJump)
            jumpTimer -= Time.deltaTime;

        if(wallJumpTimer > 0)
        {
            if (hasWallJumped && moveInputDirection == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
                hasWallJumped = false;
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
    }

    private void NormalJump()
    {
        if (canNormalJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            CheckJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * moveInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            CheckJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded && !isWallSliding && moveInputDirection == 0)
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        else if(canMove)
            rb.velocity = new Vector2(moveSpeed * moveInputDirection, rb.velocity.y);
   
        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }

    private void Flip()
    {
        if (!isWallSliding && canFlip)
        {
            facingDirection *= -1;
            isFacinfRight = !isFacinfRight;
            transform.Rotate(0, 180, 0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }

}
