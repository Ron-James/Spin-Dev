using System;
using ArcadeVP;
using UnityEngine;

public class NitroController : MonoBehaviour
{
    ArcadeVehicleController arcadeVehicleController;
    [SerializeField] private ActionReference<float> currentSpeedBoost = new(0);
    [SerializeField] private ActionReference<float> defaultMaxSpeed = new(1);
    [SerializeField] private ActionReference<float> defaultAcceleration = new(1);

    private void Awake()
    {
        arcadeVehicleController = GetComponent<ArcadeVehicleController>();
    }

    private void OnEnable()
    {
        currentSpeedBoost.ActionCasted.Subscribe(this, nameof(OnSpeedBoost), OnSpeedBoost);
    }
    
    private void OnDisable()
    {
        currentSpeedBoost.ActionCasted.UnsubscribeAll(this);
    }
    
    private void OnSpeedBoost(float speedBoost)
    {
        arcadeVehicleController.accelaration = defaultAcceleration * speedBoost;
        arcadeVehicleController.MaxSpeed = defaultMaxSpeed * speedBoost;
    }

}
