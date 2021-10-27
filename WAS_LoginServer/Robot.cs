using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WAS_LoginServer
{
    class Robot
    {
        public Socket m_Socket { get; set; }
        public string m_Name { get; set; }

        public Robot(Socket s, string name)
        {
            m_Socket = s;
            m_Name = name;
        }

        public void Dispose()
        {

        }

        ~Robot()
        {
            m_Socket.Close();
            m_Socket = null;
        }
    }
}
