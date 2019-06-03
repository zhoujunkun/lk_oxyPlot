using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
namespace xoyplot_zjk
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged 
    {

        private readonly Task task;

        private int refresh;

        private string title;
        private double minimum;
        int count;
        private bool complete;
        Random rnd = new Random();
        public MainWindow() 
        {
            Measurements = new Collection<Measurement>();
            this.Points = new List<DataPoint>();
            this.task = Task.Factory.StartNew(
                () =>
                {
                    double x = 0;
                    while (!complete)
                    {
                        this.Title = "Plot updated: " + DateTime.Now;
                        //  this.Points.Add(new DataPoint(x, Math.Sin(x)));
                       // this.Points.Add(new DataPoint(x, rnd.NextDouble() * 5));
                        this.Measurements.Add(new Measurement
                        {
                          Time=x,
                          Distance= rnd.NextDouble() * 5,
                          Sighal=Math.Sin(x)
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

            DataContext = this;
        }
        int remve_index = 0;
        int range = 50;

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
    }

    public class Measurement
    {
        public double Time { get; set; }
        public double Sighal { get; set; }
        public double Distance { get; set; }

     
    }
}
