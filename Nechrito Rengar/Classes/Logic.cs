using EloBuddy;
using EloBuddy.SDK;
using System.Linq;
using ItemData = EloBuddy.SDK.Item;

namespace Nechrito_Rengar.Classes
{
    class Logic
    {
        public static AIHeroClient Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }
        private static readonly int[] BlueSmite = { 3706, 1400, 1401, 1402, 1403 };

        private static readonly int[] RedSmite = { 3715, 1415, 1414, 1413, 1412 };
        protected static SpellSlot Smite;
        public static void CastHydra()
        {
            if (ItemData.CanUseItem(ItemId.Ravenous_Hydra_Melee_Only))
                ItemData.UseItem(ItemId.Ravenous_Hydra_Melee_Only);
            else if (ItemData.CanUseItem(ItemId.Tiamat_Melee_Only))
                ItemData.UseItem(ItemId.Tiamat_Melee_Only);
        }
        public static void CastYoumoo()
        {
            if (ItemData.CanUseItem(ItemId.Youmuus_Ghostblade)) ItemData.UseItem(ItemId.Youmuus_Ghostblade);
        }

        public static bool HasItem()
        {
            return ItemData.CanUseItem(ItemId.Tiamat_Melee_Only) ||
                   ItemData.CanUseItem(ItemId.Ravenous_Hydra_Melee_Only);
        }

        // Thanks jQuery for letting me use this! Great guy.
        protected static void SmiteCombo()
        {
            if (BlueSmite.Any(id => Item.HasItem(id)))
            {
                Smite = Player.GetSpellSlotFromName("s5_summonersmiteplayerganker");
                return;
            }

            if (RedSmite.Any(id => Item.HasItem(id)))
            {
                Smite = Player.GetSpellSlotFromName("s5_summonersmiteduel");
                return;
            }

            Smite = Player.GetSpellSlotFromName("summonersmite");
        }
        public static bool RengarHasUlti
        {
            get { return Player.HasBuff("RengarR"); }
        }
    }
}
