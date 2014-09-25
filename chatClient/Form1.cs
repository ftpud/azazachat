using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Threading;

namespace chatClient
{
    public partial class Form1 : Form
    {

        

        public Form1()
        {
            InitializeComponent();
        }

        chatClient chat;
        System.Windows.Forms.Timer bgworker = new System.Windows.Forms.Timer();
        private void Form1_Load(object sender, EventArgs e)
        {
            chat = new chatClient();
            bgworker.Interval = 100;
            bgworker.Start();
            bgworker.Tick += bgworker_Tick;
                        
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            chat.Stop();
            base.OnClosing(e);
        }

        void bgworker_Tick(object sender, EventArgs e)
        {
            string s = chat.getMessage();
            if (s != "") { listBox1.Items.Insert(0, s); }
        }

        private void send_Click(object sender, EventArgs e)
        {
            chat.sendMessage(textBox1.Text);
            textBox1.Text = "";
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
         
        }
    }

    // chat client class
    public class chatClient
    {
        int port = 12312;
        TcpClient client = new TcpClient();


        List<string> Msgs = new List<string>();
        List<string> SendMessages = new List<string>();

        public void Stop()
        { cThread.Abort(); }

        public string getMessage()
        {
            if (Msgs.Count > 0)
            { 
                lock(Msgs)
                {
                    string s = Msgs[0];
                    Msgs.RemoveAt(0);
                    return s;
                }
            }
            return "";
        }

        public void sendMessage(string str)
        {
            SendMessages.Add(str);
        }

        Thread cThread;
        public chatClient()
        {
            client.Connect("localhost", port);
            if (client.Connected)
            {
                 cThread = new Thread(new ParameterizedThreadStart(chatThread));
                cThread.Start(client);
            }
        }

        void chatThread(object client)
        {
            TcpClient Client = (TcpClient)client;
            NetworkStream ns = Client.GetStream();

            while (Client.Connected)
            {
                try {           
                    // read
                    if(ns.DataAvailable)
                    {
                        byte[] buf = new byte [4096];
                        int read = ns.Read(buf, 0, 4096);
                        string input = UTF8Encoding.UTF8.GetString(buf, 0, read);
                        lock(Msgs)
                        {
                        Msgs.Add(input);
                        }
                    }
                    // write
                    if (SendMessages.Count > 0)
                    { 
                        byte[] buff = UTF8Encoding.UTF8.GetBytes( SendMessages[0] );
                        lock(SendMessages){
                        SendMessages.RemoveAt(0);
                        }
                        ns.Write(buff, 0, buff.Length);
                    }
                }
                catch 
                {
                    Msgs.Add("disconnected ><");
                    return;
                }
                Thread.Sleep(100);
            }
        }

    }
}
