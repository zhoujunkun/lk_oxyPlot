using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZSeial;
using zLkControl;
using static zLkControl.StructHelper;

namespace xoyplot_zjk
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged 
    {
        public z_serial lk_serial;
        public thinyFrame lkFrame = new thinyFrame(1);
        SensorDataItem lkSensor = new SensorDataItem();
        //QC
        LK03QC lk03_qc = new LK03QC();
        private UInt16[] lk_distCalibration = new UInt16[3]; //标定完校准
        private UInt16[] lk_gain = new UInt16[3];  //标定完当前距离的增益
        private UInt16[] lk_average = new UInt16[3];  //平均值
        private bool[] lk_ifLkHavedStand = new bool[3];   //是否已经标定过
//
        private  Task task;
        static readonly object locker = new object();
        private Queue<byte> lk_serial_queue = new Queue<byte>();
        private int refresh;

        private string title;
        private double minimum;
        int count;
        private bool complete;
        Random rnd = new Random();
        public MainWindow() 
        {
            InitializeComponent();
            zk_serial_init();
            Measurements = new Collection<Measurement>();
            this.Points = new List<DataPoint>();
            DataContext = this;
            
        }
        int remve_index = 0;
        int range = 20;

        public event PropertyChangedEventHandler PropertyChanged;

        public IList<DataPoint> Points { get; set; }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
                this.RaisePropertyChanged("Title");
            }
        }
        public double lk_Minimum
        {
            get
            {
                return this.minimum;
            }

            set
            {
                this.minimum = value;
                this.RaisePropertyChanged("lk_Minimum");
            }
        }
        public int Refresh
        {
            get
            {
                return this.refresh;
            }

            set
            {
                if (this.refresh == value)
                {
                    return;
                }

                this.refresh = value;
                this.RaisePropertyChanged("Refresh");
            }
        }
        protected void RaisePropertyChanged(string property)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public Collection<Measurement> Measurements { get; private set; }

        private void slider_value_change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        /// <summary>
        ///串口初始化
        /// </summary>
        public void zk_serial_init()
        {
            lk_serial = new z_serial(btn_connect, baud_Selcet, Com_Selcet, "115200");
            lk_serial.addComList(Com_Selcet);

        }

        
        public enum lk_frameRv_type { dist_cmd = 1, ack_cmd };
        private UInt16 sighal { set; get; }
        private UInt16 distance { set; get; }
        private UInt16 agc { set; get; }
        /// <summary>
        /// 通用监听协议解析完成函数
        /// </summary>
        /// <param name="lkSensor"></param>
        private void genralListen(byte[]buf)
        {
            byte frame_type = buf[1];
            lk_frameRv_type type = (lk_frameRv_type)frame_type;
            switch (type)
            {
                case lk_frameRv_type.dist_cmd:
                    {
                         sighal = (UInt16)((buf[1] << 8) | buf[2]);
                         distance = (UInt16)((buf[4] << 8) | buf[3]);
                         agc = (UInt16)((buf[6] << 8) | buf[5]);
                        display();                        
                    }
                    break;
                case lk_frameRv_type.ack_cmd:
                    {
                        LKSensorCmd.TYPE frme_type= (LKSensorCmd.TYPE) buf[2];
                        ackId_func(frme_type, buf[3]);
                    }
                    break;
                default:
                    break;
            }

        }

        private void ackId_func(LKSensorCmd.TYPE type, byte id)
        {
            switch (type)
            {
                case LKSensorCmd.TYPE.DataGet:
                    {

                    }break;
                case LKSensorCmd.TYPE.ParmsSave:
                    {

                    }
                    break;
                case LKSensorCmd.TYPE.ParamGet:
                    {

                    }
                    break;
                case LKSensorCmd.TYPE.Upload:
                    {

                    }
                    break;
                case LKSensorCmd.TYPE.ACK:
                    {
                      
                    }
                    break;
                case LKSensorCmd.TYPE.QC:
                    {
                        LKSensorCmd.QCcmdID qc_id = (LKSensorCmd.QCcmdID)id;
                        qc_ack(qc_id);
                    }
                    break;
                case LKSensorCmd.TYPE.Erro:
                    {

                    }
                    break;
            }
        }
/// <summary>
/// qc ack id
/// </summary>
/// <param name="id"></param>
        private void qc_ack(LKSensorCmd.QCcmdID id)
        {
            switch (id)
            {
                case LKSensorCmd.QCcmdID.stand_start:
                    {
                    } break;
                case LKSensorCmd.QCcmdID.StandParamFirst:
                    {
                        MessageBox.Show("档位1标定保存成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamSecond:
                    {
                        MessageBox.Show("档位2标定保存成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamThird:
                    {
                        MessageBox.Show("档位3标定保存成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamFirstReset:
                    {             
                        lk03_qc.ifFirstStand = true;  //可以进行标定
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            label_first.Content = "1档标定距离(m)：";
                            stand_slider.Value = 45;
                        }), new object[0]);
                        MessageBox.Show("档位1切换成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamSecondReset:
                    {
                        lk03_qc.ifSecondStand = true;
                        MessageBox.Show("档位2切换成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamThirdReset:
                    {
                        lk03_qc.ifThirdStand = true;
                        MessageBox.Show("档位3切换成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.GetParam:
                    {
                    }
                    break;


            }

        }



        /// <summary>
        /// 发送帧数据
        /// </summary>
        /// <param name="sendMsg"></param>
        private void send_frame(sendDataitem sendMsg)
        {
            sendMsg.sendFrame = lkFrame.sendFrame_compend(sendMsg);
            int send_lens = sendMsg.sendFrame.Length;
            lk_serial.zSerPort.Write(sendMsg.sendFrame, 0, send_lens);
        }
        double x = 0;
        /// <summary>
        /// 开始连续测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_start(object sender, RoutedEventArgs e)
        {
            sendDataitem send_msg = new sendDataitem();
            send_msg.Type = (byte)(LKSensorCmd.TYPE.DataGet);
            send_msg.id = (byte)(LKSensorCmd.GetDataID.DistContinue);
            send_msg.ifHeadOnly = true;
            send_frame(send_msg);
        }
        private void display()
        {
            task= new Task(() =>
            {
                lock(locker)
                {
                    this.Measurements.Add(new Measurement
                    {
                        Time = x,
                        Distance = distance,
                        Sighal = sighal
                    });
                    // Change the refresh flag, this will trig InvalidatePlot() on the Plot control
                    this.Refresh++;
                    count++;
                    x += 1;
                    if (Measurements.Count > 100)
                    {
                        Measurements.RemoveAt(0);
                    }
                    if (count > range)
                    {
                        lk_Minimum = count - range;
                    }
                    if(ifStandDistStart)   //标定开始
                    {

                    }
                }
               
            });
            task.Start();

        }
        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_serial_connect(object sender, RoutedEventArgs e)
        {
           if( lk_serial.Com_connect())
            {
                lk_serial.add_data_handelRecieved(serial_recieve);
            }
        }
        /// <summary>
        /// 串口数据接收
        /// </summary>
        /// <param name="buf"></param>
        private void serial_recieve(byte[]buf)
        {
            byte[] buffer2 = new byte[10];
            for (int i=0;i<buf.Length;i++)
            {
                lk_serial_queue.Enqueue(buf[i]);
            }
            if(lk_serial_queue.Count>=9)
            {
                byte[] buffer=   lk_serial_queue.ToArray();
                if(buffer[0]==0xff)
                {
                    if (rx_checkSum(buffer, 9))
                    {
                        genralListen(buffer);
                    }
                }
                for(int i=0;i<9;i++)
                {   
                    buffer2[i] =  lk_serial_queue.Dequeue();
                }
            }
           
        }


        private bool rx_checkSum(byte[]buf, byte lens)
        {
            byte ret = 0;
            for(int i=0;i< lens; i++)
            {
                ret += buf[i];
            }
            if (ret == 0xff)
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// 停止测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_stop(object sender, RoutedEventArgs e)
        {
            sendDataitem send_msg = new sendDataitem();
            send_msg.Type = (byte)(LKSensorCmd.TYPE.DataGet);
            send_msg.id = (byte)(LKSensorCmd.GetDataID.DistStop);
            send_msg.ifHeadOnly = true;
            send_frame(send_msg);
        }
        bool ifStandDistStart = false;  //开始标定标记,true 开始标定
        private void Btn_Clicked_Stand(object sender, RoutedEventArgs e)
        {
            if ((lk03_qc.ifFirstStand) | (lk03_qc.ifSecondStand) | (lk03_qc.ifThirdStand))
            {
                ifStandDistStart = true;
            }
            else
            {
                MessageBox.Show("请先选择档位!");
            }
        }

        private void Btn_Clicked_saveParam(object sender, RoutedEventArgs e)
        {
            //发送保存参数
            //发送消息 前基准
            sendDataitem send_msg = new sendDataitem();
            send_msg.Type = (byte)(LKSensorCmd.TYPE.QC);
            send_msg.ifHeadOnly = false;  //含有数据帧
            byte[] setByte = new byte[4];
            if (lk03_qc.ifFirstStand)
            {
                send_msg.id = (byte)(LKSensorCmd.QCcmdID.StandParamFirst);  //校准参数
                setByte[0] = (byte)(lk_distCalibration[0] >> 8);
                setByte[1] = (byte)(lk_distCalibration[0] & 0xff);
                setByte[2] = (byte)(lk_gain[0] >> 8);
                setByte[3] = (byte)(lk_gain[0] & 0xff);
            }
            else if (lk03_qc.ifSecondStand)
            {
                send_msg.id = (byte)(LKSensorCmd.QCcmdID.StandParamSecond);  //校准参数
                setByte[0] = (byte)(lk_distCalibration[1] >> 8);
                setByte[1] = (byte)(lk_distCalibration[1] & 0xff);
                setByte[2] = (byte)(lk_gain[1] >> 8);
                setByte[3] = (byte)(lk_gain[1] & 0xff);
            }
            else if (lk03_qc.ifThirdStand)
            {
                send_msg.id = (byte)(LKSensorCmd.QCcmdID.StandParamThird);  //校准参数
                setByte[0] = (byte)(lk_distCalibration[2] >> 8);
                setByte[1] = (byte)(lk_distCalibration[2] & 0xff);
                setByte[2] = (byte)(lk_gain[2] >> 8);
                setByte[3] = (byte)(lk_gain[2] & 0xff);
            }
            send_msg.sendbuf = setByte;    //数据帧缓存
            send_msg.len = (UInt16)setByte.Length; //数据帧字节长度
            send_frame(send_msg);
        }

        private void Btn_Clicked_getStandParam(object sender, RoutedEventArgs e)
        {
            sendDataitem send_msg = new sendDataitem();
            send_msg.Type = (byte)(LKSensorCmd.TYPE.QC);
            send_msg.id = (byte)(LKSensorCmd.QCcmdID.GetParam);
            send_msg.ifHeadOnly = true;
            send_frame(send_msg);
        }

        private void Btn_first_click(object sender, RoutedEventArgs e)
        {
            sendDataitem send_msg = new sendDataitem();
            send_msg.Type = (byte)(LKSensorCmd.TYPE.QC);
            send_msg.id = (byte)(LKSensorCmd.QCcmdID.StandParamFirstReset);
            send_msg.ifHeadOnly = true;
            send_frame(send_msg);
        }

        private void Btn_third_click(object sender, RoutedEventArgs e)
        {
            sendDataitem send_msg = new sendDataitem();
            send_msg.Type = (byte)(LKSensorCmd.TYPE.QC);
            send_msg.id = (byte)(LKSensorCmd.QCcmdID.StandParamThirdReset);
            send_msg.ifHeadOnly = true;
            send_frame(send_msg);
        }

        private void Btn_second_click(object sender, RoutedEventArgs e)
        {
            sendDataitem send_msg = new sendDataitem();
            send_msg.Type = (byte)(LKSensorCmd.TYPE.QC);
            send_msg.id = (byte)(LKSensorCmd.QCcmdID.StandParamSecondReset);
            send_msg.ifHeadOnly = true;
            send_frame(send_msg);
        }

        private void window_close_click(object sender, EventArgs e)
        {
            lk_serial.Close();
        }
    }
    public class LK03QC 
    {
        public bool ifFirstStand { set; get; }
        public bool ifSecondStand { set; get; }
        public bool ifThirdStand { set; get; }
        public bool ifFirstStandResetAck { set; get; }
        public bool ifSecondStandResetAck { set; get; }
        public bool ifThirdStandResetAck { set; get; }

    }
    public class Measurement
    {
        public double Time { get; set; }
        public double Sighal { get; set; }
        public double Distance { get; set; }
    }
}
