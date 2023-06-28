using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;

    [SerializeField] private int amountOfJumps;
    [SerializeField] private int amountOfJumpsLeft;

    private float moveInputDirection;

    private bool isFacinfRight = true;
    private bool isRunning;
    [SerializeField] private bool isGrounded;
    private bool canJump;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float groundCheckRadius;

    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] Transform groundCheck;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
    }

    
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y <= 0)
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
    }

    private void CheckInput()
    {
        moveInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            Jump();
    }

    private void Jump()
    {
        if (canJump)
        {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        amountOfJumpsLeft--;
        }
    }

    private void ApplyMovement()
    {
        rb.velocity = new Vector2(moveSpeed * moveInputDirection, rb.velocity.y);
    }

    private void Flip()
    {
        isFacinfRight = !isFacinfRight;
        transform.Rotate(0, 180, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
