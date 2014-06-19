#define GAME_TIME

using LiveSplit.GrooveCity;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;

namespace LiveSplit.UI.Components
{
    class Component : IComponent
    {
        public ComponentSettings Settings { get; set; }

        public string ComponentName
        {
            get { return "Yoshi's Island Auto Splitter"; }
        }

        public float PaddingBottom { get { return 0; } }
        public float PaddingTop { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }

        public bool Refresh { get; set; }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        public Process Game { get; set; }

        protected static readonly DeepPointer IsInALevel = new DeepPointer("snes9x.exe", 0x002EFBA4, 0x2904a);
        protected static readonly DeepPointer LevelFrames = new DeepPointer("snes9x.exe", 0x002EFBA4, 0x3a9);

        public TimeSpan GameTime { get; set; }

        public TimeSpan? OldLevelTime { get; set; }
        public short WasInALevel { get; set; }

        protected TimerModel Model { get; set; }

        public Component()
        {
            Settings = new ComponentSettings();
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (Game == null || Game.HasExited)
            {
                Game = null;
                var process = Process.GetProcessesByName("snes9x").FirstOrDefault();
                if (process != null)
                {
                    Game = process;
                }
            }

            if (Model == null)
            {
                Model = new TimerModel() { CurrentState = state };
                state.OnStart += state_OnStart;
            }

            if (Game != null)
            {
                float time;
                short frames;
                TimeSpan levelTime = OldLevelTime ?? TimeSpan.Zero;

                byte isInALevel;
                IsInALevel.Deref<byte>(Game, out isInALevel);

                LevelFrames.Deref<short>(Game, out frames);
                time = frames / 60.0f;
                levelTime = TimeSpan.FromSeconds(time);

                if (OldLevelTime != null)
                {
                    if (state.CurrentPhase == TimerPhase.NotRunning && WasInALevel == 4 && isInALevel == 0)
                    {
                        Model.Start();
                    }
                    else if (state.CurrentPhase == TimerPhase.Running)
                    {
                        if (WasInALevel == 1 && isInALevel == 0)
                        {
                            Model.Split();
                        }

                        //if (titleScreenShowing && !creditsPlaying)
                            //Model.Reset();
                    }
                    if (OldLevelTime > levelTime)
                    {
                        GameTime += OldLevelTime ?? TimeSpan.Zero;
                    }
                }

#if GAME_TIME
                state.IsLoading = true;
                var currentGameTime = GameTime + levelTime;
                state.SetGameTime(currentGameTime < TimeSpan.Zero ? TimeSpan.Zero : currentGameTime);
#endif

                OldLevelTime = levelTime;
                WasInALevel = isInALevel;
            }
        }

        void state_OnStart(object sender, EventArgs e)
        {
            GameTime = TimeSpan.Zero - (OldLevelTime ?? TimeSpan.Zero);
            OldLevelTime = null;
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
        }

        public float VerticalHeight
        {
            get { return 0; }
        }

        public float MinimumWidth
        {
            get { return 0; }
        }

        public float HorizontalWidth
        {
            get { return 0; }
        }

        public float MinimumHeight
        {
            get { return 0; }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return document.CreateElement("x");
        }

        public System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return null;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
        }

        public void RenameComparison(string oldName, string newName)
        {
        }
    }
}
