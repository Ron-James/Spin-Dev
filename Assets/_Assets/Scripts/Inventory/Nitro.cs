using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(fileName = "Nitro", menuName = "Consumables/Nitro")]
public class Nitro : Consumable
{
    [Title("Stats")]
    [SerializeField] private ActionReference<float> duration = new(5);
    [SerializeField] private ActionReference<float> speedBoostMultiplier = new(10);
    [SerializeField] private ActionReference<float> currentSpeedBoost = new(0);
    public override async Task Consume(int quantity = 1)
    {
        currentSpeedBoost.Value = speedBoostMultiplier;
        float time = duration * quantity;
        float ticker = 0;
        while(ticker < time)
        {
            ticker += Time.deltaTime;
            currentSpeedBoost.Value = speedBoostMultiplier;
            await Awaitable.NextFrameAsync();
        }
        currentSpeedBoost.Value = 1;
        await base.Consume(quantity);
    }
}