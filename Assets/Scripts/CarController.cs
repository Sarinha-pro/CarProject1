using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{    
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBreakForce;
    private bool isBreaking;
    private bool isHandbrakeActive;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("Wheel Transforms")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("Car Settings")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float breakForce = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;

    [Header("Lights")]
    [SerializeField] private Light[] headlights; // Faróis

    [Header("Suspension")]
    [SerializeField] private float suspensionSpring = 5000f; // Força da suspensão
    [SerializeField] private float suspensionDamper = 1000f; // Amortecimento da suspensão
    [SerializeField] private float suspensionDistance = 0.3f; // Distância da suspensão

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;  // Prefab do projétil
    [SerializeField] private Transform firePoint;      // Ponto de onde o tiro será disparado
    [SerializeField] private float bulletSpeed = 10f;  // Velocidade do projétil
    [SerializeField] private float fireRate = 0.5f;    // Intervalo entre os disparos

    private float nextFireTime = 0f; // Controle do tempo para o próximo disparo
    public static int score = 0;     // Pontuação

    private void Start()
    {
        // Ajustando a suspensão para o estilo de jeep
        AdjustSuspension();
    }

    private void FixedUpdate() 
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        RestartPosition(); 
        HandleHandbrake();
    }

    private void Update()
    {
        // Verifica se o tempo para disparar foi atingido e se o jogador pressionou o botão de disparo
        if (Time.time >= nextFireTime && Input.GetKeyDown(KeyCode.Space)) 
        {
            Shoot();
            nextFireTime = Time.time + fireRate; // Atualiza o tempo para o próximo disparo
        }
    }

    private void Shoot()
    {
        // Instancia o projétil na posição do ponto de disparo
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Obtém o Rigidbody2D do projétil e define sua velocidade
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = firePoint.right * bulletSpeed; // A direção do projétil segue o eixo X (direção do "firePoint")
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKey(KeyCode.Space); // Freio normal
        isHandbrakeActive = Input.GetKey(KeyCode.LeftShift); // Freio de mão
    }

    private void HandleMotor()
    {
        // Tração nas quatro rodas
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
        rearRightWheelCollider.motorTorque = verticalInput * motorForce;

        // Controle do freio
        currentBreakForce = isBreaking || isHandbrakeActive ? breakForce : 0f;
        ApplyBreaking();   
    }

    private void ApplyBreaking()
    {
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        rearLeftWheelCollider.brakeTorque = currentBreakForce;
        rearRightWheelCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering()
    {
        // Direção nas rodas dianteiras
        currentSteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheelCollider(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheelCollider(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheelCollider(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheelCollider(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheelCollider(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void RestartPosition()
    {
        if (Input.GetKey("r"))
        {
            Debug.Log("RestartPosition");
            transform.position = new Vector3(3f, transform.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    // Ajustando a suspensão para o estilo de jeep
    private void AdjustSuspension()
    {
        JointSpring spring = frontLeftWheelCollider.suspensionSpring;
        spring.spring = suspensionSpring;
        spring.damper = suspensionDamper;
        frontLeftWheelCollider.suspensionSpring = spring;
        frontRightWheelCollider.suspensionSpring = spring;
        rearLeftWheelCollider.suspensionSpring = spring;
        rearRightWheelCollider.suspensionSpring = spring;

        frontLeftWheelCollider.suspensionDistance = suspensionDistance;
        frontRightWheelCollider.suspensionDistance = suspensionDistance;
        rearLeftWheelCollider.suspensionDistance = suspensionDistance;
        rearRightWheelCollider.suspensionDistance = suspensionDistance;
    }

    // Implementando o freio de mão
    private void HandleHandbrake()
    {
        if (isHandbrakeActive)
        {
            ApplyHandbrake();
        }
        else
        {
            // Se o freio de mão não estiver ativo, as rodas podem girar normalmente
            RemoveHandbrake();
        }
    }

    private void ApplyHandbrake()
    {
        frontLeftWheelCollider.brakeTorque = breakForce * 10f; // Aumenta a força do freio de mão
        frontRightWheelCollider.brakeTorque = breakForce * 10f;
        rearLeftWheelCollider.brakeTorque = breakForce * 10f;
        rearRightWheelCollider.brakeTorque = breakForce * 10f;
    }

    private void RemoveHandbrake()
    {
        frontLeftWheelCollider.brakeTorque = 0f;
        frontRightWheelCollider.brakeTorque = 0f;
        rearLeftWheelCollider.brakeTorque = 0f;
        rearRightWheelCollider.brakeTorque = 0f;
    }

    // Método para exibir a pontuação (pode ser usado com UI)
    public static void DisplayScore()
    {
        Debug.Log("Pontuação: " + score);
    }
}
