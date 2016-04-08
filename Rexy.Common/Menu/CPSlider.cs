using System;
using LeagueSharp.Common.Properties;
using SharpDX;

namespace LeagueSharp.Common
{
    /// <summary>
    ///     The color picker slider.
    /// </summary>
    public class CPSlider
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CPSlider" /> class.
        /// </summary>
        /// <param name="x">
        ///     The X.
        /// </param>
        /// <param name="y">
        ///     The Y.
        /// </param>
        /// <param name="height">
        ///     The Height.
        /// </param>
        /// <param name="percent">
        ///     The Percent.
        /// </param>
        public CPSlider(int x, int y, int height, float percent = 1)
        {
            xPos = x;
            yPos = y;
            Height = height - Resources.CPActiveSlider.Height;
            this.percent = percent;

            ActiveSprite = new Render.Sprite(Resources.CPActiveSlider, new Vector2(X, Y));
            InactiveSprite = new Render.Sprite(Resources.CPInactiveSlider, new Vector2(X, Y));

            ActiveSprite.Add(2);
            InactiveSprite.Add(2);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The windows event process message event.
        /// </summary>
        /// <param name="args">
        ///     The arguments.
        /// </param>
        public void OnWndProc(WndEventComposition args)
        {
            switch (args.Msg)
            {
                case WindowsMessages.WM_LBUTTONDOWN:
                    if (Utils.IsUnderRectangle(
                        Utils.GetCursorPos(),
                        X,
                        Y,
                        Width,
                        Height + Resources.CPActiveSlider.Height))
                    {
                        ActiveSprite.Visible = Moving = true;
                        InactiveSprite.Visible = false;
                        UpdatePercent();
                    }
                    break;
                case WindowsMessages.WM_MOUSEMOVE:
                    if (Moving)
                    {
                        UpdatePercent();
                    }
                    break;
                case WindowsMessages.WM_LBUTTONUP:
                    ActiveSprite.Visible = Moving = false;
                    InactiveSprite.Visible = true;
                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the percent.
        /// </summary>
        private void UpdatePercent()
        {
            Percent = (Utils.GetCursorPos().Y - (Resources.CPActiveSlider.Height/2f) - Y)/Height;
            ColorPicker.UpdateColor();
            ActiveSprite.Y = InactiveSprite.Y = Y + (int) (percent*Height);
        }

        #endregion

        #region Fields

        /// <summary>
        ///     The height.
        /// </summary>
        public int Height;

        /// <summary>
        ///     Indicates whether the slider is moving.
        /// </summary>
        public bool Moving;

        /// <summary>
        ///     The active sprite.
        /// </summary>
        internal Render.Sprite ActiveSprite;

        /// <summary>
        ///     The inactive sprite.
        /// </summary>
        internal Render.Sprite InactiveSprite;

        /// <summary>
        ///     The X-axis position.
        /// </summary>
        private readonly int xPos;

        /// <summary>
        ///     The Y-axis position.
        /// </summary>
        private readonly int yPos;

        /// <summary>
        ///     Indicates whether the slider is visible.
        /// </summary>
        private bool isVisible = true;

        /// <summary>
        ///     The percent.
        /// </summary>
        private float percent;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the percent.
        /// </summary>
        public float Percent
        {
            get { return percent; }

            set { percent = Math.Max(0, Math.Min(1, value)); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the slider is visible.
        /// </summary>
        public bool Visible
        {
            get { return isVisible; }

            set { ActiveSprite.Visible = InactiveSprite.Visible = isVisible = value; }
        }

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        public int Width
        {
            get { return Resources.CPActiveSlider.Width; }
        }

        /// <summary>
        ///     Gets or sets the X.
        /// </summary>
        public int X
        {
            get { return xPos + ColorPicker.X; }

            set { ActiveSprite.X = InactiveSprite.X = value; }
        }

        /// <summary>
        ///     Gets or sets the Y.
        /// </summary>
        public int Y
        {
            get { return yPos + ColorPicker.Y; }

            set { ActiveSprite.Y = InactiveSprite.Y = value + (int) (percent*Height); }
        }

        #endregion
    }
}