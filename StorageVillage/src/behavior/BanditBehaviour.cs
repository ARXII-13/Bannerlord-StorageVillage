using TaleWorlds.CampaignSystem;
using System;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;
using System.Linq;
using StorageVillage.src.util;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Localization;

namespace StorageVillage.src.behavior
{
    public class BanditBehavior : CampaignBehaviorBase
    {
        //private Settlement targetSettlement = null;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));

            //CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(
            //    this,
            //    new Action<MobileParty>(this.HandleBanditDailyTickEvent)
            //);
        }

        public override void SyncData(IDataStore dataStore)
        {
            //dataStore.SyncData("targetSettlement", ref targetSettlement);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddMenus(campaignGameStarter);
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.BANDIT_MENU_ID,
                menuText: "Bandit Management",
                initDelegate: new OnInitDelegate(BanditMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_MENU_ID,
                optionId: "storage_village_menu_bandit_target",
                optionText: "{=!}Set Bandit Target Settlement",
                condition: MenuConditionForBandit,
                consequence: MenuConsequenceForBandit,
                isLeave: false,
                index: 1
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_MENU_ID,
                optionId: "storage_village_bandit_leave",
                optionText: "{=!}Back to Storage",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForLeave,
                isLeave: false,
                index: -1
            );
        }

        public static void BanditMenuInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=!}Bandit");
        }

        private bool MenuConditionForBandit(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            return true;
        }

        private bool MenuConditionForLeave(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private void MenuConsequenceForBandit(MenuCallbackArgs args)
        {
            List<InquiryElement> elements = new List<InquiryElement>();

            List<Settlement> settlements =  Settlement.FindAll(settlement =>
                settlement.IsCastle || settlement.IsTown).ToList();

            settlements.Sort();
            foreach (Settlement settlement in settlements)
            {
                //elements.Add(new InquiryElement(settlement, settlement.Name.ToString(), new ImageIdentifier()));
                //elements.Add(new InquiryElement(settlement, settlement.Name.ToString(), new ImageIdentifier(CampaignUIHelper.sett(hero.CharacterObject, false))));
                elements.Add(new InquiryElement(settlement, settlement.Name.ToString(), new ImageIdentifier(CampaignUIHelper.GetCharacterCode(Clan.PlayerClan.Leader.CharacterObject, false))));
            }
            string title = new TextObject("{=koX9okuG}None").ToString();

            elements.Add(new InquiryElement((object)null, title, new ImageIdentifier(ImageIdentifierType.Null)));

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                titleText: "Deposit",
                descriptionText: "Please select the target settlement for the bandits to patrol:",
                inquiryElements: elements,
                isExitShown: true,
                maxSelectableOptionCount: 1,
                affirmativeText: "Confirm",
                negativeText: "Cancel",
                affirmativeAction: new Action<System.Collections.Generic.List<InquiryElement>>(SelectedTargetSettlement),
                negativeAction: null
            ));
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

        private void SelectedTargetSettlement(System.Collections.Generic.List<InquiryElement> amount)
        {
            //int depositAmount = ConvertStringToNumber(amount);
            //currentBalance += depositAmount;
            //PartyBase.MainParty.LeaderHero.ChangeHeroGold(-depositAmount);
            //UpdateBankDescription();
            //GameMenu.SwitchToMenu(Constants.BANK_RESULT_MENU_ID);
        }

        private void HandleBanditDailyTickEvent(MobileParty party)
        {
            if (!party.IsBandit) {
                return;
            }

            string name = "Sargot";

            var matches = Settlement.FindAll(settlement =>
                settlement.Name.ToString().Equals(name)).ToList();

            if (matches.Count == 0)
            {
                return;
            }

            party.SetMovePatrolAroundSettlement(matches[0]);
        }
    }
}
