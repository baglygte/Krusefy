using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Krusefy
{
    internal class Server
    {
        public const String WEB_DIR = "\\server\\";
        public const String VERSION = "HTTP/1.1";
        public const String NAME = "Krusefy Server";
        private bool running = false;

        private TcpListener listener;
        private MusicPlayer musicPlayer;
        private PlaylistHandler playlistHandler;
        public Server(PlaylistHandler ph, MusicPlayer mp)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string serverIP = null;
            foreach(var ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    serverIP = ip.ToString();
                    Debug.WriteLine("Hosting server on: " + serverIP.ToString());
                }
            }
            listener = new TcpListener(IPAddress.Parse(serverIP), 8080);
            playlistHandler = ph;
            musicPlayer = mp;
        }

        public void Start()
        {
            Thread serverThread = new Thread(new ThreadStart(Run));
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void Run()
        {
            running = true;
            listener.Start();
            while (running)
            {
                TcpClient client = listener.AcceptTcpClient();
                HandleClient(client);
                client.Close();
            }
            running = false;
            listener.Stop();
        }

        private void HandleClient(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream());
            String msg = "";

            while (reader.Peek() != -1)
            {
                msg += reader.ReadLine() + "\n";
            }

            // Get a request
            Request request = Request.GetRequest(msg);

            // Decide what response to provide

            // If the type of request is unknown, return 404 page
            if (request.type != "GET")
            {
                Response resp = Response.MakeErrorPage(); // Make response
                resp.Post(client.GetStream()); // Send response
                return;
            }

            if (request.command.Contains("fun_")) // If a function call is commanded
            {
                string decodedString = WebUtility.UrlDecode(request.command);
                Debug.WriteLine(decodedString);
                string[] splitString = decodedString.Split('_');
                switch (splitString[1])
                {
                    case "play":
                        musicPlayer.PlayPause();
                        break;
                    case "next":
                        musicPlayer.Next();
                        break;
                    case "prev":
                        musicPlayer.Prev();
                        break;
                    //case "selectplaylist":
                    //    playlistHandler.PlaylistViewCall(splitString[2]);
                    //    break;
                    case "playtrack":
                        //Debug.WriteLine(splitString[2]);
                        //musicPlayer.PlayTrack(new Track(int.Parse(splitString[2]), playlistHandler));
                        break;
                }
            }
            else
            {
                string file = Environment.CurrentDirectory + Server.WEB_DIR + request.command;
                FileInfo f = new FileInfo(file);
                if (f.Exists && f.Extension.Contains("."))
                {
                    Response resp = Response.MakeFromFile(f); // Make response
                    resp.Post(client.GetStream()); // Send response
                    return;
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(f + "\\");
                    if (!di.Exists)
                    {
                        Response resp = Response.MakeErrorPage(); // Make response
                        resp.Post(client.GetStream()); // Send response
                        return;
                    }
                    FileInfo[] files = di.GetFiles();
                    foreach (FileInfo ff in files)
                    {
                        if (ff.Name.Contains("index.html"))
                        {
                            Response resp = Response.MakeFromFile(ff); // Make response
                            resp.Post(client.GetStream()); // Send response
                            return;
                        }
                    }
                }
            }
        }
    }
}
