using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Krusefy
{
    internal class Response
    {
        private Byte[] data = null;

        private Response(Byte[] data)
        {
            this.data = data;
        }
        //public static Response From(Request request)
        //{
        //    if (request.type != "GET")
        //    {
        //        Debug.WriteLine("Cannot handle request type.");
        //        return ErrorPage();
        //    }

        //    if (request.command.Contains("fun_")) // If a function call is commanded
        //    {
        //        if (request.command.Contains("play"))
        //        {
        //            Debug.WriteLine("play/pause");
        //        }
        //        if (request.command.Contains("getPlaylist"))
        //        {
        //            //string decodedString = System.Web.HttpUtility.UrlDecode(request.command);
        //            //string[] splitString = decodedString.Split("||");
        //        }
        //    }
        //    else
        //    {
        //        string file = Environment.CurrentDirectory + Server.WEB_DIR + request.command;
        //        FileInfo f = new FileInfo(file);
        //        if (f.Exists && f.Extension.Contains("."))
        //        {
        //            return MakeFromFile(f);
        //        }
        //        else
        //        {
        //            DirectoryInfo di = new DirectoryInfo(f + "\\");
        //            if (!di.Exists)
        //                return ErrorPage();
        //            FileInfo[] files = di.GetFiles();
        //            foreach (FileInfo ff in files)
        //            {
        //                if (ff.Name.Contains("index.html"))
        //                    return MakeFromFile(ff);
        //            }
        //        }
        //    }
        //    return ErrorPage();
        //}

        public static Response MakeFromFile(FileInfo f)
        {
            FileStream fs = f.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new Byte[fs.Length];
            reader.Read(d, 0, d.Length);
            fs.Close();

            return new Response(d);
        }

        public static Response MakeErrorPage()
        {
            string file = Environment.CurrentDirectory + Server.WEB_DIR + "404.html";
            FileInfo fi = new FileInfo(file);
            FileStream fs = fi.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new Byte[fs.Length];
            reader.Read(d, 0, d.Length);
            fs.Close();

            return new Response(d);
        }
        public void Post(NetworkStream stream)
        {
            StringBuilder sbHeader = new StringBuilder();

            sbHeader.AppendLine(Server.VERSION);
            // CONTENT-LENGTH
            sbHeader.AppendLine("Content-Length: " + data.Length);

            // Append one more line breaks to seperate header and content.
            sbHeader.AppendLine();

            List<byte> response = new List<byte>();
            // response.AddRange(bHeadersString);
            response.AddRange(Encoding.ASCII.GetBytes(sbHeader.ToString()));
            response.AddRange(data);
            byte[] responseByte = response.ToArray();
            stream.Write(responseByte, 0, responseByte.Length);
        }
    }
}
