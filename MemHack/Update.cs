using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MemHack
{
    class Update : Timer
    {
        public Update(EventHandler eventHandler)
        {
            Interval = 1000 / fps;
            Tick += eventHandler;
        }

        int fps = 25;
        public int FPS
        {
            get
            {
                return fps;
            }
            set
            {
                Interval = 1000 / value;
                fps = value;
            }
        }
    }
}
