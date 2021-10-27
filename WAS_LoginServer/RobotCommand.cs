using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAS_LoginServer
{
    enum Command : ulong
    {
        None,
        ColorOff,
        ColorSet,
        BlinkColor,
        ColorBack
    }
    

    class RobotCommand
    {
        public enum Side : ushort
        {
            Both,
            Left,
            Right
        }
        public Command type { get; set; }

        protected RobotCommand(Command type)
        {
            this.type = type;
        }
    }
}
