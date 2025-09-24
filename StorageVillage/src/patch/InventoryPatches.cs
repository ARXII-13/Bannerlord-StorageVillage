using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using StorageVillage.src.util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace StorageVillage.src.patch {
    [HarmonyPatch]
    public static class InventoryPatches {
        [HarmonyPatch(typeof(InventoryLogic), "DoneLogic")]
        [HarmonyPrefix]
        public static void DoneLogic(InventoryLogic __instance) {
            bool donateFlag = __instance.SetDonateFoodFlag();
            if (!donateFlag) {
                return;
            }

            var transactionHistoryField = AccessTools.Field(__instance.GetType(), "_transactionHistory");
            if (transactionHistoryField == null) {
                return;
            }

            var transactionHistory = transactionHistoryField.GetValue(__instance);
            if (transactionHistory == null) {
                return;
            }

            var getSoldItemsMethod = transactionHistory.GetType().GetMethod("GetSoldItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (getSoldItemsMethod == null) {
                return;
            }

            System.Diagnostics.Debug.WriteLine("test harmony Postfix DoneLogic");
            var soldItemsObj = getSoldItemsMethod.Invoke(transactionHistory, null);
            var soldItems = (List<(ItemRosterElement, int)>)soldItemsObj;
            (float foodIncrease, float securityIncrease, float relationIncrease) = getDonationResults(soldItems);
            applyDonationResults(foodIncrease, securityIncrease, relationIncrease);

            __instance.ClearFlagsData();
        }

        private static (float foodIncrease, float securityIncrease, float relationIncrease) getDonationResults(List<(ItemRosterElement, int)> soldItems) {
            float foodIncrease = 0;
            float moraleBonus = 0;
            float relationIncrease = 0;
            for (int i = 0; i < soldItems.Count; i++) {
                var (itemElement, count) = soldItems[i];
                ItemObject item = itemElement.EquipmentElement.Item;
                if (!item.IsFood) {
                    continue;
                }
                int amount = itemElement.Amount;
                foodIncrease += (int)(amount * Constants.FOOD_DONATE_STOCK_FACTOR);
                moraleBonus += (int)(amount * item.FoodComponent.MoraleBonus * Constants.FOOD_DONATE_MORALE_FACTOR);
                relationIncrease += (int)(amount * item.Value * Constants.FOOD_DONATE_RELATION_FACTOR);
            }

            return (foodIncrease, moraleBonus, relationIncrease);
        }

        private static void applyDonationResults(float foodIncrease, float securityIncrease, float relationIncrease) {
            Settlement currentSettlement = Settlement.CurrentSettlement;
            if (currentSettlement == null) {
                return;
            }

            int foodStocksUpperLimit = currentSettlement.Town.FoodStocksUpperLimit();
            float currentFood = currentSettlement.Town.FoodStocks;
            if (foodIncrease + currentFood > foodStocksUpperLimit) {
                currentSettlement.Town.FoodStocks = foodStocksUpperLimit;
            }
            else {
                currentSettlement.Town.FoodStocks += foodIncrease;
            }
            TextObject foodIncreaseText = new TextObject(
                "{=DONATION_FOOD_INCREASE}Food stock in {SETTLEMENT_NAME} increased by {INCREASE_AMOUNT} to {NEW_AMOUNT}."
            );
            foodIncreaseText.SetTextVariable("SETTLEMENT_NAME", currentSettlement.Name);
            foodIncreaseText.SetTextVariable("INCREASE_AMOUNT", foodIncrease);
            foodIncreaseText.SetTextVariable("NEW_AMOUNT", currentSettlement.Town.FoodStocks);
            InformationManager.DisplayMessage(new InformationMessage(foodIncreaseText.ToString()));

            int securityUpperLimit = 100;
            float currentSecurity = currentSettlement.Town.Security;
            if (securityIncrease + currentSecurity > securityUpperLimit) {
                currentSettlement.Town.Security = securityUpperLimit;
            }
            else {
                currentSettlement.Town.Security += securityIncrease;
            }
            TextObject securityIncreaseText = new TextObject(
                "{=DONATION_SECURITY_INCREASE}Security in {SETTLEMENT_NAME} increased by {INCREASE_AMOUNT} to {NEW_AMOUNT}."
            );
            securityIncreaseText.SetTextVariable("SETTLEMENT_NAME", currentSettlement.Name);
            securityIncreaseText.SetTextVariable("INCREASE_AMOUNT", securityIncrease);
            securityIncreaseText.SetTextVariable("NEW_AMOUNT", currentSettlement.Town.Security);
            InformationManager.DisplayMessage(new InformationMessage(securityIncreaseText.ToString()));

            Hero playerHero = Hero.MainHero;
            Hero owner = currentSettlement.Owner;
            if (owner != null && owner.IsAlive) {
                if ((int)relationIncrease > 0) {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(playerHero, owner, (int)relationIncrease);
                }
            }
            MBReadOnlyList<Hero> notables = currentSettlement.Notables;
            for (int i = 0; i < notables.Count; i++) {
                Hero notable = notables[i];
                if (notable.IsAlive) {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(playerHero, notable, (int)relationIncrease);
                }
            }

        }
    }
}