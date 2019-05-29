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
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        Random rnd = new Random();
        double time = 0d;
        public MainWindow()
        {
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.IsEnabled = true;
            
        }
        int remve_index = 0;
        int range = 50;
        private void Timer_Tick(object sender, EventArgs e)
        {
            Data.Add(new DataPoint(time++, rnd.NextDouble() * 5));
            if(Data.Count>100)
            {
                Data.RemoveAt(0);
            }
            if(time > range)
            {
                lk_yaxi.Minimum = time - range;
            }
        }
        public ObservableCollection<DataPoint> Data
        {
            get { return (ObservableCollection<DataPoint>)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(ObservableCollection<DataPoint>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<DataPoint>()));
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            timer.IsEnabled = (sender as CheckBox)?.IsChecked ?? false;
            if (timer.IsEnabled)
            {
                // Reset
                time = 0d;
                Data = new ObservableCollection<DataPoint>();
            }
        }

        private void btn_click_show(object sender, RoutedEventArgs e)
        {
            
          //  Data.Clear();
            Data.RemoveAt(1);
        }

        private void btn_click_new(object sender, RoutedEventArgs e)
        {
            // Reset
            time = 0d;
            Data = new ObservableCollection<DataPoint>();
        }
    }
}
