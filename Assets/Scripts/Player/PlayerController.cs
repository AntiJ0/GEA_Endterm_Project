using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;            
    public float waterMoveSpeed = 2f;       
    public float jumpPower = 5f;

    [Header("Gravity Settings")]
    public float normalGravity = -9.81f;    
    public float waterGravity = -2f;        
    public float floatUpSpeed = 2f;

    [Header("Water Exit Slow Settings")]
    public float waterExitSlowDuration = 0.5f;  

    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;

    float xRotation = 0f;
    CharacterController controller;
    Transform cam;

    Vector3 velocity;
    bool isGrounded;

    int waterContactCount = 0;
    float currentGravity;

    float waterSlowTimer = 0f;   

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>().transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentGravity = normalGravity;
    }

    void Update()
    {
        HandleMove();
        HandleLook();

        if (waterSlowTimer > 0)
            waterSlowTimer -= Time.deltaTime;
    }

    void HandleMove()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.right * h + transform.forward * v;

        float finalSpeed = moveSpeed;

        if (waterContactCount > 0)
        {
            finalSpeed = waterMoveSpeed;
        }
        else if (waterSlowTimer > 0)
        {
            finalSpeed = waterMoveSpeed;
        }

        controller.Move(moveDir * finalSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpPower * -2f * normalGravity);

        if (waterContactCount > 0 && Input.GetButton("Jump"))
        {
            velocity.y = floatUpSpeed;
        }
        else
        {
            velocity.y += currentGravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -100f, 100f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Block block = other.GetComponent<Block>();

        if (block != null && block.type == BlockType.Water)
        {
            waterContactCount++;
            currentGravity = waterGravity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Block block = other.GetComponent<Block>();

        if (block != null && block.type == BlockType.Water)
        {
            waterContactCount--;

            if (waterContactCount <= 0)
            {
                waterContactCount = 0;

                waterSlowTimer = waterExitSlowDuration;

                currentGravity = normalGravity;
            }
        }
    }
}