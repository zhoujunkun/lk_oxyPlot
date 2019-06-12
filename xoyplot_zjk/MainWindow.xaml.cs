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
        private  Task task;

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

        public enum FRAME_TYPE { DataGet = 1, ParmsSave, ParamGet, Upload, ACK, QC, Erro };
        private UInt16 sighal { set; get; }
        private UInt16 distance { set; get; }
        private UInt16 agc { set; get; }
        /// <summary>
        /// 通用监听协议解析完成函数
        /// </summary>
        /// <param name="lkSensor"></param>
        private void genralListen(byte[]buf)
        {
            byte frame_type = buf[0];
            FRAME_TYPE type = (FRAME_TYPE)frame_type;
            switch (type)
            {
                case FRAME_TYPE.DataGet:
                    {
                         sighal = (UInt16)((buf[1] << 8) | buf[2]);
                         distance = (UInt16)((buf[3] << 8) | buf[4]);
                         agc = (UInt16)((buf[5] << 8) | buf[6]);
                        display();
                        
                    }
                    break;
                case FRAME_TYPE.Upload:
                    {

                    }
                    break;
                case FRAME_TYPE.ACK:
                    {
                      
                    }
                    break;
                case FRAME_TYPE.QC:
                    {
                  
                    }
                    break;
                case FRAME_TYPE.Erro:
                    {

                    }
                    break;
                default:
                    break;
            }

        }

        private void distanceId_func(LKSensorCmd.FRAME_GetDataID id, byte[] buff)
        {
            switch (id)
            {
                case LKSensorCmd.FRAME_GetDataID.DistContinue:
                    {

                    }break;
            }
        }

        double x = 0;
        private void btn_start(object sender, RoutedEventArgs e)
        {
            task = Task.Factory.StartNew(
                          () =>
                          {
                              
                              while (!complete)
                              {
                                  this.Title = "Plot updated: " + DateTime.Now;
                        //  this.Points.Add(new DataPoint(x, Math.Sin(x)));
                        // this.Points.Add(new DataPoint(x, rnd.NextDouble() * 5));
                        this.Measurements.Add(new Measurement
                                  {
                                      Time = x,
                                      Distance = rnd.NextDouble() * 5,
                                      Sighal = Math.Sin(x)
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
                                  Thread.Sleep(50);
                              }
                          });
        }
        private void display()
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
           if(  rx_checkSum(buf))
            {
                genralListen(buf);
            }
        }


        private bool rx_checkSum(byte[]buf)
        {
            byte ret = 0;
            for(int i=0;i<buf.Length;i++)
            {
                ret += buf[i];
            }
            if (ret == 0xff)
            {
                return true;
            }
            else return false;
        }
    }

    public class Measurement
    {
        public double Time { get; set; }
        public double Sighal { get; set; }
        public double Distance { get; set; }

     
    }
}
