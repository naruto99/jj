﻿// <copyright file="Bootstrap.cs" company="LeagueSharp">
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
    using System.Globalization;
    using System.Security.Permissions;
    using System.Threading;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK;
    using EloBuddy;
    using LeagueSharp.SDK.Core.Utils;

    /// <summary>
    ///     Bootstrap is an initialization pointer for the AppDomainManager to initialize the library correctly once loaded in
    ///     game.
    /// </summary>
    public class Bootstrap
    {
        #region Static Fields

        /// <summary>
        ///     Indicates whether the bootstrap has been initialized.
        /// </summary>
        private static bool initialized;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     External attachment handle for the Sandbox to load in the SDK library.
        /// </summary>
        /// <param name="args">
        ///     The additional arguments the loader passes.
        /// </param>
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static void Init(string[] args)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            // Initial notification.
            Logging.Write()(LogLevel.Info, "[-- SDK Bootstrap Loading --]");

            // Load Resource Content.
            ResourceLoader.Initialize();
            Logging.Write()(LogLevel.Info, "[SDK Bootstrap] Resources Initialized.");

            // Load GameObjects.
            GameObjects.Initialize();
            Logging.Write()(LogLevel.Info, "[SDK Bootstrap] GameObjects Initialized.");

            // Load Damages.
            Damage.Initialize();
            Logging.Write()(LogLevel.Info, "[SDK Bootstrap] Damage Library Initialized.");

            // Final notification.
            Logging.Write()(LogLevel.Info, "[-- SDK Bootstrap Loading --]");
        }

        #endregion
    }
}
