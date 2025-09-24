using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace StorageVillage.src.model {
    class StoragePartyLimitModel : DefaultPartySizeLimitModel {
        private const int MAX_OBJECT_NUMBER = 99999;
        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false) {
            ExplainedNumber baseResult = base.GetPartyMemberSizeLimit(party, includeDescriptions);

            if (!(party is null) && party.Name.Equals(new TextObject("Troops Storage"))) {
                return new ExplainedNumber(MAX_OBJECT_NUMBER);
            }

            return baseResult;
        }

        public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false) {
            ExplainedNumber baseResult = base.GetPartyPrisonerSizeLimit(party, includeDescriptions);

            if (!(party is null) && party.Name.Equals(new TextObject("Troops Storage"))) {
                return new ExplainedNumber(MAX_OBJECT_NUMBER);
            }

            return baseResult;
        }

    }
}
