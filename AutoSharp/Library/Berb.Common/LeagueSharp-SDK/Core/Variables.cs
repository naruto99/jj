﻿// <copyright file="Variables.cs" company="LeagueSharp">
//    Copyright (c) 2015 LeagueSharp.
// 
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
// 
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
// 
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/
// </copyright>

namespace LeagueSharp.SDK
{
    using System;
    using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;
using EloBuddy;

    /// <summary>
    ///     Variables of the LeagueSharp development kit.
    /// </summary>
    public class Variables
    {
        #region Static Fields

        /// <summary>
        ///     The game version.
        /// </summary>
        public static readonly System.Version GameVersion = new System.Version("6.7");

        /// <summary>
        ///     The kit version.
        /// </summary>
        public static readonly System.Version KitVersion = typeof(Bootstrap).Assembly.GetName().Version;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the Orbwalker instance.
        /// </summary>
        //public static Orbwalker Orbwalker { get; internal set; }

        /// <summary>
        ///     Gets the TargetSelector instance.
        /// </summary>
        //public static TargetSelector TargetSelector { get; internal set; }

        /// <summary>
        ///     Gets the TickCount based on the game runtime clock.
        /// </summary>
        public static int TickCount => (int)(Game.Time * 1000);

        #endregion

        #region Properties
        

        #endregion
    }
}
