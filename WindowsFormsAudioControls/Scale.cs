using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordAudioStream
{
    public partial class Scale : UserControl
    {
        public enum ScaleOrientation
        {
            Horizontal,
            Vertical
        }


        private int min = -60;
        private int max = 0;
        private int smallGradation = 2;
        private int largeGradation = 10;
        private ScaleOrientation orientation = ScaleOrientation.Horizontal;
        private Color smallGrationColor = Color.LightGray;
        private Color largeGrationColor = Color.DarkGray ;


        public int Min { get => min; set => min = value; }
        public int Max { get => max; set => max = value; }
        public int SmallGradation { get => smallGradation; set => smallGradation = value; }
        public int LargeGradation { get => largeGradation; set => largeGradation = value; }
        public ScaleOrientation Orientation { get => orientation; set => orientation = value; }
        public Color SmallGrationColor { get => smallGrationColor; set => smallGrationColor = value; }
        public Color LargeGrationColor { get => largeGrationColor; set => largeGrationColor = value; }

        public Scale()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int fullScale = 0;
            float scaleFactor;
            
            if (Orientation == ScaleOrientation.Horizontal)
                fullScale = Width-1;
            else if (Orientation == ScaleOrientation.Vertical)
                fullScale = Height-1;

            scaleFactor = Math.Abs(  (float)fullScale / ( (float) min  - (float) max ));

            base.OnPaint(e);
            using (Pen pen = new System.Drawing.Pen(this.smallGrationColor))
            {
                for (int val = min, gradationIndex =0; val <= max; val += smallGradation, gradationIndex++)
                {
                    int pos = (int) ( (float) gradationIndex * (float) smallGradation * scaleFactor);
                    if (Orientation == ScaleOrientation.Horizontal)                       
                        e.Graphics.DrawLine(pen, pos, 0, pos, 3);
                    else if (Orientation == ScaleOrientation.Vertical)
                        e.Graphics.DrawLine(pen, 0, pos, 3, pos);
                }
            }

            using (Pen pen = new System.Drawing.Pen(this.largeGrationColor))
            {
                for (int val = min, gradationIndex = 0; val <= max; val += largeGradation, gradationIndex++)
                {
                    int pos = (int)((float)gradationIndex * (float)largeGradation * scaleFactor);
                    if (Orientation == ScaleOrientation.Horizontal)
                        e.Graphics.DrawLine(pen, pos, 0, pos, 5);
                    else if (Orientation == ScaleOrientation.Vertical)
                        e.Graphics.DrawLine(pen, 0, pos, 5, pos);

                }
            }

            using (Brush brush = new System.Drawing.SolidBrush(this.largeGrationColor))
            {
                //

                for (int val = min , gradationIndex = 0; val <= max; val += largeGradation, gradationIndex++)
                {
                    int pos = (int)((float)gradationIndex * (float)largeGradation * scaleFactor);
                    string stringValue = $"{val}"; ;
                    SizeF sf = e.Graphics.MeasureString(stringValue, this.Font);

                    if (Orientation == ScaleOrientation.Horizontal)
                    {
                        if (val == max) pos -= (int)sf.Width;
                        else if (val > min) pos -= (int)sf.Width / 2;

                        e.Graphics.DrawString(stringValue, this.Font, brush, pos , 7);
                    }
                    else if (Orientation == ScaleOrientation.Vertical)
                    {
                        if (val == max) pos -= (int)sf.Height;
                        else if (val > min) pos -= (int)sf.Height / 2;

                        e.Graphics.DrawString(stringValue, this.Font, brush, 7, pos );
                    }
                }
            }
        }
    }
}
