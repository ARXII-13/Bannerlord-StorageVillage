using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace StorageVillage.src.patch
{
    [HarmonyPatch]
    public static class InventoryPatches
    {
        [HarmonyPatch(typeof(InventoryLogic), "DoneLogic")]
        [HarmonyPrefix]
        public static void DoneLogic(InventoryLogic __instance)
        {
            bool donateFlag = __instance.SetDonateFoodFlag();
            if (!donateFlag)
            {
                return;
            }

            var transactionHistoryField = AccessTools.Field(__instance.GetType(), "_transactionHistory");
            if (transactionHistoryField == null)
            {
                return;
            }

            var transactionHistory = transactionHistoryField.GetValue(__instance);
            if (transactionHistory == null)
            {
                return;
            }

            var getSoldItemsMethod = transactionHistory.GetType().GetMethod("GetSoldItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (getSoldItemsMethod == null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine("test harmony Postfix DoneLogic");
            var soldItemsObj = getSoldItemsMethod.Invoke(transactionHistory, null);
            var soldItems = (List<(ItemRosterElement, int)>)soldItemsObj;
            for (int i = 0; i < soldItems.Count; i++)
            {
                var (item, count) = soldItems[i];
                System.Diagnostics.Debug.WriteLine($"Sold {count} of {item.ToString()}");
            }

            System.Diagnostics.Debug.WriteLine(soldItems);
            Settlement currentSettlement = Settlement.CurrentSettlement;
            if (currentSettlement == null)
            {
                return;
            }

            int foodIncrease = 50;
            int foodStocksUpperLimit = currentSettlement.Town.FoodStocksUpperLimit();
            float currentFood = currentSettlement.Town.FoodStocks;
            if (foodIncrease + currentFood > foodStocksUpperLimit)
            {
                currentSettlement.Town.FoodStocks = foodStocksUpperLimit;
            }
            else
            {
                currentSettlement.Town.FoodStocks += foodIncrease;
            }

            __instance.ClearFlagsData();
        }
    }
}