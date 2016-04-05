using System;
using System.Linq;
using BrianSharp.Plugin;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace BrianSharp
{
    using ItemData = Item;

    internal class Program
    {
        #region Public Properties

        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        #endregion

        #region Static Fields

        public static SpellSlot Flash, Smite, Ignite;

        public static Menu Menu;

        public static Item Tiamat, Hydra, Youmuu, Zhonya, Seraph, Sheen, Iceborn, Trinity;

        #endregion

        #region Methods

        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Tiamat = new Item(ItemId.Tiamat_Melee_Only);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only);
            Youmuu = new Item(ItemId.Youmuus_Ghostblade);
            Zhonya = new Item(ItemId.Zhonyas_Hourglass);
            Seraph = new Item(ItemId.Seraphs_Embrace);
            Sheen = new Item(ItemId.Sheen);
            Iceborn = new Item(ItemId.Iceborn_Gauntlet);
            Trinity = new Item(ItemId.Trinity_Force);
            Flash = Player.GetSpellSlotFromName("summonerflash");
            foreach (var spell in
                Player.Spellbook.Spells.Where(
                    i =>
                        i.Name.ToLower().Contains("smite")
                        && (i.Slot == SpellSlot.Summoner1 || i.Slot == SpellSlot.Summoner2)))
            {
                Smite = spell.Slot;
            }
            Ignite = Player.GetSpellSlotFromName("summonerdot");

            Yasuo.Yasuoo();
        }

        #endregion
    }
}