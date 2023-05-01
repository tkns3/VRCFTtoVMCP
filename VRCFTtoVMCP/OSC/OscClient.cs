using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace VRCFTtoVMCP.Osc
{
    internal class OscClient : IDisposable
    {
        UdpClient udpClient_;
        IPEndPoint endPoint_;

        public OscClient(string address, int destport)
        {
            var ip = IPAddress.Parse(address);
            endPoint_ = new IPEndPoint(ip, destport);
            udpClient_ = new UdpClient(endPoint_.AddressFamily);
        }

        public void Dispose()
        {
            udpClient_.Close();
        }

        public void Send(Message message)
        {
            using (var stream = new MemoryStream())
            {
                message.Write(stream);
                Send(Util.GetBuffer(stream), (int)stream.Position);
            }
        }

        public void Send(Bundle bundle)
        {
            using (var stream = new MemoryStream())
            {
                bundle.Write(stream);
                Send(Util.GetBuffer(stream), (int)stream.Position);
            }
        }

        private void Send(byte[] data, int size)
        {
            try
            {
                udpClient_.Send(data, size, endPoint_);
            }
            catch (System.Exception)
            {
                //
            }
        }
    }
}
