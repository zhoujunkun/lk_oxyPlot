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
using System.Linq;
using MahApps.Metro.Controls;
using lk_windows;
using LK_PROTECL;
using lk_protecl;
using lk_verify;

namespace xoyplot_zjk
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged 
    {
        public z_serial lk_serial;
        public tinyFrame lkFrame = new tinyFrame();
        SensorDataItem lkSensor = new SensorDataItem();
        //QC
        LK03QC lk03_qc = new LK03QC();
        //protecl
        LK_CHECKSUM_PROTEL checkSumProtecl = new LK_CHECKSUM_PROTEL();
        //dowload bin window
        lk_download_win dowload_win;
        private  Task task;
        static readonly object locker = new object();
        private Queue<byte> lk_serial_queue = new Queue<byte>();
        private int refresh;
        private double minimum;
        int count;
        Random rnd = new Random();
        public MainWindow() 
        {
            InitializeComponent();
            zk_serial_init();
            Measurements = new Collection<Measurement>();
            this.Points = new List<DataPoint>();
            DataContext = this;
            Thread log_t = new Thread(lk_log);
            checkSumProtecl.addGeneralFun(genralListen);
            //log_t.Start("还未标定，请标定。。。");
            //log_t.IsBackground = true;

        }
        public event PropertyChangedEventHandler PropertyChanged;
        public IList<DataPoint> Points { get; set; }
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
   

        private UInt16 sighal { set; get; }
        private UInt16 distance { set; get; }
        private UInt16 agc { set; get; }
        /// <summary>
        /// 通用监听协议解析完成函数
        /// </summary>
        /// <param name="lkSensor"></param>
        private void genralListen(byte[]buf, LK_CHECKSUM_PROTEL.lk_frameRv_type type)
        {
            switch (type)
            {
                case LK_CHECKSUM_PROTEL.lk_frameRv_type.dist_cmd:
                    {
                         sighal = (UInt16)((buf[0] << 8) | buf[1]);
                         distance = (UInt16)((buf[2] << 8) | buf[3]);
                         agc = (UInt16)((buf[4] << 8) | buf[5]);
                        display();                        
                    }
                    break;
                case LK_CHECKSUM_PROTEL.lk_frameRv_type.ack_cmd:
                    {
                        LKSensorCmd.TYPE frme_type= (LKSensorCmd.TYPE) buf[0];
                        ackId_func(frme_type, buf[1]);
                    }
                    break;
                case LK_CHECKSUM_PROTEL.lk_frameRv_type.param_cmd:
                    {
                        byte[] qc_param = new byte[15];
                        for(int i=0;i< 15; i++)
                        {
                            qc_param[i] = buf[i];
                        }
                       // ackId_func(frme_type, buf[3]);
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
                        lk_log("档位1标定保存成功！");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            btn_stand.Content = "已经标定";
                        }), new object[0]);
                        lk03_qc.ifLkHavedStand[0] = true;    //档位1已经标定标记
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamSecond:
                    {
                        lk_log("档位2标定保存成功！");
                        lk03_qc.ifLkHavedStand[1] = true;    //档位2已经标定标记
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamThird:
                    {
                        lk_log("档位3标定保存成功！");
                        lk03_qc.ifLkHavedStand[2] = true;    //档位3已经标定标记
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamFirstReset:
                    {
                        clear_texbox_standMsg(enum_index_stand.first);
                        lk_log("档位1复位成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamSecondReset:
                    {
                        clear_texbox_standMsg(enum_index_stand.second);
                        lk_log("档位2复位成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandParamThirdReset:
                    {
                        clear_texbox_standMsg(enum_index_stand.third);
                        lk_log("档位3复位成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandFirstSwitch:
                    {
                        lk03_qc.ifFirstStand = true;  //可以进行标定
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            label_first.Content = "1档标定距离(m)：";
                            stand_slider.Value = 10;
                        }), new object[0]);
                        cureent_stand_device = 1;
                        lk_log("档位1切换成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandSecondSwitch:
                    {
                        lk03_qc.ifSecondStand = true;
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            label_first.Content = "2档标定距离(m)：";
                            stand_slider.Value = 45;
                        }), new object[0]);
                        cureent_stand_device = 2;
                        lk_log("档位2切换成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.StandThirdSwitch:
                    {
                        lk03_qc.ifThirdStand = true;
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            label_first.Content = "3档标定距离(m)：";
                            stand_slider.Value = 90;
                        }), new object[0]);
                        cureent_stand_device = 3;
                        lk_log("档位3切换成功！");
                    }
                    break;
                case LKSensorCmd.QCcmdID.GetParam:
                    {
                    }
                    break;


            }

        }



        ///// <summary>
        ///// 发送帧数据
        ///// </summary>
        ///// <param name="sendMsg"></param>
        //private void send_frame(sendDataitem sendMsg)
        //{
        //    sendMsg.sendFrame = lkFrame.sendFrame_compend(sendMsg);
        //    int send_lens = sendMsg.sendFrame.Length;
        //    lk_serial.zSerPort.Write(sendMsg.sendFrame, 0, send_lens);
        //}
        double x = 0;
        /// <summary>
        /// 开始连续测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_start(object sender, RoutedEventArgs e)
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_getData);
            frame.id = (byte)(Protecl_typical_cmd.getData_id.continueDist);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }

        int set_range_display = 20;
        int dist_ave { set; get; }
        int singhal_ave { set; get; }
        int agc_ave { set; get; }
        int cureent_stand_device = 0;    //当前标定设备
        bool ifCalculate;  //是否开始计算
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
                        Sighal = sighal,
                        Agc= agc
                    });
                    // Change the refresh flag, this will trig InvalidatePlot() on the Plot control
                    this.Refresh++;
                    count++;
                    x += 1;
                    if (Measurements.Count > set_range_display)
                    {
                        Measurements.RemoveAt(0);
                    }
                    if (count > set_range_display)
                    {
                        lk_Minimum = count - set_range_display;
                    }
                    if(ifCalculate)  //标定开始
                    {
                        ifCalculate = false;   //点击标定后计算一次
                        double[] dist_average = new double[20];
                        double[] singhal_average = new double[20];
                        double[] agc_average = new double[20];
                        for (int i=0;i<set_range_display;i++)
                        {
                            dist_average[i] = Measurements[i].Distance;
                           // singhal_average[i] = Measurements[i].Sighal;
                            agc_average[i] = Measurements[i].Agc;
                        }
                        dist_ave = (int)dist_average.Average();
                        singhal_ave = (int)singhal_average.Average();
                        agc_ave = (int)agc_average.Average();
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            if (cureent_stand_device==1)
                            {
                                int dis = lk03_qc.distCalibration_arry[0] = (UInt16)(dist_ave - stand_slider.Value * 100);
                                textBox_calibration_1.Text = dis.ToString();
                                textBox_average_1.Text = dist_ave.ToString();
                                textBox_gain_1.Text = agc_ave.ToString();
                            }
                            else if (cureent_stand_device==2)
                            {
                                int dis= lk03_qc.distCalibration_arry[1] = (UInt16)(dist_ave - stand_slider.Value * 100);
                                textBox_calibration_2.Text = dis.ToString();
                                textBox_average_2.Text = dist_ave.ToString();
                                textBox_gain_2.Text = agc_ave.ToString(); 
                            }
                            else if (cureent_stand_device==3)
                            {
                                int dis = lk03_qc.distCalibration_arry[2] = (UInt16)(dist_ave - stand_slider.Value * 100);
                                textBox_calibration_3.Text = dis.ToString();
                                textBox_average_3.Text = dist_ave.ToString();
                                textBox_gain_3.Text = agc_ave.ToString();
                            }
                        }), new object[0]);
                      
                    }
                }
               
            });
            task.Start();

        }
        private void lk_log(object log)
        {
                base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                {
                    string log_d = "提示：==>"+log + "\r\n";
                    text_Log.Text = log_d;
                    
                }), new object[0]);
                      
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
           
            for (int i=0;i< buf.Length; i++)
            {
                checkSumProtecl.protecl_ansys(buf[i]);
            }         
        }

    
        /// <summary>
        /// 停止测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_stop(object sender, RoutedEventArgs e)
        {

        }
       
        private void Btn_Clicked_Stand(object sender, RoutedEventArgs e)
        {

                if (lk03_qc.ifFirstStand)
                {
                    ifCalculate = true;
                    cureent_stand_device = 1;
                }
                else
                {
                    MessageBox.Show("请先选择档位!");
                }

        }

        private void Btn_Clicked_saveParam(object sender, RoutedEventArgs e)
        {
            byte[] setByte = new byte[4];
            setByte[0] = (byte)(lk03_qc.distCalibration_arry[0] >> 8);
            setByte[1] = (byte)(lk03_qc.distCalibration_arry[0] & 0xff);
            setByte[2] = (byte)(lk03_qc.gain_arry[0] >> 8);
            setByte[3] = (byte)(lk03_qc.gain_arry[0] & 0xff);
        }

        private void Btn_Clicked_getStandParam(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_first_click(object sender, RoutedEventArgs e)
        {


        }

        private void Btn_third_click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_second_click(object sender, RoutedEventArgs e)
        {

        }

        private void window_close_click(object sender, EventArgs e)
        {
            lk_serial.Close();
        }

        private void Btn_Clicked_saveParam_2(object sender, RoutedEventArgs e)
        {
            byte[] setByte = new byte[4];
            setByte[0] = (byte)(lk03_qc.distCalibration_arry[1] >> 8);
            setByte[1] = (byte)(lk03_qc.distCalibration_arry[1] & 0xff);
            setByte[2] = (byte)(lk03_qc.gain_arry[1] >> 8);
            setByte[3] = (byte)(lk03_qc.gain_arry[1] & 0xff);
        }

        private void Btn_Clicked_Stand_2(object sender, RoutedEventArgs e)
        {
            if (lk03_qc.ifSecondStand)
            {
                ifCalculate = true;
                cureent_stand_device = 2;
            }
            else
            {
                MessageBox.Show("请先选择档位!");
            }
        }

        private void Btn_Clicked_saveParam_3(object sender, RoutedEventArgs e)
        {

            byte[] setByte = new byte[4];        
            setByte[0] = (byte)(lk03_qc.distCalibration_arry[2] >> 8);
            setByte[1] = (byte)(lk03_qc.distCalibration_arry[2] & 0xff);
            setByte[2] = (byte)(lk03_qc.gain_arry[2] >> 8);
            setByte[3] = (byte)(lk03_qc.gain_arry[2] & 0xff);
        }

        private void Btn_Clicked_Stand_3(object sender, RoutedEventArgs e)
        {

        }
        enum enum_index_stand { first=0,second,third};
        private void btn_reset_stand_click(object sender, RoutedEventArgs e)
        {

        }
        private void clear_texbox_standMsg(enum_index_stand index)
        {
            switch(index)
            {
                case enum_index_stand.first:
                    {
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            textBox_calibration_1.Text = "";
                            textBox_average_1.Text = "";
                            textBox_gain_1.Text = "";
                        }), new object[0]);
                    }
                    break;
                case enum_index_stand.second:
                    {
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            textBox_calibration_2.Text = "";
                            textBox_average_2.Text = "";
                            textBox_gain_2.Text = "";
                        }), new object[0]);
                    }
                    break;
                case enum_index_stand.third:
                    {
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            textBox_calibration_3.Text = "";
                            textBox_average_3.Text = "";
                            textBox_gain_3.Text = "";
                        }), new object[0]);
                    }
                    break;
            }
        }

        private void btn_reset_stand2_click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_reset_stand3_click(object sender, RoutedEventArgs e)
        {

        }
        private info infoWin;
        private void Btn_Clicked_About(object sender, RoutedEventArgs e)
        {
            //infoWin = new info(this);
            //infoWin.Owner = this;
            //infoWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //infoWin.ShowDialog();
        }


        private void Btn_Clicked_upload(object sender, RoutedEventArgs e)
        {
            dowload_win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dowload_win.ShowDialog();
        }

        private void window_load_init(object sender, RoutedEventArgs e)
        {
            dowload_win = new lk_download_win(this);
            dowload_win.Owner = this;
        }
    }
    public class LK03QC 
    {
        public bool ifFirstStand { set; get; }
        public bool ifSecondStand { set; get; }
        public bool ifThirdStand { set; get; }
        public UInt16[] distCalibration_arry = new UInt16[3]; //标定完校准
        public UInt16[] gain_arry = new UInt16[3];  //标定完当前距离的增益
        public UInt16[] average_arry = new UInt16[3];  //平均值
        public bool[] ifLkHavedStand = new bool[3];   //是否已经标定过
    }
    public class Measurement
    {
        public double Time { get; set; }
        public double Sighal { get; set; }
        public double Distance { get; set; }
        public double Agc { get; set; }
    }
}
