using System;

namespace LeagueSharp.Common
{
    /// <summary>
    ///     The menu keybind.
    /// </summary>
    [Serializable]
    public struct KeyBind
    {
        #region Fields

        /// <summary>
        ///     Indicates whether the keybind is active.
        /// </summary>
        public bool Active;

        /// <summary>
        ///     The key.
        /// </summary>
        public uint Key;

        /// <summary>
        ///     The key bind type.
        /// </summary>
        public KeyBindType Type;

        /// <summary>
        ///     The key.
        /// </summary>
        public uint SecondaryKey;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBind" /> struct.
        /// </summary>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value.
        /// </param>
        public KeyBind(uint key, KeyBindType type, bool defaultValue = false)
        {
            Key = key;
            SecondaryKey = 0;
            Type = type;
            Active = defaultValue;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBind" /> struct.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="secondaryKey">The secondary key.</param>
        /// <param name="type">The type.</param>
        /// <param name="defaultValue">The default value.</param>
        public KeyBind(uint key, uint secondaryKey, KeyBindType type, bool defaultValue = false)
        {
            Key = key;
            SecondaryKey = secondaryKey;
            Type = type;
            Active = defaultValue;
        }

        #endregion
    }
}