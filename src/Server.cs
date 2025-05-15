using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Schema;
using System;
using System.Threading.Tasks;
using System.Reflection;

namespace Application
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var address = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(address, 4221);
            server.Start();

            CancellationTokenSource cancellationToken = new CancellationTokenSource();


            while (!cancellationToken.IsCancellationRequested)
            {
                var tcpclient = await server.AcceptTcpClientAsync();
                await handleRequest(tcpclient);
                //await sendResponse(tcpclient);
            }


            Console.WriteLine("lol");
        }

        static async Task handleRequest(TcpClient client)
        {
            await using NetworkStream ns = client.GetStream();

            var message = $"time {DateTime.Now}";


            var responseTemplate = formatResponse(message);

            var responseBytes = Encoding.UTF8.GetBytes(responseTemplate);
            await ns.WriteAsync(responseBytes, 0, responseBytes.Length);


            string secondMessage = "number 2";

            responseTemplate = formatResponse(secondMessage);
            responseBytes = Encoding.UTF8.GetBytes(responseTemplate);
            await ns.WriteAsync(responseBytes, 0, responseBytes.Length);

            await ns.FlushAsync();
        }
        static string formatResponse(string response)
        {
            var responseTemplate = $"HTTP/1.1 200 OK\r\n" +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {response.Length}\r\n" +
            $"\r\n" +
            $"{response}";

            return responseTemplate;

        }
        /*
        static async Task sendResponse(TcpClient client)
        {
            
        }*/
    }
}