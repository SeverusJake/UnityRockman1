using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 speed = new Vector2(50, 50);
    public float jumpPower = 0.1f;

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(speed.x * inputX, speed.y * inputY, 0);

        movement *= Time.deltaTime;

        transform.Translate(movement);

        Debug.DrawRay(transform.position, Vector2.down, Color.red, 0.7f);
        if (IsGrounded())
        {
            Debug.Log("Ground");
        }
        else
        {
            Debug.Log("Air");
        }

        if (Input.GetButton("Jump") && IsGrounded())
        {
            Jump();
        }
    }

    private void Jump() => transform.Translate(new Vector2(0, jumpPower));

    private bool IsGrounded()
    {
        var groundCheck = Physics2D.Raycast(transform.position, Vector2.down, 0.7f);

        Debug.Log("ground: " + groundCheck.collider.CompareTag("Ground"));
        if (groundCheck.collider != null)
        {
            Debug.Log("collider: " + groundCheck.collider.name);
        }

        return groundCheck.collider != null && groundCheck.collider.CompareTag("Ground");
    }
}
