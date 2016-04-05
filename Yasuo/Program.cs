using System;
using System.Linq;
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

        #endregion

        #region Methods

        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Yasuo.Yasuoo();
        }

        #endregion
    }
}