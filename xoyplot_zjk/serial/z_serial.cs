using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows;
using System.Management;
using System.Threading;

namespace ZSeial
{
    public class z_serial
    {
        public SerialPort zSerPort;
        public object LockThis = new object();
        public Button btn_connnect { set; get; }
        public ComboBox com_port { set; get; }
        public string combox_port { set; get; }
        public ComboBox combox_baud { set; get; }
        long dataCounts { set; get; }

        public delegate void serialDataChangeHandle(byte[] buff);
        serialDataChangeHandle func_serial;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="btn_cont">串口连接按键</param>
        /// <param name="cobox_baud">串口COM控件</param>
        /// <param name="defBaud">默认波特率</param>
        public z_serial(Button btn_cont, ComboBox cobox_baud, string defBaud)
        {
            btn_connnect = btn_cont;
            combox_baud = cobox_baud;
            ComboxInitBaudRate(defBaud);
        }

        /// <summary>
        /// 数据转换
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public string fromHexToString(byte[] hex)
        {
            string hexStr = null;
            foreach (byte str in hex)
            {
                hexStr += string.Format("{0:X2} ", str);
            }
            return hexStr;
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="comport"></param>
        /// <param name="baudrate"></param>
        /// <returns></returns>
        private bool connect(string comport, int baudrate)
        {
            zSerPort = new SerialPort(comport, baudrate);
            bool result = Start(); //启动
            if (result == false)
            {
                MessageBox.Show("串口打开失败,查看是否占用！");
                return false;
            }
            else
            {
               // zSerPort.DataReceived += SerialPortDataReceived;

            }
            return true;
        }
        /// <summary>
        /// 串口数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int sizes = zSerPort.BytesToRead;
            byte[] buf = new byte[sizes];
            zSerPort.Read(buf, 0, sizes);
            object lockThis = LockThis;
            lock (lockThis)
            {
                func_serial(buf);
            }
        }
        /// <summary>
        /// 接收函数
        /// </summary>
        /// <param name="func"></param>
        public void SerialRecieveChangedHandle(serialDataChangeHandle func)
        {
            func_serial = func;
        }
        //检查串口是否打开
        public bool check()
        {
            bool result;
            try
            {
                if (zSerPort.IsOpen)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 串口发送数据
        /// </summary>
        /// <param name="buff"></param>
        public void sendStr(string sst)
        {
            if (check())
            {
                object lockThis = LockThis;
                lock (lockThis)
                {
                    zSerPort.Write(sst);
                }

            }

        }
        public void ComboxInitBaudRate(string defBaud)
        {
            int[] bauds = new int[] { 9600, 14400, 19200, 38400, 57600, 115200 };
            for (int i = 0; i < bauds.Length; i++)
            {
                object baud = bauds[i].ToString();
                combox_baud.Items.Add(baud);
            }
            combox_baud.Text = defBaud;
        }

        public void Close()
        {
            combox_baud.IsEnabled = true;
            com_port.IsEnabled = true;
            zSerPort.Close();
        }
        public bool Start()
        {
            bool result;
            try
            {
                zSerPort.Open();
                combox_baud.IsEnabled = false;
                com_port.IsEnabled = false;
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="strPort">串口号</param>
        public bool Com_connect(string strPort)
        {
            if (strPort == string.Empty)
            {
                MessageBox.Show("Please choose a Seial Port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }
            if (combox_baud.SelectedItem == null)
            {
                MessageBox.Show("Please choose BarudRate first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }
            if (combox_port == string.Empty)
            {
                MessageBox.Show("Please choose a Seial Port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }
            if (check())
            {
                btn_connnect.Content = "打开";
                Close();
            }
            else
            {

                int barate = int.Parse((string)combox_baud.SelectedItem);
                bool result = connect(strPort, barate);
                if (result == false)
                {
                    return false;
                }
                else
                {
                    btn_connnect.Content = "断开";
                }

            }
            return true;
        }
        /// <summary>
        /// 向串口发送数据，读取返回数据
        /// </summary>
        /// <param name="sendData">发送的数据</param>
        /// <returns>返回的数据</returns>
        public byte[] ReadPort(string cmd)
        {
            //发送数据
            zSerPort.Write(cmd);

            //读取返回数据
            while (zSerPort.BytesToRead == 0)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(100); //100毫秒内数据接收完毕，可根据实际情况调整
            byte[] recData = new byte[zSerPort.BytesToRead];
            zSerPort.Read(recData, 0, recData.Length);

            return recData;


        }
    }

    }
