using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Splitter.Core), "Splitter", "1.5.0", "Sinner2001", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Splitter
{
    public class Core : MelonMod
    {
        public static int SplitStep = 5;
        public static KeyCode SplitKey = KeyCode.LeftControl;
        public static bool RoundUp = true;
        // public static bool EnableDebugLogs = true;
        public static bool ConsumeAllIfBelowStep = true;
        public static bool OneByOne = true;

        public override void OnInitializeMelon()
        {
            MelonPreferences.CreateCategory("Splitter", "Splitter Settings");
            
            MelonPreferences.CreateEntry("Splitter", "SplitStep", 5, 
                description: "Scroll step size for splitting (default: 5)");
            
            MelonPreferences.CreateEntry("Splitter", "SplitKey", "LeftControl",
                description: "Modifier key for splitting (e.g., LeftControl, LeftAlt, LeftShift) (default: LeftControl)");
            
            MelonPreferences.CreateEntry("Splitter", "RoundUp", true,
                description: "Round up (true) or down (false) when splitting odd numbers (default: true)");
            
            // MelonPreferences.CreateEntry("Splitter", "EnableDebugLogs", true,
            //     description: "Enable detailed debug logging");
                
            MelonPreferences.CreateEntry("Splitter", "ConsumeAllIfBelowStep", true,
                description: "Take all items if remaining quantity is below split step size. If false, the maximum stuck items are multiples of 5.(default: true)");

            MelonPreferences.CreateEntry("Splitter", "OneByOne", true,
                description: "If ConsumeAllIfBelowStep And OneByOne is true, take all items one by one. if ConsumeAllIfBelowStep true and OneByOne false, take all items at once (default: true)");

            SplitStep = MelonPreferences.GetEntry<int>("Splitter", "SplitStep").Value;
            RoundUp = MelonPreferences.GetEntry<bool>("Splitter", "RoundUp").Value;
            // EnableDebugLogs = MelonPreferences.GetEntry<bool>("Splitter", "EnableDebugLogs").Value;
            
            if (!System.Enum.TryParse(MelonPreferences.GetEntry<string>("Splitter", "SplitKey").Value, out SplitKey))
            {
                SplitKey = KeyCode.LeftControl;
                MelonPreferences.GetEntry<string>("Splitter", "SplitKey").Value = "LeftControl";
            }
            ConsumeAllIfBelowStep = MelonPreferences.GetEntry<bool>("Splitter", "ConsumeAllIfBelowStep").Value;
            OneByOne = MelonPreferences.GetEntry<bool>("Splitter", "OneByOne").Value;

            LoggerInstance.Msg("Splitter Initialized...");
            HarmonyInstance.PatchAll(typeof(StackSplitPatch));
            LoggerInstance.Msg("Splitter patched...");
        }
    }
}