using System.Runtime.CompilerServices;
using HarmonyLib;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Persistence.Datas;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Items;
using MelonLoader;
using UnityEngine;
using static Splitter.Core;

namespace Splitter
{
    public class StackSplitPatch : MelonMod
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemUIManager), "Update")]
        private static void PostfixUpdate(ItemUIManager __instance)
        {
            HandleLeftClickSplit(__instance);
            HandleWheelSplit(__instance);
        }

        private static void HandleLeftClickSplit(ItemUIManager __instance)
        {
            if (__instance == null) return;

            bool flag = wheelCachedSlot?.ItemInstance?.GetItemData().ID.ToLower() == "cash";
            if (Input.GetMouseButtonDown(0) && Input.GetKey(SplitKey) && !flag)
            {
                leftClickCachedSlot = __instance.HoveredSlot?.assignedSlot;
                if (leftClickCachedSlot == null) return;

                ItemInstance itemInstance = leftClickCachedSlot.ItemInstance;
                if (itemInstance == null || itemInstance is CashInstance) return;

                ItemData itemData = itemInstance.GetItemData();
                if (itemData == null) return;

                int newAmount = (itemData.Quantity == 1) 
                    ? 1 
                    : RoundUp 
                        ? Mathf.CeilToInt(itemData.Quantity / 2f) 
                        : Mathf.FloorToInt(itemData.Quantity / 2f);

                __instance.SetDraggedAmount(newAmount);
            }
            else if (Input.GetMouseButtonUp(0) || flag)
            {                
                leftClickCachedSlot = null;
            }
        }

        private static void HandleWheelSplit(ItemUIManager __instance)
        {
            if (__instance == null) return;

            bool flag = wheelCachedSlot?.ItemInstance?.GetItemData().ID.ToLower() == "cash";
            if (Input.GetMouseButtonDown(1) && Input.GetKey(SplitKey) && !flag)
            {
                wheelCachedSlot = __instance.HoveredSlot?.assignedSlot;
                wheelRightClickHeld = true;
            }
            else if (Input.GetMouseButtonUp(1) || flag)
            {
                wheelRightClickHeld = false;
                wheelCachedSlot = null;
            }
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            int currentAmount = __instance.draggedAmount;
            if (currentAmount <= 0) return;

            // Validasi wheelCachedSlot dan ItemInstance
            if (wheelCachedSlot == null || wheelCachedSlot.ItemInstance == null)
            {
                return;
            }

            ItemInstance itemInstance = wheelCachedSlot.ItemInstance;
            if (itemInstance is CashInstance) return;

            ItemData itemData = itemInstance.GetItemData();
            if (itemData == null)
            {
                return;
            }

            int maxSplit = 999;
            if (itemData.Quantity >= 1)
            {
                maxSplit = Mathf.Max(itemData.Quantity - 1, 1);
            }

            if (itemData.Quantity == 1) return;

            int direction = scroll > 0 ? 1 : -1;
            int step = SplitStep;

            int newAmount = CalculateNewAmount(currentAmount, direction, step, maxSplit);

            if (!ConsumeAllIfBelowStep && newAmount % step != 0 && direction > 0)
            {
                __instance.SetDraggedAmount(currentAmount - 1);
                return;
            }
            else if (ConsumeAllIfBelowStep && !OneByOne && newAmount % step != 0 && direction > 0)
            {
                __instance.SetDraggedAmount(Mathf.Min(currentAmount + maxSplit, itemData.Quantity));
                return;
            }
            else if (ConsumeAllIfBelowStep && newAmount % step != 0 && direction > 0)
            {
                return;
            }
            __instance.SetDraggedAmount(newAmount);
        }
        private static int CalculateNewAmount(int current, int direction, int step, int max)
        {
            if (direction > 0)
            {
                if (current == 1)
                    return Mathf.Min(step, max);
                    
                int nextStep = ((current - 1) / step + 1) * step;
                return Mathf.Min(nextStep, max);
            }
            else
            {
                if (current <= step)
                    return 1;
                    
                int prevStep = ((current - 1) / step) * step;
                return Mathf.Max(prevStep, 1);
            }
        }

        // private static void LogScrollAction(int oldAmount, int newAmount, int maxSplit)
        // {
        //     Logger.Msg($"Cur Item | {oldAmount} → {newAmount} (max: {maxSplit})");
        // }

        public StackSplitPatch()
        {
            // Logger = new MelonLogger.Instance("Splitter");
        }

        private static ItemSlot leftClickCachedSlot;
        
        private static ItemSlot wheelCachedSlot;
        private static bool wheelRightClickHeld;
        
        // private static MelonLogger.Instance Logger;
    }
}