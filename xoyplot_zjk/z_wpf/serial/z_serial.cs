using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
namespace ZSeial
{
    public class z_serial
    {
        Regex reg_com = new Regex(@"COM[0-9]*");  //正则表达式提取COM
        Regex reg_str = new Regex(@"[\((][^\))]+[\))]$");
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
        public z_serial(Button btn_cont, ComboBox cobox_baud, ComboBox port, string defBaud)
        {
            com_port = port;
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

        public void add_data_handelRecieved(serialDataChangeHandle func)
        {
            zSerPort.DataReceived += SerialPortDataReceived;
            SerialRecieveChangedHandle(func);
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

                combox_baud.Items.Add(bauds[i].ToString());
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
        public bool Com_connect()
        {
            string text = com_port.SelectedItem.ToString();
            string port = reg_com.Match(text).Value;  //提取COM

            if (port == string.Empty)
            {
                MessageBox.Show("Please choose a Seial Port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }
            if (combox_baud.SelectedItem == null)
            {
                MessageBox.Show("Please choose BarudRate first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
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
                bool result = connect(port, barate);
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

        #region 串口端口
        /// <summary>
        /// 枚举win32 api
        /// </summary>
        public enum HardwareEnum_T
        {
            // 硬件
            Win32_Processor, // CPU 处理器
            Win32_PhysicalMemory, // 物理内存条
            Win32_Keyboard, // 键盘
            Win32_PointingDevice, // 点输入设备，包括鼠标。
            Win32_FloppyDrive, // 软盘驱动器
            Win32_DiskDrive, // 硬盘驱动器
            Win32_CDROMDrive, // 光盘驱动器
            Win32_BaseBoard, // 主板
            Win32_BIOS, // BIOS 芯片
            Win32_ParallelPort, // 并口
            Win32_SerialPort, // 串口
            Win32_SerialPortConfiguration, // 串口配置
            Win32_SoundDevice, // 多媒体设置，一般指声卡。
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
            Win32_USBController, // USB 控制器
            Win32_NetworkAdapter, // 网络适配器
            Win32_NetworkAdapterConfiguration, // 网络适配器设置
            Win32_Printer, // 打印机
            Win32_PrinterConfiguration, // 打印机设置
            Win32_PrintJob, // 打印机任务
            Win32_TCPIPPrinterPort, // 打印机端口
            Win32_POTSModem, // MODEM
            Win32_POTSModemToSerialPort, // MODEM 端口
            Win32_DesktopMonitor, // 显示器
            Win32_DisplayConfiguration, // 显卡
            Win32_DisplayControllerConfiguration, // 显卡设置
            Win32_VideoController, // 显卡细节。
            Win32_VideoSettings, // 显卡支持的显示模式。

            // 操作系统
            Win32_TimeZone, // 时区
            Win32_SystemDriver, // 驱动程序
            Win32_DiskPartition, // 磁盘分区
            Win32_LogicalDisk, // 逻辑磁盘
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置
            Win32_PageFile, // 系统页文件信息
            Win32_PageFileSetting, // 页文件设置
            Win32_BootConfiguration, // 系统启动配置
            Win32_ComputerSystem, // 计算机信息简要
            Win32_OperatingSystem, // 操作系统信息
            Win32_StartupCommand, // 系统自动启动程序
            Win32_Service, // 系统安装的服务
            Win32_Group, // 系统管理组
            Win32_GroupUser, // 系统组帐号
            Win32_UserAccount, // 用户帐号
            Win32_Process, // 系统进程
            Win32_Thread, // 系统线程
            Win32_Share, // 共享
            Win32_NetworkClient, // 已安装的网络客户端
            Win32_NetworkProtocol, // 已安装的网络协议
            Win32_PnPEntity,//all device
        }
        /// <summary>
        /// 获取硬件设备信息
        /// </summary>
        /// <param name="hardType">设备类型</param>
        /// <param name="propKey">硬件名称</param>
        /// <returns></returns>
        public static string[] MulGetHardwareInfo(HardwareEnum_T hardType, string propKey)
        {

            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null)
                        {
                            if (hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
                            {
                                strs.Add(hardInfo.Properties[propKey].Value.ToString());
                            }
                        }
                    }
                    searcher.Dispose();
                }
                return strs.ToArray();
            }
            catch
            {
                MessageBox.Show("MulGetHardwareInfo erro!");
                return null;
            }

        }

        /// <summary>
        /// 获取串口列表
        /// </summary>
        /// <returns></returns>
        private static string[] GetSerialPortList()
        {
            return MulGetHardwareInfo(HardwareEnum_T.Win32_PnPEntity, "Name");
        }
        /// <summary>
        /// 获取串口列表线程
        /// </summary>
        private void OnGetSerialPortList()
        {

            try
            {
                PortNameArray = GetSerialPortList();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            ReadOver = true;
        }
        string[] PortNameArray;
        bool ReadOver;
        public Thread threadReadValue { get; private set; }
        /// <summary>
        /// 通过线程获取串口列表
        /// </summary>
        public string[] GetSerialPortArray()
        {
            PortNameArray = null;
            try
            {
                threadReadValue = new System.Threading.Thread(OnGetSerialPortList);
                threadReadValue.IsBackground = true;
                ReadOver = false;
                threadReadValue.Start();

                while (ReadOver == false)
                {
                    System.Threading.Thread.Sleep(200);
                }
                threadReadValue = null;
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            return PortNameArray;
        }
        /*获取串口端口号*/
        public string[] addComList(ComboBox combox)
        {

            //通过WMI获取COM端口
            string[] ss = GetSerialPortArray();

            foreach (string Com in ss)
            {
                if (Com.Contains("USB 串行设备") == true)
                {

                    string text = Com.Replace("USB 串行设备", "迈测激光");
                    combox.Items.Add(text);
                }
                else
                {
                    string txs = reg_str.Match(Com).Value;
                    string com = reg_com.Match(txs).Value;
                    string port = com + "->" + Com.Replace(txs, "");
                    combox.Items.Add(port);
                }
            }
            return ss;
        }
        #endregion

    }

}
