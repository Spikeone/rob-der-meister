using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using System.Text.Json;

namespace WAS_LoginServer
{
    public partial class frmLoginServer : Form
    {
        private byte[] m_buffer = new byte[8092];
        private Socket m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        List<Robot> m_listRobots;

        public frmLoginServer()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            string[] commands = new string[]{
                "Color Off Both",
                "Color Off Left",
                "Color Off Right",
                "Color Red Both",
                "Color Red Left",
                "Color Red Right",
                "Blink Yellow Black Both",
                "Blink Yellow Black Left",
                "Blink Yellow Black Right"
            };

            cbCommands.Items.AddRange(commands);

            m_listRobots = new List<Robot>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupServer();
        }

        private void SetupServer()
        {
            lblStatus.Text = "Starting";
            Log("Setting up server...");
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 3665));

            m_ServerSocket.NoDelay = true;

            m_ServerSocket.Listen(1);

            m_ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            lblStatus.Text = "Running";
            lblStatus.BackColor = Color.Green;
            Log("...running!");
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket s = m_ServerSocket.EndAccept(ar);
            lbxClients.Items.Add(s.RemoteEndPoint.ToString());
            lblConnectedClients.Text = "Connected Robots: " + m_listRobots.Count.ToString();
            Log("New client connected from: " + s.RemoteEndPoint.ToString());
            s.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
            m_ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            if (s.Connected)
            {
                int receivedBytes;
                try
                {
                    receivedBytes = s.EndReceive(ar);
                }
                catch (Exception)
                {
                    for (int i = 0; i < m_listRobots.Count; i++)
                    {
                        if (m_listRobots[i].m_Socket.RemoteEndPoint.ToString().Equals(s.RemoteEndPoint.ToString()))
                        {
                            lbxClients.Items.RemoveAt(lbxClients.Items.IndexOf(s.RemoteEndPoint.ToString()));
                            Log("Robot " + s.RemoteEndPoint.ToString() + " disconnected");

                            removeRobotBySocket(s);
                            lblConnectedClients.Text = "Connected Robots: " + m_listRobots.Count.ToString();
                        }
                    }
                    return;
                }

                if (receivedBytes != 0)
                {
                    byte[] dataBuffer = new byte[receivedBytes];
                    Array.Copy(m_buffer, dataBuffer, receivedBytes);

                    string strReceived = Encoding.UTF8.GetString(dataBuffer);

                    HandlePacket(s, strReceived);
                }
                else
                {
                    for (int i = 0; i < m_listRobots.Count; i++)
                    {
                        if (m_listRobots[i].m_Socket.RemoteEndPoint.ToString().Equals(s.RemoteEndPoint.ToString()))
                        {
                            lbxClients.Items.RemoveAt(lbxClients.Items.IndexOf(s.RemoteEndPoint.ToString()));
                            removeRobotBySocket(s);
                            lblConnectedClients.Text = "Connected Robots: " + m_listRobots.Count.ToString();
                            Log("Robot " + s.RemoteEndPoint.ToString() + " sent emtpy packet, disconnected");
                        }
                    }
                    return;
                }
                try
                {
                    s.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
                }
                catch
                { }
            }
        }

        private void SendData(Socket s, string strMessage)
        {
            try
            {
                byte[] bytData = Encoding.UTF8.GetBytes(strMessage);
                s.BeginSend(bytData, 0, bytData.Length, SocketFlags.None, new AsyncCallback(SendCallback), s);
                m_ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                Log(s.RemoteEndPoint.ToString() + " ==> " + strMessage);
            }
            catch
            { }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                s.EndSend(ar);
            }
            catch
            { }
        }

        private void SendDataAll(string strMessage)
        {
            for (int i = 0; i < m_listRobots.Count; i++)
            {
                SendData(m_listRobots[i].m_Socket, strMessage);
            }
        }

        private void Respond(Socket s, string strMessage)
        {
            SendData(s, strMessage);
        }

        private void HandlePacket(Socket s, string strData)
        {
            string[] splittedData = strData.Split('/');
            switch (splittedData[0])
            {
                default:
                    Log("Unknown packet: " + strData);
                    break;
                case "0x000":
                    Log("Login request: " + strData);
                    handleLoginRequest(s, strData);
                    break;
            }
        }

        public void handleLoginRequest(Socket s, string strData)
        {
            string[] splittedData = strData.Split('/');
            // 0 = 0x000 (Connection Request)       splittedData[0]
            // 1 = Name                             splittedData[1]

            int listIndex = lbxClients.Items.IndexOf(s.RemoteEndPoint.ToString());

            if (listIndex == -1)
                return;

            m_listRobots.Add(new Robot(s, splittedData[1]));
            lblConnectedClients.Text = "Connected Robots: " + m_listRobots.Count.ToString();
        }

        private void removeRobotBySocket(Socket s)
        {
            //m_listPlayerObject.RemoveAll(item => item.m_Socket == s);
            List<Robot> listTempRobots = m_listRobots.FindAll(item => item.m_Socket == s);

            foreach(Robot rob in listTempRobots)
            {
                rob.Dispose();
            }

            m_listRobots.RemoveAll(item => item.m_Socket == s);
        }

        private void Log(string strMessage)
        {
            txbLog.AppendText(strMessage + Environment.NewLine);
        }

        private void btnToSelected_Click(object sender, EventArgs e)
        {
            //SendData(,txbMessage.Text);
            //txbMessage.Text = "";
        }

        private void btnSendToAll_Click(object sender, EventArgs e)
        {
            SendDataAll(txbMessage.Text);
            txbMessage.Text = "";
        }

        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            int cmdIndex = cbCommands.SelectedIndex;
            Log("Selected Command Index: " + cmdIndex);

            string strCommand;

            switch(cmdIndex)
            {
                case 0:
                    strCommand = JsonSerializer.Serialize(new ColorOffCommand(RobotCommand.Side.Both));
                    break;
                case 1:
                    strCommand = JsonSerializer.Serialize(new ColorOffCommand(RobotCommand.Side.Left));
                    break;
                case 2:
                    strCommand = JsonSerializer.Serialize(new ColorOffCommand(RobotCommand.Side.Right));
                    break;
                case 3:
                    strCommand = JsonSerializer.Serialize(new ColorSetCommand(Color.Red, RobotCommand.Side.Both));
                    break;
                case 4:
                    strCommand = JsonSerializer.Serialize(new ColorSetCommand(Color.Red, RobotCommand.Side.Left));
                    break;
                case 5:
                    strCommand = JsonSerializer.Serialize(new ColorSetCommand(Color.Red, RobotCommand.Side.Right));
                    break;
                case 6:
                    strCommand = JsonSerializer.Serialize(new BlinkCommand(1000, Color.Yellow, 500, Color.Black, RobotCommand.Side.Both));
                    break;
                case 7:
                    strCommand = JsonSerializer.Serialize(new BlinkCommand(1000, Color.Yellow, 500, Color.Black, RobotCommand.Side.Left));
                    break;
                case 8:
                    strCommand = JsonSerializer.Serialize(new BlinkCommand(1000, Color.Yellow, 500, Color.Black, RobotCommand.Side.Right));
                    break;
                default:
                    Log("Invalid Command Index!");
                    return;
            }

            Log("Serilized Command: " + strCommand);
            SendDataAll(strCommand);
        }
    }
}
