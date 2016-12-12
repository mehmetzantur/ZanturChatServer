using NetworksApi.TCP.SERVER;
using System.Text;
using System.Windows.Forms;

namespace ZanturChatServer
{

    public delegate void UpdateMessage(string txt);
    public delegate void UpdateListBox(ListBox box, string value, bool remove);
    public delegate void UpdateCounter(int count);

    public partial class frmServer : Form
    {
        Server server;

        public frmServer()
        {
            InitializeComponent();
        }

        private void frmServer_Load(object sender, System.EventArgs e)
        {
            tabControl1.SelectTab(tabPage2);
            tabControl1.TabPages.Remove(tabPage1);
        }


        private void ChangeMessage(string txt)
        {
            if (txtMessages.InvokeRequired)
            {
                Invoke(new UpdateMessage(ChangeMessage), new object[] { txt });
            }
            else
            {

                txtMessages.Text += txt + "\r\n";
                txtMessages.SelectionStart = txtMessages.Text.Length;
                txtMessages.ScrollToCaret();
            }
        }




        private void ChangeListBox(ListBox box, string value, bool remove)
        {
            if (box.InvokeRequired)
            {
                Invoke(new UpdateListBox(ChangeListBox), new object[] { box, value, remove });
            }
            else
            {
                if (remove)
                {
                    box.Items.Remove(value);
                }
                else
                {
                    box.Items.Add(value);
                }
            }
        }


        private void ChangeCounter(int count)
        {
            if (tabPage1.InvokeRequired)
            {
                Invoke(new UpdateCounter(ChangeCounter), new object[] { count });
            }
            else
            {
                tabPage1.Text = "Online Users (" + count.ToString() + ")";
            }
        }



        private void server_OnDataReceived(object Sender, ReceivedArguments R)
        {
            string[] data = R.ReceivedData.Split('#');
            string ReceivedChat = R.Name + " : " + R.ReceivedData;
            ChangeMessage(ReceivedChat);

            
            
            if (data[0] == "privMsg")
            {
                server.SendTo(data[1], "privMsg#" + R.Name + "#" + data[2]);
            }
            else if (data[0] == "userAllMsg")
            {
                string yazi = R.ReceivedData;
                server.BroadCast(yazi);
            }

            txtMessages.SelectionStart = txtMessages.Text.Length;
            txtMessages.ScrollToCaret();
        }


        private void server_OnClientDisconnected(object Sender, DisconnectedArguments R)
        {
            ChangeListBox(listBox1, R.Name, true);
            ChangeCounter(server.NumberOfConnections);
            StringBuilder sendingInfo = new StringBuilder();
            sendingInfo.Append(server.NumberOfConnections.ToString());
            for (int i = 0; i < server.NumberOfConnections; i++)
            {
                CheckForIllegalCrossThreadCalls = false;
                string userName = listBox1.Items[i].ToString();
                sendingInfo.Append("#");
                sendingInfo.Append(userName);
            }

            server.BroadCast(sendingInfo.ToString());
        }

        private void server_OnClientConnected(object Sender, ConnectedArguments R)
        {
            //server.BroadCast(R.Name + " Has Connected");
            ChangeListBox(listBox1, R.Name, false);
            ChangeCounter(server.NumberOfConnections);
            StringBuilder sendingInfo = new StringBuilder();
            sendingInfo.Append(server.NumberOfConnections.ToString());
            for (int i = 0; i < server.NumberOfConnections; i++)
            {
                CheckForIllegalCrossThreadCalls = false;
                string userName = listBox1.Items[i].ToString();
                sendingInfo.Append("#");
                sendingInfo.Append(userName);
            }

            server.BroadCast(sendingInfo.ToString());

        }



        



        private void btnSendAll_Click(object sender, System.EventArgs e)
        {
            if (txtChat.Text != "")
            {
                string ReceivedChat = txtChat.Text;
                server.BroadCast("allMsg#" + ReceivedChat);
                ChangeMessage(ReceivedChat);
                txtChat.Clear();
                txtChat.SelectionStart = txtChat.Text.Length;
                txtChat.ScrollToCaret();
            }
            else
            {
                MessageBox.Show("Please enter your message!");
            }
        }

        private void btnUserKick_Click(object sender, System.EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                server.DisconnectClient((string)listBox1.SelectedItem);
            }
            else
            {
                MessageBox.Show("Please select user for kick!");
            }
        }

        private void frmServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            listBox1.SelectedIndex = listBox1.SelectedIndex;
        }


        bool isOpen=false;
        private void btnStart_Click(object sender, System.EventArgs e)
        {
            if (isOpen==false)
            {
                if (txtIp.Text != "" && txtPort.Text != "")
                {
                    server = new Server(txtIp.Text, txtPort.Text);
                    server.OnClientConnected += new OnConnectedDelegate(server_OnClientConnected);
                    server.OnClientDisconnected += new OnDisconnectedDelegate(server_OnClientDisconnected);
                    server.OnDataReceived += new OnReceivedDelegate(server_OnDataReceived);
                    //server.OnServerError += new OnErrorDelegate(server_OnServerError);
                    server.Start();
                    isOpen = true;
                    btnStart.Text = "Stop";
                    tabControl1.TabPages.Insert(0, tabPage1);
                    tabControl1.SelectTab(tabPage1);
                }
                else
                {
                    MessageBox.Show("Please enter ip and port numbers!");

                }
            }
            else
            {
                server.Stop();
                isOpen = false;
                btnStart.Text = "Start";
            }
        }




        private void MesajDegisim(string txt)
        {
            if (txtMessages.InvokeRequired)
            {
                Invoke(new UpdateMessage(MesajDegisim), new object[] { txt });
            }
            else
            {

                txtMessages.Text += txt + "\r\n";
                txtMessages.SelectionStart = txtMessages.Text.Length;
                txtMessages.ScrollToCaret();
            }
        }



        private void button1_Click(object sender, System.EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen Kullanıcı Seçiniz..");
            }
            else if (txtChat.Text != "")
            {
                string yazi = "adminMsg#" + txtChat.Text;
                server.SendTo((string)listBox1.SelectedItem, yazi);
                if (listBox1.SelectedItem != null)
                {
                    MesajDegisim(yazi);
                    txtChat.Clear();
                }
                txtMessages.SelectionStart = txtMessages.Text.Length;
                txtMessages.ScrollToCaret();

            }
            else
            {
                MessageBox.Show("Lütfen Mesajınızı Giriniz..");
            }
        }
    }
}
