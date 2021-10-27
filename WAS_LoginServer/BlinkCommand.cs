using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WAS_LoginServer
{
    class BlinkCommand : RobotCommand
    {
        public ulong[] intervals { get; set; } = new ulong[2];
        public Color[] colors { get; set; } = new Color[2];
        public Side side { get; set; }

        public BlinkCommand(ulong interval1, int r1, int g1, int b1, ulong interval2, int r2, int g2, int b2, Side side) 
            : this(interval1, Color.FromArgb(r1, g1, b1), interval2, Color.FromArgb(r2, g2, b2), side) { }

        public BlinkCommand(ulong interval1, Color color1, ulong interval2, Color color2, Side side) : base(Command.BlinkColor)
        {
            intervals[0] = interval1;
            intervals[1] = interval2;

            colors[0] = color1;
            colors[1] = color2;

            this.side = side;
        }
    }
}
