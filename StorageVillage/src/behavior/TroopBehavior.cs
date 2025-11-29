using System;
using Helpers;
using StorageVillage.src.util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace StorageVillage.src.behavior {
    public class TroopBehavior : CampaignBehaviorBase {
        private MobileParty troopsParty;

        public override void RegisterEvents() {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore) {
            dataStore.SyncData("troopData", ref troopsParty);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter) {
            AddMenus(campaignGameStarter);
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter) {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.TROOP_MENU_ID,
                menuText: "{=TROOP_MENU}Troops Management",
                initDelegate: new OnInitDelegate(TroopMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.TROOP_MENU_ID,
                optionId: "storage_village_menu_troop",
                optionText: "{=TROOP_STORAGE}Troops Storage",
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
                optionText: "{=DONATE_TROOPS}Donate Troops",
                condition: delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                    return true;
                },
                consequence: MenuConsequenceForDonateTroops,
                isLeave: false,
                index: 2
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.TROOP_MENU_ID,
                optionId: "storage_village_menu_donate_prisoner",
                optionText: "{=DONATE_PRISONERS}Donate Prisoners",
                condition: delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                    return true;
                },
                consequence: MenuConsequenceForDonatePrisoner,
                isLeave: false,
                index: 3
             );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.TROOP_MENU_ID,
                optionId: "storage_village_troop_leave",
                optionText: "{=BACK_TO_STORAGE_MENU}Back to Storage",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForLeave,
                isLeave: false,
                index: -1
            );
        }

        public void TroopMenuInit(MenuCallbackArgs args) {
            args.MenuTitle = new TextObject("{=TROOP_MENU}Troops Management");
        }

        private bool MenuConditionForLeave(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private bool MenuConditionForSubMenu(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            return true;
        }

        private void MenuConsequenceForTroop(MenuCallbackArgs args) {
            if (troopsParty == null) {
                troopsParty = new MobileParty();
            }

            troopsParty.Party.SetCustomName(new TextObject("{=TROOP_STORAGE}Troops Storage"));
            PartyScreenHelper.OpenScreenAsManageTroopsAndPrisoners(troopsParty);
        }

        private void MenuConsequenceForDonateTroops(MenuCallbackArgs args) {
            PartyScreenHelper.OpenScreenAsDonateGarrisonWithCurrentSettlement();
        }

        private void MenuConsequenceForDonatePrisoner(MenuCallbackArgs args) {
            PartyScreenHelper.OpenScreenAsDonatePrisoners();
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

    }
}
