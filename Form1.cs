using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RestSharp;
using System.Xml;
using System.IO;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;


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
            var opt2 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option2").InnerText);
            var opt3 = Convert.ToInt32(doc.SelectSingleNode("Settings/General/Option3").InnerText);

            var watcher = new FileSystemWatcher(@opt2);
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = false;

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
        private void redStateOn()
        {
            var client = new RestClient("http://raspberrypi.local:5000/led/red/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "1");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        private void redStateOff()
        {
            var client = new RestClient("http://raspberrypi.local:5000/led/red/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "0");
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
        private void greenStateOn()
        {
            var client = new RestClient("http://raspberrypi.local:5000/led/green/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "1");
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
        private void GenerateLog(string OutputLine)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt2 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option2").InnerText);
            var opt3 = Convert.ToInt32(doc.SelectSingleNode("Settings/General/Option3").InnerText);

            var directory = new DirectoryInfo(@opt2);
            var myFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().ToString();
            string updateFile = @opt2 + myFile;
            DateTime lastModified = System.IO.File.GetLastWriteTime(updateFile);
            DateTime lastCreated = System.IO.File.GetCreationTime(updateFile);
            var timeDiff = lastModified - lastCreated;

            //Pass the filepath and filename to the StreamWriter 
            string currentTime = DateTime.Now.ToString(@"dd_MM_yyyy");
            StreamWriter sw = File.AppendText($"{currentTime}.txt");
            //Write a line of text
            //sw.WriteLine(OutputLine);
            //Write a second line of text
            sw.WriteLine($"{OutputLine},{lastCreated.ToString(@"hh\:mm\:ss")},{timeDiff.ToString(@"hh\:mm\:ss")}");
            //Close the file
            sw.Close();
        }
        private void GenerateLogUpDown(string OutputLine)
        {
            //Pass the filepath and filename to the StreamWriter 
            string currentTime = DateTime.Now.ToString(@"dd_MM_yyyy");
            StreamWriter sw = File.AppendText($"{currentTime}.txt");
            //Write a line of text
            //sw.WriteLine(OutputLine);
            //Write a second line of text
            sw.WriteLine($"{OutputLine},{DateTime.Now.ToString(@"hh\:mm\:ss")}");
            //Close the file
            sw.Close();
        }
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
            greenStateOn();
            IdleStateOff();
            redStateOff();
            Console.WriteLine("Green");
        }
         private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine("Yellow");
            IdleStateOn();
            redStateOff();
            greenStateOff();
            GenerateLog("RUN");
        }
        private void DoPing() 
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt1 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option1").InnerText);
            Ping myPing = new Ping();

            var client = new RestClient("http://raspberrypi.local:5000/led/red/");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var data = JObject.Parse(response.Content);
            string redState = data.GetValue("red").ToString();
            Console.WriteLine(redState);

            PingReply reply = myPing.Send(opt1, 1000);
            if (reply.Status.ToString() == "Success")
            {
                Console.WriteLine("Status :  " + reply.Status + " \n Time : " + reply.RoundtripTime.ToString() + " \n Address : " + reply.Address);
                if (redState == "1") { IdleStateOn(); redStateOff(); GenerateLogUpDown("ON"); }
            }
            else
            {
                if (redState == "0") { GenerateLogUpDown("DOWN"); }
                redStateOn();
                IdleStateOff();
                greenStateOff();
                Console.WriteLine("DOWN");
            }
        
        }
        
        private void Form1_Load(object sender, EventArgs e)
		{
            backgroundWorker1_DoWork();
            while (true) { DoPing(); Thread.Sleep(10000); }
        }

	}
}
