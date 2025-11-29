using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace StorageVillage.src.data {
    class SaveDefiner : SaveableTypeDefiner {

        public SaveDefiner() : base(1_438_813) {

        }

        protected override void DefineClassTypes() {
        }

        protected override void DefineContainerDefinitions() {
            base.ConstructContainerDefinition(typeof(Dictionary<string, ItemRoster>));
        }
    }
}


