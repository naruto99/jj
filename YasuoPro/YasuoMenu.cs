using System.Linq;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace YasuoPro
{
    internal static class YasuoMenu
    {
        internal static Menu Config;

        public static void Init()
        {
            Config = MainMenu.AddMenu("YasuoPro", "YasuoPro");
            Config.AddLabel("Ported by Rexy");

            Config.AddLabel("Combo Items");

            Extensions.AddBool("Items.Enabled", "Use Items");
            Extensions.AddBool("Items.UseTIA", "Use Tiamat");
            Extensions.AddBool("Items.UseHDR", "Use Hydra");
            Extensions.AddBool("Items.UseBRK", "Use BORK");
            Extensions.AddBool("Items.UseBLG", "Use Bilgewater");
            Extensions.AddBool("Items.UseYMU", "Use Youmu");

            Config.AddLabel("Combo");

            Extensions.AddBool("Combo.UseQ", "Use Q");
            Extensions.AddBool("Combo.UseQ2", "Use Q2");
            Extensions.AddBool("Combo.StackQ", "Stack Q if not in Range");
            Extensions.AddBool("Combo.UseW", "Use W");
            Extensions.AddBool("Combo.UseE", "Use E");
            Extensions.AddBool("Combo.ETower", "Use E under Tower", false);
            Extensions.AddBool("Combo.EAdvanced", "Predict E position with Waypoints");
            Extensions.AddBool("Combo.NoQ2Dash", "Dont Q2 while dashing", false);

            Config.AddLabel("BlackList");

            foreach (var hero in EntityManager.Heroes.Enemies)
            {
                Extensions.AddBool("ult" + hero.ChampionName, "Ult " + hero.ChampionName);
            }

            Config.AddLabel("0 => Lowest Health || 1 => TS Priority ||2 => Most enemies");
            Extensions.AddSlider("Combo.UltMode", "Ult Prioritization", 0, 0, 2);
            Extensions.AddSlider("Combo.knockupremainingpct", "Knockup Remaining % for Ult", 95, 40, 100);
            Extensions.AddBool("Combo.UseR", "Use R");
            Extensions.AddBool("Combo.UltTower", "Ult under Tower", false);
            Extensions.AddBool("Combo.UltOnlyKillable", "Ult only Killable", false);
            Extensions.AddBool("Combo.RPriority", "Ult if priority 5 target is knocked up", true);
            Extensions.AddSlider("Combo.RMinHit", "Min Enemies for Ult", 1, 1, 5);
            Extensions.AddBool("Combo.OnlyifMin", "Only Ult if minimum enemies met", false);
            Extensions.AddSlider("Combo.MinHealthUlt", "Minimum health to Ult %", 0, 0, 100);
            Extensions.AddBool("Combo.UseIgnite", "Use Ignite");
            Config.AddSeparator();

            Config.AddLabel("Harass");
            Extensions.AddKeyBind("Harass.KB", "Harass Key", 'H', KeyBind.BindTypes.PressToggle);
            Extensions.AddBool("Harass.InMixed", "Harass in Mixed Mode", false);
            Extensions.AddBool("Harass.UseQ", "Use Q");
            Extensions.AddBool("Harass.UseQ2", "Use Q2");
            Extensions.AddBool("Harass.UseE", "Use E", false);
            Extensions.AddBool("Harass.UseEMinion", "Use E Minions", false);
            Config.AddSeparator();

            Config.AddLabel("Farm");
            Extensions.AddBool("Farm.UseQ", "Use Q");
            Extensions.AddBool("Farm.UseQ2", "Use Q - Tornado");
            Extensions.AddSlider("Farm.Qcount", "Minions for Q (Tornado)", 1, 1, 10);
            Extensions.AddBool("Farm.UseE", "Use E");
            Config.AddSeparator();

            Config.AddLabel("WaveClear");
            Extensions.AddBool("Waveclear.UseItems", "Use Items");
            Extensions.AddSlider("Waveclear.MinCountHDR", "Minion count for Cleave", 2, 1, 10);
            Extensions.AddSlider("Waveclear.MinCountYOU", "Minion count for Youmu", 2, 1, 10);
            Extensions.AddBool("Waveclear.UseTIA", "Use Tiamat");
            Extensions.AddBool("Waveclear.UseHDR", "Use Hydra");
            Extensions.AddBool("Waveclear.UseYMU", "Use Youmu", false);

            Extensions.AddBool("Waveclear.UseQ", "Use Q");
            Extensions.AddBool("Waveclear.UseQ2", "Use Q - Tornado");
            Extensions.AddSlider("Waveclear.Qcount", "Minions for Q (Tornado)", 1, 1, 10);
            Extensions.AddBool("Waveclear.UseE", "Use E");
            Extensions.AddBool("Waveclear.ETower", "Use E under Tower", false);
            Extensions.AddBool("Waveclear.UseENK", "Use E even if not killable");
            Extensions.AddBool("Waveclear.Smart", "Smart Waveclear");
            Config.AddSeparator();

            Config.AddLabel("KillSteal");
            Extensions.AddBool("Killsteal.Enabled", "KillSteal");
            Extensions.AddBool("Killsteal.UseQ", "Use Q");
            Extensions.AddBool("Killsteal.UseE", "Use E");
            Extensions.AddBool("Killsteal.UseR", "Use R");
            Extensions.AddBool("Killsteal.UseIgnite", "Use Ignite");
            Extensions.AddBool("Killsteal.UseItems", "Use Items");
            Config.AddSeparator();

            Config.AddLabel("Evade");
            Config.AddLabel("Targetted Spells");

            foreach (
                var spell in
                    TargettedDanger.spellList.Where(
                        x => EntityManager.Heroes.Enemies.Any(e => e.CharData.BaseSkinName == x.championName)))
            {
                Extensions.AddBool("enabled." + spell.spellName, spell.spellName, true);
                Extensions.AddSlider("enabled." + spell.spellName + ".delay", spell.spellName + " Delay", 0, 0, 1000);
            }

            foreach (
                var spell in
                    TargettedDanger.spellList.Where(
                        x => EntityManager.Heroes.Enemies.Any(e => x.championName == "Baron")))
            {
                Config.AddLabel(spell.championName + " [Experimental] ");
                Extensions.AddBool("enabled." + spell.spellName, spell.spellName, true);
            }

            Config.AddSeparator();

            Config.AddLabel("Flee Settings");
            Config.AddLabel("0 => To Nexus || 1 => To Allies || 2 => To Cursor");
            Extensions.AddSlider("Flee.Mode", "Flee Mode", 0, 0, 2);
            Extensions.AddBool("Flee.StackQ", "Stack Q during Flee");
            Extensions.AddBool("Flee.UseQ2", "Use Tornado", false);

            Config.AddSeparator();

            Extensions.AddBool("Evade.Enabled", "Evade Enabled");
            Extensions.AddBool("Evade.OnlyDangerous", "Evade only Dangerous", false);
            Extensions.AddBool("Evade.FOW", "Dodge FOW Skills");
            Extensions.AddSlider("Evade.MinDangerLevelWW", "Min Danger Level WindWall", 1, 1, 5);
            Extensions.AddSlider("Evade.MinDangerLevelE", "Min Danger Level Dash", 1, 1, 5);
            Extensions.AddBool("Evade.WTS", "Windwall Targetted");
            Extensions.AddBool("Evade.WSS", "Windwall Skillshots");
            Extensions.AddBool("Evade.UseW", "Evade with Windwall");
            Extensions.AddBool("Evade.UseE", "Evade with E");
            Extensions.AddSlider("Evade.Delay", "Windwall Base Delay", 0, 0, 1000);

            Config.AddSeparator();

            Config.AddLabel("Misc");
            Extensions.AddBool("Misc.SafeE", "Safety Check for E");
            Extensions.AddBool("Misc.AutoStackQ", "Auto Stack Q", false);
            Extensions.AddBool("Misc.AutoR", "Auto Ultimate");
            Extensions.AddSlider("Misc.RMinHit", "Min Enemies for Autoult", 1, 1, 5);
            Extensions.AddKeyBind("Misc.TowerDive", "Tower Dive Key", 'T', KeyBind.BindTypes.HoldActive);
            Config.AddLabel("0 => Low || 1 => Medium || 2 => High 3 => Very High");
            Extensions.AddSlider("Hitchance.Q", "Q Hitchance", 2, 0, 3);
            Extensions.AddSlider("Misc.Healthy", "Healthy Amount HP", 5, 0, 100);
            Extensions.AddBool("Misc.AG", "Use Q (Tornado) on Gapcloser");
            Extensions.AddBool("Misc.Interrupter", "Use Q (Tornado) to Interrupt");
            Extensions.AddBool("Misc.Walljump", "Use Walljump", false);
            Extensions.AddBool("Misc.Debug", "Debug", false);

            Config.AddSeparator();

            Config.AddLabel("Drawings");
            Extensions.AddBool("Drawing.Disable", "Disable Drawings", true);
            Extensions.AddBool("Drawing.DrawQ", "Draw Q");
            Extensions.AddBool("Drawing.DrawE", "Draw E");
            Extensions.AddBool("Drawing.DrawR", "Draw R");
            Extensions.AddBool("Drawing.SS", "Draw Skillshot Drawings", false);
        }
    }
}