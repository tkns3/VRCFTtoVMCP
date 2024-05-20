using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VRCFTtoVMCP
{
    internal static class OscJsonServer
    {
        internal static bool TrackingEnable = false;

        static string AvatarInfoForTrackingOff = "";
        static string AvatarInfoForTrackingOn = "";
        static HttpListener? _listener;
        static Thread? _thread;

        internal static int Start(int port)
        {
            AvatarInfoForTrackingOff = OscJsonString("avtr_00000000-0000-0000-0000-000000000000");
            AvatarInfoForTrackingOn = OscJsonString("avtr_00000000-0000-0000-0000-000000000001");
            port = FindAvailablePort(port, 65535);
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            _listener.Start();
            _thread = new Thread(() => ServerLoop());
            _thread.Start();
            return port;
        }

        internal static void Stop()
        {
            _listener?.Stop();
            _thread?.Join();
            _thread = null;
            _listener = null;
        }

        static void ServerLoop()
        {
            try
            {
                while (_listener != null)
                {
                    var context = _listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;
                    if (request != null)
                    {
                        byte[] text = Encoding.UTF8.GetBytes(TrackingEnable ? AvatarInfoForTrackingOn : AvatarInfoForTrackingOff);
                        response.OutputStream.Write(text, 0, text.Length);
                    }
                    else
                    {
                        response.StatusCode = 404;
                    }
                    response.Close();
                }
            }
            catch
            {
                // 
            }
        }

        static int FindAvailablePort(int startPort, int endPort)
        {
            for (int port = startPort; port <= endPort; port++)
            {
                TcpListener? tcpListener = null;
                try
                {
                    tcpListener = new TcpListener(IPAddress.Loopback, port);
                    tcpListener.Start();
                    return port;
                }
                catch (Exception)
                {
                    // 何かエラーが発生した場合、次のポートを試すためにループを継続
                }
                finally
                {
                    tcpListener?.Stop();
                }
            }
            return -1;
        }

        static string OscJsonString(string id)
        {
            string s = VRChat.ResourceString(id + ".json");
            JObject inJson = JObject.Parse(s);
            var parameters = (JArray?)inJson["parameters"];

            var outJson = new JObject();
            outJson["FULL_PATH"] = "/avatar";
            outJson["ACCESS"] = 0;
            outJson["CONTENTS"] = new JObject();
            outJson["CONTENTS"]["change"] = new JObject();
            outJson["CONTENTS"]["change"]["DESCRIPTION"] = "Avatar ID, updated whenever the user switches into a valid avatar.";
            outJson["CONTENTS"]["change"]["FULL_PATH"] = "/avatar/change";
            outJson["CONTENTS"]["change"]["ACCESS"] = 1;
            outJson["CONTENTS"]["change"]["TYPE"] = "s";
            outJson["CONTENTS"]["change"]["VALUE"] = new JArray(id);
            outJson["CONTENTS"]["parameters"] = new JObject();
            outJson["CONTENTS"]["parameters"]["FULL_PATH"] = "/avatar/parameters";
            outJson["CONTENTS"]["parameters"]["ACCESS"] = 0;
            outJson["CONTENTS"]["parameters"]["CONTENTS"] = new JObject();

            foreach (var p in parameters.Cast<JObject>())
            {
                string name = (string)p["name"];
                name = name[3..];
                outJson["CONTENTS"]["parameters"]["CONTENTS"][name] = new JObject();
                outJson["CONTENTS"]["parameters"]["CONTENTS"][name]["FULL_PATH"] = $"/avatar/parameters/v2/{name}";
                outJson["CONTENTS"]["parameters"]["CONTENTS"][name]["ACCESS"] = 3;
                outJson["CONTENTS"]["parameters"]["CONTENTS"][name]["TYPE"] = "f";
                outJson["CONTENTS"]["parameters"]["CONTENTS"][name]["VALUE"] = new JArray(0.0f);
            }

            return outJson.ToString(Formatting.None);
        }
    }
}
