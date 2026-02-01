using UnityEngine;
using System;

public class veolicty_control_v1 : MonoBehaviour
{   
    public float lvx = 0.0f;
    public float lvy = 0.0f;
    public float lvz = 0.0f;
    public float avz = 0.0f;
    public bool movemenentActive = false;
    public Rigidbody rb;

    [Header("Buoyancy Settings")]
    public float buoyancySpeed = 0.2f;   // upward m/s
    public float waterSurfaceY = 4.85f;    // Y position of pool surface
    public float surfaceTolerance = 0.02f;

    void Start(){
        this.rb = GetComponent<Rigidbody>();
    }

    private void moveVelocityRigidbody(){
        Vector3 movement = new Vector3(
            -lvx * Time.deltaTime,
            lvz * Time.deltaTime,
            lvy * Time.deltaTime
        );

        transform.Translate(movement);
        transform.Rotate(0, avz * Time.deltaTime, 0);
    }

    private void ApplyBuoyancy()
    {
        // Only float up if below the water surface
        if (transform.position.y < waterSurfaceY - surfaceTolerance)
        {
            transform.Translate(Vector3.up * buoyancySpeed * Time.deltaTime);
        }
        else
        {
            // Clamp exactly at surface
            Vector3 pos = transform.position;
            pos.y = waterSurfaceY;
            transform.position = pos;
        }
    }

    public void moveVelocity(RosMessageTypes.Geometry.TwistMsg velocityMessage){
        this.lvx = (float)velocityMessage.linear.x;
        this.lvy = (float)velocityMessage.linear.y;
        this.lvz = (float)velocityMessage.linear.z;
        this.avz = (float)velocityMessage.angular.z;
        this.movemenentActive = true;
    }

    void FixedUpdate(){
        ApplyBuoyancy();

        if (movemenentActive){
            moveVelocityRigidbody();
            this.movemenentActive = false;
        }
    }
}
