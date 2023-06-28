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

    private float moveInputDirection;

    private bool isFacinfRight = true;
    private bool isRunning;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isTouchingWall;
    [SerializeField] private bool isWallSliding;
    private bool canJump;

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
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        if(isTouchingWall && !isGrounded && rb.velocity.y < 0)
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
        if ((isGrounded && rb.velocity.y <= 0) || isWallSliding)
        {
            amountOfJumpsLeft = amountOfJumps;
        }
        if (amountOfJumpsLeft <= 0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
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
            Jump();

        if(Input.GetButtonUp("Jump"))
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
    }

    private void Jump()
    {
        if (canJump && !isWallSliding)
        {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        amountOfJumpsLeft--;
        }
        else if(isWallSliding && moveInputDirection == 0 && canJump)
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        else if ((isWallSliding || isTouchingWall) && moveInputDirection != 0 && canJump)
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * moveInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }

    }

    private void ApplyMovement()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(moveSpeed * moveInputDirection, rb.velocity.y);
        }
        else if (!isGrounded && !isWallSliding && moveInputDirection != 0)
        {
            Vector2 forceToAdd = new Vector2(moveForceInAir * moveInputDirection, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > moveSpeed)
                rb.velocity = new Vector2(moveSpeed * moveInputDirection, rb.velocity.y);
        }
        else if (!isGrounded && !isWallSliding && moveInputDirection == 0)
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);

        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }

    private void Flip()
    {
        if (!isWallSliding)
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
