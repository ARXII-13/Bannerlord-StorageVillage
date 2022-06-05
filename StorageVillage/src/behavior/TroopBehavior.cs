using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Localization;
using System;
using TaleWorlds.CampaignSystem.Party;
using StorageVillage.src.util;

namespace StorageVillage.src.behavior
{
    public class TroopBehavior : CampaignBehaviorBase
    {
        private MobileParty troopsParty;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("troopData", ref troopsParty);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddMenus(campaignGameStarter);
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.TROOP_MENU_ID, 
                menuText: "Troops Management", 
                initDelegate: new OnInitDelegate(TroopMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.TROOP_MENU_ID,
                optionId: "storage_village_menu_troop",
                optionText: "{=!}Troops",
                condition: delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;
                    return true;
                },
                consequence: MenuConsequenceForTroop,
                isLeave: false,
                index: 1
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.TROOP_MENU_ID,
                optionId: "storage_village_menu_donate_troops",
                optionText: "{=!}Donate Troops",
                condition: delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveTroops;
                    return true;
                },
                consequence: MenuConsequenceForDonateTroops,
                isLeave: false,
                index: 2
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.TROOP_MENU_ID,
                optionId: "storage_village_menu_donate_prisoner",
                optionText: "{=!}Donate Prisoners",
                condition: delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveTroops;
                    return true;
                },
                consequence: MenuConsequenceForDonatePrisoner,
                isLeave: false,
                index: 3
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.TROOP_MENU_ID,
                optionId: "storage_village_troop_leave",
                optionText: "{=!}Back to Storage",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForLeave,
                isLeave: false,
                index: -1
            );
        }

        public static void TroopMenuInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=!}Troops Management");
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

        private void MenuConsequenceForTroop(MenuCallbackArgs args)
        {
            if (troopsParty == null)
            {
                troopsParty = new MobileParty();
            }

            troopsParty.SetCustomName(new TextObject("Troops Storage"));
            PartyScreenManager.OpenScreenAsManageTroopsAndPrisoners(troopsParty);
        }

        private void MenuConsequenceForDonateTroops(MenuCallbackArgs args)
        {
            PartyScreenManager.OpenScreenAsDonateGarrisonWithCurrentSettlement();
        }

        private void MenuConsequenceForDonatePrisoner(MenuCallbackArgs args)
        {
            PartyScreenManager.OpenScreenAsDonatePrisoners();
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

    }
}
