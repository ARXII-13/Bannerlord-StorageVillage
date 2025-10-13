using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.PerSave;

namespace StorageVillage.src.setting {
    public class StorageVillageSettings : AttributePerSaveSettings<StorageVillageSettings> {
        public override string FolderName => "StorageVillage";

        public override string Id => "StorageVillage";
        public override string DisplayName => $"StorageVillage {typeof(StorageVillageSettings).Assembly.GetName().Version.ToString(3)}";

        [SettingPropertyFloatingInteger("{=BANK_INTEREST_RATE_SETTING}Bank Weekly Interest Rate", 0f, 10.0f, HintText = "{=BANK_INTEREST_RATE_SETTING_HINT}Set the weekly interest rate in percentage", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=BANK_SETTINGS}Bank Settings", GroupOrder = 0)]
        public float interestRate { get; private set; } = 2.0f;

        [SettingPropertyBool("{=FOOD_DONATION_SECURITY_ENABLE}Food Donation Security Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 0)]
        public bool foodDonationSecurityIncrease { get; private set; } = true;

        [SettingPropertyBool("{=FOOD_DONATION_FOOD_ENABLE}Food Donation Food Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 0)]
        public bool foodDonationFoodIncrease { get; private set; } = true;

        [SettingPropertyBool("{=FOOD_DONATION_RELATION_ENABLE}Food Donation Relation Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 0)]
        public bool foodDonationRelationIncrease { get; private set; } = true;

        [SettingPropertyBool("{=FOOD_DONATION_INFLUENCE_ENABLE}Food Donation Influence Increase enabled", Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=FOOD_DONATION_SETTING}Food Donation Settings", GroupOrder = 0)]
        public bool foodDonationInfluenceIncrease { get; private set; } = true;
    }
}
