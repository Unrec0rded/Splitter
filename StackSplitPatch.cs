using HarmonyLib;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Persistence.Datas;
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
            leftClickCachedSlot = __instance.HoveredSlot?.assignedSlot;
            ItemInstance itemInstance = leftClickCachedSlot?.ItemInstance;
            ItemData itemData = itemInstance?.GetItemData();

            if (Input.GetKey(SplitKey))
            {
                if (Input.GetMouseButtonDown(0))
                {       
                    if (leftClickCachedSlot != null && itemData != null)
                    {
                        int newAmount = (itemData.Quantity == 1) ? 1 
                        : RoundUp 
                            ? (int)Mathf.Ceil(itemData.Quantity / 2f)
                            : (int)Mathf.Floor(itemData.Quantity / 2f);
                            
                        __instance.SetDraggedAmount(newAmount);
                        // LogScrollAction(itemData.Quantity, newAmount, itemData.Quantity);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    leftClickCachedSlot = null;
                }
            }
        }

    private static void HandleWheelSplit(ItemUIManager __instance)
    {
        if (Input.GetMouseButtonDown(1))
        {
            wheelCachedSlot = __instance.HoveredSlot?.assignedSlot;
            wheelRightClickHeld = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            wheelRightClickHeld = false;
            wheelCachedSlot = null;
        }

        if ((!Input.GetKey(SplitKey)) 
            || !wheelRightClickHeld)
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        int currentAmount = __instance.draggedAmount;
        if (currentAmount <= 0) return;

        int maxSplit = 999;
        int direction = scroll > 0 ? 1 : -1;
        int step = SplitStep;

        ItemInstance itemInstance = wheelCachedSlot?.ItemInstance;
        if (itemInstance?.GetItemData() is ItemData itemData)
        {
            if (itemData.Quantity <= 1)
            {
                __instance.SetDraggedAmount(1);
                return;
            }
            maxSplit = Mathf.Max(itemData.Quantity - 1, 1);            
        }
        
        int newAmount = CalculateNewAmount(currentAmount, direction, step, maxSplit);

        if (!ConsumeAllIfBelowStep && newAmount % step != 0 && direction > 0)
        {
            newAmount =  currentAmount - 1;
            __instance.SetDraggedAmount(newAmount);
            return;
        } 
        else if (ConsumeAllIfBelowStep && !OneByOne && newAmount % step != 0 && direction > 0)
        {
            newAmount = Mathf.Min(currentAmount + maxSplit, wheelCachedSlot?.ItemInstance?.GetItemData().Quantity ?? 0);
            __instance.SetDraggedAmount(newAmount);
            return;
        }
        else if (ConsumeAllIfBelowStep && newAmount % step != 0 && direction > 0)
        {
            return;
        }
        __instance.SetDraggedAmount(newAmount);
        
        
        // if (EnableDebugLogs)
        //     LogScrollAction(currentAmount, newAmount, maxSplit);
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
            Logger = new MelonLogger.Instance("Splitter");
        }

        private static ItemSlot leftClickCachedSlot;
        
        private static ItemSlot wheelCachedSlot;
        private static bool wheelRightClickHeld;
        
        private static MelonLogger.Instance Logger;
    }
}