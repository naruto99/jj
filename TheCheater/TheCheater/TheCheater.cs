﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using TheCheater;
using Color = System.Drawing.Color;

namespace WhoIsYourCheater.TheCheater
{
    class TheCheater
    {
        private static readonly Dictionary<int, List<IDetector>> _detectors = new Dictionary<int, List<IDetector>>();
        private static Menu _mainMenu;
        private static Vector2 _screenPos;

        public static void Load()
        {
            _mainMenu = MainMenu.AddMenu("The Cheater", "thecheater");
            _mainMenu.AddLabel("TheCheater - Ported by Rexy");
            var detectionType = _mainMenu.Add("detection",
                new ComboBox("Detection", 0, new [] {"Preferred", "Safe", "AntiHumanizer"}));
            detectionType.OnValueChange += (sender, args) =>
            {
                foreach (var detector in _detectors)
                {
                    detector.Value.ForEach(item => item.ApplySetting((DetectorSetting)args.NewValue));
                }
            };
            _mainMenu.Add("enabled", new CheckBox("Enabled"));
            _mainMenu.Add("drawing", new CheckBox("Drawing"));
            var posX = _mainMenu.Add("positionx", new Slider("Position X", Drawing.Width - 270, 0, Drawing.Width - 20));
            var posY = _mainMenu.Add("positiony", new Slider("Position Y", Drawing.Height/2, 0, Drawing.Height - 20));
            posX.OnValueChange += (sender, args) => _screenPos.X = args.NewValue;
            posY.OnValueChange += (sender, args) => _screenPos.Y = args.NewValue;

            _screenPos.X = posX.CurrentValue;
            _screenPos.Y = posY.CurrentValue;

            Obj_AI_Base.OnNewPath += OnNewPath;
            Drawing.OnDraw += Draw;
    
            Chat.Print("TheCheater Loaded");
        }

        private static void Draw(EventArgs args)
        {
            if (!_mainMenu["drawing"].Cast<CheckBox>().CurrentValue) return;

            Drawing.DrawLine(new Vector2(_screenPos.X, _screenPos.Y + 15), new Vector2(_screenPos.X + 180, _screenPos.Y + 15), 2, Color.Cyan);
           
            var column = 1;
            Drawing.DrawText(_screenPos.X, _screenPos.Y, Color.Cyan, "Cheat-pattern detection:");
            foreach (var detector in _detectors)
            {
                var maxValue = detector.Value.Max(item => item.GetScriptDetections());
                Drawing.DrawText(_screenPos.X, column * 20 + _screenPos.Y, Color.Cyan, EntityManager.Heroes.AllHeroes.First(hero => hero.NetworkId == detector.Key).Name + ": " + maxValue + (maxValue > 0 ? " (" + detector.Value.First(itemId => itemId.GetScriptDetections() == maxValue).GetName() + ")" : string.Empty));
                column++;
            }
        }

        private static void OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.Type != GameObjectType.AIHeroClient || !_mainMenu["enabled"].Cast<CheckBox>().CurrentValue) return;

            if (!_detectors.ContainsKey(sender.NetworkId))
            {
                var detectors = new List<IDetector> { new SacOrbwalkerDetector(), new LeaguesharpOrbwalkDetector() };
                detectors.ForEach(detector => detector.Initialize((AIHeroClient)sender));
                _detectors.Add(sender.NetworkId, detectors);
            }
            else
                _detectors[sender.NetworkId].ForEach(detector => detector.FeedData(args.Path.Last()));
        }


    }
}
