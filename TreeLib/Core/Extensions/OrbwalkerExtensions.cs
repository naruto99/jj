using EloBuddy.SDK;

namespace TreeLib.Extensions
{
    public static class OrbwalkerExtensions
    {
        public static bool IsActive(this Orbwalker.ActiveModes mode)
        {
            return mode != Orbwalker.ActiveModes.None;
        }

        public static bool IsComboMode(this Orbwalker.ActiveModes mode)
        {
            return mode.Equals(Orbwalker.ActiveModes.Combo) || mode.Equals(Orbwalker.ActiveModes.Harass);
        }

        public static bool IsFarmMode(this Orbwalker.ActiveModes mode)
        {
            return mode.Equals(Orbwalker.ActiveModes.LastHit) || mode.Equals(Orbwalker.ActiveModes.LaneClear);
        }

        public static string GetModeString(this Orbwalker.ActiveModes mode)
        {
            return mode.Equals(Orbwalker.ActiveModes.Harass) ? "Harass" : mode.ToString();
        }
    }
}