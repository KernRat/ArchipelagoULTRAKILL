﻿using UnityEngine;

namespace ArchipelagoULTRAKILL.Powerups
{
    public class StaminaPowerup : MonoBehaviour
    {
        private PowerUpMeter meter;
        public float juiceAmount;
        private bool juiceGiven;

        private void Start()
        {
            meter = PowerUpMeter.Instance;
            if (juiceAmount == 0f) juiceAmount = 15f;
            if (meter.juice < juiceAmount)
            {
                meter.latestMaxJuice = juiceAmount;
                meter.juice = juiceAmount;
            }
            meter.powerUpColor = ColorBlindSettings.Instance.staminaColor;
            juiceGiven = true;
            Core.staminaPowerup = true;
        }

        private void Update()
        {
            if (juiceGiven && meter.juice <= 0f)
            {
                EndPowerUp();
                return;
            }
        }

        public void EndPowerUp()
        {
            Core.staminaPowerup = false;
            Core.poweredUp = false;
            if (LocationManager.powerupQueue.Count > 0) Core.obj.GetComponent<Core>().Invoke("AddPowerup", 1f);
            Destroy(gameObject);
        }
    }
}
