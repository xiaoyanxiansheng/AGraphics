using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Wobble : MonoBehaviour
{
    Renderer rend;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;  
    Vector3 angularVelocity;
    // public float MaxWobble = 0.03f;
    // public float WobbleSpeed = 1f;
    // public float Recovery = 1f;
    float wobbleAmountX;
    float wobbleAmountZ;
    float wobbleAmountToAddX;
    float wobbleAmountToAddZ;
    float pulse;
    float time = 0.5f;

    public float WobbleHeight = 1;
    public float WobbleCount = 5;
    public float WobbleSpeed = 1;
    public float WobbleSpeed1 = 1;
    public float WobbleSpeed2 = 0.5f;

    // Use this for initialization
    void Start()
    {
        rend = GetComponent<Renderer>();
    }
    // private void Update()
    // {
    //     time += Time.deltaTime;
    //     // decrease wobble over time
    //     wobbleAmountToAddX = Mathf.Lerp(1, 0, Time.deltaTime * (Recovery));
    //     wobbleAmountToAddZ = Mathf.Lerp(1, 0, Time.deltaTime * (Recovery));
    //
    //     // make a sine wave of the decreasing wobble
    //     pulse = 2 * Mathf.PI * WobbleSpeed;
    //     wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
    //     wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);
    //
    //     Debug.Log(wobbleAmountX);
    //     
    //     // send it to the shader
    //     rend.sharedMaterial.SetFloat("_WobbleX", wobbleAmountX);
    //     rend.sharedMaterial.SetFloat("_WobbleZ", wobbleAmountZ);
    //
    //     // velocity
    //     velocity = (lastPos - transform.position) / Time.deltaTime;
    //     angularVelocity = transform.rotation.eulerAngles - lastRot;
    //     
    //     // add clamped velocity to wobble
    //     wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
    //     wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
    //
    //     // keep last position
    //     lastPos = transform.position;
    //     lastRot = transform.rotation.eulerAngles;
    // }

    private void Update()
    {
        float deltaTime = Time.deltaTime * WobbleSpeed;
        time += deltaTime;
        
        wobbleAmountToAddX *= 1 - deltaTime * WobbleSpeed1;
        //wobbleAmountToAddX = Mathf.Min(0.2f, Mathf.Max(-0.2f,wobbleAmountToAddX));
        //wobbleAmountToAddX *= wobbleAmountToAddX;
        
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(2*Mathf.PI * WobbleCount * time) * WobbleHeight;

        rend.sharedMaterial.SetFloat("_WobbleX", wobbleAmountX);
        rend.sharedMaterial.SetFloat("_WobbleZ", wobbleAmountZ);
        
        velocity = (lastPos - transform.position) / deltaTime;
        velocity.x = Mathf.Min(WobbleSpeed2, Mathf.Max(-WobbleSpeed2, velocity.x));
        velocity *= 0.01f;
        wobbleAmountToAddX += Mathf.Clamp(velocity.x + (angularVelocity.z * 0.2f), -1, 1);
        
        
        lastPos = transform.position;
    }
}