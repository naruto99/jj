using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using TreeLib.Managers;
using TreeLib.SpellData;

namespace TreeLib.Core
{
    public static class Bootstrap
    {
        public static Menu Menu;
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            Loading.OnLoadingComplete += args =>
            {
                Menu = MainMenu.AddMenu("TreeLib", "TreeLib");
                SpellManager.Initialize();
                Evade.Init();
            };
        }
    }
}