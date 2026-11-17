using InventorySystem.Items.Firearms;
using InventorySystem.Items.MicroHID.Modules;
using LabApi.Features.Wrappers;
using Scp268 = InventorySystem.Items.Usables.Scp268;

namespace Sunrise.Features.AntiWallhack.ForcedVisibility;

internal static class ItemVisibilityHelper
{
    public static float GetVisibilityRange(Item? item)
    {
        switch (item)
        {
            case null:
            {
                return 0;
            }
            case FirearmItem firearm:
            {
                if (firearm.Base is ParticleDisruptor disruptor)
                    return disruptor.AllowHolster ? 0 : 100;

                if (firearm.IsReloading)
                    return 11.5f;
                else
                    return 0;
            }
            case MicroHIDItem microHid:
            {
                return microHid.Phase switch
                {
                    MicroHidPhase.Standby => 0,
                    MicroHidPhase.WindingUp or MicroHidPhase.WoundUpSustain or MicroHidPhase.WindingDown => 15,
                    MicroHidPhase.Firing => 45,
                    _ => 0,
                };
            }
            case ConsumableItem consumable:
            {
                // medkit 14.5
                // painkillers 14.5
                // cola 14.5, candy 14.5
                // adrenaline 14.5,
                // steroids 14.5
                return consumable.IsUsing ? 14.5f : 0;
            }
            case RadioItem radio:
            {
                // radio 11 when receiving
                return radio.Base._enabled ? 11 : 0;
            }
            // coin 5
            // 268 5
            case CoinItem coin:
            {
                return coin.Base._lastUseSw.ElapsedMilliseconds < 600 ? 5 : 0;
            }
            case UsableItem:
            {
                return item.Base switch
                {
                    Scp268 { IsUsing: true } => 5,
                    _ => 0,
                };
            }
            case ThrowableItem throwable:
            {
                return !throwable.Base.AllowHolster ? 5 : 0;
            }
            default:
            {
                return 0;
            }
        }
    }
}
