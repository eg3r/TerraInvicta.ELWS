using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace ELWS.Core.Patches;

public struct TechData
{
    public string DisplayName;
    public string TechStatus;
    public string ToolTipString;
    public Color Color;
    public ChildTechGridItemController Component;
}

[HarmonyPatch(typeof(ResearchScreenController))]
public static class ResearchScreenControllerPatch
{
    private static Dictionary<string, TechData> _techCache = new();
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
        _techCache = new Dictionary<string, TechData>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.Hide))]
    private static bool HidePrefix(
        ResearchScreenController __instance,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();

        __instance.primaryResearchPanel.enabled = false;
        __instance.archivesOverlayPanel.enabled = false;
        __instance.selectProjectOverlay.enabled = false;
        __instance.selectTechOverlay.enabled = false;
        __instance.rightButtonOverlayPanel.enabled = false;
        __instance.fullTechTreeCanvas.enabled = false;
        __instance.fullTechTreeCanvasNP.enabled = false;
        __instance.selectiveTechTreeCanvas.enabled = false;
        __instance.ResearchScreenPrimaryUITutorialController.HideTutorial();


        //__instance.Canvas.enabled = false;
        // __instance.Hide();

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.Hide))]
    private static void HidePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("Hide - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.HideNoCache))]
    private static bool HideNoCachePrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.HideNoCache))]
    private static void HideNoCachePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("HideNoCache - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.Show))]
    private static bool ShowPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.Show))]
    private static void ShowPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("Show - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    // 2250
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ShowFullTechTree))]
    private static bool ShowFullTechTreePrefix(
        ResearchScreenController __instance,
        string ___selectedProjectEntry,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();

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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ShowFullTechTree))]
    private static void ShowFullTechTreePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ShowFullTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    // 3449
    [HarmonyPrefix]
    [HarmonyPatch("ResetAllPanels")]
    private static bool ResetAllPanelsPrefix(
        ResearchScreenController __instance,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();

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

        // __instance.fullTechTreeObject.SetActive(false);
        // __instance.fullTechTreeObjectNP.SetActive(false);

        __instance.fullTechTreeCanvas.enabled = false;
        __instance.fullTechTreeCanvasNP.enabled = false;
        __instance.selectiveTechTreeCanvas.enabled = true;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ResetAllPanels")]
    private static void ResetAllPanelsPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ResetAllPanels - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.SetSelectedTechEntry))]
    private static bool SetSelectedTechEntryPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.SetSelectedTechEntry))]
    private static void SetSelectedTechEntryPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("SetSelectedTechEntry - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ChangeTechSelectionSort))]
    private static bool ChangeTechSelectionSortPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ChangeTechSelectionSort))]
    private static void ChangeTechSelectionSortPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ChangeTechSelectionSort - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.FillOutTechData))]
    private static bool FillOutTechDataPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.FillOutTechData))]
    private static void FillOutTechDataPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("FillOutTechData - elapsed: \t\t" + __state.ElapsedMilliseconds);
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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.CloseFullTechTree))]
    private static void CloseFullTechTreePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("CloseFullTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ShowNoProjectTechTree))]
    private static void ShowNoProjectTechTreePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ShowNoProjectTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.CloseNoProjectTechTree))]
    private static bool CloseNoProjectTechTreePrefix(
        ResearchScreenController __instance,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();

        __instance.fullTechTreeCanvas.enabled = false;
        __instance.fullTechTreeCanvasNP.enabled = false;

        __instance.rightButtonOverlayPanel.enabled = false;
        __instance.mainGridObject.SetActive(true);
        __instance.archivesButton.SetActive(true);

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.CloseNoProjectTechTree))]
    private static void CloseNoProjectTechTreePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("CloseNoProjectTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ResetAllConnectionColors))]
    private static bool ResetAllConnectionColorsPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ResetAllConnectionColors))]
    private static void ResetAllConnectionColorsPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ResetAllConnectionColors - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.SetupConnections))]
    private static bool SetupConnectionsPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.SetupConnections))]
    private static void SetupConnectionsPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("SetupConnections - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch("ToggleTechVisibility")]
    private static bool ToggleTechVisibilityPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ToggleTechVisibility")]
    private static void ToggleTechVisibilityPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ToggleTechVisibility - elapsed: \t\t" + __state.ElapsedMilliseconds);
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
    private static void InitializeNoProjectTechTreePostfix(Stopwatch __state)
    {
        isInit = true;
        UnityModManager.Logger.Log("InitializeNoProjectTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.InitializeSelectiveTechTree))]
    private static bool InitializeSelectiveTechTreePrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.InitializeSelectiveTechTree))]
    private static void InitializeSelectiveTechTreePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("InitializeSelectiveTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch("BuildTree")]
    private static bool BuildTreePrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("BuildTree")]
    private static void BuildTreePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("BuildTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.InitializeFullTechTree))]
    private static bool InitializeFullTechTreePrefix(
        bool selectiveTree,
        string selectiveTech,
        bool noProjectTree,
        bool selectiveFromFull,
        ResearchScreenController __instance,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        
        __instance.EnableFull(selectiveFromFull);

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.InitializeFullTechTree))]
    private static void InitializeFullTechTreePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("InitializeFullTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.OnTechinTreeClicked))]
    private static bool OnTechinTreeClickedPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.OnTechinTreeClicked))]
    private static void OnTechinTreeClickedPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("OnTechinTreeClicked - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSortedTechList))]
    private static bool UpdateSortedTechListPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSortedTechList))]
    private static void UpdateSortedTechListPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("UpdateSortedTechList - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSelectTechPanel))]
    private static bool UpdateSelectTechPanelPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSelectTechPanel))]
    private static void UpdateSelectTechPanelPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("UpdateSelectTechPanel - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.FillOutProjectData))]
    private static bool FillOutProjectDataPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.FillOutProjectData))]
    private static void FillOutProjectDataPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("FillOutProjectData - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSortedProjectList))]
    private static bool UpdateSortedProjectListPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSortedProjectList))]
    private static void UpdateSortedProjectListPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("UpdateSortedProjectList - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ChangeProjectSelectionSort))]
    private static bool ChangeProjectSelectionSortPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ChangeProjectSelectionSort))]
    private static void ChangeProjectSelectionSortPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ChangeProjectSelectionSort - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSelectProjectPanel))]
    private static bool UpdateSelectProjectPanelPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateSelectProjectPanel))]
    private static void UpdateSelectProjectPanelPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("UpdateSelectProjectPanel - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.FillOutArchiveData))]
    private static bool FillOutArchiveDataPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.FillOutArchiveData))]
    private static void FillOutArchiveDataPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("FillOutArchiveData - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateArchivesPanel))]
    private static bool UpdateArchivesPanelPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateArchivesPanel))]
    private static void UpdateArchivesPanelPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("UpdateArchivesPanel - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateResearchLists))]
    private static bool UpdateResearchListsPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.UpdateResearchLists))]
    private static void UpdateResearchListsPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("UpdateResearchLists - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ExitRightPanel))]
    private static bool ExitRightPanelPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ExitRightPanel))]
    private static void ExitRightPanelPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ExitRightPanel - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ExitResearchScreen))]
    private static bool ExitResearchScreenPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ExitResearchScreen))]
    private static void ExitResearchScreenPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ExitResearchScreen - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.OnSelectTechButtonSelected))]
    private static bool OnSelectTechButtonSelectedPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.OnSelectTechButtonSelected))]
    private static void OnSelectTechButtonSelectedPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("OnSelectTechButtonSelected - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.OnSelectProjectButtonSelected))]
    private static bool OnSelectProjectButtonSelectedPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.OnSelectProjectButtonSelected))]
    private static void OnSelectProjectButtonSelectedPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("OnSelectProjectButtonSelected - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.Initialize))]
    private static bool InitializePrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.Initialize))]
    private static void InitializePostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("Initialize - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }


    /// /////////////////////////////////////////
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ShowTech))]
    private static bool ShowTechPrefix(
        TIFactionState faction,
        TIGenericTechTemplate tech,
        GameObject panel,
        TMP_Text techName,
        TMP_Text techStatus,
        Image icon,
        ResearchScreenController __instance)
    {
        if (!_techCache.TryGetValue(tech.displayName, out var cachedItem))
            return true;

        techName.SetText(cachedItem.DisplayName);
        techStatus.SetText(cachedItem.TechStatus);
        cachedItem.Component.toolTipString = cachedItem.ToolTipString;
        cachedItem.Component.techTooltip.SetDelegate("BodyText", (() => cachedItem.ToolTipString));

        if (!ResearchScreenController.fullTechTreeOn)
            foreach (var imageComponent in panel.GetComponentsInChildren<Image>().Where(i => i != icon))
                imageComponent.color = cachedItem.Color;
        else
        {
            panel.SetActive(!cachedItem.Component.hidden && panel.activeSelf);
            panel.GetComponent<Image>().color = cachedItem.Color;
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ShowTech))]
    private static void ShowTechPostfix(
        TIFactionState faction,
        TIGenericTechTemplate tech,
        GameObject panel,
        TMP_Text techName,
        TMP_Text techStatus,
        Image icon,
        ResearchScreenController __instance)
    {
        if (_techCache.ContainsKey(tech.displayName))
            return;

        var comp = panel.GetComponent<ChildTechGridItemController>();

        var newCacheItem = new TechData()
        {
            Color = panel.GetComponent<Image>().color,
            Component = comp,
            ToolTipString = comp.toolTipString,
            DisplayName = tech.displayName,
            TechStatus = techStatus.text
        };

        _techCache.Add(tech.displayName, newCacheItem);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.DisplayTechTree))]
    private static bool DisplayTechTreePrefix(
        TIGenericTechTemplate genericTech,
        ResearchScreenController __instance,
        ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.DisplayTechTree))]
    private static void DisplayTechTreePostfix(
        TIGenericTechTemplate genericTech,
        ResearchScreenController __instance,
        Stopwatch __state)
    {
        UnityModManager.Logger.Log("DisplayTechTree - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ResearchScreenController.ExitArchivePanel))]
    private static bool ExitArchivePanelPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResearchScreenController.ExitArchivePanel))]
    private static void ExitArchivePanelPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ExitArchivePanel - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }


    [HarmonyPrefix]
    [HarmonyPatch("ToggleTechLineVisibility")]
    private static bool ToggleTechLineVisibilityPrefix(ref Stopwatch __state)
    {
        __state = new Stopwatch();
        __state.Start();
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ToggleTechLineVisibility")]
    private static void ToggleTechLineVisibilityPostfix(Stopwatch __state)
    {
        UnityModManager.Logger.Log("ToggleTechLineVisibility - elapsed: \t\t" + __state.ElapsedMilliseconds);
    }
}