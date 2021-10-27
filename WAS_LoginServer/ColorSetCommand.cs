using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WAS_LoginServer
{
    class ColorSetCommand : RobotCommand
    {
        public Color color { get; set; }
        public Side side { get; set; }

        public ColorSetCommand(Color color, Side side) : base(Command.ColorSet)
        {
            this.color = color;
            this.side = side;
        }
    }
}
