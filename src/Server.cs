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
        }

        static async Task handleRequest(TcpClient client)
        {
            await using NetworkStream networkStream = client.GetStream();
            using var reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);

            string? requestLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(requestLine))
            {
                // Malformed request, return early or handle as needed
                return;
            }

            // Example: "GET /path HTTP/1.1"
            string[] requestParts = requestLine.Split(' ');

            //Bad request
            if (requestParts.Length < 2)
            {
                var failed = formatResponse("Request is malformed", "400");
                var failedBytes = Encoding.UTF8.GetBytes(failed);

                await networkStream.WriteAsync(failedBytes, 0, failedBytes.Length);
                // Malformed request, return early or handle as needed
                return;
            }
            string requestType = requestParts[0];
            string path = requestParts[1];

            var message = LookForEndPoint(path);
            string responseTemplate;
            if (message == "Path Not Found")
            {
                responseTemplate = formatResponse(message, "404");
            }
            else
            {
                responseTemplate = formatResponse(message);
            }
            if (message == "")
            {
                responseTemplate = "HTTP/1.1 200 OK\r\n\r\n";
            }




            var responseBytes = Encoding.UTF8.GetBytes(responseTemplate);
            await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            client.Close();
        }

        private static string LookForEndPoint(string path)
        {
            switch (path)
            {
                case var p when p.StartsWith("/echo"):
                    return EndPointEcho(path);
                case "/":
                    return "";
                default:
                    return "Path Not Found";
            }
        }

        private static string EndPointEcho(string path)
        {
            string message = path.Substring(path.LastIndexOf("/") + 1);
            return message;
        }

        static string formatResponse(string response = "", string requestType = "200")
        {
            string responseCode;

            if (requestType == "200")
                responseCode = "HTTP/1.1 200 OK";
            else if (requestType == "400")
                responseCode = "HTTP/1.1 400 Bad Request";
            else
                responseCode = "HTTP/1.1 404 Not Found";

            string responseTemplate = $"{responseCode}\r\n" +
            $"Content-Type: text/plain\r\n" +
            $"Content-Length: {response.Length}\r\n" +
            $"\r\n" +
            $"{response}";

            return responseTemplate;

        }
    }
}