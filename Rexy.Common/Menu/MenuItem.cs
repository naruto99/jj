using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using EloBuddy;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
using Rectangle = SharpDX.Rectangle;

namespace LeagueSharp.Common
{
    /// <summary>
    ///     The menu item.
    /// </summary>
    public class MenuItem
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Menu" /> class.
        /// </summary>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <param name="displayName">
        ///     The display name.
        /// </param>
        /// <param name="championUnique">
        ///     Indicates whether the menu item is champion unique.
        /// </param>
        public MenuItem(string name, string displayName, bool championUnique = false)
        {
            if (championUnique)
            {
                name = ObjectManager.Player.ChampionName + name;
            }

            Name = name;
            DisplayName = displayName;
            FontStyle = FontStyle.Regular;
            FontColor = Color.White;
            ShowItem = true;
            Tag = 0;
            configName = Assembly.GetCallingAssembly().GetName().Name
                         + Assembly.GetCallingAssembly().GetType().GUID;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the tooltip duration.
        /// </summary>
        public int TooltipDuration
        {
            get { return CommonMenu.Instance.Item("LeagueSharp.Common.TooltipDuration").GetValue<Slider>().Value; }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The value changed event.
        /// </summary>
        public event EventHandler<OnValueChangeEventArgs> ValueChanged;

        #endregion

        #region Fields

        /// <summary>
        ///     The display name.
        /// </summary>
        public string DisplayName;

        /// <summary>
        ///     The font color.
        /// </summary>
        public ColorBGRA FontColor;

        /// <summary>
        ///     The font style.
        /// </summary>
        public FontStyle FontStyle;

        /// <summary>
        ///     The menu font size.
        /// </summary>
        public int MenuFontSize;

        /// <summary>
        ///     The name.
        /// </summary>
        public string Name;

        /// <summary>
        ///     The parent.
        /// </summary>
        public Menu Parent;

        /// <summary>
        ///     Indicates whether to show the item/
        /// </summary>
        public bool ShowItem;

        /// <summary>
        ///     The tag.
        /// </summary>
        public int Tag;

        /// <summary>
        ///     The tooltip.
        /// </summary>
        public string Tooltip;

        /// <summary>
        ///     The tooltip color.
        /// </summary>
        public Color TooltipColor;

        /// <summary>
        ///     Indicates whether the menu item is drawing the tooltip.
        /// </summary>
        internal bool DrawingTooltip;

        /// <summary>
        ///     Indicates whether the menu item is being interacted with.
        /// </summary>
        internal bool Interacting;

        /// <summary>
        ///     Indicates whether the value was set.
        /// </summary>
        public bool ValueSet;

        /// <summary>
        ///     The value type.
        /// </summary>
        internal MenuValueType ValueType;

        /// <summary>
        ///     The configuration name.
        /// </summary>
        private readonly string configName;

        /// <summary>
        ///     Indicates whether the menu item won't save.
        /// </summary>
        private bool dontSave;

        /// <summary>
        ///     Indicates whether the menu item is shared.
        /// </summary>
        private bool isShared;

        /// <summary>
        ///     Indicates whether the menu item is visible.
        /// </summary>
        private bool isVisible;

        /// <summary>
        ///     The serialized data.
        /// </summary>
        private byte[] serialized;

        /// <summary>
        ///     The value.
        /// </summary>
        private object value;

        /// <summary>
        ///     The stage of the KeybindSetting
        /// </summary>
        internal KeybindSetStage KeybindSettingStage = KeybindSetStage.NotSetting;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the height.
        /// </summary>
        internal int Height
        {
            get { return MenuSettings.MenuItemHeight; }
        }

        /// <summary>
        ///     Gets the item base position.
        /// </summary>
        internal Vector2 MyBasePosition
        {
            get
            {
                if (Parent == null)
                {
                    return MenuSettings.BasePosition;
                }

                return Parent.MyBasePosition;
            }
        }

        /// <summary>
        ///     Gets the needed width.
        /// </summary>
        internal int NeededWidth
        {
            get
            {
                return MenuDrawHelper.Font.MeasureText(MultiLanguage._(DisplayName)).Width + Height*2 + 10
                       + ((ValueType == MenuValueType.StringList)
                           ? GetValue<StringList>()
                               .SList.Select(v => MenuDrawHelper.Font.MeasureText(v).Width + 25)
                               .Concat(new[] {0})
                               .Max()
                           : (ValueType == MenuValueType.KeyBind)
                               ? GetValue<KeyBind>().SecondaryKey == 0
                                   ? MenuDrawHelper.Font.MeasureText(
                                       " [" + Utils.KeyToText(GetValue<KeyBind>().Key) + "]").Width
                                   : MenuDrawHelper.Font.MeasureText(" [" + Utils.KeyToText(GetValue<KeyBind>().Key) + "]")
                                       .Width
                                     +
                                     MenuDrawHelper.Font.MeasureText(" [" +
                                                                     Utils.KeyToText(GetValue<KeyBind>().SecondaryKey) +
                                                                     "]").Width
                                     +
                                     MenuDrawHelper.Font.MeasureText(" [" + Utils.KeyToText(GetValue<KeyBind>().Key) + "]")
                                         .Width/4
                               : 0);
            }
        }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        internal Vector2 Position
        {
            get
            {
                var xOffset = 0;

                if (Parent != null)
                {
                    xOffset = (int) (Parent.Position.X + Parent.Width);
                }

                return new Vector2(0, MyBasePosition.Y) + new Vector2(xOffset, 0)
                       + YLevel*new Vector2(0, MenuSettings.MenuItemHeight);
            }
        }

        /// <summary>
        ///     Gets the save file name.
        /// </summary>
        internal string SaveFileName
        {
            get { return isShared ? "SharedConfig" : configName; }
        }

        /// <summary>
        ///     Gets the save key.
        /// </summary>
        internal string SaveKey
        {
            get { return Utils.Md5Hash("v3" + DisplayName + Name); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the item is visible.
        /// </summary>
        internal bool Visible
        {
            get { return MenuSettings.DrawMenu && isVisible && ShowItem; }

            set { isVisible = value; }
        }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        internal int Width
        {
            get { return Parent != null ? Parent.ChildrenMenuWidth : MenuSettings.MenuItemWidth; }
        }

        /// <summary>
        ///     Gets the Y level.
        /// </summary>
        internal int YLevel
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }

                return Parent.YLevel + Parent.Children.Count
                       + Parent.Items.TakeWhile(test => test.Name != Name).Count(c => c.ShowItem);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Sets the menu item to not save.
        /// </summary>
        /// <returns>
        ///     The item instance.
        /// </returns>
        public MenuItem DontSave()
        {
            dontSave = true;
            return this;
        }

        /// <summary>
        ///     Gets the item value.
        /// </summary>
        /// <typeparam name="T">
        ///     The item type.
        /// </typeparam>
        /// <returns>
        ///     The value.
        /// </returns>
        public T GetValue<T>()
        {
            return (T) value;
        }

        /// <summary>
        ///     Gets a value indicating whether the item is active.
        /// </summary>
        /// <returns>
        ///     Value indicating whether the item is active.
        /// </returns>
        public bool IsActive()
        {
            switch (ValueType)
            {
                case MenuValueType.Boolean:
                    return GetValue<bool>();
                case MenuValueType.Circle:
                    return GetValue<Circle>().Active;
                case MenuValueType.KeyBind:
                    return GetValue<KeyBind>().Active;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Sets the font style.
        /// </summary>
        /// <param name="fontStyle">
        ///     The font style.
        /// </param>
        /// <param name="fontColor">
        ///     The font color.
        /// </param>
        /// <returns>
        ///     The item instance.
        /// </returns>
        public MenuItem SetFontStyle(FontStyle fontStyle = FontStyle.Regular, Color? fontColor = null)
        {
            FontStyle = fontStyle;
            FontColor = fontColor ?? Color.White;

            return this;
        }

        /// <summary>
        ///     Sets the menu item to be shared.
        /// </summary>
        /// <returns>
        ///     The item instance.
        /// </returns>
        public MenuItem SetShared()
        {
            isShared = true;

            return this;
        }

        /// <summary>
        ///     Sets the menu item tag.
        /// </summary>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        /// <returns>
        ///     The item instance.
        /// </returns>
        public MenuItem SetTag(int tag = 0)
        {
            Tag = tag;

            return this;
        }

        /// <summary>
        ///     Sets the tooltip.
        /// </summary>
        /// <param name="tooltip">
        ///     The tooltip string.
        /// </param>
        /// <param name="tooltipColor">
        ///     The tooltip color.
        /// </param>
        /// <returns>
        ///     The menu instance.
        /// </returns>
        public MenuItem SetTooltip(string tooltip, Color? tooltipColor = null)
        {
            Tooltip = tooltip;
            TooltipColor = tooltipColor ?? Color.White;

            return this;
        }

        /// <summary>
        ///     Sets the value.
        /// </summary>
        /// <typeparam name="T">
        ///     The value type.
        /// </typeparam>
        /// <param name="newValue">
        ///     The new value.
        /// </param>
        /// <returns>
        ///     The item instance.
        /// </returns>
        public MenuItem SetValue<T>(T newValue)
        {
            ValueType = MenuValueType.None;
            if (newValue.GetType().ToString().Contains("Boolean"))
            {
                ValueType = MenuValueType.Boolean;
            }
            else if (newValue.GetType().ToString().Contains("Slider"))
            {
                ValueType = MenuValueType.Slider;
            }
            else if (newValue.GetType().ToString().Contains("KeyBind"))
            {
                ValueType = MenuValueType.KeyBind;
            }
            else if (newValue.GetType().ToString().Contains("Int"))
            {
                ValueType = MenuValueType.Integer;
            }
            else if (newValue.GetType().ToString().Contains("Circle"))
            {
                ValueType = MenuValueType.Circle;
            }
            else if (newValue.GetType().ToString().Contains("StringList"))
            {
                ValueType = MenuValueType.StringList;
            }
            else if (newValue.GetType().ToString().Contains("Color"))
            {
                ValueType = MenuValueType.Color;
            }
            else
            {
                Console.WriteLine(@"CommonLibMenu: Data type not supported");
            }

            var readBytes = SavedSettings.GetSavedData(SaveFileName, SaveKey);
            var v = newValue;

            try
            {
                if (!ValueSet && readBytes != null)
                {
                    switch (ValueType)
                    {
                        case MenuValueType.KeyBind:
                            var savedKeyValue = (KeyBind) (object) Utils.Deserialize<T>(readBytes);
                            if (savedKeyValue.Type == KeyBindType.Press)
                            {
                                savedKeyValue.Active = false;
                            }

                            newValue = (T) (object) savedKeyValue;
                            break;
                        case MenuValueType.Circle:
                            var savedCircleValue = (Circle) (object) Utils.Deserialize<T>(readBytes);
                            var newCircleValue = (Circle) (object) newValue;
                            savedCircleValue.Radius = newCircleValue.Radius;
                            newValue = (T) (object) savedCircleValue;
                            break;
                        case MenuValueType.Slider:
                            var savedSliderValue = (Slider) (object) Utils.Deserialize<T>(readBytes);
                            var newSliderValue = (Slider) (object) newValue;
                            if (savedSliderValue.MinValue == newSliderValue.MinValue
                                && savedSliderValue.MaxValue == newSliderValue.MaxValue)
                            {
                                newValue = (T) (object) savedSliderValue;
                            }

                            break;
                        case MenuValueType.StringList:
                            var savedListValue = (StringList) (object) Utils.Deserialize<T>(readBytes);
                            var newListValue = (StringList) (object) newValue;
                            if (savedListValue.SList.SequenceEqual(newListValue.SList))
                            {
                                newValue = (T) (object) savedListValue;
                            }

                            break;
                        default:
                            newValue = Utils.Deserialize<T>(readBytes);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                newValue = v;
                Console.WriteLine(e);
            }

            OnValueChangeEventArgs valueChangedEvent = null;

            if (ValueSet)
            {
                var handler = ValueChanged;
                if (handler != null)
                {
                    valueChangedEvent = new OnValueChangeEventArgs(value, newValue);
                    handler(this, valueChangedEvent);
                }
            }

            if (valueChangedEvent != null)
            {
                if (valueChangedEvent.Process)
                {
                    value = newValue;
                }
            }
            else
            {
                value = newValue;
            }

            ValueSet = true;
            serialized = Utils.Serialize(value);
            return this;
        }

        /// <summary>
        ///     Shows the item.
        /// </summary>
        /// <param name="showItem">
        ///     Indicates whether to show the item.
        /// </param>
        /// <returns>
        ///     The item instance.
        /// </returns>
        public MenuItem Show(bool showItem = true)
        {
            ShowItem = showItem;

            return this;
        }

        /// <summary>
        ///     Show the tooltip.
        /// </summary>
        /// <param name="hide"></param>
        public void ShowTooltip(bool hide = false)
        {
            if (!string.IsNullOrEmpty(Tooltip))
            {
                DrawingTooltip = !hide;
            }
        }

        /// <summary>
        ///     Shows the tooltip notification.
        /// </summary>
        public void ShowTooltipNotification()
        {
            if (!string.IsNullOrEmpty(Tooltip))
            {
                var notif = new Notification(Tooltip).SetTextColor(System.Drawing.Color.White);
                Notifications.AddNotification(notif);
                Utility.DelayAction.Add(TooltipDuration, () => notif.Dispose());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Indicates whether the position is inside the menu item.
        /// </summary>
        /// <param name="position">
        ///     The position.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        internal bool IsInside(Vector2 position)
        {
            return Utils.IsUnderRectangle(
                position,
                Position.X,
                Position.Y,
                !string.IsNullOrEmpty(Tooltip) ? Width + Height : Width,
                Height);
        }

        /// <summary>
        ///     On draw event.
        /// </summary>
        internal void OnDraw()
        {
            var s = MultiLanguage._(DisplayName);

            MenuDrawHelper.DrawBox(
                Position,
                Width,
                Height,
                MenuSettings.BackgroundColor,
                1,
                System.Drawing.Color.Black);

            if (DrawingTooltip)
            {
                MenuDrawHelper.DrawToolTipText(
                    new Vector2(Position.X + Width, Position.Y),
                    this,
                    TooltipColor);
            }

            Font font;
            switch (FontStyle)
            {
                case FontStyle.Bold:
                    font = MenuDrawHelper.FontBold;
                    break;
                default:
                    font = MenuDrawHelper.Font;
                    break;
            }

            switch (ValueType)
            {
                case MenuValueType.Boolean:
                    MenuDrawHelper.DrawOnOff(
                        GetValue<bool>(),
                        new Vector2(Position.X + Width - Height, Position.Y),
                        this);
                    break;

                case MenuValueType.Slider:
                    MenuDrawHelper.DrawSlider(Position, this);
                    break;

                case MenuValueType.KeyBind:
                    var val = GetValue<KeyBind>();

                    if (Interacting)
                    {
                        s = MultiLanguage._("Press new key(s)");
                    }

                    if (val.Key != 0)
                    {
                        var x = !string.IsNullOrEmpty(Tooltip)
                            ? (int) Position.X + Width - Height
                              - font.MeasureText("[" + Utils.KeyToText(val.Key) + "]").Width - 35
                            : (int) Position.X + Width - Height
                              - font.MeasureText("[" + Utils.KeyToText(val.Key) + "]").Width - 10;

                        font.DrawText(
                            null,
                            "[" + Utils.KeyToText(val.Key) + "]",
                            new Rectangle(x, (int) Position.Y, Width, Height),
                            FontDrawFlags.VerticalCenter,
                            new ColorBGRA(1, 169, 234, 255));
                    }

                    if (val.SecondaryKey != 0)
                    {
                        var x_secondary = !string.IsNullOrEmpty(Tooltip)
                            ? (int) Position.X + Width - Height
                              - font.MeasureText("[" + Utils.KeyToText(val.Key) + "]").Width
                              - font.MeasureText("[" + Utils.KeyToText(val.Key) + "]").Width/4
                              - font.MeasureText("[" + Utils.KeyToText(val.SecondaryKey) + "]").Width
                              - 35
                            : (int) Position.X + Width - Height
                              - font.MeasureText("[" + Utils.KeyToText(val.Key) + "]").Width
                              - font.MeasureText("[" + Utils.KeyToText(val.Key) + "]").Width/4
                              - font.MeasureText("[" + Utils.KeyToText(val.SecondaryKey) + "]").Width
                              - 10;

                        font.DrawText(
                            null,
                            "[" + Utils.KeyToText(val.SecondaryKey) + "]",
                            new Rectangle(x_secondary, (int) Position.Y, Width, Height),
                            FontDrawFlags.VerticalCenter,
                            new ColorBGRA(1, 169, 234, 255));
                    }

                    MenuDrawHelper.DrawOnOff(
                        val.Active,
                        new Vector2(Position.X + Width - Height, Position.Y),
                        this);

                    break;

                case MenuValueType.Integer:
                    var intVal = GetValue<int>();
                    MenuDrawHelper.Font.DrawText(
                        null,
                        intVal.ToString(),
                        new Rectangle((int) Position.X + 5, (int) Position.Y, Width, Height),
                        FontDrawFlags.VerticalCenter | FontDrawFlags.Right,
                        new ColorBGRA(255, 255, 255, 255));
                    break;

                case MenuValueType.Color:
                    var colorVal = GetValue<System.Drawing.Color>();
                    MenuDrawHelper.DrawBox(
                        Position + new Vector2(Width - Height, 0),
                        Height,
                        Height,
                        colorVal,
                        1,
                        System.Drawing.Color.Black);
                    break;

                case MenuValueType.Circle:
                    var circleVal = GetValue<Circle>();
                    MenuDrawHelper.DrawBox(
                        Position + new Vector2(Width - Height*2, 0),
                        Height,
                        Height,
                        circleVal.Color,
                        1,
                        System.Drawing.Color.Black);
                    MenuDrawHelper.DrawOnOff(
                        circleVal.Active,
                        new Vector2(Position.X + Width - Height, Position.Y),
                        this);
                    break;

                case MenuValueType.StringList:
                    var slVal = GetValue<StringList>();

                    var t = slVal.SList[slVal.SelectedIndex];

                    MenuDrawHelper.DrawArrow(
                        "<<",
                        Position + new Vector2(Width - Height*2, 0),
                        this,
                        System.Drawing.Color.Black);
                    MenuDrawHelper.DrawArrow(
                        ">>",
                        Position + new Vector2(Width - Height, 0),
                        this,
                        System.Drawing.Color.Black);

                    MenuDrawHelper.Font.DrawText(
                        null,
                        MultiLanguage._(t),
                        new Rectangle(
                            (int) Position.X - 5 - 2*Height,
                            (int) Position.Y,
                            Width,
                            Height),
                        FontDrawFlags.VerticalCenter | FontDrawFlags.Right,
                        new ColorBGRA(255, 255, 255, 255));
                    break;
            }

            if (!string.IsNullOrEmpty(Tooltip))
            {
                MenuDrawHelper.DrawToolTipButton(new Vector2(Position.X + Width, Position.Y), this);
            }

            font.DrawText(
                null,
                s,
                new Rectangle((int) Position.X + 5, (int) Position.Y, Width, Height),
                FontDrawFlags.VerticalCenter,
                FontColor);
        }

        /// <summary>
        ///     Called when the game receives a window message.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="cursorPos">
        ///     The cursor position.
        /// </param>
        /// <param name="key">
        ///     The key.
        /// </param>
        internal void OnReceiveMessage(WindowsMessages message, Vector2 cursorPos, uint key, WndEventComposition wndArgs)
        {
            if (message == WindowsMessages.WM_MOUSEMOVE)
            {
                if (Visible && IsInside(cursorPos))
                {
                    if (cursorPos.X > Position.X + Width - 67
                        && cursorPos.X < Position.X + Width - 67 + Height + 8)
                    {
                        ShowTooltip();
                    }
                }
                else
                {
                    ShowTooltip(true);
                }
            }

            switch (ValueType)
            {
                case MenuValueType.Boolean:
                    if (message != WindowsMessages.WM_LBUTTONDOWN)
                    {
                        return;
                    }

                    if (!Visible)
                    {
                        return;
                    }

                    if (!IsInside(cursorPos))
                    {
                        return;
                    }

                    if (cursorPos.X > Position.X + Width)
                    {
                        break;
                    }

                    if (cursorPos.X > Position.X + Width - Height)
                    {
                        SetValue(!GetValue<bool>());
                    }

                    break;
                case MenuValueType.Slider:
                    if (!Visible)
                    {
                        Interacting = false;
                        return;
                    }

                    if (message == WindowsMessages.WM_MOUSEMOVE && Interacting
                        || message == WindowsMessages.WM_LBUTTONDOWN && !Interacting && IsInside(cursorPos))
                    {
                        var val = GetValue<Slider>();
                        var t = val.MinValue
                                + ((cursorPos.X - Position.X)*(val.MaxValue - val.MinValue))/Width;
                        val.Value = (int) t;
                        SetValue(val);
                    }

                    if (message != WindowsMessages.WM_LBUTTONDOWN && message != WindowsMessages.WM_LBUTTONUP)
                    {
                        return;
                    }

                    if (!IsInside(cursorPos) && message == WindowsMessages.WM_LBUTTONDOWN)
                    {
                        return;
                    }

                    Interacting = message == WindowsMessages.WM_LBUTTONDOWN;
                    break;
                case MenuValueType.Color:
                    if (message != WindowsMessages.WM_LBUTTONDOWN)
                    {
                        return;
                    }

                    if (!Visible)
                    {
                        return;
                    }

                    if (!IsInside(cursorPos))
                    {
                        return;
                    }

                    if (cursorPos.X > Position.X + Width)
                    {
                        break;
                    }

                    if (cursorPos.X > Position.X + Width - Height)
                    {
                        var c = GetValue<System.Drawing.Color>();
                        ColorPicker.Load(delegate(System.Drawing.Color args) { this.SetValue(args); }, c);
                    }

                    break;
                case MenuValueType.Circle:
                    if (message != WindowsMessages.WM_LBUTTONDOWN)
                    {
                        return;
                    }

                    if (!Visible)
                    {
                        return;
                    }

                    if (!IsInside(cursorPos))
                    {
                        return;
                    }

                    if (cursorPos.X > Position.X + Width)
                    {
                        break;
                    }

                    if (cursorPos.X - Position.X > Width - Height)
                    {
                        var val = GetValue<Circle>();
                        val.Active = !val.Active;
                        SetValue(val);
                    }
                    else if (cursorPos.X - Position.X > Width - 2*Height)
                    {
                        var c = GetValue<Circle>();
                        ColorPicker.Load(
                            delegate(System.Drawing.Color args)
                            {
                                var val = this.GetValue<Circle>();
                                val.Color = args;
                                this.SetValue(val);
                            },
                            c.Color);
                    }

                    break;
                case MenuValueType.KeyBind:
                    if (!MenuGUI.IsChatOpen && !Shop.IsOpen)
                    {
                        switch (message)
                        {
                            case WindowsMessages.WM_KEYDOWN:
                                var val = GetValue<KeyBind>();
                                if (key == val.Key || key == val.SecondaryKey)
                                {
                                    if (val.Type == KeyBindType.Press)
                                    {
                                        if (!val.Active)
                                        {
                                            val.Active = true;
                                            SetValue(val);
                                        }
                                    }
                                }
                                break;
                            case WindowsMessages.WM_KEYUP:

                                var val2 = GetValue<KeyBind>();
                                if (key == val2.Key || key == val2.SecondaryKey)
                                {
                                    if (val2.Type == KeyBindType.Press)
                                    {
                                        val2.Active = false;
                                        SetValue(val2);
                                    }
                                    else
                                    {
                                        val2.Active = !val2.Active;
                                        SetValue(val2);
                                    }
                                }
                                break;
                        }
                    }

                    if (key == 8 && message == WindowsMessages.WM_KEYUP && Interacting)
                    {
                        var val = GetValue<KeyBind>();
                        val.Key = 0;
                        val.SecondaryKey = 0;
                        SetValue(val);
                        Interacting = false;
                        KeybindSettingStage = KeybindSetStage.NotSetting;
                    }

                    if (message == WindowsMessages.WM_KEYUP && Interacting &&
                        KeybindSettingStage != KeybindSetStage.NotSetting)
                    {
                        if (KeybindSettingStage == KeybindSetStage.Keybind1)
                        {
                            var val = GetValue<KeyBind>();
                            val.Key = key;
                            SetValue(val);
                            KeybindSettingStage = KeybindSetStage.Keybind2;
                        }
                        else if (KeybindSettingStage == KeybindSetStage.Keybind2)
                        {
                            var val = GetValue<KeyBind>();
                            val.SecondaryKey = key;
                            SetValue(val);
                            Interacting = false;
                            KeybindSettingStage = KeybindSetStage.NotSetting;
                        }
                    }

                    if (message == WindowsMessages.WM_KEYUP && Interacting &&
                        KeybindSettingStage == KeybindSetStage.NotSetting)
                    {
                        var val = GetValue<KeyBind>();
                        val.Key = key;
                        val.SecondaryKey = 0;
                        SetValue(val);
                        Interacting = false;
                    }


                    if (!Visible)
                    {
                        return;
                    }

                    if (message != WindowsMessages.WM_LBUTTONDOWN
                        && wndArgs.Msg != WindowsMessages.WM_RBUTTONDOWN)
                    {
                        return;
                    }

                    if (!IsInside(cursorPos))
                    {
                        return;
                    }

                    if (cursorPos.X > Position.X + Width)
                    {
                        break;
                    }

                    if (cursorPos.X > Position.X + Width - Height)
                    {
                        var val = GetValue<KeyBind>();
                        val.Active = !val.Active;
                        SetValue(val);
                    }
                    else
                    {
                        if (wndArgs.Msg == WindowsMessages.WM_RBUTTONDOWN)
                        {
                            KeybindSettingStage = KeybindSetStage.Keybind1;
                        }
                        //this.Stage = KeybindSetStage.NotSetting;
                        Interacting = !Interacting;
                    }

                    break;
                case MenuValueType.StringList:
                    if (!Visible)
                    {
                        return;
                    }

                    if (message != WindowsMessages.WM_LBUTTONDOWN)
                    {
                        return;
                    }

                    if (!IsInside(cursorPos))
                    {
                        return;
                    }

                    if (cursorPos.X > Position.X + Width)
                    {
                        break;
                    }

                    var slVal = GetValue<StringList>();
                    if (cursorPos.X > Position.X + Width - Height)
                    {
                        slVal.SelectedIndex = slVal.SelectedIndex == slVal.SList.Length - 1
                            ? 0
                            : (slVal.SelectedIndex + 1);
                        SetValue(slVal);
                    }
                    else if (cursorPos.X > Position.X + Width - 2*Height)
                    {
                        slVal.SelectedIndex = slVal.SelectedIndex == 0
                            ? slVal.SList.Length - 1
                            : (slVal.SelectedIndex - 1);
                        SetValue(slVal);
                    }

                    break;
            }
        }

        /// <summary>
        ///     Save to file.
        /// </summary>
        /// <param name="dics">
        ///     Data collection.
        /// </param>
        internal void SaveToFile(ref Dictionary<string, Dictionary<string, byte[]>> dics)
        {
            if (!dontSave)
            {
                if (!dics.ContainsKey(SaveFileName))
                {
                    dics[SaveFileName] = new Dictionary<string, byte[]>();
                }

                dics[SaveFileName][SaveKey] = serialized;
            }
        }

        #endregion
    }
}