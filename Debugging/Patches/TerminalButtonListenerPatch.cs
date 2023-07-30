using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Debugging;
using UnityEngine;

// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace ELWS.Debugging.Patches;

[HarmonyPatch(typeof(TerminalButtonListener), nameof(TerminalButtonListener.Tick))]
public class TerminalButtonListenerPatch
{
    static void Postfix(Terminal ___terminal, ref bool ___isShowingConsole)
    {
        if (Input.GetKeyDown(KeyCode.Keypad9) && TemplateManager.global.debug_ConsoleActive)
        {
            ___isShowingConsole = !___isShowingConsole;
            if (___isShowingConsole)
                ___terminal.Show();
            else
                ___terminal.Hide();
        }
    }
}