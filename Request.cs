using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krusefy
{
    internal class Request
    {
        public String type;
        public String command;
        public Request(String t, String c)
        {
            type = t; command = c;
            //Debug.WriteLine("Requested: " + c);
        }

        public static Request GetRequest(String request)
        {
            if (String.IsNullOrEmpty(request))
                return null;

            String[] tokens = request.Split(' ');
            String type = tokens[0];
            String url = tokens[1];

            return new Request(type, url);
        }
    }
}
