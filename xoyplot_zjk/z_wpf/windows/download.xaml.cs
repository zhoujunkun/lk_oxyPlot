using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using xoyplot_zjk;

namespace lk_windows
{
    /// <summary>
    /// download.xaml 的交互逻辑
    /// </summary>
    public partial class lk_download_win : MetroWindow
    {
        public lk_download_win(MainWindow MyMainWindow)
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// 重写OnClosing事件 解决窗口关闭不能再开的bug。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void openFileUpdataClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
