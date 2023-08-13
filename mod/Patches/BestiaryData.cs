﻿using HarmonyLib;
using System.Collections.Generic;

namespace ArchipelagoULTRAKILL.Patches
{
    [HarmonyPatch(typeof(BestiaryData), "SetEnemy")]
    public class BestiaryData_SetEnemy_Patch
    {
        public static List<EnemyType> standard = new List<EnemyType>()
        {
            EnemyType.V2,
            EnemyType.Minos,
            EnemyType.Gabriel,
            EnemyType.V2Second,
            EnemyType.Leviathan,
            EnemyType.GabrielSecond
        };

        public static List<EnemyType> extended = new List<EnemyType>() 
        { 
            EnemyType.VeryCancerousRodent,
            EnemyType.Mandalore
        };

        public static void Prefix(ref EnemyType enemy, ref int newState)
        {
            if (Core.DataExists() && newState >= 2)
            {
                if (standard.Contains(enemy) && Core.data.bossRewards > 0) LocationManager.CheckLocation(StatsManager.Instance.levelNumber.ToString() + "_b");
                if (extended.Contains(enemy) && Core.data.bossRewards == 2) LocationManager.CheckLocation(StatsManager.Instance.levelNumber.ToString() + "_b");
            }
        }
    }
}
