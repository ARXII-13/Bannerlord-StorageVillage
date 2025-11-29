using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using StorageVillage.src.setting;
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

            var soldItemsObj = getSoldItemsMethod.Invoke(transactionHistory, null);
            var soldItems = (List<(ItemRosterElement, int)>)soldItemsObj;
            (int foodIncrease, int securityIncrease, int relationIncrease, int influenceIncrease) = getDonationResults(soldItems);
            applyDonationResults(foodIncrease, securityIncrease, relationIncrease, influenceIncrease);

            __instance.ClearFlagsData();
        }

        private static (int foodIncrease, int securityIncrease, int relationIncrease, int renownIncrease) getDonationResults(List<(ItemRosterElement, int)> soldItems) {
            float foodIncrease = 0;
            float securityIncrease = 0;
            float relationIncrease = 0;
            float influenceIncrease = 0;
            for (int i = 0; i < soldItems.Count; i++) {
                var (itemElement, _) = soldItems[i];
                ItemObject item = itemElement.EquipmentElement.Item;
                if (!item.IsFood) {
                    continue;
                }
                int amount = itemElement.Amount;
                foodIncrease += amount * Constants.FOOD_DONATE_STOCK_FACTOR;
                securityIncrease += amount * item.FoodComponent.MoraleBonus * Constants.FOOD_DONATE_MORALE_FACTOR;
                relationIncrease += amount * item.Value * Constants.FOOD_DONATE_RELATION_FACTOR;
                influenceIncrease += amount * item.Value * Constants.FOOD_DONATE_INFLUENCE_FACTOR;
            }

            return ((int)foodIncrease, (int)securityIncrease, (int)relationIncrease, (int)influenceIncrease);
        }

        private static void applyDonationResults(int foodIncrease, int securityIncrease, int relationIncrease, int influenceIncrease) {
            Settlement currentSettlement = Settlement.CurrentSettlement;
            if (currentSettlement == null) {
                return;
            }
            StorageVillageSettings settings = StorageVillageSettings.Instance;

            if (settings.foodDonationFoodIncrease) {
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
                foodIncreaseText.SetTextVariable("NEW_AMOUNT", (int)currentSettlement.Town.FoodStocks);
                InformationManager.DisplayMessage(new InformationMessage(foodIncreaseText.ToString()));
            }

            if (settings.foodDonationSecurityIncrease) {
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
                securityIncreaseText.SetTextVariable("NEW_AMOUNT", (int)currentSettlement.Town.Security);
                InformationManager.DisplayMessage(new InformationMessage(securityIncreaseText.ToString()));
            }

            Hero playerHero = Hero.MainHero;
            Hero owner = currentSettlement.Owner;
            if (owner != null && owner.IsAlive) {
                if (settings.foodDonationInfluenceIncrease) {
                    if (playerHero.Clan?.Kingdom?.Name == owner.Clan?.Kingdom?.Name) {
                        GainKingdomInfluenceAction.ApplyForGivingFood(playerHero, owner, influenceIncrease);
                        TextObject influenceIncreaseText = new TextObject(
                            "{=DONATION_INFLUENCE_INCREASE}Influence increased by {INCREASE_AMOUNT} to {NEW_AMOUNT}."
                        );
                        influenceIncreaseText.SetTextVariable("INCREASE_AMOUNT", influenceIncrease);
                        influenceIncreaseText.SetTextVariable("NEW_AMOUNT", (int)playerHero.Clan.Influence);
                        InformationManager.DisplayMessage(new InformationMessage(influenceIncreaseText.ToString()));
                    }
                }

                if (settings.foodDonationRelationIncrease && relationIncrease > 0) {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(playerHero, owner, relationIncrease);
                }
            }
            if (settings.foodDonationRelationIncrease && relationIncrease > 0) {
                MBReadOnlyList<Hero> notables = currentSettlement.Notables;
                for (int i = 0; i < notables.Count; i++) {
                    Hero notable = notables[i];
                    if (notable.IsAlive) {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(playerHero, notable, relationIncrease);
                    }
                }
            }
        }
    }
}