using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using static System.Net.Mime.MediaTypeNames;

namespace ZastitaInformacija
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private string textToDisplay;
        private string textToSend;
        private BifidCipher bifidCipher = new BifidCipher();
        private RC6 rc6=new RC6();
        public Form1()
        {
            InitializeComponent();
            var adrese = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress adr in adrese) 
            {
                if(adr.AddressFamily == AddressFamily.InterNetwork)
                {
                    txtLocalIP.Text=adr.ToString();
                    txtRemoteIP.Text= adr.ToString(); 
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label8.Visible = false;
            txtRich.Visible = false;
            /*string kripotvani= RC6.Encrypt("abcdefghkdhjvbdh");
            byte[] bajt = Encoding.UTF8.GetBytes("abcdefghkdhjvbdh");
            txtRich.Text = Encoding.UTF8.GetString(bajt);
            //txtRich.Text = krip;
            txtSend.Text = RC6.Decrypt(kripotvani);*/
        }
       

        private  void Button1_ClickAsync(object sender, EventArgs e)
        {

                client = new TcpClient();
            
                IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(txtRemoteIP.Text), int.Parse(txtRemotePort.Text));
                client.Connect(IpEnd);
            
                try
                {
                if (client.Connected == true)
                {
                    Button1.Enabled = false;
                    BtnStart.Enabled = false;
                    listBox1.Items.Add("Connected to Server, You can start messaging." + "\n");
                    stream = client.GetStream();
                    stream.Flush();
                    backgroundWorker1.RunWorkerAsync();
                    backgroundWorker2.WorkerSupportsCancellation = true;
                }
                //else return;


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                finally
                {
               // client.Close();
                }
            

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while(client.Connected)
            {
                try
                {
                    if (stream.CanRead)
                    {
                        byte[] buffer = new byte[1024];
                        int numberOfBytesRead = 0;
                        StringBuilder strBuilder = new StringBuilder();
                        do
                        {
                  
                            numberOfBytesRead = stream.Read(buffer, 0, buffer.Length);
                            strBuilder.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, numberOfBytesRead));
                           
                        }
                        while (stream.DataAvailable);
                        if(numberOfBytesRead > 0)
                        {
                            string textToDisplay = strBuilder.ToString();
                            if(textToDisplay.EndsWith("1")) //flag za dekripciju
                            {
                                textToDisplay = textToDisplay.Remove(textToDisplay.Length - 1);
                                this.txtRich.Invoke(new MethodInvoker(delegate ()
                                {
                                    txtRich.Text=textToDisplay;
                                }));
                                while(!ValidateKeyInput())
                                {
                                    MessageBox.Show("Invali key, there can be no reppeating letter " +
                                    "nor more than 25 letters");
                                    txtKey.Focus();
                                }
                                bifidCipher.GeneratePolybus(txtKey.Text);
                                textToDisplay = bifidCipher.Decrypt(textToDisplay);
                            }
                             this.listBox1.Invoke(new MethodInvoker(delegate ()
                            {
                                listBox1.Items.Add("Friend :>>  " + textToDisplay + "\n");
                            }));
                        }
                       
                        strBuilder.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Sorry. You can not read from this stream. ");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                finally
                {
                    //stream.Dispose();
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            
                try
                {
                    byte[] writeBuffer = Encoding.ASCII.GetBytes(textToSend);
                    stream.Write(writeBuffer, 0, writeBuffer.Length);

                    this.listBox1.Invoke(new MethodInvoker(delegate ()
                    {
                        listBox1.Items.Add("Me :>>  " + textToDisplay + "\n");
                    }));

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            
            
            backgroundWorker2.CancelAsync();
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            
                TcpListener listener = new TcpListener(IPAddress.Parse(txtLocalIP.Text), int.Parse(txtLocalPort.Text));
                listener.Start();
            try
            {
                BtnStart.Enabled = false;
                client = await listener.AcceptTcpClientAsync();
                if (client.Connected == true) 
                { 
                    BtnStart.Enabled = false;
                    Button1.Enabled = false;
                    listBox1.Items.Add("You can start messaging."+"\n");
                }
                stream = client.GetStream();
                await stream.FlushAsync();
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.WorkerSupportsCancellation = true;
               
                
            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "kaboom");
            }
            finally 
            {
                listener.Stop();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(txtSend.Text != "" && txtKey.Text == "")
            {
                textToSend=textToDisplay=txtSend.Text;
                backgroundWorker2.RunWorkerAsync();
                txtSend.Focus();
            }
            else
            {
                if (txtSend.Text != "" && txtKey.Text != "")
                {
                    if(!ValidateKeyInput())
                    {
                        txtSend.Text = "";
                        MessageBox.Show("There can be no reppeating letter " +
                            "nor more than 25 letters");
                        txtSend.Focus();
                    }
                    else
                    {
                        textToSend = textToDisplay = txtSend.Text;
                        bifidCipher.GeneratePolybus(txtKey.Text);
                        textToSend = bifidCipher.EncryptMessage(textToSend);
                        this.txtRich.Invoke(new MethodInvoker(delegate ()
                        {
                            txtRich.Text = textToSend;
                        }));
                        textToSend += "1";
                        backgroundWorker2.RunWorkerAsync();
                        txtSend.Focus();
                    }
                    
                }
            }
               
            txtSend.Text = "";
            txtSend.Focus();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(stream != null)
                stream.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked==true)
            {
                label8.Visible = true;
                txtRich.Visible = true;
            }
            else
            {
                label8.Visible = false;
                txtRich.Visible = false;
            }
        }

        private void txtLocalPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtKey_KeyPress(object sender, KeyPressEventArgs e)
        {
           if(!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
        }

       public bool ValidateKeyInput()
        {
            var input = txtKey.Text;
            if (input.Length > 25 || (!CheckForRepeatingLetters(input)))
                return false;
            else
                return true;

        }
        public bool CheckForRepeatingLetters(string input)
        {
            var set = new HashSet<char>();
            set.Add((char)(input[0]));
            for (int i = 1; i < input.Length; i++)
            {
                var condition=set.Add((char)(input[i]));
                if (!condition)
                {
                    return false;
                }

            }
            return true;
        }
    }
}

