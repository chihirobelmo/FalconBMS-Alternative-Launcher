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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using FalconBMS.Launcher.Input;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class RecommendReboot
    {
        public RecommendReboot()
        {
            InitializeComponent();
        }

        public static void ShowRecommendReboot()
        {
            RecommendReboot ownWindow = new RecommendReboot();
            ownWindow.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
