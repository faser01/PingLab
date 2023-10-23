using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using Timer = System.Timers.Timer;
using System.Collections.Generic;

namespace PingLab
{
    public partial class Form1 : Form
    {
        IPAddress[] ips;
        Timer timer;
        int packetSize = 1024; //  размер пакета ICMP
        string logFileName = "log.txt";
        int pingCount;


        public Form1()
        {
            InitializeComponent();
            timer = new Timer { Interval = 2000 }; // Интервал в миллисекундах (2 секунды)
            timer.Elapsed += TimerElapsed;
        }

        Dictionary<IPAddress, int> pingCounter = new Dictionary<IPAddress, int>();

        void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var ip in ips)
            {
                if (!pingCounter.ContainsKey(ip))
                {
                    pingCounter[ip] = 0; // инициализируем счетчик
                }

                if (pingCounter[ip] < pingCount)
                {
                    var result = MyPing(ip, packetSize);
                    LogToFile(result);
                    pingCounter[ip]++; 
                }
            }
        }

        string MyPing(IPAddress ip, int bufferSize)
        {
            string result = string.Empty;
            string br = "\r\n";
            string hostName = string.Empty;
            Ping ping = new Ping();
            PingOptions pingOptions = new PingOptions();
            pingOptions.DontFragment = true;

            try
            {
                byte[] buffer = new byte[bufferSize];

                hostName = Dns.GetHostEntry(ip).HostName;
                PingReply pingReply = ping.Send(ip, (int)timer.Interval, buffer, pingOptions);
                if (pingReply.Status == IPStatus.Success)
                {
                    result = $"Узел {hostName} ({ip}) доступен {DateTime.Now.ToString("HH:mm:ss.fff")}, Размер пакета: {bufferSize}, Частота проверки: {timer.Interval / 1000.0} seconds.{br}";
                }
                else
                {
                    result = $"Узел {hostName} ({ip}) НЕ ДОСТУПЕН {DateTime.Now.ToString("HH:mm:ss.fff")}, Размер пакета: {bufferSize}, Частота проверки: {timer.Interval / 1000.0} seconds.{br}";
                }
            }
            catch (Exception ex)
            {
                result = $"ОШИБКА: {ex.Message}{br}";
            }

            this.Invoke((MethodInvoker)delegate {
                tbResult.Text += result + Environment.NewLine;
            });

            return result;
        }

        void LogToFile(string message)
        {
            File.AppendAllText(logFileName, message);
        }

       

        private void button1_Click_1(object sender, EventArgs e)
        {

            pingCount = (int)numericUpDown1.Value;
            pingCounter.Clear(); // очищаем счетчик

            List<IPAddress> ipList = new List<IPAddress>();

            foreach (var host in tbIPAddresses.Lines)
            {
                if (string.IsNullOrWhiteSpace(host))
                    continue;

                try
                {
                    var ips = Dns.GetHostEntry(host).AddressList[0];
                    ipList.Add(ips);
                }
                catch (SocketException)
                {
                    MessageBox.Show($"Не удалось разрешить хост: {host}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                  
                }
            }

            ips = ipList.ToArray();
            timer.Start();
        }
    }

  
}