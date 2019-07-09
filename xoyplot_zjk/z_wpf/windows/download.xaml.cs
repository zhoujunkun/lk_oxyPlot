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
using lk_tool;
using ZSeial;

namespace lk_windows
{
    /// <summary>
    /// download.xaml 的交互逻辑
    /// </summary>
    public partial class lk_download_win : MetroWindow
    {
        download lk_dowload = new download();
        z_serial lk_serial;
        public bool ifDowonloadOK { set; get; }  //是否可以升级
        public lk_download_win(MainWindow MyMainWindow)
        {
            lk_serial = MyMainWindow.lk_serial;
            lk_dowload.set_control(lk_serial, progressUpload);
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
            if( lk_dowload.open_firmware())
            {
                ifDowonloadOK = true;
                texbockFileName.Text = lk_dowload.firemwar_name;
                textBlockFileSize.Text = lk_dowload.firemwar_size;
            }
            else
            {

            }
        }

        private void btn_uploadClick(object sender, RoutedEventArgs e)
        {
           if( lk_serial.check())
            {
               if (ifDowonloadOK)
                {
                    MessageBoxResult dr = MessageBox.Show("是否确认升级固件！", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (dr == MessageBoxResult.OK)
                    {
                        lk_dowload.upload();
                    }
                }
               else
                {
                    MessageBox.Show("打开固件");
                }
              
               
            }
        }
    }
}
