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
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.Library;

namespace StorageVillage.src.behavior
{
    public class BanditBehavior : CampaignBehaviorBase
    {
        private Settlement targetSettlement;
        private MobileParty targetParty;

        private const string BANDIT_INFO_TEXT_VARIABLE = "BANDIT_INFO";
        private const string BANDIT_TARGET_RESULT_TEXT_VARIABLE = "BANDIT_TARGET_RESULT";

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));

            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(
                this,
                new Action<MobileParty>(this.HandleBanditDailyTickEvent)
            );
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("targetSettlement", ref targetSettlement);
            dataStore.SyncData("targetParty", ref targetParty);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddMenus(campaignGameStarter);
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.BANDIT_MENU_ID,
                menuText: $"{{{BANDIT_INFO_TEXT_VARIABLE}}}",
                initDelegate: new OnInitDelegate(HandleBanditMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_MENU_ID,
                optionId: "storage_village_menu_bandit_target_settlement",
                optionText: "{=!}Set Target Settlement to Patrol",
                condition: MenuConditionForBandit,
                consequence: MenuConsequenceForTargetSettlement,
                isLeave: false,
                index: 1
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_MENU_ID,
                optionId: "storage_village_menu_bandit_target_party",
                optionText: "{=!}Set Target Party to Engage",
                condition: MenuConditionForBandit,
                consequence: MenuConsequenceForTargetParty,
                isLeave: false,
                index: 2
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

            campaignGameStarter.AddGameMenu(
                menuId: Constants.BANDIT_RESULT_MENU_ID,
                menuText: $"{{{BANDIT_TARGET_RESULT_TEXT_VARIABLE}}}",
                initDelegate: new OnInitDelegate(HandleBanditResultMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_RESULT_MENU_ID,
                optionId: "storage_village_bandit_result_leave",
                optionText: "{=!}Back to Bandit Menu",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForBack,
                isLeave: false,
                index: 1
            );
        }

        public void HandleBanditMenuInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=!}Bandit");
            UpdateBanditDescription();
        }

        private void UpdateBanditDescription()
        {
            if (targetParty != null)
            {
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_INFO_TEXT_VARIABLE,
                    text: new TextObject($"The bandits are engaging {targetParty.Name}."));
            }
            else if (targetSettlement != null)
            {
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_INFO_TEXT_VARIABLE,
                    text: new TextObject($"The bandits are patrolling around {targetSettlement.Name}."));
            }
            else
            {
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_INFO_TEXT_VARIABLE,
                    text: new TextObject($"The bandits have no specific objective."));
            }
        }

        private bool MenuConditionForBandit(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.LeadAssault;
            return true;
        }

        private bool MenuConditionForLeave(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private void MenuConsequenceForTargetSettlement(MenuCallbackArgs args)
        {
            List<InquiryElement> options = new List<InquiryElement>();
            string title = new TextObject("{=koX9okuG}None").ToString();
            options.Add(new InquiryElement((object)null, title, new ImageIdentifier(ImageIdentifierType.Null)));

            List<Settlement> settlements =  Settlement.FindAll(settlement =>
                settlement.IsCastle || settlement.IsTown).ToList();
            settlements.Sort((Settlement x, Settlement y) => x.Name.ToString().CompareTo(y.Name.ToString()));

            foreach (Settlement settlement in settlements)
            {
                options.Add(new InquiryElement(settlement, settlement.Name.ToString(), new ImageIdentifier(ImageIdentifierType.Null)));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                titleText: "Set Target Settlement",
                descriptionText: "Please select the target settlement for the bandits to patrol:",
                inquiryElements: options,
                isExitShown: true,
                maxSelectableOptionCount: 1,
                affirmativeText: "Confirm",
                negativeText: "Cancel",
                affirmativeAction: new Action<List<InquiryElement>>(SelectedTargetSettlement),
                negativeAction: null
            ));
        }

        private void MenuConsequenceForTargetParty(MenuCallbackArgs args)
        {
            List<InquiryElement> options = new List<InquiryElement>();
            string title = new TextObject("{=koX9okuG}None").ToString();
            options.Add(new InquiryElement((object)null, title, new ImageIdentifier(ImageIdentifierType.Null)));

            List<MobileParty> parties = MobileParty.AllLordParties.ToList();
            parties.Sort((MobileParty x, MobileParty y) => x.Name.ToString().CompareTo(y.Name.ToString()));

            foreach (MobileParty party in parties)
            {
                options.Add(new InquiryElement(party, party.Name.ToString(), new ImageIdentifier(CampaignUIHelper.GetCharacterCode(party.LeaderHero.CharacterObject, false))));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                titleText: "Set Target Party",
                descriptionText: "Please select the target party for the bandits to engage:",
                inquiryElements: options,
                isExitShown: true,
                maxSelectableOptionCount: 1,
                affirmativeText: "Confirm",
                negativeText: "Cancel",
                affirmativeAction: new Action<List<InquiryElement>>(SelectedTargetParty),
                negativeAction: null
            ));
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

        private void MenuConsequenceForBack(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(Constants.BANDIT_MENU_ID);
        }

        private void HandleBanditResultMenuInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=!}Bandit Target Set");
            UpdateBanditResultDescription();
        }

        private void UpdateBanditResultDescription()
        {
            if (targetParty != null) {
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_TARGET_RESULT_TEXT_VARIABLE,
                    text: new TextObject($"The new target party is set! \n \nThe bandits will try to enagage {targetParty.Name}."));
            } else if (targetSettlement != null) {
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_TARGET_RESULT_TEXT_VARIABLE,
                    text: new TextObject($"The new target settlement is set! \n \nThe bandits will try to patrol around {targetSettlement.Name}."));
            } else {
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_TARGET_RESULT_TEXT_VARIABLE,
                    text: new TextObject($"The target is reset! \n \nThe bandits will go back to their original targets."));
            }
        }

        private void SelectedTargetSettlement(List<InquiryElement> element)
        {
            if (element.Count() < 1)
            {
                targetSettlement = null;
            }
            else {
                if (element.First() == null)
                {
                    targetSettlement = null;
                    targetParty = null;
                }
                else
                {
                    Settlement settlement = element.First().Identifier as Settlement;
                    targetSettlement = settlement;
                    targetParty = null;
                }
                UpdateBanditResultDescription();
                GameMenu.SwitchToMenu(Constants.BANDIT_RESULT_MENU_ID);
            }
        }
        private void SelectedTargetParty(List<InquiryElement> element)
        {
            if (element.Count() < 1)
            {
                targetParty = null;
            }
            else {
                if (element.First() == null)
                {
                    targetSettlement = null;
                    targetParty = null;
                }
                else
                {
                    MobileParty party = element.First().Identifier as MobileParty;
                    targetParty = party;
                    targetSettlement = null;
                }
                UpdateBanditResultDescription();
                GameMenu.SwitchToMenu(Constants.BANDIT_RESULT_MENU_ID);
            }
        }

        private void HandleBanditDailyTickEvent(MobileParty party)
        {
            if (!party.IsBandit) {
                return;
            }

            if (targetSettlement != null)
            {
                party.Ai.SetMovePatrolAroundSettlement(targetSettlement);
            }

            if (targetParty != null)
            {
                party.Ai.SetMoveGoAroundParty(targetParty);
            }
        }
    }
}
