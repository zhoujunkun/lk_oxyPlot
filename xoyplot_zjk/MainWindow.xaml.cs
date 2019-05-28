using System;
using System.Collections.Generic;
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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
namespace xoyplot_zjk
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlotModel plotModel { get; set; }

        private readonly Task task;
        private int refresh;
        private string title;
        private bool complete;
        public MainWindow()
        {

            this.Points = new List<DataPoint>();
          
            //plotModel.LegendBackground = OxyColor.FromArgb(200, 255, 255, 255);
            // plotModel.LegendBorder = OxyColors.Black;
            //plotModel.LegendOrientation = LegendOrientation.Horizontal;
            //plotModel.LegendPlacement = LegendPlacement.Outside;
            //plotModel.LegendPosition = LegendPosition.RightTop;
            //
            //var linerAxis = new LinearAxis();
            //linerAxis.MajorStep = 1;
            //linerAxis.MinorStep = 1;
            //linerAxis.MajorGridlineThickness = 3;
            //linerAxis.MinorTicklineColor = OxyColors.Gray;
            //linerAxis.TicklineColor = OxyColors.Red;
            //linerAxis.TickStyle = TickStyle.Crossing;
            //plotModel.Axes.Add(linerAxis);

            //var linearAxis2 = new LinearAxis();
            //linearAxis2.MajorGridlineThickness = 3;
            //linearAxis2.MinorGridlineThickness = 1;
            //linearAxis2.MajorStep = 1;
            //linearAxis2.MinorStep = 1;
            //linearAxis2.MinorTicklineColor = OxyColors.Gray;
            //linearAxis2.Position = AxisPosition.Bottom;
            //linearAxis2.TicklineColor = OxyColors.Blue;
            //plotModel.Axes.Add(linearAxis2);

            //function series


            var worker = new BackgroundWorker { WorkerSupportsCancellation = true };

            worker.DoWork += (s, e) =>
            {
                double x = 0;
                double y = 0;
                while (!worker.CancellationPending)
                {
                    Title = "Plot updated: " + DateTime.Now;
                    Points.Add(new DataPoint(x, Math.Sin(y)));
                    // Change the refresh flag, this will trig InvalidatePlot() on the Plot control
                    this.Refresh++;
                   // lk_plot.InvalidateFlag++;
                    y += 0.1;
                    x += 1;
                    //if(x>20)
                    //{
                    //    lk_Y.MinimumRange = x - 20;
                    //}
                    
                    this.Refresh++;
                    Thread.Sleep(100);
                }
            };
            worker.RunWorkerAsync();
            this.Closed += (s, e) => worker.CancelAsync();
            DataContext = this;

        }

        public void Close()
        {
            this.complete = true;
            this.task.Wait();
        }
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
    }
}
