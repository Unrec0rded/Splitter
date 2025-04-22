using HarmonyLib;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Persistence.Datas;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Items;
using MelonLoader;
using UnityEngine;

namespace Splitter
{
    public class LeftClickPatch : MelonMod
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

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {         
                if (Input.GetMouseButtonDown(0))
                {       
                    if (leftClickCachedSlot != null && itemData != null)
                    {
                        int newAmount = (itemData.Quantity == 1) 
                            ? 1 
                            : (int)Mathf.Ceil(itemData.Quantity / 2f);
                            
                        __instance.SetDraggedAmount(newAmount);
                        LogScrollAction(itemData.Quantity, newAmount, itemData.Quantity);
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
        // Right click handling for wheel control
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

        // Wheel logic
        if ((!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) 
            || !wheelRightClickHeld)
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        int currentAmount = __instance.draggedAmount;
        if (currentAmount <= 0) return;

        // Initialize maxSplit with a default value
        int maxSplit = 999; // Default maximum split value

        // Check item quantity and adjust maxSplit accordingly
        ItemInstance itemInstance = wheelCachedSlot?.ItemInstance;
        if (itemInstance?.GetItemData() is ItemData itemData)
        {
            if (itemData.Quantity <= 1)
            {
                __instance.SetDraggedAmount(1);
                return;
            }
            maxSplit = Mathf.Max(itemData.Quantity - 1, 1); // Adjust maxSplit based on item quantity
        }

        int direction = scroll > 0 ? 1 : -1;
        int step = 5;

        int newAmount = CalculateNewAmount(currentAmount, direction, step, maxSplit);
        
        __instance.SetDraggedAmount(newAmount);
        LogScrollAction(currentAmount, newAmount, maxSplit);
    }
        private static int CalculateNewAmount(int current, int direction, int step, int max)
        {
            if (direction > 0)
            {
                // Saat menambah (scroll up)
                if (current == 1)
                    return Mathf.Min(step, max);
                    
                int nextStep = ((current - 1) / step + 1) * step;
                return Mathf.Min(nextStep, max);
            }
            else
            {
                // Saat mengurangi (scroll down)
                if (current <= step)
                    return 1;
                    
                int prevStep = ((current - 1) / step) * step;
                return Mathf.Max(prevStep, 1);
            }
        }

        private static void LogScrollAction(int oldAmount, int newAmount, int maxSplit)
        {
            Logger.Msg($"Cur Item | {oldAmount} → {newAmount} (max: {maxSplit})");
        }

        public LeftClickPatch()
        {
            Logger = new MelonLogger.Instance("Splitter");
        }

        // Left click split variables
        private static ItemSlot leftClickCachedSlot;
        
        // Wheel split variables
        private static ItemSlot wheelCachedSlot;
        private static bool wheelRightClickHeld;
        
        private static MelonLogger.Instance Logger;
    }
}