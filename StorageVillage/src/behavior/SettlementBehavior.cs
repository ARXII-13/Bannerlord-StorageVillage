using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Localization;
using System;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using StorageVillage.src.util;

namespace StorageVillage.src.behavior
{
    public class SettlementBehavior : CampaignBehaviorBase
    {
        private ItemRoster roster;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("inventoryData", ref roster);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddMenus(campaignGameStarter);
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.MAIN_MENU_ID, 
                menuText: "Storage", 
                initDelegate: new OnInitDelegate(StorageMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: "storage_village_menu_inventory",
                optionText: "{=!}Inventory",
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
                 optionText: "{=!}Troops Management",
                 condition: MenuConditionForSubMenu,
                 consequence: MenuConsequenceForTroop,
                 isLeave: false,
                 index: 2
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: Constants.BANK_MENU_ID,
                optionText: "{=!}National Bank",
                condition: MenuConditionForSubMenu,
                consequence: MenuConsequenceForBank,
                isLeave: false,
                index: 3
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: Constants.BANDIT_MENU_ID,
                optionText: "{=!}Bandit Management",
                condition: MenuConditionForSubMenu,
                consequence: MenuConsequenceForBandit,
                isLeave: false,
                index: 4
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: "storage_village_menu_leave",
                optionText: "{=!}Back to town center",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForLeave,
                isLeave: false,
                index: -1
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: "town",
                optionId: Constants.MAIN_MENU_ID,
                optionText: "{=!}Storage Village",
                condition: MenuConditionForStorageMenu,
                consequence: MenuConsequenceForStorage,
                isLeave: false,
                index: -2
            );
        }

        public static void StorageMenuInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=!}Storage");
        }

        private bool MenuConditionForStorageMenu(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            return true;
        }

        private bool MenuConditionForLeave(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private bool MenuConditionForSubMenu(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            return true;
        }

        private void MenuConsequenceForStorage(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

        private void MenuConsequenceForInventory(MenuCallbackArgs args)
        {
            if (roster == null) {
                roster = new ItemRoster();
            }

            InventoryManager.OpenScreenAsStash(roster);
        }
        private void MenuConsequenceForTroop(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.TROOP_MENU_ID);
        }

        private void MenuConsequenceForBank(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.BANK_MENU_ID);
        }

        private void MenuConsequenceForBandit(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.BANDIT_MENU_ID);
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args)
        {
            string menu = Settlement.CurrentSettlement.IsVillage ? "village" : (Settlement.CurrentSettlement.IsCastle ? "castle" : "town");
            GameMenu.SwitchToMenu(menu);
        }

    }
}
