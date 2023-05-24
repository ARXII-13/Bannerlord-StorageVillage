using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace StorageVillage.src.model
{
    class StoragePartyLimitModel : DefaultPartySizeLimitModel
    {
        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.GetPartyMemberSizeLimit(party, includeDescriptions);

            if (!(party is null) && party.Name.Equals(new TextObject("Troops Storage")))
            {
                return new ExplainedNumber(99999);
            }

            return baseResult;
        }

        public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.GetPartyPrisonerSizeLimit(party, includeDescriptions);

            if (!(party is null) && party.Name.Equals(new TextObject("Troops Storage")))
            {
                return new ExplainedNumber(99999);
            }

            return baseResult;
        }

    }
}
