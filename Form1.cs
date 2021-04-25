using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using System.Xml;
using System.IO;
using System.Net.NetworkInformation;


namespace ict_towerlight
{
	public partial class ICT_Towerlight : Form
	{
		public ICT_Towerlight()
		{
			InitializeComponent();
        }

        //Added on February 21 by Raja Nazirul, Add background worker to ping testhead and show status.
        private void backgroundWorker1_DoWork()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt1 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option1").InnerText);

            while (true)
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(opt1, 1000);
                if (reply.Status.ToString() == "Success")
                {
                    Console.WriteLine("Status :  " + reply.Status + " \n Time : " + reply.RoundtripTime.ToString() + " \n Address : " + reply.Address);
                    //Console.WriteLine(reply.ToString());
                    var client = new RestClient("http://raspberrypi.local:5000/led/red/");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AlwaysMultipartFormData = true;
                    request.AddParameter("state", "0");
                    IRestResponse response = client.Execute(request);
                    Console.WriteLine(response.Content);
                    CheckDirectory();
                }
                else
                {
                    var client = new RestClient("http://raspberrypi.local:5000/led/red/");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AlwaysMultipartFormData = true;
                    request.AddParameter("state", "1");
                    IRestResponse response = client.Execute(request);
                    Console.WriteLine(response.Content);
                    IdleStateOff();
                    greenStateOff();
                }
            }
        }

        private void IdleStateOn()
        {
            var client = new RestClient("http://raspberrypi.local:5000/led/yellow/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "1");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        private void IdleStateOff()
        {
            var client = new RestClient("http://raspberrypi.local:5000/led/yellow/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "0");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        private void greenStateOff()
        {
            var client = new RestClient("http://raspberrypi.local:5000/led/green/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "0");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        private void CheckDirectory()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt2 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option2").InnerText);
            var opt3 = Convert.ToInt32(doc.SelectSingleNode("Settings/General/Option3").InnerText);

            var directory = new DirectoryInfo(@opt2);
            var myFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().ToString();
            string updateFile = @opt2 + myFile;

            DateTime lastModified = System.IO.File.GetLastWriteTime(updateFile);
            string currentTime = DateTime.Now.ToString();
            var timeDiff = DateTime.Now - lastModified;
            var ts = timeDiff.Minutes;
            Console.WriteLine("Status :  " + opt2 + " \n Time : " + timeDiff.ToString(@"dd\.hh\:mm\:ss"));

            var watcher = new FileSystemWatcher(@opt2);
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = false;

            if (watcher.EnableRaisingEvents == true)
            {
                var client = new RestClient("http://raspberrypi.local:5000/led/green/");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("state", "1");
                IRestResponse response = client.Execute(request);
                IdleStateOff();
                Console.WriteLine("Green");
                GenerateLog(timeDiff.ToString(@"dd\.hh\:mm\:ss"));
            }
            else
            {
                var client = new RestClient("http://raspberrypi.local:5000/led/green/");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("state", "0");
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
                IdleStateOn();
                Console.WriteLine("Yellow");
                //GenerateLog("IDLE");
            }
        }

        private void GenerateLog(string OutputLine)
        {
            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw = File.AppendText("output.txt");
            //Write a line of text
            sw.WriteLine(OutputLine);
            string currentTime = DateTime.Now.ToString();
            //Write a second line of text
            sw.WriteLine(currentTime);
            //Close the file
            sw.Close();
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw = File.AppendText("output.txt");
            //Write a line of text
            //sw.WriteLine(value);
            string currentTime = DateTime.Now.ToString();
            //Write a second line of text
            sw.WriteLine(currentTime);
            //Close the file
            sw.Close();
            //GenerateLog(value);
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }


        private void Form1_Load(object sender, EventArgs e)
		{
            backgroundWorker1_DoWork();
		}
	}
}
