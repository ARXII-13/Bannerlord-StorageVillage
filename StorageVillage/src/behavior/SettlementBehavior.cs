using System;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Localization;

using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;

using StorageVillage.src.util;

namespace StorageVillage.src.behavior
{
    public class SettlementBehavior : CampaignBehaviorBase
    {
        private string[] SETTLEMENT_OBJECT_TO_SUPPORT = new string[] { "town", "castle" };

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
                menuText: "{=STORAGE_MENU}Storage",
                initDelegate: new OnInitDelegate(StorageMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.MAIN_MENU_ID,
                optionId: Constants.INVENTORY_MENU_ID,
                optionText: "{=INVENTORY_MENU}Inventory",
                condition: delegate (MenuCallbackArgs args)
                {
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

            foreach (string menuId in SETTLEMENT_OBJECT_TO_SUPPORT)
            {
                campaignGameStarter.AddGameMenuOption(
                    menuId: menuId,
                    optionId: Constants.MAIN_MENU_ID,
                    optionText: "{=STORAGE_MENU}Storage",
                    condition: MenuConditionForStorageMenu,
                    consequence: MenuConsequenceForStorage
                );
            }
        }

        public static void StorageMenuInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=STORAGE_MENU}Storage");
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
            if (roster == null)
            {
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

        private void MenuConsequenceForDonateFood(MenuCallbackArgs args)
        {
            // InventoryManager.OpenScreenAsStash(newRoster);
            // InventoryManager.OpenScreenAsTrade(newRoster, Settlement.CurrentSettlement.SettlementComponent, InventoryManager.InventoryCategoryType.Goods);
            OpenScreenAsDonateFood();


            // Debug.Print("MenuConsequenceForDonateFood newRoster");
            // Debug.Print(newRoster.ToString());
            // Debug.WriteDebugLineOnScreen(newRoster.ToString());
        }

        private void OpenScreenAsDonateFood()
        {
            InventoryManager inventoryManager = Campaign.Current.InventoryManager;
            ItemRoster newRoster = new ItemRoster();
            InventoryLogic inventoryLogic = new InventoryLogic(null);
            inventoryLogic.Initialize(
                newRoster,
                MobileParty.MainParty,
                isTrading: false,
                isSpecialActionsPermitted: false,
                CharacterObject.PlayerCharacter,
                InventoryManager.InventoryCategoryType.Goods,
                Settlement.CurrentSettlement.Town.MarketData,
                useBasePrices: false,
                new TextObject("{=DONATE_FOOD_TO_SETTLEMENT}Donate Food to Settlement")
            );

            FieldInfo inventroyLogicfield = typeof(InventoryManager).GetField("_inventoryLogic",
                BindingFlags.Instance | BindingFlags.NonPublic);

            inventroyLogicfield.SetValue(inventoryManager, inventoryLogic);

            FieldInfo currentModeField = typeof(InventoryManager).GetField("_currentMode",
                BindingFlags.Instance | BindingFlags.NonPublic);
            currentModeField.SetValue(inventoryManager, InventoryMode.Stash);

            inventoryLogic.SetDonateFoodFlag(true);
            // FieldInfo isDonateFoodToSettlementField = AccessTools.Field(typeof(InventoryLogic), "_isDonateFoodToSettlement");
            // if (inventoryLogic == null)
            // {
            //     System.Diagnostics.Debug.WriteLine("InventoryLogic instance is null!");
            //     return;
            // }
            // System.Diagnostics.Debug.WriteLine(inventoryLogic.ToString());
            // System.Diagnostics.Debug.WriteLine(isDonateFoodToSettlementField.ToString());

            // isDonateFoodToSettlementField.SetValue(inventoryLogic, true);

            // Then proceed to push the state
            InventoryState inventoryState = Game.Current.GameStateManager.CreateState<InventoryState>();
            inventoryState.InitializeLogic(inventoryLogic);

            Game.Current.GameStateManager.PushState(inventoryState);
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args)
        {
            string menu = Settlement.CurrentSettlement.IsVillage ? "village" : (Settlement.CurrentSettlement.IsCastle ? "castle" : "town");
            GameMenu.SwitchToMenu(menu);
        }

    }
}
