using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem.Inventory;

public static class InventoryLogicExtension
{
    private static readonly ConditionalWeakTable<InventoryLogic, InventoryData> _data = new ConditionalWeakTable<InventoryLogic, InventoryData>();

    // Class holding custom fields
    private class InventoryData
    {
        public bool IsDonateFoodToSettlement;
    }

    public static void SetDonateFoodFlag(this InventoryLogic logic, bool value)
    {
        var data = _data.GetOrCreateValue(logic);
        data.IsDonateFoodToSettlement = value;
    }

    public static bool SetDonateFoodFlag(this InventoryLogic logic)
    {
        if (_data.TryGetValue(logic, out var data))
        {
            return data.IsDonateFoodToSettlement;
        }
        return false;
    }

    public static void ClearFlagsData(this InventoryLogic logic)
    {
        if (logic == null) return;
        _data.Remove(logic);
    }
}