using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using VRCFTtoVMCP.OSC;

namespace VRCFTtoVMCP
{
    internal class VrcOscReceiver
    {
        bool _running = false;
        Thread? _thread;
        IPEndPoint? _endPoint;
        UdpClient? _udpClient;
        Parser _parser = new();

        public void Start(int port)
        {
            Stop();
            _endPoint = new IPEndPoint(IPAddress.Any, port);
            _udpClient = new UdpClient(AddressFamily.InterNetwork);
            _udpClient.Client.Bind(_endPoint);
            _thread = new Thread(() => ReceiveLoop(_udpClient, _endPoint));
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            _udpClient?.Close();
            _thread?.Join();
            _thread = null;
            _udpClient = null;
        }

        private void ReceiveLoop(UdpClient udpClient, IPEndPoint endPoint)
        {
            _running = true;

            while (_running)
            {
                try
                {
                    var buffer = udpClient.Receive(ref endPoint);
                    MessageCount.CountUpVRCFT2ThisApp();
                    int pos = 0;
                    _parser.Parse(buffer, ref pos, buffer.Length);
                    while (_parser.messageCount > 0)
                    {
                        var message = _parser.Dequeue();
                        if (VRCFTParametersV2Parser.TryParse(message.address, out var param))
                        {
                            VRCFTParametersStore.SetWeight(param, (float)message.values[0]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine(ex);
                    _running = false;
                }
            }
        }
    }
}
