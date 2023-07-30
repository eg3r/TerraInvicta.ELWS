using ELWS.Core.Patches;

namespace ELWS.Core;

public static class ModState
{
    public static void Reset()
    {
        ResearchScreenControllerPatch.ClearCache();
    }

    public static void Load()
    {
    }
}