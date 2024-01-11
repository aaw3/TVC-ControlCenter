using KeyboardHook1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlCenter
{
    public partial class Form2 : Form
    {
        Form1 f1;
        public Form2(Form1 form1Instance)
        {
            f1 = form1Instance;
            InitializeComponent();
        }

        public static KeyboardHook hook = new KeyboardHook();
        private void Form2_Load(object sender, EventArgs e)
        {
            hook.KeyDown += Hook_KeyDown;
            hook.Install();
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{OpenTV}");
            Debug.WriteLine("Turning on TeamViewer");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{KillTV}");
            Debug.WriteLine("Turning off TeamViewer");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{ScreenON}");
            Debug.WriteLine("Turning on Screen");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{ScreenOFF}");
            Debug.WriteLine("Turning off Screen");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{UnlockKeyboard}");
            Debug.WriteLine("Unlocking Keyboard");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{LockKeyboard}");
            Debug.WriteLine("Locking Keyboard");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{Screenshot}");
            Debug.WriteLine("Taking Screenshot");
        }
        private void button8_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{UnblankScreen}");
            Debug.WriteLine("Unblanking Screen");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{BlankScreen}");
            Debug.WriteLine("Blanking Screen");
        }

        public void Hook_KeyUp(KeyboardHook.VKeys key)
        {

        }

        public static void ShuttingDown()
        {
            hook.Uninstall();
        }

        int MouseSpeed;
        public void Hook_KeyDown(KeyboardHook.VKeys key)
        {
            Debug.WriteLine(key);

            switch (key)
            {

                case KeyboardHook.VKeys.MULTIPLY:
                    
                    f1.SendCommand("{UnlockKeyboard}");
                    Console.Beep(1200, 250);

                    break;

                case KeyboardHook.VKeys.SUBTRACT:
             
                    Thread thr = new Thread(() =>
                    {
                        Console.Beep(2000, 250);
                    });
             
                    f1.SendCommand("{LockKeyboard}");
                    thr.Start();

                    break;

                case KeyboardHook.VKeys.ADD:
             
                    f1.SendCommand("{ScreenOFF}");
                    
                    Console.Beep(650, 250);

                    break;

                case KeyboardHook.VKeys.NUMPAD8:
             
                    f1.SendCommand("{moveu}" + MouseSpeed + "{/moveu}");

                    break;

                case KeyboardHook.VKeys.NUMPAD2:
             
                    f1.SendCommand("{moved}" + MouseSpeed + "{/moved}");

                    break;

                case KeyboardHook.VKeys.NUMPAD4:
             
                    f1.SendCommand("{movel}" + MouseSpeed + "{/movel}");

                    break;

                case KeyboardHook.VKeys.NUMPAD6:
             
                    f1.SendCommand("{mover}" + MouseSpeed + "{/mover}");

                    break;

                case KeyboardHook.VKeys.NUMPAD5:
             
                    f1.SendCommand("{click}");

                    break;

                case KeyboardHook.VKeys.APPS:

                    f1.SendCommand("key: ");

                    break;
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{ShowTV}");
        }
        private void button11_Click(object sender, EventArgs e)
        {
            f1.SendCommand("{HideTV}");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                MouseSpeed = int.Parse(textBox1.Text);
            }
            catch (Exception ex)
            { }
        }
    }
}
