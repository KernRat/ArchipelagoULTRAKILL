﻿using HarmonyLib;
using UnityEngine;

namespace ArchipelagoULTRAKILL.Patches
{
    [HarmonyPatch(typeof(ShopZone), "TurnOn")]
    public class ShopZone_TurnOn_Patch
    {
        public static void Postfix(ShopZone __instance)
        {
            foreach (VariationInfo variation in Traverse.Create(__instance).Field<Canvas>("shopCanvas").Value.GetComponentsInChildren<VariationInfo>())
            {
                LevelManager.UpdateShopVariation(variation);
            }
        }
    }
}
