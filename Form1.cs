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
using System.Threading.Tasks;


namespace ict_towerlight
{
    public partial class ICT_Towerlight : Form
	{
		public ICT_Towerlight()
		{
			InitializeComponent();
        }

        //static class Standby
        //{
        //    public static string IdleIndicator;
        //}

        //Added on February 21 by Raja Nazirul, Add background worker to ping testhead and show status.
        private void backgroundWorker1_DoWork()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt1 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option1").InnerText);
            var opt2 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option2").InnerText);

            var watcher = new FileSystemWatcher(@opt2);
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = false;
        }
        private void IdleStateOn()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt4 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option4").InnerText);

            var client = new RestClient(@opt4 + "/led/yellow/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "1");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        private void redStateOn()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt4 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option4").InnerText);

            var client = new RestClient(@opt4 + "/led/red/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "1");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        private void redStateOff()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt4 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option4").InnerText);

            var client = new RestClient(@opt4 + "/led/red/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "0");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        private void IdleStateOff()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt4 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option4").InnerText);

            var client = new RestClient(@opt4 + "/led/yellow/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "0");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        private void greenStateOn()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt4 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option4").InnerText);

            var client = new RestClient(@opt4 + "/led/green/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("state", "1");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        private void greenStateOff()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt4 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option4").InnerText);

            var client = new RestClient(@opt4 + "/led/green/");
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

        private CancellationTokenSource cancellationToken;

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
            Console.WriteLine("Green");
        }
         private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (cancellationToken != null )
            {
                cancellationToken.Cancel();
            }
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine("Yellow");
            GenerateLog("RUN");
            //Standby.IdleIndicator = "IDLE";
            greenStateOn();
            IdleStateOff();
            redStateOff();
            MachineRun();
        }
        private void DoPing() 
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt1 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option1").InnerText);
            var opt4 = Convert.ToString(doc.SelectSingleNode("Settings/General/Option4").InnerText);

            var client = new RestClient(@opt4 + "/led/red/");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            string redState = "";
            if (response.Content != "")
            {
                var data = JObject.Parse(response.Content);
                redState = data.GetValue("red").ToString();
                Console.WriteLine(redState);
            }
            try
            {
                Ping myPing = new Ping();
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
            catch (PingException) 
            {
                Console.WriteLine("ada masalah");
            }
        
        }
        
        private void Form1_Load(object sender, EventArgs e)
		{
            backgroundWorker1_DoWork();
            while (true) { DoPing(); Thread.Sleep(10000); }
        }

		private async void MachineRun()
		{
            XmlDocument doc = new XmlDocument();
            doc.Load("location.xml");
            var opt3 = Convert.ToInt32(doc.SelectSingleNode("Settings/General/Option3").InnerText);
            cancellationToken = new CancellationTokenSource();
            try
            {
                    await Task.Delay(opt3, cancellationToken.Token);
                    IdleStateOn();
                    redStateOff();
                    greenStateOff();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    
                }
        }
	}
}
