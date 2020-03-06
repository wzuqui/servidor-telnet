using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServidorTelnet.Telnet;
using Xunit;

namespace ServidorTelnetTests.Telnet
{
    public class TelnetServerTests
    {
        private readonly TelnetServer _telnetServer;

        public TelnetServerTests()
        {
            _telnetServer = new TelnetServer();
        }
        
        [Fact]
        public async Task DeveAbrirPorta()
        {
            using (var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                server.Bind(new IPEndPoint(IPAddress.Loopback, 9090));
                server.Listen(1);

                await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9090));
                using (Socket accepted = await server.AcceptAsync())
                {
                    var data = new byte[1];
                    for (int i = 0; i < 10; i++)
                    {
                        data[0] = (byte)i;

                        await accepted.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                        data[0] = 0;

                        Assert.Equal(1, await client.ReceiveAsync(new ArraySegment<byte>(data), SocketFlags.None));
                        Assert.Equal(i, data[0]);
                    }
                }
            }


        }
    }
}