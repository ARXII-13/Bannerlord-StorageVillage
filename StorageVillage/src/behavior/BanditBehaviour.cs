using System;
using System.Collections.Generic;
using System.Linq;
using StorageVillage.src.util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace StorageVillage.src.behavior {
    public class BanditBehavior : CampaignBehaviorBase {
        private Settlement targetSettlement;
        private MobileParty targetParty;

        private const string BANDIT_INFO_TEXT_VARIABLE = "BANDIT_INFO";
        private const string BANDIT_TARGET_RESULT_TEXT_VARIABLE = "BANDIT_TARGET_RESULT";

        public override void RegisterEvents() {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));

            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(
                this,
                new Action<MobileParty>(this.HandleBanditDailyTickEvent)
            );
        }

        public override void SyncData(IDataStore dataStore) {
            dataStore.SyncData("targetSettlement", ref targetSettlement);
            dataStore.SyncData("targetParty", ref targetParty);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter) {
            AddMenus(campaignGameStarter);
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter) {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.BANDIT_MENU_ID,
                menuText: $"{{{BANDIT_INFO_TEXT_VARIABLE}}}",
                initDelegate: new OnInitDelegate(HandleBanditMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_MENU_ID,
                optionId: "storage_village_menu_bandit_target_settlement",
                optionText: "{=SET_BANDIT_TARGET_SETTLEMENT}Set Target Settlement to Patrol",
                condition: MenuConditionForBandit,
                consequence: MenuConsequenceForTargetSettlement,
                isLeave: false,
                index: 1
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_MENU_ID,
                optionId: "storage_village_menu_bandit_target_party",
                optionText: "{=SET_BANDIT_TARGET_PARTY}Set Target Party to Engage",
                condition: MenuConditionForBandit,
                consequence: MenuConsequenceForTargetParty,
                isLeave: false,
                index: 2
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANDIT_MENU_ID,
                optionId: "storage_village_bandit_leave",
                optionText: "{=BACK_TO_STORAGE}Back to Storage",
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
                optionText: "{=BACK_TO_BANDIT_MENU}Back to Bandit Menu",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForBack,
                isLeave: false,
                index: 1
            );
        }

        public void HandleBanditMenuInit(MenuCallbackArgs args) {
            args.MenuTitle = new TextObject("{=BANDIT}Bandit");
            UpdateBanditDescription();
        }

        private void UpdateBanditDescription() {
            if (targetParty != null) {
                TextObject optionText = new TextObject(
                    "{=BANDIT_ENGAGE_PARTY}The bandits are engaging {PARTY_NAME}"
                );
                optionText.SetTextVariable("PARTY_NAME", targetParty.Name);

                MBTextManager.SetTextVariable(
                    variableName: BANDIT_INFO_TEXT_VARIABLE,
                    text: optionText
                );
            }
            else if (targetSettlement != null) {
                TextObject optionText = new TextObject(
                    "{=BANDIT_ENGAGE_SETTLEMENT}The bandits are engaging {SETTLEMENT_NAME}"
                );
                optionText.SetTextVariable("SETTLEMENT_NAME", targetSettlement.Name);

                MBTextManager.SetTextVariable(
                    variableName: BANDIT_INFO_TEXT_VARIABLE,
                    text: optionText
                );
            }
            else {
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_INFO_TEXT_VARIABLE,
                    text: new TextObject("{=BANDIT_NO_TARGET}The bandits have no specific target")
                );
            }
        }

        private bool MenuConditionForBandit(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.LeadAssault;
            return true;
        }

        private bool MenuConditionForLeave(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private void MenuConsequenceForTargetSettlement(MenuCallbackArgs args) {
            List<InquiryElement> options = new List<InquiryElement>();
            string title = new TextObject("{=koX9okuG}None").ToString();
            options.Add(new InquiryElement((object)null, title, new ImageIdentifier(ImageIdentifierType.Null)));

            List<Settlement> settlements = Settlement.FindAll(settlement =>
                settlement.IsCastle || settlement.IsTown || settlement.IsVillage).ToList();
            settlements.Sort((Settlement x, Settlement y) => x.Name.ToString().CompareTo(y.Name.ToString()));

            foreach (Settlement settlement in settlements) {
                options.Add(new InquiryElement(settlement, settlement.Name.ToString(), new ImageIdentifier(ImageIdentifierType.Null)));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                titleText: new TextObject("{=SET_TARGET_SETTLEMENT_TITLE}Set Target Settlement").ToString(),
                descriptionText: new TextObject("{=SET_TARGET_SETTLEMENT_DESCRIPTION}Please select the target settlement for the bandits to patrol:").ToString(),
                inquiryElements: options,
                isExitShown: true,
                maxSelectableOptionCount: 1,
                minSelectableOptionCount: 1,
                affirmativeText: new TextObject("{=CONFIRM}Confirm").ToString(),
                negativeText: new TextObject("{=CANCEL}Cancel").ToString(),
                affirmativeAction: new Action<List<InquiryElement>>(SelectedTargetSettlement),
                negativeAction: null
            ));
        }

        private void MenuConsequenceForTargetParty(MenuCallbackArgs args) {
            List<InquiryElement> options = new List<InquiryElement>();
            string title = new TextObject("{=koX9okuG}None").ToString();
            options.Add(new InquiryElement((object)null, title, new ImageIdentifier(ImageIdentifierType.Null)));
            List<MobileParty> parties = MobileParty.AllLordParties.ToList();
            parties.Sort((MobileParty x, MobileParty y) => x.Name.ToString().CompareTo(y.Name.ToString()));
            foreach (MobileParty party in parties) {
                try {
                    CharacterObject characterObject = party.LeaderHero.CharacterObject;
                    if (characterObject != null) {
                        CharacterCode characterCode = CampaignUIHelper.GetCharacterCode(party.LeaderHero.CharacterObject, false);
                        if (characterCode != null) {
                            options.Add(new InquiryElement(party, party.Name.ToString(), new ImageIdentifier(characterCode)));
                        }
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }

            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                titleText: new TextObject("{=SET_TARGET_PARTY_TITLE}Set Target Party").ToString(),
                descriptionText: new TextObject("{=SET_TARGET_PARTY_DESCRIPTION}Please select the target party for the bandits to engage:").ToString(),
                inquiryElements: options,
                isExitShown: true,
                maxSelectableOptionCount: 1,
                minSelectableOptionCount: 1,
                affirmativeText: new TextObject("{=CONFIRM}Confirm").ToString(),
                negativeText: new TextObject("{=CANCEL}Cancel").ToString(),
                affirmativeAction: new Action<List<InquiryElement>>(SelectedTargetParty),
                negativeAction: null
            ));
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

        private void MenuConsequenceForBack(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.BANDIT_MENU_ID);
        }

        private void HandleBanditResultMenuInit(MenuCallbackArgs args) {
            args.MenuTitle = new TextObject("{=SET_BANDIT_TARGET}Set Bandit Target");
            UpdateBanditResultDescription();
        }

        private void UpdateBanditResultDescription() {
            if (targetParty != null) {
                TextObject optionText = new TextObject(
                    "{=BANDIT_ENGAGE_PARTY}The bandits are engaging {PARTY_NAME}."
                );
                optionText.SetTextVariable("PARTY_NAME", targetParty.Name);
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_TARGET_RESULT_TEXT_VARIABLE,
                    text: optionText
                );
            }
            else if (targetSettlement != null) {
                TextObject optionText = new TextObject(
                    "{=BANDIT_ENGAGE_SETTLEMENT}The bandits are engaging {SETTLEMENT_NAME}."
                );
                optionText.SetTextVariable("SETTLEMENT_NAME", targetSettlement.Name);
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_TARGET_RESULT_TEXT_VARIABLE,
                    text: optionText
                );
            }
            else {
                TextObject optionText = new TextObject(
                    "{=BANDIT_TARGET_RESET}The target is reset! \n \nThe bandits will go back to their original targets."
                );
                MBTextManager.SetTextVariable(
                    variableName: BANDIT_TARGET_RESULT_TEXT_VARIABLE,
                    text: optionText
                );
            }
        }

        private void SelectedTargetSettlement(List<InquiryElement> element) {
            if (element.Count() < 1) {
                targetSettlement = null;
            }
            else {
                if (element.First() == null) {
                    targetSettlement = null;
                    targetParty = null;
                }
                else {
                    Settlement settlement = element.First().Identifier as Settlement;
                    settlement.Town.FoodStocks += 100;
                    targetSettlement = settlement;
                    targetParty = null;
                }
                UpdateBanditResultDescription();
                GameMenu.SwitchToMenu(Constants.BANDIT_RESULT_MENU_ID);
            }
        }
        private void SelectedTargetParty(List<InquiryElement> element) {
            if (element.Count() < 1) {
                targetParty = null;
            }
            else {
                if (element.First() == null) {
                    targetSettlement = null;
                    targetParty = null;
                }
                else {
                    MobileParty party = element.First().Identifier as MobileParty;
                    targetParty = party;
                    targetSettlement = null;
                }
                UpdateBanditResultDescription();
                GameMenu.SwitchToMenu(Constants.BANDIT_RESULT_MENU_ID);
            }
        }

        private void HandleBanditDailyTickEvent(MobileParty party) {
            if (!party.IsBandit) {
                return;
            }

            if (targetSettlement != null) {
                party.Ai.SetMovePatrolAroundSettlement(targetSettlement);
            }

            if (targetParty != null) {
                party.Ai.SetMoveGoAroundParty(targetParty);
            }
        }
    }
}
