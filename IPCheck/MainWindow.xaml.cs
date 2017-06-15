using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace IPCheck
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Net.Mail.SmtpClient sc;
        string nowip
        {
            get { return _nowip; }
            set
            {
                _nowip = value;
                this.showIP.Text = value;
            }
        }
        private string _nowip;
        string sendermail = null;
        string recevermail = null;
        private static Mutex mutex = new Mutex();
        private DispatcherTimer dispatchertimer;
        private DateTime nextCheck;
        private TimeSpan checkInterval = new TimeSpan(0, 5, 0);
        public MainWindow()
        {
            InitializeComponent();
            outputLog("application start!");
            sc = new System.Net.Mail.SmtpClient();
            
            while (true)
            {
                Login login = new Login();
                login.ShowDialog();
                if (login.DialogResult == true)
                {
                    var user = login.usertext.Text.Trim();
                    var pwd = login.pwdtext.SecurePassword;
                    var host = login.hosttext.Text.Trim();
                    var port = login.porttext.Text.Trim();
                    var usessl = login.usessl.IsChecked;
                    if (user == "" || pwd.Length==0|| host == "" || port == "")
                    {
                        continue;
                    }
                    sc.Credentials = new System.Net.NetworkCredential(user, pwd);
                    //SMTPサーバーを指定する
                    sc.Host = host;
                    //ポート番号を指定する（既定値は25）
                    sc.Port = Convert.ToInt32(port);
                    //SMTPサーバーに送信する設定にする（既定はNetwork）
                    sc.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    if (usessl == true)
                        sc.EnableSsl = true;
                    sendermail = user;
                    recevermail = user;
                    sendToText.Text = user;
                    tickTimeText.Text = checkInterval.TotalSeconds.ToString();
                    dispatchertimer = new DispatcherTimer(DispatcherPriority.Normal);
                    dispatchertimer.Interval = new TimeSpan(0, 0, 1);
                    dispatchertimer.Tick += new EventHandler(dispatcherTimer_Tick);
                    dispatchertimer.Start();
                    Checkip();
                    nextCheck = DateTime.Now;
                    nextCheck = nextCheck.Add(checkInterval);
                    break;
                }
                else
                {
                    this.Close();
                    break;
                }
                
            }
            
        }

        public void dispatcherTimer_Tick(object sender,EventArgs e)
        {
            DateTime now = DateTime.Now;
            if (now.Ticks > nextCheck.Ticks)
            {
                nextCheck = nextCheck.Add(checkInterval);
                Checkip();
            }
            showtime.Text = TimeSpan.FromTicks(nextCheck.Ticks - now.Ticks).ToString(@"hh\:mm\:ss");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginSetting ls = new LoginSetting();
            //送信者
            string senderMail = ls.username;
            //宛先
            string recipientMail = ls.username;
            //件名
            string subject = "こんにちは test mail";
            //本文
            string body = "こんにちは。\r\n\r\nそれではまた。";

            //SmtpClientオブジェクトを作成する
            new Task(() =>
            {
                //メールを送信する
                sc.Send(senderMail, recipientMail, subject, body);
                //後始末（.NET Framework 4.0以降）
                sc.Dispose();
            }).Start();
        }

        public void outputLog(string message)
        {
            outlog.Items.Add(DateTime.Now.ToString("yy/MM/dd HH:mm:ss ") + message);
            outlog.SelectedIndex = outlog.Items.Count - 1;
        }

        public void sendMail(string message,string subject = "title")
        {

            //SmtpClientオブジェクトを作成する
            new Task(() =>
            {
                //メールを送信する
                mutex.WaitOne();
                sc.Send(sendermail, recevermail, subject, message);
                mutex.ReleaseMutex();
                //後始末（.NET Framework 4.0以降）
                sc.Dispose();
            }).Start();
        }

        public void setIP(string newip)
        {
            if (nowip == null)
                nowip = newip;
            else
            {
                if(nowip != newip)
                {
                    sendMail("The ip address has changed to: " + newip);
                    nowip = newip;
                }
            }
        }

        public void Checkip()
        {
            outputLog("start chekking ip");
            new Task(() =>
            {
                Console.WriteLine("Start getting ip");
                string externalIP;
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        this.outputLog("get ip using http://checkip.dyndns.org/");
                    }));
                    externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                    externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                                 .Matches(externalIP)[0].ToString();
                    Console.WriteLine("The ip address is:" + externalIP);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if(externalIP != null)
                        {
                            this.outputLog("get ip address: " + externalIP);
                            this.setIP(externalIP);
                        }
                        else
                        {
                            this.outputLog("failed to get ip");
                        }
                    }));
                }
                catch { }
            }).Start();
        }

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tickTimeText.Text != "")
                {
                    checkInterval = TimeSpan.FromSeconds(Convert.ToInt32(tickTimeText.Text));
                    nextCheck = DateTime.Now.Add(checkInterval);
                }
                else
                {
                    tickTimeText.Text = Convert.ToString(checkInterval.TotalSeconds);
                }
                if (sendToText.Text != "")
                {
                    recevermail = sendToText.Text;
                }
            }
            catch(Exception ex)
            {
                outputLog(ex.Message);
            }
        }

        private void scanBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                nextCheck = DateTime.Now.Add(checkInterval);
                Checkip();
            }
            catch(Exception ex)
            {
                outputLog(ex.Message);
            }
        }
    }
}
