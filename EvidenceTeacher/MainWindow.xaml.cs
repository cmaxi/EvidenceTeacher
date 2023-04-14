using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
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

namespace EvidenceTeacher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private bool changeProgressBar(int value)
        {
            pbStatus.Value = value;
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//regex odstanit za zavinacem
            //datum 20230414-hhmm
            DateTime now = DateTime.Now;
            string email = this.email.Text;
            string[] emails = email.Split('@');
            string ddhm = this.ddhm.Text;
            string pathTxt = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\souhrn.txt";
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\souhrn.nfo";
            string pathFTP = "ftp://283265.w65.wedos.net/" + emails[0] + now.ToString("yyyyMMdd-hhmm") + ".nfo";
            string pathFTPtxt = "ftp://283265.w65.wedos.net/" + emails[0] +  now.ToString("yyyyMMdd-hhmm") + ".txt";


            changeProgressBar(10);

            if(email == "")
            {
                MessageBox.Show("Email nesmi byt prazdny");
                pbStatus.Value = 0;
                return;
            }

            if (!email.Contains('@'))
            {
                MessageBox.Show("Email neobsahuje zavinac");
                pbStatus.Value = 0;
                return;
            }

            if(ddhm == "")
            {
                MessageBox.Show("Neplatne evidencni cislo");
                pbStatus.Value = 0;
                return;
            }

            try
            {
                // Create the file, or overwrite if the file exists.
                using (FileStream fs = File.Create(pathTxt))
                {
                    using (var fw = new StreamWriter(fs))
                    {
                        fw.WriteLine(email);
                        fw.WriteLine(ddhm);
                        fw.Flush();
                    }
                }
                changeProgressBar(30);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Nepodarilo se vytvorit vychozi soubory");
                Console.WriteLine(ex.ToString());
                return;
            }

    



            string strCmdText;
            strCmdText = "/C msinfo32 /nfo "+path;
            try
            {
                if (!File.Exists(path)) {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = strCmdText;
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
                //System.Diagnostics.Process.Start("CMD.exe", strCmdText);
                
            }
            catch
            {
                MessageBox.Show("Nepodarilo se vytvorit msinfo32");
                return;
            }

            System.Threading.Thread.Sleep(1000);

            try
            {
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential("w283265_sber", "VAEcHBtL");
                    client.UploadFile(pathFTP, WebRequestMethods.Ftp.UploadFile, path);
                    client.UploadFile(pathFTPtxt, WebRequestMethods.Ftp.UploadFile, pathTxt);
                }
                pbStatus.Value = 100;
            }
            catch
            {
                MessageBox.Show("Nepodarilo se uploudovat soubory na FTP");
                return;
            }
            MessageBox.Show("Vse v poradku, dekujeme!");
        }
    }
}
