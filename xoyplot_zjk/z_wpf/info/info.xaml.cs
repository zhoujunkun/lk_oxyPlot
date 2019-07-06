using MahApps.Metro.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using xoyplot_zjk;

namespace zLkControl
{

    /// <summary>
    /// info.xaml 的交互逻辑
    /// </summary>
    public partial class info : MetroWindow, IComponentConnector
    {
        internal info infoWindow =null ;
        public info(MainWindow MyMainWindow)
        {
            this.InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

        private void Hyperlink_Click2(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
    }
}
