using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global

namespace ELWS.Core.Transpiler;

[HarmonyPatch(typeof(ResearchScreenController), nameof(ResearchScreenController.InitializeFullTechTree))]
public static class ResearchScreenControllerTranspiler
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        const int startIndex = 4;
        const int endIndex = 14;
        var codes = new List<CodeInstruction>(instructions);
        codes[startIndex].opcode = OpCodes.Nop;
        codes.RemoveRange(startIndex + 1, endIndex - startIndex);

        return codes.AsEnumerable();
    }
}