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
using lk_protecl;
using lk_verify;
using xoyplot_zjk.z_wpf.console;
using System.Windows.Controls;

namespace xoyplot_zjk
{

    class Lk_sensor_data
    {
        public UInt16[] qc_distCalibration_arry = new UInt16[3]; //标定完校准
        public UInt16[] qc_gain_arry = new UInt16[3];  //标定完当前距离的增益
        public UInt16[] qc_average_arry = new UInt16[3];  //平均值
        public byte qc_current_stand;         //当前标定的档位

        public UInt16 baud_rate { set; get; }
        public UInt16 distance { set; get; }


    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged 
    {
        public z_serial lk_serial;
        public tinyFrame lkFrame = new tinyFrame();
        SensorDataItem lkSensor = new SensorDataItem();
        //qc
        enum enum_qc_index_stand { first = 0, second, third };
        Lk_sensor_data lk_Sensor_Data = new Lk_sensor_data();
        //cmd func list
        CmdFuncLists cmdFuncLists = new CmdFuncLists();
         //dowload bin window
        lk_download_win dowload_win;
        //ack
       // Lk_other_protecl lk_Other_Protecl = new Lk_other_protecl();
        protecl_ack genral_ack = new protecl_ack();
        //
        private  Task task;
        static readonly object locker = new object();
        private Queue<byte> lk_serial_queue = new Queue<byte>();
        private int refresh;
        private double minimum;
        int count;
        Random rnd = new Random();
        bool ifCalculate;  //是否开始计算

        public MainWindow() 
        {
            InitializeComponent();
            zk_serial_init();
            Measurements = new Collection<Measurement>();
            this.Points = new List<DataPoint>();
            DataContext = this;
            Thread mangeta_thread = new Thread(thread_func_mangage);
            //tinyFrame
            lkFrame.addGenralListener(genral_ack.genralListen);
            genral_ack.add_usr_ackId(user_ack_id);   //用户应答id
            genral_ack.add_programer_ackId(proramer_ack_id); //非用户应答id
            //other
            //lk_Other_Protecl.other_protecl_init();
            //lk_Other_Protecl.sensor_ack.set_consol_ack(lk_log);
            //lk_Other_Protecl.sensor_ack.add_other_display(display); //添加数据显示
            //lk_Other_Protecl.sensor_ack.add_other_reset_paramAck(clear_texbox_standMsg);//添加回调清除
            //sensor
            sensor_set_powerOnMode_init(sensor_autoRun_mode_combox);
            sensor_set_baudRate_init(sensor_baudRate_combox);

            mangeta_thread.Start();
            lk_log("点击对应串口连接 ->获取传感器参数信息按钮");
        }
          enum buad_enum_ { baudRate_9600 = 0, baudRate_14400, baudRate_19200, baudRate_38400, baudRate_57600, baudRate_115200 };
        UInt16[] qc_standDist = new UInt16[3];
        UInt16[] qc_standAgc = new UInt16[3];
        byte[] qc_ifStand = new byte[3];
       public void sensor_programer_qc_refresh(byte[]buf)
        {
             for(int i=0;i<3;i++)
            {
                qc_standDist[i] = (UInt16)(buf[i * 5] << 8 | (buf[i * 5 + 1] & 0xff));
                qc_standAgc[i] = (UInt16)(buf[i * 5+2] << 8 | (buf[i * 5 + 3] & 0xff));
                qc_ifStand[i] =  buf[i * 5 + 4];
            }
            base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
            {
                textBox_calibration_1.Text = qc_standDist[0].ToString();
                textBox_gain_1.Text = qc_standAgc[0].ToString();
                if (qc_ifStand[0] == 0)
                {
                    texblock_standFirstLog.Text = "未标定";
                }
                else texblock_standFirstLog.Text = "已标定";
                textBox_calibration_2.Text = qc_standDist[1].ToString();
                textBox_gain_2.Text = qc_standAgc[1].ToString();
                if (qc_ifStand[1] == 0)
                {
                    texblock_standSecondLog.Text = "未标定";
                }
                else texblock_standSecondLog.Text = "已标定";
                textBox_calibration_3.Text = qc_standDist[2].ToString();
                textBox_gain_3.Text = qc_standAgc[2].ToString();
                if (qc_ifStand[2] == 0)
                {
                    texblock_standThirdLog.Text = "未标定";
                }
                else texblock_standThirdLog.Text = "已标定";

            }), new object[0]);
        }
        
        /// <summary>
        /// 传感器参数更新,
        /// </summary>
        /// <param name="buf"></param>
        public void sensor_param_refresh(byte[] buf)
        {
          
            byte baudRate = buf[0];
            UInt16 frontSwitch = (UInt16)(buf[1] << 8 | buf[2]);
            UInt16 baseSwitch = (UInt16)(buf[3] << 8 | buf[4]);
            byte dist_base = buf[5];
            byte senor_auto_run = buf[6];
            UInt16 sensor_oupute_freq = (UInt16)(buf[7] << 8 | buf[8]);
           
            base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
            {
                sensor_baudRate_combox.SelectedIndex = baudRate;
                if(senor_auto_run == 1) //上电自动运行
                {
                    sensor_autoRun_mode_combox.SelectedIndex = 0;
                }else sensor_autoRun_mode_combox.SelectedIndex =1;
                sensor_ouput_data_freq_slider.Value = sensor_oupute_freq;
                sensor_front_switch_slider.Value = frontSwitch;
                sensor_base_switch_slider.Value = baseSwitch;
                if(dist_base==1)
                {
                    RadioBtn_Front.IsChecked = true;
                }
                else
                {
                    RadioBtn_Base.IsChecked = true;
                }
               
            }), new object[0]);
        }


        /// <summary>
        /// 反馈处理任务
        /// </summary>
        private void thread_func_mangage()
        {

            Thread.Sleep(1000);
        }
        /// <summary>
        /// 传感器波特率设置初始化
        /// </summary>
        /// <param name="defBaud"></param>
        private void sensor_set_baudRate_init(ComboBox cbx)
        {
            int[] bauds = new int[] { 9600, 14400, 19200, 38400, 57600, 115200 };
            for (int i = 0; i < bauds.Length; i++)
            {

                cbx.Items.Add(bauds[i].ToString());
            }
        }

        /// <summary>
        /// 传感器上电自动运行模式
        /// </summary>
        /// <param name="cbx"></param>
        private void sensor_set_powerOnMode_init(ComboBox cbx)
        {
            string[] bauds = new string[] { "自动测量", "关闭自动测量"};
            for (int i = 0; i < bauds.Length; i++)
            {

                cbx.Items.Add(bauds[i]);
            }
        }

        
        private void lk_texboclog(string logg)
        {
            base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
            {
                //string log_d = "提示：==>" + log + "\r\n";
                //text_Log.Text = log_d;
                text_string.Text+= logg + "\r\n";
            }), new object[0]);

        }
        private void lk_log(string logg)
        {
            base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
            {
                text_Log.Text = logg;
            }), new object[0]);

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
            lkFrame.set_consol(lk_serial);
            lkFrame.test_int(38);
        }
        double x = 0;
        /// <summary>
        /// 开始连续测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_start(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.dist_continue();
        }
        int set_range_display = 20;
        int dist_ave { set; get; }
        int singhal_ave { set; get; }
        int agc_ave { set; get; }
 
        /// <summary>
        /// 显示
        /// </summary>
        public void display()
        {
            task= new Task(() =>
            {
                lock(locker)
                {
                    this.Measurements.Add(new Measurement
                    {
                        Time = x,
                        Distance = lk_Sensor_Data.distance,
                        //Sighal = lk_Other_Protecl.sensor_ack.other_sighal,
                        //Agc= lk_Other_Protecl.sensor_ack.other_agc
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
                            singhal_average[i] = Measurements[i].Sighal;
                            agc_average[i] = Measurements[i].Agc;
                        }
                        dist_ave = (int)dist_average.Average();
                        singhal_ave = (int)singhal_average.Average();
                        agc_ave = (int)agc_average.Average();
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            if (lk_Sensor_Data.qc_current_stand == 1)
                            {
                                int dis = lk_Sensor_Data.qc_distCalibration_arry[0] = (UInt16)(dist_ave - stand_slider.Value * 100);
                                textBox_calibration_1.Text = dis.ToString();
                                textBox_average_1.Text = dist_ave.ToString();
                                textBox_gain_1.Text = agc_ave.ToString();
                            }
                            else if (lk_Sensor_Data.qc_current_stand == 2)
                            {
                                int dis= lk_Sensor_Data.qc_distCalibration_arry[1] = (UInt16)(dist_ave - stand_slider.Value * 100);
                                textBox_calibration_2.Text = dis.ToString();
                                textBox_average_2.Text = dist_ave.ToString();
                                textBox_gain_2.Text = agc_ave.ToString(); 
                            }
                            else if (lk_Sensor_Data.qc_current_stand == 3)
                            {
                                int dis = lk_Sensor_Data.qc_distCalibration_arry[2] = (UInt16)(dist_ave - stand_slider.Value * 100);
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
                lkFrame.anysys_frame(buf[i]);
              //  lk_Other_Protecl.protecl_ansys(buf[i]);
            }         
        }

    
        /// <summary>
        /// 停止测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_stop(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.dist_stop();
        }
       
        private void Btn_Clicked_Stand(object sender, RoutedEventArgs e)
        {

            if (lk_Sensor_Data.qc_current_stand == 1)
                {
                  stand_slider.Value = 10;
                  ifCalculate = true;
                }
                else
                {
                    MessageBox.Show("请先选择档位!");
                }

        }

        private void Btn_Clicked_saveParam(object sender, RoutedEventArgs e)
        {
            byte[] setByte = new byte[4];
            setByte[0] = (byte)(lk_Sensor_Data.qc_distCalibration_arry[0] >> 8);
            setByte[1] = (byte)(lk_Sensor_Data.qc_distCalibration_arry[0] & 0xff);
            setByte[2] = (byte)(lk_Sensor_Data.qc_gain_arry[0] >> 8);
            setByte[3] = (byte)(lk_Sensor_Data.qc_gain_arry[0] & 0xff);
            cmdFuncLists.qc_save_first_param(setByte);
        }

        private void Btn_Clicked_getStandParam(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.qc_get_param();
        }

        private void Btn_first_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.qc_first_switch();

        }

        private void Btn_third_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.qc_third_switch();
        }

        private void Btn_second_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.qc_second_switch();
        }

        private void window_close_click(object sender, EventArgs e)
        {
            if(lk_serial.check())
            {
                lk_serial.Close();
            }    
        }

        private void Btn_Clicked_saveParam_2(object sender, RoutedEventArgs e)
        {
            byte[] setByte = new byte[4];
            setByte[0] = (byte)(lk_Sensor_Data.qc_distCalibration_arry[1] >> 8);
            setByte[1] = (byte)(lk_Sensor_Data.qc_distCalibration_arry[1] & 0xff);
            setByte[2] = (byte)(lk_Sensor_Data.qc_gain_arry[1] >> 8);
            setByte[3] = (byte)(lk_Sensor_Data.qc_gain_arry[1] & 0xff);
            cmdFuncLists.qc_save_second_param(setByte);
        }

        private void Btn_Clicked_Stand_2(object sender, RoutedEventArgs e)
        {
            if (lk_Sensor_Data.qc_current_stand == 2)
            {                        
                ifCalculate = true;
            }
            else
            {
                MessageBox.Show("请先选择档位!");
            }
        }

        private void Btn_Clicked_saveParam_3(object sender, RoutedEventArgs e)
        {

            byte[] setByte = new byte[4];        
            setByte[0] = (byte)(lk_Sensor_Data.qc_distCalibration_arry[2] >> 8);
            setByte[1] = (byte)(lk_Sensor_Data.qc_distCalibration_arry[2] & 0xff);
            setByte[2] = (byte)(lk_Sensor_Data.qc_gain_arry[2] >> 8);
            setByte[3] = (byte)(lk_Sensor_Data.qc_gain_arry[2] & 0xff);
            cmdFuncLists.qc_save_third_param(setByte);
        }

        private void Btn_Clicked_Stand_3(object sender, RoutedEventArgs e)
        {
            if (lk_Sensor_Data.qc_current_stand == 3)
            {
                ifCalculate = true;
            }
            else
            {
                MessageBox.Show("请先选择档位!");
            }
        }
       
        private void btn_reset_stand_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.qc_reset_first_parm();
        }
        /// <summary>
        /// 重新标定时清楚对应数据
        /// </summary>
        /// <param name="index"></param>
        private void clear_texbox_standMsg(byte inde_s)
        {
            enum_qc_index_stand index = (enum_qc_index_stand)inde_s;
            switch (index)
            {
                case enum_qc_index_stand.first:
                    {
                        //base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        //{
                        //    textBox_calibration_1.Text = "";
                        //    textBox_average_1.Text = "";
                        //    textBox_gain_1.Text = "";
                        //}), new object[0]);
                    }
                    break;
                case enum_qc_index_stand.second:
                    {
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            textBox_calibration_2.Text = "";
                            textBox_average_2.Text = "";
                            textBox_gain_2.Text = "";
                        }), new object[0]);
                    }
                    break;
                case enum_qc_index_stand.third:
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
            cmdFuncLists.qc_reset_second_parm();
            texblock_standSecondLog.Text = "未标定";
        }

        private void btn_reset_stand3_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.qc_reset_third_parm();
            texblock_standThirdLog.Text = "未标定";
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

        private void radioBtn_front_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.sensor_front_base(set_high);
        }

        private void radioBtn_base_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.sensor_front_base(set_low);
        }


        private void Btn_Click_getParam(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.sensor_get_param();
        }               
        byte[] set_high = new byte[] { 1 };
        byte[] set_low = new byte[] { 0 };
        private void checkBox_Atuo_click(object sender, RoutedEventArgs e)
        {        
            
            cmdFuncLists.sensor_autoMel(set_high);
        }

        private void reset_button_click(object sender, RoutedEventArgs e)
        {
            cmdFuncLists.sensor_reset();
        }

        private void btn_click_powerOn_mode(object sender, RoutedEventArgs e)
        {
            string text = sensor_autoRun_mode_combox.SelectedItem.ToString();
            if (text == "自动测量")
            {
                cmdFuncLists.sensor_autoMel(set_high);
            }
            else if(text == "关闭自动测量")
            {
                cmdFuncLists.sensor_autoMel(set_low);
            }
            else
            {
                MessageBox.Show("请选择模式");
            }
        }


        private void btn_click_setBaudRate(object sender, RoutedEventArgs e)
        {
            int baudRate_index = sensor_baudRate_combox.SelectedIndex;
            byte[] set = new byte[] { (byte)baudRate_index };
            if(baudRate_index == -1)
            {
                MessageBox.Show("请选择对应的波特率");
            }
            cmdFuncLists.sensor_baudRate(set);
           
            
        }

        private void btn_click_ouput_data_freq(object sender, RoutedEventArgs e)
        {
            UInt16 ouput_data_freq = (UInt16)sensor_ouput_data_freq_slider.Value;
            byte[] data = new byte[2];
            data[0]=(byte) (ouput_data_freq >> 8);
            data[1] = (byte)(ouput_data_freq & 0xff);
            cmdFuncLists.sensor_outdata_freq(data);
        }

        private void btn_click_front_switch(object sender, RoutedEventArgs e)
        {
            UInt16 ouput_data_freq = (UInt16)sensor_front_switch_slider.Value;
            byte[] data = new byte[2];
            data[0] = (byte)(ouput_data_freq >> 8);
            data[1] = (byte)(ouput_data_freq & 0xff);
            cmdFuncLists.sensor_set_switch_front(data);
        }

        private void btn_click_base_switch(object sender, RoutedEventArgs e)
        {
            UInt16 ouput_data_freq = (UInt16)sensor_base_switch_slider.Value;
            byte[] data = new byte[2];
            data[0] = (byte)(ouput_data_freq >> 8);
            data[1] = (byte)(ouput_data_freq & 0xff);
            cmdFuncLists.sensor_set_switch_base(data);
        }
        /// <summary>
        /// 开发人员应答
        /// </summary>
        /// <param name="id"></param>
        /// <param name="buf"></param>
        public void proramer_ack_id(Protecl_typical_cmd.programer_ack_id id, byte[] buf)
        {
            switch (id)
            {
                case Protecl_typical_cmd.programer_ack_id.qc_get_param_ack:
                    {
                        sensor_programer_qc_refresh(buf);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standFirst_switch_ack:
                    {
                        lk_Sensor_Data.qc_current_stand = 1;
                        lk_log("档位1切换成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            stand_slider.Value = 10;
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standSecond_switch_ack:
                    {
                        lk_Sensor_Data.qc_current_stand = 2;
                        lk_log("档位2切换成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            stand_slider.Value = 45;
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standthird_switch_ack:
                    {
                        lk_Sensor_Data.qc_current_stand = 3;
                        lk_log("档位3切换成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            stand_slider.Value = 90;
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standFirst_reset_ack:
                    {
                        lk_log("档位1复位成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            texblock_standFirstLog.Text = "未标定";
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standSecond_reset_ack:
                    {
                        lk_log("档位2复位成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            texblock_standSecondLog.Text = "未标定";
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standthird_reset_ack:
                    {
                        lk_log("档位3复位成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            texblock_standThirdLog.Text = "未标定";
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standFirst_save_ack:
                    {
                        lk_log("档位1保存成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            texblock_standFirstLog.Text = "标定成功";
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standSecond_save_ack:
                    {
                        lk_log("档位2保存成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            texblock_standSecondLog.Text = "标定成功";
                        }), new object[0]);
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standthird_save_ack:
                    {
                        lk_log("档位3保存成功");
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            texblock_standThirdLog.Text = "标定成功";
                        }), new object[0]);
                    }
                    break;
            }

        }
        /// <summary>
        /// usr_ack 通用应答
        /// </summary>
        /// <param name="id"> </param>
        private void user_ack_id(Protecl_typical_cmd.user_ack_id id, byte[] buf)
        {
            switch (id)
            {
                case Protecl_typical_cmd.user_ack_id.dist_base://
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.dist_continue_ack:
                    {
                       lk_Sensor_Data.distance = (UInt16)(buf[0] << 8 | buf[1]);
                        display();
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.dist_once_ack:
                    {
                        UInt16 distance = (UInt16)(buf[0] << 8 | buf[1]);
                        lk_log(distance.ToString());
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.dist_stop_ack:
                    {
                        lk_log("停止测量成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.get_paramAll_base: //
                    {
                        sensor_param_refresh(buf);
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_baudRate_ack:
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_frontSwich_ack:
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_backSwich_ack:
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_disBase_ack:
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_powerOn_mode_ack:
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_all:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_baudRate_ack:
                    {
                        lk_log("波特率设置成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_frontSwich_ack:
                    {
                        lk_log("设置前开关量成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_backSwich_ack:
                    {
                        lk_log("设置后开关量成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_distBase_ack:
                    {
                        lk_log("设置基准成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_powerOn_mode_ack:
                    {
                        lk_log("开机自动运行成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_outData_freq_ack:
                    {
                        lk_log("输出频率设置成功");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.system_boot_paramReset_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.system_boot_firmware_ctl_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.system_boot_firmware_pakage_ack:
                    {
                    }
                    break;


            }

        }
    }

    public class Measurement
    {
        public double Time { get; set; }
        public double Sighal { get; set; }
        public double Distance { get; set; }
        public double Agc { get; set; }
    }
}
