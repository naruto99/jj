using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace SephKhazix
{
    class KhazixMenu
    {
        internal Menu Menu;

        public KhazixMenu()
        {
            Menu = MainMenu.AddMenu("SephKhazix", "SephKhazix");


            //Harass
            Menu.AddLabel("Harass");
            Menu.AddBool("UseQHarass", "Use Q");
            Menu.AddBool("UseWHarass", "Use W");
            Menu.AddBool("Harass.AutoWI", "Auto-W immobile");
            Menu.AddBool("Harass.AutoWD", "Auto W");
            Menu.AddKeyBind("Harass.Key", "Harass key", "H".ToCharArray()[0], KeyBind.BindTypes.PressToggle);
            Menu.AddBool("Harass.InMixed", "Harass in Mixed Mode", false);

            //Combo
            Menu.AddLabel("Combo");
            Menu.AddBool("UseQCombo", "Use Q");
            Menu.AddBool("UseWCombo", "Use W");
            Menu.AddBool("UseECombo", "Use E");
            Menu.AddBool("UseEGapclose", "Use E To Gapclose for Q");
            Menu.AddBool("UseEGapcloseW", "Use E To Gapclose For W");
            Menu.AddBool("UseRGapcloseW", "Use R after long gapcloses");
            Menu.AddBool("UseRCombo", "Use R");
            Menu.AddBool("UseItems", "Use Items");

            //Farm
            Menu.AddLabel("Farm");
            Menu.AddBool("UseQFarm", "Use Q");
            Menu.AddBool("UseEFarm", "Use E");
            Menu.AddBool("UseWFarm", "Use W");
            Menu.AddSlider("Farm.WHealth", "Health % to use W", 80);
            Menu.AddBool("UseItemsFarm", "Use Items");

            //Kill Steal
            Menu.AddLabel("KillSteal");
            Menu.AddBool("Kson", "Use KillSteal");
            Menu.AddBool("UseQKs", "Use Q");
            Menu.AddBool("UseWKs", "Use W");
            Menu.AddBool("UseEKs", "Use E");
            Menu.AddBool("Ksbypass", "Bypass safety checks for E KS", false);
            Menu.AddBool("UseEQKs", "Use EQ in KS");
            Menu.AddBool("UseEWKs", "Use EW in KS");
            Menu.AddBool("UseTiamatKs", "Use items");
            Menu.AddSlider("Edelay", "E Delay (ms)", 0, 0, 300);
            Menu.AddBool("UseIgnite", "Use Ignite");

            Menu.AddLabel("Safety Menu");
            Menu.AddBool("Safety.Enabled", "Enable Safety Checks");
            Menu.AddKeyBind("Safety.Override", "Safety Override Key", 'T', KeyBind.BindTypes.HoldActive);
            Menu.AddBool("Safety.autoescape", "Use E to get out when low");
            Menu.AddBool("Safety.CountCheck", "Min Ally ratio to Enemies to jump");
            Menu.Add("Safety.Ratio",new Slider("Ally:Enemy Ratio (/5)",1,0,5));
            Menu.AddBool("Safety.TowerJump", "Avoid Tower Diving");
            Menu.AddSlider("Safety.MinHealth", "Healthy %", 15);
            Menu.AddBool("Safety.noaainult", "No Autos while Stealth", false);

            //Double Jump
            Menu.AddLabel("Double Jump");
            Menu.AddBool("djumpenabled", "Enabled");
            Menu.AddSlider("JEDelay", "Delay between jumps", 250, 250, 500);
            Menu.AddSlider("jumpmode", "Jump Mode",0,0,1);
            Menu.AddBool("save", "Save Double Jump Abilities");
            Menu.AddBool("noauto", "Wait for Q instead of autos");
            Menu.AddBool("jcursor", "Jump to Cursor (true) or false for script logic");
            Menu.AddBool("secondjump", "Do second Jump");
            Menu.AddBool("jcursor2", "Second Jump to Cursor (true) or false for script logic");
            Menu.AddBool("jumpdrawings", "Enable Jump Drawinsg");


            //Drawings
            Menu.AddLabel("Drawings");
            Menu.AddBool("Drawings.Disable", "Disable all");
            Menu.AddBool("DrawQ", "Draw Q");
            Menu.AddBool("DrawW", "Draw W");
            Menu.AddBool("DrawE", "Draw E");
            Menu.AddBool("Debugon","Debug On");
        }

        internal bool GetBool(string name)
        {
            return Menu[name].Cast<CheckBox>().CurrentValue;
        }

        internal bool GetKeyBind(string name)
        {
            return Menu[name].Cast<KeyBind>().CurrentValue;
        }

        internal float GetSliderFloat(string name)
        {
            return Menu[name].Cast<Slider>().CurrentValue;
        }

        internal int GetSlider(string name)
        {
            return Menu[name].Cast<Slider>().CurrentValue;
        }
    }
}
