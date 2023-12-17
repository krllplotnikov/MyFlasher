using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyFlasher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string input;
        byte[] inputData;
        byte[] firmwareData;
        byte EraseType = 0;
        bool dataReceived = false;
        bool retry = false;

        void Count(object obj)
        {
            retry = true;
        }

        bool Start()
        {
            byte[] txData = { 0x7F };

            serialPort.Write(txData, 0, txData.Length);


            TimerCallback tm = new TimerCallback(Count);
            System.Threading.Timer timer = new System.Threading.Timer(tm, null, 100, 100);

            while (!dataReceived || !retry)
            {
                if (retry)
                {
                    timer.Dispose();
                    retry = false;
                    return false;
                }
                else if(dataReceived)
                {
                    dataReceived = false;
                    timer.Dispose();
                    break;
                }
            }


            if (inputData == null || inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            } else if(inputData[0] == 0x7F)
            {
                return true;
            }
            return true;
        }

        bool GetID()
        {
            byte[] txData = { 0x02, 0xFD };

            serialPort.Write(txData, 0, txData.Length);

            while (!dataReceived) ;
            dataReceived = false;

            if (inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            }
            else if (inputData[0] == 0x7F)
            {
                return true;
            }
            return true;
        }

        bool Get()
        {
            byte[] txData = { 0x00, 0xFF };

            serialPort.Write(txData, 0, txData.Length);

            while (!dataReceived) ;
            dataReceived = false;

            EraseType = inputData[9];

            if (inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            }
            else if (inputData[0] == 0x7F)
            {
                return true;
            }
            return true;
        }

        bool Erase()
        {
            if (EraseType == 0x43)
            {
                byte[] txData = { 0x43, 0xBC };

                serialPort.Write(txData, 0, txData.Length);

                while (!dataReceived) ;
                dataReceived = false;

                if (inputData.Length == 0 || inputData[0] == 0x1F)
                {
                    return false;
                }

                txData = new byte[] { 0xFF, 0x00 };
                serialPort.Write(txData, 0, txData.Length);

                while (!dataReceived) ;
                dataReceived = false;

                if (inputData.Length == 0 || inputData[0] == 0x1F)
                {
                    return false;
                }
                else if (inputData[0] == 0x7F)
                {
                    return true;
                }
                return true;
            }
            else
            {
                byte[] txData = { 0x44, 0xBB };

                serialPort.Write(txData, 0, txData.Length);

                while (!dataReceived) ;
                dataReceived = false;

                if (inputData.Length == 0 || inputData[0] == 0x1F)
                {
                    return false;
                }

                txData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 };
                serialPort.Write(txData, 0, txData.Length);

                while (!dataReceived) ;
                dataReceived = false;

                if (inputData.Length == 0 || inputData[0] == 0x1F)
                {
                    return false;
                }
                else if (inputData[0] == 0x7F)
                {
                    return true;
                }
                return true;
            }
        }

        bool Write(int addr, byte[] data, int length)
        {
            byte[] writeAddrArr = new byte[4];
            byte crc = 0;
            byte[] txData = { 0x31, 0xCE };

            serialPort.Write(txData, 0, txData.Length);

            while (!dataReceived) ;
            dataReceived = false;

            if (inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            }

            writeAddrArr[0] = (byte)(addr >> 24);
            writeAddrArr[1] = (byte)(addr >> 16);
            writeAddrArr[2] = (byte)(addr >> 8);
            writeAddrArr[3] = (byte)(addr);
            crc = (byte)((int)writeAddrArr[0] ^ (int)writeAddrArr[1] ^ (int)writeAddrArr[2] ^ (int)writeAddrArr[3]);

            txData = new byte[] { writeAddrArr[0], writeAddrArr[1], writeAddrArr[2], writeAddrArr[3], crc };
            serialPort.Write(txData, 0, txData.Length);

            while (!dataReceived) ;
            dataReceived = false;

            if (inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            }

            crc = (byte)(length - 1);
            for (int j = 0; j < length; j++)
            {
                crc ^= data[j];
            }

            txData = new byte[length + 2];
            txData[0] = (byte)(length - 1);
            for (int j = 1; j <= length; j++)
            {
                txData[j] = data[j - 1];
            }
            txData[length + 1] = crc;
            serialPort.Write(txData, 0, txData.Length);

            while (!dataReceived) ;
            dataReceived = false;

            if (inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            }
            else if (inputData[0] == 0x7F)
            {
                return true;
            }
            return true;
        }

        bool Go()
        {
            byte[] goAddrArr = new byte[4];
            int goAddr = 0x08000000;
            byte crc = 0;
            byte[] txData = { 0x21, 0xDE };

            serialPort.Write(txData, 0, txData.Length);

            while (!dataReceived) ;
            dataReceived = false;

            if (inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            }

            goAddrArr[0] = (byte)(goAddr >> 24);
            goAddrArr[1] = (byte)(goAddr >> 16);
            goAddrArr[2] = (byte)(goAddr >> 8);
            goAddrArr[3] = (byte)(goAddr);
            crc = (byte)((int)goAddrArr[0] ^ (int)goAddrArr[1] ^ (int)goAddrArr[2] ^ (int)goAddrArr[3]);

            txData = new byte[] { goAddrArr[0], goAddrArr[1], goAddrArr[2], goAddrArr[3], crc };
            serialPort.Write(txData, 0, txData.Length);

            while (!dataReceived) ;
            dataReceived = false;

            if (inputData.Length == 0 || inputData[0] == 0x1F)
            {
                return false;
            }
            else if (inputData[0] == 0x7F)
            {
                return true;
            }
            return true;
        }

        private void btnFlash_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen) serialPort.Close();
            serialPort.PortName = comboBoxCOM.Text;
            try
            {
                serialPort.Open();
                serialPort.ReadTimeout = 1000;
            }
            catch
            {
                MessageBox.Show("Port " + serialPort.PortName + " can not open!",
                "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            String filePath = textBoxFilePath.Text;
            firmwareData = File.ReadAllBytes(filePath);

            int writeCycles = (int)Math.Ceiling((float)firmwareData.Length / 256);
            int writeAddr = 0x08000000;
            int writeLength;
            int allLength = firmwareData.Length;
            byte[] writeData;

            byte attempts = 10;


            while (!Start()) 
            {
                attempts--;
                if (attempts <= 0)
                {
                    label.Text = "Failed!";
                    if (serialPort.IsOpen) serialPort.Close();
                    return;
                }
            }
            attempts = 10;

            while (!Get())
            {
                attempts--;
                if (attempts <= 0)
                {
                    label.Text = "Failed!";
                    if (serialPort.IsOpen) serialPort.Close();
                    return;
                }
            }
            attempts = 10;


            while (!GetID())
            {
                attempts--;
                if (attempts <= 0)
                {
                    label.Text = "Failed!";
                    if (serialPort.IsOpen) serialPort.Close();
                    return;
                }
            }
            attempts = 10;



            while (!Erase())
            {
                attempts--;
                if (attempts <= 0)
                {
                    label.Text = "Failed!";
                    if (serialPort.IsOpen) serialPort.Close();
                    return;
                }
            };
            attempts = 10;

            label.Text = "Flashing...";

            for(int j = 0;  j < writeCycles; j++)
            {
                if(allLength > 256)
                {
                    writeLength = 256;
                }
                else
                {
                    writeLength = allLength;
                }

                writeData = new byte[writeLength];
                for(int i = 0; i < writeLength; i++)
                {
                    writeData[i] = firmwareData[i + j * 256];
                }

                while (!Write(writeAddr, writeData, writeData.Length)) 
                {
                    attempts--;
                    if (attempts <= 0)
                    {
                        label.Text = "Failed!";
                        if (serialPort.IsOpen) serialPort.Close();
                        return;
                    }
                }

                progressBar.Value = (int)(((float)j / (writeCycles - 1)) * 100);

                allLength -= 256;
                writeAddr += writeLength;
            }

            label.Text = "Completed!";

            while (!Go()) ;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxCOM.Items.Add(port);
            };
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if(fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFilePath.Text = fileDialog.FileName;
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            input = serialPort.ReadExisting();
            inputData = Encoding.ASCII.GetBytes(input);
            dataReceived = true;
        }
    }
}
