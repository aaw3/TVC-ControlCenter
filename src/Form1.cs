using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyboardHook1;
using Thread = System.Threading.Thread;
using System.Runtime.InteropServices;

namespace ControlCenter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static TcpClient client;
        byte[] buffer = new byte[1];
        Timer t = new Timer();

        //KeyboardHook hook = new KeyboardHook();
        private void Form1_Load(object sender, EventArgs e)
        {


            Form2 f2 = new Form2(this);
            f2.Show();
            t.Enabled = true;
            t.Interval = 1000;
            t.Tick += (object s,  EventArgs eargs) =>
            {
                //DisplayAlert("Timer", "Timer Interval", "OK");
                if (client != null)
                {
                    if (client.Connected)
                    {
                        if (client.Client.Poll(0, SelectMode.SelectRead))
                        {
                            try
                            {
                                if (client.Client.Receive(buffer, SocketFlags.Peek) == 0)
                                {
                                    //DisplayAlert("Client Disconnected", "Client Disconnected", "OK");
                                    ClientDisconnected();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error while testing connection, restarting...");
                                ClientDisconnected();
                            }
                        }
                    }
                }
            };

            t.Start();
        }
        
        public void ClientDisconnected()
        {
            try
            {
                client.Close();
                client.GetStream().Close();
            }
            catch (Exception ex)
            {

            }
            t.Stop();
            Invoke(new Action(() => label4.Text = "Not Connected"));
            DisconnectionBeep();
        }

        public void DisconnectionBeep()
        {
            Thread thr = new Thread(() => {
                Thread.Sleep(500);
                Console.Beep(1200, 250);
                Console.Beep(1000, 250);
                Console.Beep(800, 250);
            });
            thr.Start();
        }



        static string IPstring;
        static int connectToPort;
        Image bitmapImage;
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
                return;

            SendCommand(textBox1.Text);
        }

        public void SendCommand(string message)
        {

            if (message.Length >= 5)
            {
                if (message.Substring(0, 4) == "cmd:")
                {
                    Debug.WriteLine("{cmd}" + message.Substring(4) + "{/cmd}");
                    writeMessage("{cmd}" + message.Substring(4) + "{/cmd}");
                    return;
                }
                else if (message.Substring(0, 4) == "key:")
                {
                    Debug.WriteLine("{key}" + message.Substring(4) + "{/key}");
                    writeMessage("{key}" + message.Substring(4) + "{/key}");
                    return;
                }
                else if (message.Substring(0, 4) == "tip:")
                {
                    Debug.WriteLine("{tip}" + message.Substring(4) + "{/tip}");
                    writeMessage("{tip}" + message.Substring(4) + "{/tip}");
                }
            }

            if (!writeMessage(message))
            {
                return;
            }
            if (message == "{Screenshot}")
            {

                Debug.WriteLine("Begin Read");
                var BitmapData = getData(client, 1024 * 128);
                Debug.WriteLine("End Read");

                MemoryStream ms = new MemoryStream();
                ms.Write(BitmapData, 0, BitmapData.Length);
                bitmapImage = new Bitmap(ms, false);
                ms.Dispose();

                pictureBox1.Image = bitmapImage;
            }
            else if (message == "{KillTV}")
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Contains(":"))
            {
                string[] ConnectionStrings = textBox3.Text.Split(':');
                IPstring = ConnectionStrings[0];
                connectToPort = int.Parse(ConnectionStrings[1]);

                client = new TcpClient();

                client = tryConnect().Result;

                if (client == null)
                {
                    MessageBox.Show("Port Connection Unsuccessful!\nCheck if the host is running, checked the IP Address, and formatted correctly", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Invoke(new Action(() => label4.Text = "Not Connected"));
                    return;
                }

                if (client.Connected)
                {
                    t.Start();
                    label4.Text = "Connected";
                }

                Debug.WriteLine($"IP: {IPstring} | Port: {connectToPort}");
            }
            else
            {
                MessageBox.Show("Improper Formatting!", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        public static int tryConnectTime = 1000;
        public static async Task<TcpClient> tryConnect()
        {
            try
            {
                if (client == null)
                {
                    client = new TcpClient();
                }

                var connectionTask = client.ConnectAsync(IPAddress.Parse(IPstring), connectToPort).ContinueWith(task =>
                {
                    return task.IsFaulted ? null : client;
                }, TaskContinuationOptions.ExecuteSynchronously);
                var timeoutTask = Task.Delay(tryConnectTime).ContinueWith<TcpClient>(task => null, TaskContinuationOptions.ExecuteSynchronously);
                var resultTask = Task.WhenAny(connectionTask, timeoutTask).Unwrap();
                resultTask.Wait();
                var resultTcpClient = await resultTask;

                return resultTcpClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();

                return null;
            }
        }

        public string getDataAsString(TcpClient client)
        {
            byte[] bytes = getData(client);
            if (bytes != null)
            {
                return Encoding.ASCII.GetString(bytes);
            }
            else
            {
                return null;
            }
        }

        public byte[] getData(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] fileSizeBytes = new byte[4];
                int bytes = stream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
                Debug.WriteLine("BYTES TO GET: " + bytes);
                int dataLength = BitConverter.ToInt32(fileSizeBytes, 0);

                int bytesLeft = dataLength;
                byte[] data = new byte[dataLength];

                int buffersize = 1024;
                int bytesRead = 0;

                while (bytesLeft > 0)
                {
                    int curDataSize = Math.Min(buffersize, bytesLeft);
                    if (client.Available < curDataSize)
                    {
                        curDataSize = client.Available;
                    }

                    bytes = stream.Read(data, bytesRead, curDataSize);
                    bytesRead += curDataSize;
                    bytesLeft -= curDataSize;
                    Debug.WriteLine("DATA REMAINING: " + curDataSize);
                }

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public byte[] getData(TcpClient client, int customBufferSize)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] fileSizeBytes = new byte[4];
                int bytes = stream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
                int dataLength = BitConverter.ToInt32(fileSizeBytes, 0);

                int bytesLeft = dataLength;
                byte[] data = new byte[dataLength];

                int bytesRead = 0;

                while (bytesLeft > 0)
                {
                    int curDataSize = Math.Min(customBufferSize, bytesLeft);
                    if (client.Available < curDataSize)
                    {
                        curDataSize = client.Available;
                    }

                    bytes = stream.Read(data, bytesRead, curDataSize);
                    bytesRead += curDataSize;
                    bytesLeft -= curDataSize;
                    Debug.WriteLine("DATA REMAINING: " + curDataSize);
                }

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool writeMessage(string input)
        {
            try
            {
                if (client == null)
                {
                    throw new ObjectDisposedException(client.ToString());
                }
                NetworkStream ns = client.GetStream();
                byte[] message = Encoding.ASCII.GetBytes(input);
                ns.Write(message, 0, message.Length);
                return true;
            }
            catch (Exception ex)
            {
                DisconnectionBeep();

                label4.Text = "Not Connected";
                return false;
            }
        }

        public void sendData(byte[] data)
        {
            try
            {
                if (client == null)
                {
                    throw new ObjectDisposedException(client.ToString());
                }
                NetworkStream ns = client.GetStream();
                ns.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form2.ShuttingDown();

            Application.Exit();
        }

        public string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
    }
}
