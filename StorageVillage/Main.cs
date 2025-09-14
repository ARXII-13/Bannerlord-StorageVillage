using System;
using StorageVillage.src.behavior;
using StorageVillage.src.model;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace StorageVillage
{
    public class Main : MBSubModuleBase
    {
        public const string ModId = "StorageVillage";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage($"Module {ModId} loaded"));
            System.Diagnostics.Debug.WriteLine("Debugger is attached");
            Console.WriteLine("Console is working");
            //throw new Exception("Test Exception");
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarterObject;
                campaignStarter.AddBehavior(new SettlementBehavior());
                campaignStarter.AddBehavior(new TroopBehavior());
                campaignStarter.AddBehavior(new BankBehavior());
                campaignStarter.AddBehavior(new BanditBehavior());

                campaignStarter.AddModel(new StoragePartyLimitModel());
            }
        }

        protected void testCommand()
        {
            Module.CurrentModule.AddInitialStateOption(
                new InitialStateOption(
                    "TestMainMenuOption",
                    new TextObject("Click me!", null),
                    9990,
                    () =>
                    {
                        InformationManager.DisplayMessage(new InformationMessage("InformationMessage"));
                    },
                    () =>
                    {
                        return (false, null);
                    }
                )
            );
        }

    }
}

//TODO(s)
//    1. Banking System in game with weekly interest (Done)
//    2. Merge Bandit Units and expose the threhold that as a user settings
//    3. Add a option to allow the town/castle to not recruit militia 
//    4. Increase workshop limit and can still have it during a war
