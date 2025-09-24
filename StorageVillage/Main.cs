using System;

using HarmonyLib;
using StorageVillage.src.behavior;
using StorageVillage.src.model;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using Debug = TaleWorlds.Library.Debug;

namespace StorageVillage {
    public class Main : MBSubModuleBase {
        public const string ModId = "StorageVillage";

        protected override void OnSubModuleLoad() {
            try {
                base.OnSubModuleLoad();

                var harmony = new Harmony(ModId);
                harmony.PatchAll();

                InformationManager.DisplayMessage(new InformationMessage($"Module {ModId} loaded"));
                System.Diagnostics.Debug.WriteLine("Debugger is attached");
            }
            catch (Exception e) {
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }

        protected override void OnSubModuleUnloaded() {
            base.OnSubModuleUnloaded();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject) {
            if (game.GameType is Campaign) {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarterObject;
                campaignStarter.AddBehavior(new SettlementBehavior());
                campaignStarter.AddBehavior(new TroopBehavior());
                campaignStarter.AddBehavior(new BankBehavior());
                campaignStarter.AddBehavior(new BanditBehavior());

                campaignStarter.AddModel(new StoragePartyLimitModel());
            }
        }

        protected void testCommand() {
            Module.CurrentModule.AddInitialStateOption(
                new InitialStateOption(
                    "TestMainMenuOption",
                    new TextObject("Click me!", null),
                    9990,
                    () => {
                        InformationManager.DisplayMessage(new InformationMessage("InformationMessage"));
                    },
                    () => {
                        return (false, null);
                    }
                )
            );
        }

    }
}
