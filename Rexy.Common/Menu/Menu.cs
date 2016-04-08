using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using EloBuddy;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Rectangle = SharpDX.Rectangle;

namespace LeagueSharp.Common
{
    /// <summary>
    ///     The menu.
    /// </summary>
    public class Menu
    {
        #region Static Fields

        /// <summary>
        ///     The menu settings, root menu alias.
        /// </summary>
        public static readonly Menu Root = new Menu("Menu Settings", "Menu Settings");

        /// <summary>
        ///     The root menus.
        /// </summary>
        public static Dictionary<string, Menu> RootMenus = new Dictionary<string, Menu>();

        #endregion

        #region Fields

        /// <summary>
        ///     The menu children.
        /// </summary>
        public List<Menu> Children = new List<Menu>();

        /// <summary>
        ///     The color.
        /// </summary>
        public Color Color;

        /// <summary>
        ///     The menu display name.
        /// </summary>
        public string DisplayName;

        /// <summary>
        ///     Indicates whether the menu has a root attribute.
        /// </summary>
        public bool IsRootMenu;

        /// <summary>
        ///     The menu submenus.
        /// </summary>
        public List<MenuItem> Items = new List<MenuItem>();

        /// <summary>
        ///     The menu name.
        /// </summary>
        public string Name;

        /// <summary>
        ///     The menu parent.
        /// </summary>
        public Menu Parent;

        /// <summary>
        ///     The menu font style.
        /// </summary>
        public FontStyle Style;

        /// <summary>
        ///     The cached menu count.
        /// </summary>
        private int cachedMenuCount = 2;

        /// <summary>
        ///     The cached menu count tick.
        /// </summary>
        private int cachedMenuCountT;

        /// <summary>
        ///     Indicates whether the menu is visible.
        /// </summary>
        private bool isVisible;

        /// <summary>
        ///     The unqiue id.
        /// </summary>
        private string uniqueId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a static instance of the <see cref="Menu" /> class.
        /// </summary>
        static Menu()
        {
            Root.AddItem(new MenuItem("BackgroundAlpha", "Background Opacity")).SetValue(new Slider(165, 55, 255));
            Root.AddItem(
                new MenuItem("FontName", "Font Name:").SetValue(
                    new StringList(new[] {"Tahoma", "Calibri", "Segoe UI"})));
            Root.AddItem(new MenuItem("FontSize", "Font Size:").SetValue(new Slider(13, 12, 20)));
            Root.AddItem(
                new MenuItem("FontQuality", "Font Quality").SetValue(
                    new StringList(
                        Enum.GetValues(typeof (FontQuality)).Cast<FontQuality>().Select(v => v.ToString()).ToArray(),
                        4)));
            Root.AddItem(
                new MenuItem("LeagueSharp.Common.TooltipDuration", "Tooltip Notification Duration").SetValue(
                    new Slider(1500, 0, 5000)));
            Root.AddItem(
                new MenuItem("FontInfo", "Press F5 after your change").SetFontStyle(FontStyle.Bold, Color.Yellow));

            CommonMenu.Instance.AddSubMenu(Root);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Menu" /> class.
        /// </summary>
        /// <param name="displayName">
        ///     The display name.
        /// </param>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <param name="isRootMenu">
        ///     Indicates whether the menu has root attribute.
        /// </param>
        public Menu(string displayName, string name, bool isRootMenu = false)
        {
            DisplayName = displayName;
            Name = name;
            IsRootMenu = isRootMenu;
            Style = FontStyle.Regular;
            Color = Color.White;

            if (isRootMenu)
            {
                CustomEvents.Game.OnGameEnd += delegate { this.SaveAll(); };
                Game.OnEnd += delegate { this.SaveAll(); };
                AppDomain.CurrentDomain.DomainUnload += delegate { this.SaveAll(); };
                AppDomain.CurrentDomain.ProcessExit += delegate { this.SaveAll(); };

                var rootName = Assembly.GetCallingAssembly().GetName().Name + "." + name;

                if (RootMenus.ContainsKey(rootName))
                {
                    throw new ArgumentException(@"Root Menu [" + rootName + @"] with the same name exists", "name");
                }

                RootMenus.Add(rootName, this);
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="Menu" /> class.
        /// </summary>
        ~Menu()
        {
            if (RootMenus.ContainsKey(Name))
            {
                RootMenus.Remove(Name);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the width of the children menu.
        /// </summary>
        internal int ChildrenMenuWidth
        {
            get
            {
                return
                    Items.Select(item => item.NeededWidth)
                        .Concat(new[] {Children.Select(item => item.NeededWidth).Concat(new[] {0}).Max()})
                        .Max();
            }
        }

        /// <summary>
        ///     Gets the height.
        /// </summary>
        internal int Height
        {
            get { return MenuSettings.MenuItemHeight; }
        }

        /// <summary>
        ///     Gets the menu count.
        /// </summary>
        internal int MenuCount
        {
            get
            {
                if (Utils.TickCount - cachedMenuCountT < 500)
                {
                    return cachedMenuCount;
                }

                var globalMenuList = MenuGlobals.MenuState;
                var i = 0;
                var result = 0;

                foreach (var item in globalMenuList)
                {
                    if (item == uniqueId)
                    {
                        result = i;
                        break;
                    }

                    i++;
                }

                cachedMenuCount = result;
                cachedMenuCountT = Utils.TickCount;

                return result;
            }
        }

        /// <summary>
        ///     Gets the base position.
        /// </summary>
        internal Vector2 MyBasePosition
        {
            get
            {
                if (IsRootMenu || Parent == null)
                {
                    return MenuSettings.BasePosition + MenuCount*new Vector2(0, MenuSettings.MenuItemHeight);
                }

                return Parent.MyBasePosition;
            }
        }

        /// <summary>
        ///     Gets the needed width.
        /// </summary>
        internal int NeededWidth
        {
            get { return MenuDrawHelper.Font.MeasureText(MultiLanguage._(DisplayName)).Width + 25; }
        }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        internal Vector2 Position
        {
            get
            {
                return new Vector2(0, MyBasePosition.Y)
                       + new Vector2(
                           (Parent != null)
                               ? Parent.Position.X + Parent.Width
                               : (int) MyBasePosition.X,
                           0) + YLevel*new Vector2(0, MenuSettings.MenuItemHeight);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the menu is visible.
        /// </summary>
        internal bool Visible
        {
            get
            {
                if (!MenuSettings.DrawMenu)
                {
                    return false;
                }
                return IsRootMenu || isVisible;
            }
            set
            {
                isVisible = value;

                //Hide all the children
                if (!isVisible)
                {
                    foreach (var schild in Children)
                    {
                        schild.Visible = false;
                    }

                    foreach (var sitem in Items)
                    {
                        sitem.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        internal int Width
        {
            get { return Parent != null ? Parent.ChildrenMenuWidth : MenuSettings.MenuItemWidth; }
        }

        /// <summary>
        ///     Gets the X level.
        /// </summary>
        internal int XLevel
        {
            get
            {
                var result = 0;
                var m = this;

                while (m.Parent != null)
                {
                    m = m.Parent;
                    result++;
                }

                return result;
            }
        }

        /// <summary>
        ///     Gets the Y level.
        /// </summary>
        internal int YLevel
        {
            get
            {
                if (IsRootMenu || Parent == null)
                {
                    return 0;
                }

                return Parent.YLevel + Parent.Children.TakeWhile(test => test.Name != Name).Count();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the menu.
        /// </summary>
        /// <param name="assemblyname">
        ///     The assembly name.
        /// </param>
        /// <param name="menuname">
        ///     The menu name.
        /// </param>
        /// <returns>
        ///     The <see cref="Menu" />.
        /// </returns>
        public static Menu GetMenu(string assemblyname, string menuname)
        {
            return RootMenus.FirstOrDefault(x => x.Key == assemblyname + "." + menuname).Value;
        }

        /// <summary>
        ///     Gets the value globally.
        /// </summary>
        /// <param name="assemblyname">
        ///     The assembly name.
        /// </param>
        /// <param name="menuname">
        ///     The menu name.
        /// </param>
        /// <param name="itemname">
        ///     The item name.
        /// </param>
        /// <param name="submenu">
        ///     The submenu.
        /// </param>
        /// <returns>
        ///     The <see cref="MenuItem" />.
        /// </returns>
        public static MenuItem GetValueGlobally(
            string assemblyname,
            string menuname,
            string itemname,
            string submenu = null)
        {
            var menu = RootMenus.FirstOrDefault(x => x.Key == assemblyname + "." + menuname).Value;

            if (submenu != null)
            {
                menu = menu.SubMenu(submenu);
            }

            return menu.Item(itemname);
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        public static void SendMessage(uint key, WindowsMessages message, WndEventComposition args)
        {
            foreach (var menu in RootMenus)
            {
                menu.Value.OnReceiveMessage(message, Utils.GetCursorPos(), key, args);
            }
        }

        /// <summary>
        ///     Adds an item to the menu.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <returns>
        ///     The item instance.
        /// </returns>
        public MenuItem AddItem(MenuItem item)
        {
            item.Parent = this;
            Items.Add(item);

            return item;
        }

        /// <summary>
        ///     Adds a submenu to the menu.
        /// </summary>
        /// <param name="subMenu">
        ///     The submenu.
        /// </param>
        /// <returns>
        ///     The menu instance.
        /// </returns>
        public Menu AddSubMenu(Menu subMenu)
        {
            subMenu.Parent = this;
            Children.Add(subMenu);

            return subMenu;
        }

        /// <summary>
        ///     Adds the menu to the main menu.
        /// </summary>
        public void AddToMainMenu()
        {
            InitMenuState(Assembly.GetCallingAssembly().GetName().Name);

            AppDomain.CurrentDomain.DomainUnload += (sender, args) => UnloadMenuState();
            Drawing.OnEndScene += OnDraw;
            Game.OnWndProc += args => OnWndProc(new WndEventComposition(args));
        }

        /// <summary>
        ///     Gets the menu's item by name.
        /// </summary>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <param name="championUnique">
        ///     Indicates whether the item is champion unique.
        /// </param>
        /// <returns>
        ///     The <see cref="MenuItem" />.
        /// </returns>
        public MenuItem Item(string name, bool championUnique = false)
        {
            if (championUnique)
            {
                name = ObjectManager.Player.ChampionName + name;
            }

            //Search in our own items
            foreach (var item in Items.Where(item => item.Name == name))
            {
                return item;
            }

            //Search in submenus
            return
                (from subMenu in Children where subMenu.Item(name) != null select subMenu.Item(name))
                    .FirstOrDefault();
        }

        /// <summary>
        ///     Sets the font style.
        /// </summary>
        /// <param name="fontStyle">
        ///     The font style.
        /// </param>
        /// <param name="fontColor">
        ///     Color of the font.
        /// </param>
        /// <returns>
        ///     The <see cref="Menu" />.
        /// </returns>
        public Menu SetFontStyle(FontStyle fontStyle = FontStyle.Regular, Color? fontColor = null)
        {
            Style = fontStyle;
            Color = fontColor ?? Color.White;

            return this;
        }

        /// <summary>
        ///     Gets the menu's submenu by name.
        /// </summary>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <returns>
        ///     The <see cref="Menu" />.
        /// </returns>
        public Menu SubMenu(string name)
        {
            return Children.FirstOrDefault(sm => sm.Name == name) ?? AddSubMenu(new Menu(name, name));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Determines whether the specified position is inside the menu.
        /// </summary>
        /// <param name="position">
        ///     The position.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        internal bool IsInside(Vector2 position)
        {
            return Utils.IsUnderRectangle(position, Position.X, Position.Y, Width, Height);
        }

        /// <summary>
        ///     The drawing event.
        /// </summary>
        /// <param name="args">
        ///     The event args.
        /// </param>
        internal void OnDraw(EventArgs args)
        {
            if (!Visible)
            {
                return;
            }

            Drawing.Direct3DDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
            MenuDrawHelper.DrawBox(
                Position,
                Width,
                Height,
                (Children.Count > 0 && Children[0].Visible || Items.Count > 0 && Items[0].Visible)
                    ? MenuSettings.ActiveBackgroundColor
                    : MenuSettings.BackgroundColor,
                1,
                System.Drawing.Color.Black);

            MenuDrawHelper.Font.DrawText(
                null,
                MultiLanguage._(DisplayName),
                new Rectangle((int) Position.X + 5, (int) Position.Y, Width, Height),
                FontDrawFlags.VerticalCenter,
                Color);
            MenuDrawHelper.Font.DrawText(
                null,
                ">",
                new Rectangle((int) Position.X - 5, (int) Position.Y, Width, Height),
                FontDrawFlags.Right | FontDrawFlags.VerticalCenter,
                Color);

            //Draw the menu submenus
            foreach (var child in Children.Where(child => child.Visible))
            {
                child.OnDraw(args);
            }

            //Draw the items
            for (var i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (item.Visible)
                {
                    item.OnDraw();
                }
            }
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
        internal void OnReceiveMessage(WindowsMessages message, Vector2 cursorPos, uint key, WndEventComposition args)
        {
            //Spread the message to the menu's children recursively
            foreach (var child in Children)
            {
                child.OnReceiveMessage(message, cursorPos, key, args);
            }

            foreach (var item in Items)
            {
                item.OnReceiveMessage(message, cursorPos, key, args);
            }

            //Handle the left clicks on the menus to hide or show the submenus.
            if (message != WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            if (IsRootMenu && Visible)
            {
                if (cursorPos.X - MenuSettings.BasePosition.X < MenuSettings.MenuItemWidth)
                {
                    var n = (int) (cursorPos.Y - MenuSettings.BasePosition.Y)/MenuSettings.MenuItemHeight;
                    if (MenuCount != n)
                    {
                        foreach (var schild in Children)
                        {
                            schild.Visible = false;
                        }

                        foreach (var sitem in Items)
                        {
                            sitem.Visible = false;
                        }
                    }
                }
            }

            if (!Visible)
            {
                return;
            }

            if (!IsInside(cursorPos))
            {
                return;
            }

            if (!IsRootMenu && Parent != null)
            {
                //Close all the submenus in the level 
                foreach (var child in Parent.Children.Where(child => child.Name != Name))
                {
                    foreach (var schild in child.Children)
                    {
                        schild.Visible = false;
                    }

                    foreach (var sitem in child.Items)
                    {
                        sitem.Visible = false;
                    }
                }
            }

            //Hide or Show the submenus.
            foreach (var child in Children)
            {
                child.Visible = !child.Visible;
            }

            //Hide or Show the items.
            foreach (var item in Items)
            {
                item.Visible = !item.Visible;
            }
        }

        /// <summary>
        ///     Fired when the game receives a window event.
        /// </summary>
        /// <param name="args">
        ///     The windows event process message args.
        /// </param>
        internal void OnWndProc(WndEventComposition args)
        {
            OnReceiveMessage(args.Msg, Utils.GetCursorPos(), args.WParam, args);
        }

        /// <summary>
        ///     Save all in a recurisve method.
        /// </summary>
        /// <param name="dics">
        ///     The collection.
        /// </param>
        internal void RecursiveSaveAll(ref Dictionary<string, Dictionary<string, byte[]>> dics)
        {
            foreach (var child in Children)
            {
                child.RecursiveSaveAll(ref dics);
            }

            foreach (var item in Items)
            {
                item.SaveToFile(ref dics);
            }
        }

        /// <summary>
        ///     Save all data.
        /// </summary>
        internal void SaveAll()
        {
            var dic = new Dictionary<string, Dictionary<string, byte[]>>();
            RecursiveSaveAll(ref dic);

            foreach (var dictionary in dic)
            {
                var dicToSave = SavedSettings.Load(dictionary.Key) ?? new Dictionary<string, byte[]>();

                foreach (var entry in dictionary.Value)
                {
                    dicToSave[entry.Key] = entry.Value;
                }

                SavedSettings.Save(dictionary.Key, dicToSave);
            }
        }

        /// <summary>
        ///     Initialize menu state.
        /// </summary>
        /// <param name="assemblyName">
        ///     The assembly name.
        /// </param>
        private void InitMenuState(string assemblyName)
        {
            uniqueId = assemblyName + "." + Name;

            var globalMenuList = MenuGlobals.MenuState ?? new List<string>();

            while (globalMenuList.Contains(uniqueId))
            {
                uniqueId += ".";
            }

            globalMenuList.Add(uniqueId);

            MenuGlobals.MenuState = globalMenuList;
        }

        /// <summary>
        ///     Unload the menu state.
        /// </summary>
        private void UnloadMenuState()
        {
            MenuGlobals.MenuState.Remove(uniqueId);
        }

        #endregion
    }
}