using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using UnityEngine;

// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace ELWS.Core.Patches;

[HarmonyPatch(typeof(TISpaceFleetState))]
public static class TISpaceFleetStatePatch
{
    private static float GetPropellantTonsForDesiredDv(this TISpaceShipState ship, float desiredDv)
    {
        return (float)(Mathd.Exp(desiredDv / ship.currentEV_kps) * ship.dryMass_tons - ship.dryMass_tons);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TISpaceFleetState.CreatePropellantSharingPlan_Equalization))]
    private static bool CreatePropellantSharingPlan_EqualizationPrefix(
        List<TISpaceShipState> group,
        ref List<PropellantSharingEvent> __result,
        TISpaceFleetState __instance
    )
    {
        var eqDeltaV = CalcEqualDeltaV(group);
        __result = new List<PropellantSharingEvent>();

        var shipPropellantSurpluses = group
            .Select(s => (amount: s.propellant_tons - s.GetPropellantTonsForDesiredDv(eqDeltaV), ship: s))
            .OrderByDescending(x => x.amount).ToList();

        var highIndex = 0;
        var lowIndex = shipPropellantSurpluses.Count - 1;

        while (highIndex < lowIndex)
        {
            var highest = shipPropellantSurpluses[highIndex];
            var lowest = shipPropellantSurpluses[lowIndex];

            if (highest.amount > 0 && lowest.amount < 0)
            {
                var shareAmount = Math.Min(highest.amount, Math.Abs(lowest.amount));
                highest.amount -= shareAmount;
                lowest.amount += shareAmount;

                shipPropellantSurpluses[highIndex] = highest;
                shipPropellantSurpluses[lowIndex] = lowest;

                if (shareAmount > 0)
                    __result.Add(new() { giver = highest.ship, taker = lowest.ship, amount_tons = shareAmount });
            }

            if (highest.amount <= 0)
                highIndex++;
            if (lowest.amount >= 0)
                lowIndex--;
        }

        return false;
    }

    private const float Tolerance = 0.01f;

    private static float CalcEqualDeltaV(List<TISpaceShipState> group)
    {
        var lowerBound = float.MaxValue;
        var upperBound = float.MinValue;
        var availablePropellant = 0.0f;

        foreach (var ship in group)
        {
            lowerBound = Math.Min(lowerBound, ship.currentDeltaV_kps);
            upperBound = Math.Max(upperBound, ship.currentDeltaV_kps);
            availablePropellant += ship.propellant_tons;
        }

        var midDeltaV = (lowerBound + upperBound) / 2;

        while (upperBound - lowerBound > Tolerance)
        {
            var totalPropellantRequired = group.Sum(x => x.GetPropellantTonsForDesiredDv(midDeltaV));

            if (totalPropellantRequired > availablePropellant)
                upperBound = midDeltaV;
            else
                lowerBound = midDeltaV;

            midDeltaV = (lowerBound + upperBound) / 2;
        }

        return midDeltaV;
    }
}