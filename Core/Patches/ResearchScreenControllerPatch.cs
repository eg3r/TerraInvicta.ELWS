using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace ELWS.Core.Patches;

[HarmonyPatch(typeof(ResearchScreenController))]
public static class ResearchScreenControllerPatch
{
    // private static Dictionary<string, TechData> _techCache = new();
    private static bool isInit = false;

    private static void EnableFull(this ResearchScreenController cont, bool bEnable)
    {
        if (!isInit)
            return;

        cont.fullTechTreeCanvas.enabled = bEnable;
        cont.fullTechTreeCanvasNP.enabled = !bEnable;
    }

    public static void ClearCache()
    {
        // _techCache = new Dictionary<string, TechData>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.Hide))]
    private static bool HidePrefix(ResearchScreenController __instance)
    {
        __instance.primaryResearchPanel.enabled = false;
        __instance.archivesOverlayPanel.enabled = false;
        __instance.selectProjectOverlay.enabled = false;
        __instance.selectTechOverlay.enabled = false;
        __instance.rightButtonOverlayPanel.enabled = false;
        __instance.fullTechTreeCanvas.enabled = false;
        __instance.fullTechTreeCanvasNP.enabled = false;
        __instance.selectiveTechTreeCanvas.enabled = false;
        __instance.ResearchScreenPrimaryUITutorialController.HideTutorial();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ShowFullTechTree))]
    private static bool ShowFullTechTreePrefix(ResearchScreenController __instance, string ___selectedProjectEntry)
    {
        if (!__instance.fullTechTreeObject.activeSelf)
            __instance.fullTechTreeObject.SetActive(true);

        // __instance.fullTechTreeObjectNP.SetActive(false);
        __instance.EnableFull(true);

        foreach (var mainTechObject in __instance.mainTechObjectList)
            mainTechObject.UpdateGridItem();

        var toggleVisMethod = AccessTools.Method(typeof(ResearchScreenController), "ToggleTechVisibility");
        toggleVisMethod.Invoke(__instance, null);

        if (string.IsNullOrEmpty(___selectedProjectEntry))
            return false;

        var item = __instance.mainTechObjectList.FirstOrDefault(tech => tech.tech.dataName == ___selectedProjectEntry);
        if (item != null)
            __instance.StartCoroutine(__instance.GotoSearchItemNextFrame(item, true));

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("ResetAllPanels")]
    private static bool ResetAllPanelsPrefix(ResearchScreenController __instance)
    {
        __instance.mainGridObject.SetActive(true);
        __instance.primaryResearchPanel.enabled = true;
        __instance.ShowPrimaryResearchTutorial();
        __instance.archivesOverlayPanel.enabled = false;
        __instance.rightButtonOverlayPanel.enabled = false;
        __instance.selectProjectOverlay.enabled = false;
        __instance.selectTechOverlay.enabled = false;
        __instance.archivesButton.SetActive(true);
        __instance.UpdateResearchLists(__instance.activePlayer);
        __instance.UpdateArchivesPanel(__instance.activePlayer);
        __instance.UpdateSelectTechPanel(__instance.activePlayer);

        __instance.fullTechTreeCanvas.enabled = false;
        __instance.fullTechTreeCanvasNP.enabled = false;
        __instance.selectiveTechTreeCanvas.enabled = true;
        return false;
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.CloseFullTechTree))]
    private static bool CloseFullTechTreePrefix(
        ResearchScreenController __instance,
        string ___selectedProjectEntry,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();

        __instance.fullTechTreeCanvas.enabled = false;
        __instance.fullTechTreeCanvasNP.enabled = string.IsNullOrEmpty(___selectedProjectEntry);

        __instance.rightButtonOverlayPanel.enabled = false;
        __instance.mainGridObject.SetActive(true);
        __instance.archivesButton.SetActive(true);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ShowNoProjectTechTree))]
    private static bool ShowNoProjectTechTreePrefix(
        ResearchScreenController __instance,
        bool ___isFullTechTreeInitNP,
        string ___selectedProjectEntry,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();

        if (!___isFullTechTreeInitNP)
            __instance.InitializeNoProjectTechTree(); // TODO: check for InitializeNoProjectTechTree

        if (!__instance.fullTechTreeObjectNP.activeSelf)
            __instance.fullTechTreeObjectNP.SetActive(true);

        __instance.EnableFull(false);

        __instance.sortedTechList.Clear();
        foreach (var obj in __instance.fullTechTreeGridManagerNP)
        {
            var childTechGridItemController = obj.GetComponent<ChildTechGridItemController>();
            childTechGridItemController.UpdateGridItem();
            __instance.sortedTechList.Add(childTechGridItemController);
        }

        if (string.IsNullOrEmpty(___selectedProjectEntry))
            return false;

        var techItem = __instance.sortedTechList.FirstOrDefault(t => t.tech.dataName == ___selectedProjectEntry);
        if (techItem != null)
            __instance.StartCoroutine(__instance.GotoSearchItemNextFrame(techItem));

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.CloseNoProjectTechTree))]
    private static bool CloseNoProjectTechTreePrefix(ResearchScreenController __instance)
    {
        __instance.fullTechTreeCanvas.enabled = false;
        __instance.fullTechTreeCanvasNP.enabled = false;

        __instance.rightButtonOverlayPanel.enabled = false;
        __instance.mainGridObject.SetActive(true);
        __instance.archivesButton.SetActive(true);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.InitializeNoProjectTechTree))]
    private static bool InitializeNoProjectTechTreePrefix(
        ResearchScreenController __instance,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();

        // __instance.fullTechTreeObjectNP.SetActive(true);
        __instance.EnableFull(false);
        __instance.selectiveTechTreeHeader.SetText(Loc.T("UI.Science.TechTreeHeader"));

        if (__instance.noProjectTechList.Count == 0)
        {
            foreach (var obj in __instance.FullTechTreeGridManager)
            {
                var childTechGridItemController = obj.GetComponent<ChildTechGridItemController>();
                // childTechGridItemController.UpdateGridItem();
                if (!childTechGridItemController.tech.isProject())
                    __instance.noProjectTechList.Add(childTechGridItemController);
            }
        }

        __instance.InitializeFullTechTree(noProjectTree: true);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.InitializeNoProjectTechTree))]
    private static void InitializeNoProjectTechTreePostfix()
    {
        isInit = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.InitializeFullTechTree))]
    private static bool InitializeFullTechTreePrefix(
        bool selectiveTree,
        string selectiveTech,
        bool noProjectTree,
        bool selectiveFromFull,
        ResearchScreenController __instance)
    {
        __instance.EnableFull(selectiveFromFull);
        return true;
    }
}