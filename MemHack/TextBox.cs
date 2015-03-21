using System;
using SharpDX.Direct3D9;

namespace MemHack
{
    public class TextBox
    {
        public string Text;
        public string Name;
        public bool Render;
        public bool Selected;
        public int X;
        public int Y;
        public int Width;

        public TextBox(int X, int Y, int Width, string Name, string Text)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Name = Name;
            this.Text = Text;
        }
    }
}