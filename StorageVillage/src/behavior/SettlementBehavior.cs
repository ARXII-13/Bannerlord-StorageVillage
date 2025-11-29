using System;
using System.Collections.Generic;
using Helpers;
using StorageVillage.src.setting;
using StorageVillage.src.util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace StorageVillage.src.behavior {
    public class SettlementBehavior : CampaignBehaviorBase {
        private string[] SETTLEMENT_OBJECT_TO_SUPPORT = new string[] { "town", "castle" };

        private ItemRoster roster;

        private Dictionary<string, ItemRoster> rosterMap;

        public override void RegisterEvents() {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore) {
            dataStore.SyncData("inventoryData", ref roster);
            dataStore.SyncData("inventoryTownData", ref rosterMap);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter) {
            AddMenus(campaignGameStarter);
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter) {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.MAIN_MENU_ID,
                menuText: "{=STORAGE_MENU}Storage",
                initDelegate: new OnInitDelegate(StorageMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: Constants.INVENTORY_MENU_ID,
                optionText: "{=INVENTORY_MENU}Inventory",
                condition: delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                    return true;
                },
                consequence: MenuConsequenceForInventory,
                isLeave: false,
                index: 1
            );

            campaignGameStarter.AddGameMenuOption(
                 menuId: Constants.MAIN_MENU_ID,
                 optionId: Constants.TROOP_MENU_ID,
                 optionText: "{=TROOP_MENU}Troops Management",
                 condition: MenuConditionForSubMenu,
                 consequence: MenuConsequenceForTroop,
                 isLeave: false,
                 index: 2
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: Constants.BANK_MENU_ID,
                optionText: "{=BANK_MENU}Calradia Central Bank",
                condition: MenuConditionForSubMenu,
                consequence: MenuConsequenceForBank,
                isLeave: false,
                index: 3
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: Constants.BANDIT_MENU_ID,
                optionText: "{=BANDIT_MENU}Bandit Management",
                condition: MenuConditionForSubMenu,
                consequence: MenuConsequenceForBandit,
                isLeave: false,
                index: 4
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: Constants.DONATE_FOOD_MENU_ID,
                optionText: "{=DONATE_FOOD_MENU}Donate Food to Settlement",
                condition: MenuConditionForSubMenu,
                consequence: MenuConsequenceForDonateFood,
                isLeave: false,
                index: 5
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: "storage_village_menu_leave",
                optionText: "{=BACK_TO_TOWN_CENTER}Back to town center",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForLeave,
                isLeave: false,
                index: -1
             );

            foreach (string menuId in SETTLEMENT_OBJECT_TO_SUPPORT) {
                campaignGameStarter.AddGameMenuOption(
                    menuId: menuId,
                    optionId: Constants.MAIN_MENU_ID,
                    optionText: "{=STORAGE_MENU}Storage",
                    condition: MenuConditionForStorageMenu,
                    consequence: MenuConsequenceForStorage
                );
            }
        }

        public static void StorageMenuInit(MenuCallbackArgs args) {
            args.MenuTitle = new TextObject("{=STORAGE_MENU}Storage");
        }

        private bool MenuConditionForStorageMenu(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            return true;
        }

        private bool MenuConditionForLeave(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private bool MenuConditionForSubMenu(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            return true;
        }

        private void MenuConsequenceForStorage(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

        private void MenuConsequenceForInventory(MenuCallbackArgs args) {
            if (roster == null) {
                roster = new ItemRoster();
            }

            if (rosterMap == null) {
                rosterMap = new Dictionary<string, ItemRoster>();
            }

            StorageVillageSettings settings = StorageVillageSettings.Instance;

            ItemRoster itemRoster;
            string townStorageSetting = settings.townStorageSetting.SelectedValue;
            if (townStorageSetting.Equals(Constants.STORAGE_SETTING_TYPE_TOWN)) {
                string settlementId = Settlement.CurrentSettlement.StringId;
                if (!rosterMap.ContainsKey(settlementId)) {
                    rosterMap[settlementId] = new ItemRoster();
                }
                itemRoster = rosterMap[settlementId];
            }
            else {
                itemRoster = roster;
            }

            InventoryScreenHelper.OpenScreenAsStash(itemRoster);
        }
        private void MenuConsequenceForTroop(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.TROOP_MENU_ID);
        }

        private void MenuConsequenceForBank(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.BANK_MENU_ID);
        }

        private void MenuConsequenceForBandit(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.BANDIT_MENU_ID);
        }

        private void MenuConsequenceForDonateFood(MenuCallbackArgs args) {
            OpenScreenAsDonateFood();
        }

        private void OpenScreenAsDonateFood() {
            ItemRoster newRoster = new ItemRoster();
            InventoryLogic inventoryLogic = new InventoryLogic(null);
            inventoryLogic.Initialize(
                newRoster,
                MobileParty.MainParty,
                isTrading: false,
                isSpecialActionsPermitted: false,
                CharacterObject.PlayerCharacter,
                InventoryScreenHelper.InventoryCategoryType.Goods,
                Settlement.CurrentSettlement.Town.MarketData,
                useBasePrices: false,
                InventoryScreenHelper.InventoryMode.Stash,
                new TextObject("{=DONATE_FOOD_TO_SETTLEMENT}Donate Food to Settlement")
            );

            InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
            inventoryState.InventoryMode = InventoryScreenHelper.InventoryMode.Stash;

            inventoryLogic.SetDonateFoodFlag(true);
            inventoryState.InventoryLogic = inventoryLogic;

            Game.Current.GameStateManager.PushState(inventoryState);
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args) {
            string menu = Settlement.CurrentSettlement.IsVillage ? "village" : (Settlement.CurrentSettlement.IsCastle ? "castle" : "town");
            GameMenu.SwitchToMenu(menu);
        }

    }
}
