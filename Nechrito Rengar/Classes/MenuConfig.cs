using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Nechrito_Rengar.Classes
{
    class MenuConfig
    {
        public static Menu Config;

        public static string MenuName = "Nechrito Rengar";
        public static void LoadMenu()
        {
            Config = MainMenu.AddMenu(MenuName, MenuName);
            Config.AddLabel("Nechrito Rengar ported by Rexy");
            Config.AddLabel("1 => Q Mode || 2 => APMode");
            Config.Add("Combo.Mode", new Slider("Combo Mode", 1, 1, 2));
            Config.Add("Burst.Active", new KeyBind("Burst Mode Switcher", false, KeyBind.BindTypes.PressToggle, 'T'));
            Config.Add("Triple.Active", new KeyBind("TripleQ Switcher", false, KeyBind.BindTypes.PressToggle, 'G'));
            Config.Add("AutoHp.Active", new CheckBox("Auto Hp Active"));
            Config.Add("AutoHp", new Slider("Auto Hp Value", 20));
            Config.Add("DrawCurrentMode", new CheckBox("Draw Current Mode"));
        }

        public static bool BurstModeActive
        {
            get { return Config["Burst.Active"].Cast<KeyBind>().CurrentValue; }
        }

        public static bool TripleActive
        {
            get { return Config["Triple.Active"].Cast<KeyBind>().CurrentValue; }
        }

        public static bool Passive
        {
            get
            {
                return Config["Passive"].Cast<KeyBind>().CurrentValue;
            }
        }

        public static int ComboModeValue
        {
            get { return Config["Combo.Mode"].Cast<Slider>().CurrentValue; }
        }

        public static string ComboModeString
        {
            get
            {
                if (ComboModeValue == 1)
                {
                    return "Combo";
                }
                if (ComboModeValue == 2)
                {
                    return "Burst";
                }
                if (ComboModeValue == 3)
                {
                    return "APCombo";
                }
                return "Yok";
            }
        }

        public static bool Dind
        {
            get
            {
                return Config["dind"].Cast<CheckBox>().CurrentValue;
            }
        }

        public static bool Qaa
        {
            get
            {
                return Config["QAA"].Cast<CheckBox>().CurrentValue;
            }
        }


        public static bool GankCombo
        {
            get
            {
                return Config["GankCombo"].Cast<CheckBox>().CurrentValue;
            }
        }

        public static bool OneShot
        {
            get
            {
                return Config["OneShot"].Cast<CheckBox>().CurrentValue;
            }
        }

    }
}
