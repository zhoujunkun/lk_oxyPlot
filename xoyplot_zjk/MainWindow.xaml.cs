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
        public MainWindow()
        {
            this.plotModel = new PlotModel();
            this.plotModel.Series.Add(new FunctionSeries());
            
            //plotModel.LegendBackground = OxyColor.FromArgb(200, 255, 255, 255);
            // plotModel.LegendBorder = OxyColors.Black;
            plotModel.LegendOrientation = LegendOrientation.Horizontal;
            plotModel.LegendPlacement = LegendPlacement.Outside;
            plotModel.LegendPosition = LegendPosition.RightTop;
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
            var functionSeries1 = new FunctionSeries();

            DataContext = this;
            var worker = new BackgroundWorker { WorkerSupportsCancellation = true };
            double x = 0;
            Func<double, double> z_func = (Y) => Y;
            worker.DoWork += (s, e) =>
            {
                while (!worker.CancellationPending)
                {
                    lock (this.plotModel.SyncRoot)
                    {
                        this.plotModel.Title = "Plot udated: " + DateTime.Now;
                        //    this.plotModel.Series[0] = new FunctionSeries(z_func, x, x + 4, 1.0);
                        //   plotModel.Series[0].Title = "距离";
                      
                    }
                    x += 0.1;
                    plotModel.InvalidatePlot(true);
                    Thread.Sleep(100);
                }
            };

            worker.RunWorkerAsync();
            this.Closed += (s, e) => worker.CancelAsync();
        }
    }
}
