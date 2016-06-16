using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Fiddler;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace sixfrm
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        public static string intime;
        public static string outtime;
        [STAThread]

        static void Main()
        {
            Fiddler.FiddlerApplication.BeforeRequest += delegate (Fiddler.Session oSession)
            {
                //Console.WriteLine("Before request for:\t" + oSession.fullUrl);
                //Debug.WriteLine(oSession.RequestBody);
                oSession.bBufferResponse = true;
            };
            Fiddler.FiddlerApplication.BeforeResponse += delegate (Fiddler.Session oSession)
            {
                if (oSession.fullUrl.Contains("66.185.64.100:8080"))//66.185.64.100:8080
                {
                    oSession.utilDecodeResponse();
                    //Console.WriteLine("Before response for:\t{0} response code:\t{1}", oSession.fullUrl, oSession.responseCode);
                    string requestText = oSession.GetResponseBodyAsString();
                    //Console.WriteLine(requestText);
                    if (requestText.Contains("invigilate.js"))
                    {
                        requestText = requestText.Replace("<script type=\"text/javascript\" src=\"../../../../resources/scripts/proj/invigilate.js\"></script>", "");
                        oSession.utilSetResponseBody(requestText);
                        //Console.WriteLine("Changed to: " + requestText);
                    }
                    /*if (requestText.Contains("cheat_decrement()"))
                    {
                        requestText = requestText.Replace("cheat_decrement();", "");
                        oSession.utilSetResponseBody(requestText);
                    }*/
                    if (requestText.Contains("cheat_check"))
                    {
                        requestText = requestText.Replace("cheat_check(a);", "");
                        oSession.utilSetResponseBody(requestText);
                    }
                    if (requestText.Contains("[{\"id\":null"))
                    {
                        intime = getTimeStamp();
                        //Console.WriteLine("Catching question json...");
                        Write(requestText);
                        KillProcess("showsix");
                        Process.Start("showsix.exe");
                        //Console.WriteLine("Done.");
                    }
                    if (requestText.Contains("\"ip\":null"))
                    {
                        outtime = getTimeStamp();
                        requestText = requestText.Replace("\"ip\":null", "\"ip\":" + getIP());
                        requestText = requestText.Replace("\"paperState\":\"1\"", "\"paperState\":\"2\"");
                        requestText = requestText.Replace("\"inTime\":null", "\"inTime\":" + intime);
                        requestText = requestText.Replace("\"outTime\":null", "\"outTime\":" + outtime);
                        requestText = requestText.Replace("\"userDuration\":null", "\"userDuration\":" + duration(intime, outtime));
                        oSession.utilSetResponseBody(requestText);
                        //Console.WriteLine("Changed to: " + requestText);
                    }
                }
            };
            CleanIE6();   //ie6 can't use
            Fiddler.FiddlerApplication.Startup(8877, FiddlerCoreStartupFlags.Default);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }
        static void RunCmd(string cmd)
        {
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = "cmd.exe";
            p.Arguments = "/c " + cmd;
            p.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(p);
        }
        static void CleanIE()
        {
            /*
            1.History 2.Cookies 8.Temporary Internet Files
            16.Form Data 32.Passwords 255.Delete All
            4351.Delete All - "Also delete files and settings stored by add-ons"
            */
            RunCmd("RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 8");
        }
        static void CleanIE6()
        {
            string cachePath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            DirectoryInfo di = new DirectoryInfo(cachePath);
            foreach (FileInfo fi in di.GetFiles("*.*", SearchOption.AllDirectories))
            {
                Console.WriteLine(fi.Name);
                try
                {
                    fi.Delete();
                }
                catch { }
            }
        }
        static void Write(string str)
        {
            FileStream fs = new FileStream("json.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(str);
            sw.Close();
            fs.Close();
        }
        static string getIP()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            for (int i = 0; i < ipEntry.AddressList.Length; i++)
            {
                if (ipEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ipEntry.AddressList[i].ToString();
                }
            }
            return "";
        }
        static string getTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }
        static string duration(string intime, string outtime)
        {
            long iintime = long.Parse(intime);
            long iouttime = long.Parse(outtime);
            return (((iouttime - iintime) / 1000).ToString());
        }
        public static void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                    item.Close();
                }
            }

        }
    }
}
