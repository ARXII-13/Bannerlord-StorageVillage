using System;
using System.Collections.Generic;
using System.Linq;
using StorageVillage.src.util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace StorageVillage.src.behavior {
    public class BankBehavior : CampaignBehaviorBase {
        private static int currentBalance;
        private static double currentWeeklyInterest;

        private const string BANK_INFO_TEXT_VARIABLE = "BANK_INFO";
        private const string BANK_INFO_TITLE_TEXT_VARIABLE = "BANK_INFO_TITLE";
        private const string BANK_INFO_RETURN_TEXT_VARIABLE = "BANK_INFO_APY";
        private const string BANK_INFO_INTEREST_RATE_TEXT_VARIABLE = "BANK_INFO_INTEREST_RATE";
        private const string BANK_INFO_BALANCE_TEXT_VARIABLE = "BANK_INFO_BALANCE";
        private const string BANK_RESULT_TEXT_VARIABLE = "BANK_INFO";

        private const double WEEKLY_BASE_INTEREST_RATE = 0.002;
        private const double TRADE_SKILL_PROFIT_MULTIPLIER = 1;

        private const string MONEY_ICON = "<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">";

        public override void RegisterEvents() {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));

            CampaignEvents.DailyTickEvent.AddNonSerializedListener(
                this,
                new Action(this.HandleBankWeeklyTickEvent)
            );
        }

        public override void SyncData(IDataStore dataStore) {
            dataStore.SyncData("currentBalance", ref currentBalance);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter) {
            AddMenus(campaignGameStarter);
            UpdateBankDescription();
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter) {
            campaignGameStarter.AddGameMenu(
                menuId: Constants.BANK_MENU_ID,
                menuText: $"{{{BANK_INFO_TEXT_VARIABLE}}}",
                initDelegate: new OnInitDelegate(HandleBankMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANK_MENU_ID,
                optionId: "storage_village_bank_deposit",
                optionText: "{=DEPOSIT}Deposit",
                condition: MenuConditionForBankAction,
                consequence: MenuConsequenceForDeposit,
                isLeave: false,
                index: 1
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANK_MENU_ID,
                optionId: "storage_village_bank_withdraw",
                optionText: "{=WITHDRAW}Withdraw",
                condition: MenuConditionForBankAction,
                consequence: MenuConsequenceForWithdraw,
                isLeave: false,
                index: 2
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANK_MENU_ID,
                optionId: "storage_village_bank_leave",
                optionText: "{=BACK_TO_STORAGE_MENU}Back to Storage",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForLeave,
                isLeave: false,
                index: -1
            );

            campaignGameStarter.AddGameMenu(
                menuId: Constants.BANK_RESULT_MENU_ID,
                menuText: $"{{{BANK_RESULT_TEXT_VARIABLE}}}",
                initDelegate: new OnInitDelegate(HandleBankResultMenuInit)
            );

            campaignGameStarter.AddGameMenuOption(
                menuId: Constants.BANK_RESULT_MENU_ID,
                optionId: "storage_village_bank_result_leave",
                optionText: "{=BACK_TO_BANK_MENU}Back to Bank Menu",
                condition: MenuConditionForLeave,
                consequence: MenuConsequenceForBack,
                isLeave: false,
                index: 1
            );
        }

        private void HandleBankMenuInit(MenuCallbackArgs args) {
            args.MenuTitle = new TextObject("{=!}National Bank");
            currentWeeklyInterest = CalculateWeeklyInterest();
            UpdateBankDescription();
        }

        private void UpdateBankDescription() {
            MBTextManager.SetTextVariable(
                variableName: BANK_INFO_TITLE_TEXT_VARIABLE,
                text: new TextObject("{=CALRADIA_CENTRAL_BANK}Calradia Central Bank")
            );

            var balanceText = new TextObject("{=ACCOUNT_BALANCE}Account Balance:\n{BALANCE} {ICON}");
            balanceText.SetTextVariable("BALANCE", currentBalance.ToString("N0"));
            balanceText.SetTextVariable("ICON", MONEY_ICON);
            MBTextManager.SetTextVariable(
                variableName: BANK_INFO_BALANCE_TEXT_VARIABLE,
                text: balanceText
            );

            var currentInterestText = new TextObject("{=CURRENT_INTEREST}Current Interest:\n{INTEREST}%");
            var interestRatePercentage = currentWeeklyInterest * 100;
            currentInterestText.SetTextVariable("INTEREST", interestRatePercentage.ToString("N2"));
            MBTextManager.SetTextVariable(
                variableName: BANK_INFO_INTEREST_RATE_TEXT_VARIABLE,
                text: currentInterestText
            );
            int perAmount = (int)Math.Round(100 / currentWeeklyInterest);
            TextObject projectedReturnText = new TextObject(
                "{=CURRENT_RETURN_PROJECTION}Current Weekly return:\n{INTEREST} {ICON} for every {PERAMOUNT} {ICON} invested"
            );
            projectedReturnText.SetTextVariable("INTEREST", 100.ToString("N0"));
            projectedReturnText.SetTextVariable("PERAMOUNT", perAmount.ToString("N0"));
            projectedReturnText.SetTextVariable("ICON", MONEY_ICON);
            MBTextManager.SetTextVariable(
                variableName: BANK_INFO_RETURN_TEXT_VARIABLE,
                text: projectedReturnText
            );

            MBTextManager.SetTextVariable(
                variableName: BANK_INFO_TEXT_VARIABLE,
                text: $"{{{BANK_INFO_TITLE_TEXT_VARIABLE}}}\n \n{{{BANK_INFO_BALANCE_TEXT_VARIABLE}}}\n \n{{{BANK_INFO_INTEREST_RATE_TEXT_VARIABLE}}}\n \n{{{BANK_INFO_RETURN_TEXT_VARIABLE}}}"
            );
        }

        private void HandleBankResultMenuInit(MenuCallbackArgs args) {
            args.MenuTitle = new TextObject("{=BANK_RESULT}Bank Result");
            UpdateBankResultDescription();
        }

        private void UpdateBankResultDescription() {
            TextObject resultText = new TextObject(
                "{=BANK_RESULT_DESCRIPTION}Action completed successfully!\n\nThe current balance is:\n{BALANCE} {ICON}"
            );
            resultText.SetTextVariable("BALANCE", currentBalance.ToString("N0"));
            resultText.SetTextVariable("ICON", MONEY_ICON);
            MBTextManager.SetTextVariable(
                variableName: BANK_RESULT_TEXT_VARIABLE,
                text: resultText
            );

        }

        private bool MenuConditionForLeave(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private bool MenuConditionForBankAction(MenuCallbackArgs args) {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            return true;
        }

        private void MenuConsequenceForDeposit(MenuCallbackArgs args) {
            InformationManager.ShowTextInquiry(new TextInquiryData(
                titleText: new TextObject("{=DEPOSIT}Deposit").ToString(),
                text: new TextObject("{=DEPOSIT_DESCRIPTION}Please input the amount of denar that you would like to deposit:").ToString(),
                isAffirmativeOptionShown: true,
                isNegativeOptionShown: true,
                affirmativeText: new TextObject("{=CONFIRM}Confirm").ToString(),
                negativeText: new TextObject("{=CANCEL}Cancel").ToString(),
                affirmativeAction: new Action<string>(DepositFromBank),
                negativeAction: null,
                defaultInputText: "0",
                textCondition: new Func<string, Tuple<bool, string>>(DepositTextCondition)
            ));
        }

        private void DepositFromBank(string amount) {
            int depositAmount = ConvertStringToNumber(amount);
            currentBalance += depositAmount;
            PartyBase.MainParty.LeaderHero.ChangeHeroGold(-depositAmount);
            UpdateBankDescription();
            GameMenu.SwitchToMenu(Constants.BANK_RESULT_MENU_ID);
        }

        public static Tuple<bool, string> DepositTextCondition(string amount) {
            List<TextObject> list = IsProperMoneyAmount(amount);

            int currentOwnedMoney = PartyBase.MainParty.LeaderHero.Gold;
            int depositAmount = ConvertStringToNumber(amount);
            if (currentOwnedMoney < depositAmount) {
                list.Add(new TextObject("{=DEPOSIT_INSUFFICIENT_BALANCE}You do not have sufficient balance to deposit!"));
            }

            return ConvertListToTuple(list);
        }

        private void MenuConsequenceForWithdraw(MenuCallbackArgs args) {
            InformationManager.ShowTextInquiry(new TextInquiryData(
                 titleText: new TextObject("{=WITHDRAW}Withdraw").ToString(),
                 text: new TextObject("{=WITHDRAW_DESCRIPTION}Please input the amount of denar that you would like to withdraw:").ToString(),
                 isAffirmativeOptionShown: true,
                 isNegativeOptionShown: true,
                 affirmativeText: new TextObject("{=CONFIRM}Confirm").ToString(),
                 negativeText: new TextObject("{=CANCEL}Cancel").ToString(),
                 affirmativeAction: new Action<string>(WithdrawFromBank),
                 negativeAction: null,
                 defaultInputText: "0",
                 textCondition: new Func<string, Tuple<bool, string>>(WithdrawTextCondition)
             ));
        }

        private void WithdrawFromBank(string amount) {
            int withdrawAmount = ConvertStringToNumber(amount);
            currentBalance -= withdrawAmount;
            PartyBase.MainParty.LeaderHero.ChangeHeroGold(withdrawAmount);
            UpdateBankDescription();
            GameMenu.SwitchToMenu(Constants.BANK_RESULT_MENU_ID);
        }

        public static Tuple<bool, string> WithdrawTextCondition(string amount) {
            List<TextObject> list = IsProperMoneyAmount(amount);

            int withdrawAmount = ConvertStringToNumber(amount);
            if (currentBalance < withdrawAmount) {
                list.Add(new TextObject("{=WITHDRAW_INSUFFICIENT_BALANCE}You do not have sufficient balance to withdraw!"));
            }

            return ConvertListToTuple(list);
        }

        private void MenuConsequenceForLeave(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.MAIN_MENU_ID);
        }

        private void MenuConsequenceForBack(MenuCallbackArgs args) {
            GameMenu.SwitchToMenu(Constants.BANK_MENU_ID);
        }

        private static int ConvertStringToNumber(string amount) {
            var isNumeric = int.TryParse(amount, out int n);
            if (isNumeric) {
                return Int32.Parse(amount);
            }
            return 0;
        }

        private static List<TextObject> IsProperMoneyAmount(string amount) {
            List<TextObject> list = new List<TextObject>();
            var isNumeric = int.TryParse(amount, out int n);

            if (!isNumeric) {
                list.Add(new TextObject("{=!}The input must be a proper number!"));
            }

            return list;
        }

        public static Tuple<bool, string> ConvertListToTuple(List<TextObject> list) {
            string item = string.Empty;
            bool item2 = list.Count == 0;
            if (list.Count == 1) {
                item = list[0].ToString();
            }
            else if (list.Count > 1) {
                TextObject textObject = list[0];
                for (int i = 1; i < list.Count; i++) {
                    textObject = GameTexts.FindText("str_string_newline_newline_string").SetTextVariable("STR1", textObject.ToString()).SetTextVariable("STR2", list[i].ToString());
                }

                item = textObject.ToString();
            }

            return new Tuple<bool, string>(item2, item);
        }

        private void HandleBankWeeklyTickEvent() {
            currentWeeklyInterest = CalculateWeeklyInterest();
            int interestEarned = CalculateBankInterestEarn();
            currentBalance += interestEarned;

            SkillLevelingManager.OnTradeProfitMade(Clan.PlayerClan.Leader, (int)Math.Round(interestEarned * TRADE_SKILL_PROFIT_MULTIPLIER));

            TextObject infoText = new TextObject(
                "{=WEEKLY_INTEREST_NOTIFY}Weekly interests earned: {AMOUNT} {ICON}"
            );
            infoText.SetTextVariable("AMOUNT", interestEarned.ToString("N0"));
            infoText.SetTextVariable("ICON", MONEY_ICON);
            InformationManager.DisplayMessage(new InformationMessage(infoText.ToString()));

        }

        private double CalculateWeeklyInterest() {
            float totalProsperous = 0;
            int numOfTown = 0;

            List<Settlement> settlements = Settlement.FindAll(settlement => settlement.IsTown).ToList();

            foreach (Settlement settlement in settlements) {
                if (settlement.IsTown) {
                    totalProsperous += settlement.Town.Prosperity;
                    numOfTown++;
                }
            }

            float averageProsperous = totalProsperous / numOfTown;
            int tradeLevel = Clan.PlayerClan.Leader.GetSkillValue(DefaultSkills.Trade);

            double baseInterestRate = WEEKLY_BASE_INTEREST_RATE;

            // use the exponential to calculate the interest rate adjustment 
            var prosperityAdjustment = (1 - Math.Exp(-0.0008 * (averageProsperous - 2000)));
            var adjustedInterestRate = baseInterestRate * prosperityAdjustment;

            return adjustedInterestRate;
        }

        public int CalculateBankInterestEarn() {
            return (int)Math.Round(currentWeeklyInterest * currentBalance);
        }
    }
}
