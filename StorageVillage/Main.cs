using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace StorageVillage
{
    public class Main : MBSubModuleBase
    {
        protected override void OnSubModuleLoad() { 
            Module.CurrentModule.AddInitialStateOption( 
                new InitialStateOption(
                    "TestMainMenuOption", 
                    new TextObject("Click me!", null), 
                    9990, 
                    () => {
                        InformationManager.DisplayMessage(new InformationMessage("InformationMessage"));
                    }, 
                    () =>
                    {
                        return (false, null);
                    }));
        }
    }
}
