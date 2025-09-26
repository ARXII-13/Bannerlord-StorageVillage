using TaleWorlds.SaveSystem;

namespace StorageVillage.src.data {
    class SaveDefiner : SaveableTypeDefiner {

        public SaveDefiner() : base(1_438_813) {

        }

        protected override void DefineClassTypes() {
            //AddStructDefinition(typeof(ExampleStruct), 1);

            //base.AddClassDefinition(typeof(TestData), 1);
            //base.AddClassDefinition(typeof(TestData), 1);

            //base.AddClassDefinition(typeof(CustomMapNotification), 1);
            //base.AddStructDefinition(typeof(ExampleStruct), 2);
            //base.AddClassDefinition(typeof(ExampleNested), 3);
            //base.AddStructDefinition(typeof(NestedStruct), 4);
        }

        protected override void DefineContainerDefinitions() {
            //base.ConstructContainerDefinition(typeof(List<CustomMapNotification>));
            //// Both of these are necessary: order isn't important
            //base.ConstructContainerDefinition(typeof(List<NestedStruct>));
            //base.ConstructContainerDefinition(typeof(Dictionary<string, List<NestedStruct>>));
        }
    }
}


