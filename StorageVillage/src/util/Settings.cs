using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.PerSave;
using MCM.Common;

namespace StorageVillage.src.setting {
    public class StorageVillageSettings : AttributePerSaveSettings<StorageVillageSettings> {
        public override string FolderName => "StorageVillage";

        public override string Id => "StorageVillage";
        public override string DisplayName => $"StorageVillage {typeof(StorageVillageSettings).Assembly.GetName().Version.ToString(3)}";

        [SettingPropertyDropdown("{=STORAGE_SETTING_TYPE}Storage Setting Type", Order = 0, RequireRestart = false, HintText = "{=STORAGE_SETTING_HINT}Switch between different storage settings.")]
        [SettingPropertyGroup("{=STORAGE_SETTING}Storage Settings", GroupOrder = 0)]
        public Dropdown<string> townStorageSetting { get; set; } = new Dropdown<string>(new string[]
        {
            util.Constants.STORAGE_SETTING_TYPE_GLOBAL,
            util.Constants.STORAGE_SETTING_TYPE_TOWN
        }, selectedIndex: 0);

        [SettingPropertyFloatingInteger("{=BANK_INTEREST_RATE_SETTING}Bank Weekly Interest Rate", 0f, 10.0f, HintText = "{=BANK_INTEREST_RATE_SETTING_HINT}Set the weekly interest rate in percentage", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=BANK_SETTINGS}Bank Settings", GroupOrder = 1)]
        public float interestRate { get; private set; } = 2.0f;

        [SettingPropertyBool("{=FOOD_DONATION_SECURITY_ENABLE}Food Donation Security Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 1)]
        public bool foodDonationSecurityIncrease { get; private set; } = true;

        [SettingPropertyBool("{=FOOD_DONATION_FOOD_ENABLE}Food Donation Food Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 1)]
        public bool foodDonationFoodIncrease { get; private set; } = true;

        [SettingPropertyBool("{=FOOD_DONATION_RELATION_ENABLE}Food Donation Relation Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 1)]
        public bool foodDonationRelationIncrease { get; private set; } = true;

        [SettingPropertyBool("{=FOOD_DONATION_INFLUENCE_ENABLE}Food Donation Influence Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 1)]
        public bool foodDonationInfluenceIncrease { get; private set; } = true;
    }
}
