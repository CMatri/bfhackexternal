using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MemLibs;

namespace MemHack
{
    public class Program
    {

        private Overlay overlay;

        public Program()
        {
            overlay = new Overlay();
            Application.Run(overlay);
        }

        static void Main(string[] args)
        {
            new Program();
            Console.WriteLine("Press any key to continue ... ");
            Console.ReadKey();
        }
    }
}
