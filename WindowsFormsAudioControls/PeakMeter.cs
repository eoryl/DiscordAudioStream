using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace DiscordAudioStream
{
    public partial class PeakMeter : UserControl
    {
        public enum MeterOrientation
        {
            Horizontal,
            Vertical
        }


        private MeterOrientation orientation;
        private float level;
        private float clipLevel;
        private float headroomLevel;
        private float minLevel;
        private float maxLevel;
        private Color clipLevelColor;
        private Color headRoomColor;
        private Color levelColor;
        private Color peakColor;
        private long peakDecayTimeMS;
        private float peakLevel;
        private DateTime peakLevelTime;
        private bool drawPeak;


        public MeterOrientation Orientation { get => orientation; set => orientation = value; }
        public float Level { get => level; 
            set 
            { 
                if (this.level != value)
                {
                    this.level = value;
                    this.UpdatePeak();
                    //this.Invalidate();
                    //this.Refresh();
                }
            }
        }
        public float ClipLevel { get => clipLevel; set => clipLevel = (value > 0.0f ? 0.0f : value); }
        public float HeadroomLevel { get => headroomLevel; set => headroomLevel = (value > 0.0f ? 0.0f : value); }
        public float MinLevel { get => minLevel; }
        public Color ClipLevelColor { get => clipLevelColor; set => clipLevelColor = value; }
        public Color HeadroomLevelColor { get => headRoomColor; set => headRoomColor = value; }
        public Color LevelColor { get => levelColor; set => levelColor = value; }
        public float MaxLevel{ get => maxLevel; }
        public long PeakDecayTimeMS { get => peakDecayTimeMS; set => peakDecayTimeMS = value; }
        public bool DrawPeak { get => drawPeak; set => drawPeak = value; }
        public Color PeakColor { get => peakColor; set => peakColor = value; }

        public PeakMeter()
        {

            clipLevelColor = Color.Red;
            headRoomColor = Color.Gold;
            levelColor = Color.Green;
            BackColor = Color.DarkGray;
            


            orientation = MeterOrientation.Horizontal;
            maxLevel = 0.0f;
            minLevel = -60.0f;
            clipLevel = -6.0f;
            headroomLevel = -18.0f;
            level = minLevel;

            peakDecayTimeMS = 2000;
            peakLevel = minLevel;
            peakLevelTime = DateTime.UtcNow;

            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);

        }
        public void UpdatePeak()
        {
            DateTime now = DateTime.UtcNow; 
            if (level > peakLevel)
            {
                peakLevel = level;
                peakLevelTime = now;
            }
            else
            {
                if ( peakLevelTime.AddMilliseconds(peakDecayTimeMS) < now)
                {
                    peakLevel = level;
                    peakLevelTime = now;
                }
            }
            this.Invalidate();
        }

        private int ScaleDBFsToInt(float dbFSVal, int maxIntVal)
        {
            if (maxLevel == minLevel) return 0;
            if (dbFSVal <= minLevel) return 0;
            if (dbFSVal >= maxLevel) return maxIntVal;
            float res =  (dbFSVal - minLevel);
            res /= (float) Math.Abs(maxLevel -minLevel);
            res *= (float) maxIntVal;
            return (int)res;

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int levelH, levelC, levelV, levelP,  levelM = 0;

            if (orientation == MeterOrientation.Horizontal)
            {
                levelM = Width;
            }
            else if( orientation == MeterOrientation.Horizontal)
            {
                levelM = Height;
            }

            levelH = ScaleDBFsToInt(headroomLevel, levelM);
            levelC = ScaleDBFsToInt(clipLevel, levelM);
            levelV = ScaleDBFsToInt(level, levelM);
            levelP = ScaleDBFsToInt(peakLevel, levelM);

            base.OnPaint(e);
            Rectangle r = new Rectangle(0,0,Width,Height);
            if (this.level > minLevel)
            {
                using (Brush brush = new System.Drawing.SolidBrush(LevelColor))
                {
                    if (orientation == MeterOrientation.Horizontal)
                    {
                        r.X = 0;
                        r.Width = Math.Min(levelH, levelV);
                    }
                    if (orientation == MeterOrientation.Vertical)
                    {
                        r.Y = 0;
                        r.Height = Math.Min(levelH, levelV);
                    }
                    e.Graphics.FillRectangle(brush, r.X, r.Y, r.Width, r.Height);
                }
            }
            if (this.level > headroomLevel)
            {
                using (Brush brush = new System.Drawing.SolidBrush(HeadroomLevelColor))
                {
                    if (orientation == MeterOrientation.Horizontal)
                    {
                        r.X = levelH;
                        r.Width = Math.Min((Math.Max(levelH, levelV) - levelH), levelC);

                    }
                    if (orientation == MeterOrientation.Vertical)
                    {
                        r.Y = levelH;
                        r.Height = Math.Min ( (Math.Max(levelH, levelV) - levelH), levelC);
                    }
                    e.Graphics.FillRectangle(brush, r.X, r.Y, r.Width, r.Height);
                }
            }
            if (this.level > clipLevel)
            {
                using (Brush brush = new System.Drawing.SolidBrush(ClipLevelColor))
                {
                    if (orientation == MeterOrientation.Horizontal)
                    {
                        r.X = levelC;
                        r.Width = Math.Min((Math.Max(levelC, levelV) - levelC), levelM);

                    }
                    if (orientation == MeterOrientation.Vertical)
                    {
                        r.Y = levelC;
                        r.Height = Math.Min((Math.Max(levelC, levelV) - levelC), levelM);
                    }
                    e.Graphics.FillRectangle(brush, r.X, r.Y, r.Width, r.Height);
                }
            }
            if (DrawPeak )
            {
                using (Brush brush = new System.Drawing.SolidBrush(PeakColor))
                {
                    if (orientation == MeterOrientation.Horizontal)
                    {
                        r.X = levelP;
                        r.Width = 2;

                    }
                    if (orientation == MeterOrientation.Vertical)
                    {
                        r.Y = levelP;
                        r.Height = 2;
                    }
                    e.Graphics.FillRectangle(brush, r.X, r.Y, r.Width, r.Height);
                }
            }

            if (BorderStyle != BorderStyle.None)
            {
                using (Pen pen = new System.Drawing.Pen(this.BackColor ))
                {
                    e.Graphics.DrawRectangle( pen, 0, 0, Width -1, Height - 1);
                }
            }
        }
    }
}
