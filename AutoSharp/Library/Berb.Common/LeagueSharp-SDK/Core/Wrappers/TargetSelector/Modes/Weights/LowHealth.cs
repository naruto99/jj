﻿// <copyright file="LowHealth.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.Modes.Weights
{using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK;
using EloBuddy;
    /// <summary>
    ///     Low Health
    /// </summary>
    public class LowHealth : IWeightItem
    {
        #region Public Properties

        /// <inheritdoc />
        public int DefaultWeight => 20;

        /// <inheritdoc />
        public string DisplayName => "Low Health";

        /// <inheritdoc />
        public bool Inverted => true;

        /// <inheritdoc />
        public string Name => "low-health";

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public float GetValue(AIHeroClient hero) => hero.Health;

        #endregion
    }
}