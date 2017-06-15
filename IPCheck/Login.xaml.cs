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

namespace IPCheck
{
    /// <summary>
    /// Login.xaml の相互作用ロジック
    /// </summary>
    public partial class Login : Window
    {
        LoginSetting ls = new LoginSetting();
        public Login()
        {
            InitializeComponent();
            usertext.Text = ls.username;
            hosttext.Text = ls.host;
            porttext.Text = ls.port;
            if (ls.ssl)
                usessl.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ls.username = usertext.Text;
            ls.host = hosttext.Text;
            ls.port = porttext.Text;
            ls.Save();
            this.DialogResult = true;
        }
    }
}
