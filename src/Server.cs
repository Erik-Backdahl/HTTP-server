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
                _ = Task.Run(() => handleRequest(tcpclient));
            }
        }

        static async Task handleRequest(TcpClient client)
        {
            await using NetworkStream networkStream = client.GetStream();
            using var reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);

            string? requestLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(requestLine))
            {
                return;
            }

            // Example: "GET /path HTTP/1.1"
            // requestParts[0] request Type eg: "GET, POST, NOT FOUND" 
            // requestParts[1] request Path eg: "/echo, /user-agent"
            // requestParts[2] request http version: "HTTP/1.1"

            string[] requestParts = requestLine.Split(' ');

            //Bad request
            if (requestParts.Length < 2)
            {
                var failed = FormatEasyResponse("400");
                var failedBytes = Encoding.UTF8.GetBytes(failed);
                await networkStream.WriteAsync(failedBytes, 0, failedBytes.Length);

                return;
            }

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? headerLine;

            while (!string.IsNullOrEmpty(headerLine = await reader.ReadLineAsync()))
            {
                int separatorIndex = headerLine.IndexOf(':');
                if (separatorIndex > 0)
                {
                    string key = headerLine.Substring(0, separatorIndex).Trim();
                    string value = headerLine.Substring(separatorIndex + 1).Trim();
                    headers[key] = value;
                }
            }

            string requestType = requestParts[0];
            string path = requestParts[1];

            // Read body if POST
            string? body = null;
            if (requestType == "POST" && headers.ContainsKey("Content-Length"))
            {
                int contentLength = int.Parse(headers["Content-Length"]);
                char[] buffer = new char[contentLength];
                int read = await reader.ReadAsync(buffer, 0, contentLength);
                body = new string(buffer, 0, read);
            }

            var responseTemplate = LookForEndPoint(path, headers, requestType, body);

            var responseBytes = Encoding.UTF8.GetBytes(responseTemplate);
            await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);

        }

        private static string LookForEndPoint(string path, Dictionary<string, string> headers, string requestType, string? body)
        {
            switch (path)
            {
                case var p when p.StartsWith("/echo"):
                    return EndPointEcho(path);
                case var p when p.StartsWith("/user-agent"):
                    return EndPointUserAgent(path, headers);
                case var p when p.StartsWith("/files"):
                    {
                        if (requestType == "POST")
                            return EndPointPostFiles(path, headers, body);

                        return EndPointFiles(path, headers);
                    }
                case "/": //Ignore
                    return FormatEasyResponse("200");
                default:
                    return FormatEasyResponse("404");
            }
        }

        private static string EndPointPostFiles(string path, Dictionary<string, string> headers, string? body)
        {
            string relativePath = path.StartsWith("/files/") ? path["/files/".Length..] : "";
            relativePath = relativePath.TrimStart('/');

            string baseDirectory = @"/tmp/data/codecrafters.io/http-server-tester";
            string filePath = Path.Combine(baseDirectory, relativePath);

            Directory.CreateDirectory(baseDirectory);

            File.WriteAllText(filePath, body ?? "");

            return FormatEasyResponse("201");
        }

        private static string EndPointFiles(string path, Dictionary<string, string> headers)
        {
            string relativePath = path.StartsWith("/files/") ? path["/files/".Length..] : "";
            relativePath = relativePath.TrimStart('/');

            string baseDirectory = @"/tmp/data/codecrafters.io/http-server-tester";
            string filePath = Path.Combine(baseDirectory, relativePath);

            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                return formatResponse("200", "application/octet-stream", content);
            }
            else
            {
                Console.WriteLine($"file {relativePath} does not exist");
                return formatResponse("404", "text/plain", $"\"{relativePath}\" does not exist");
            }
        }
        private static string EndPointUserAgent(string path, Dictionary<string, string> headers)
        {
            var message = headers.ContainsKey("user-agent") ? headers["User-Agent"] : "Path Not Found";
            return formatResponse("200", "text/plain", message);
        }
        private static string EndPointEcho(string path)
        {
            string message = path.Substring(path.LastIndexOf("/") + 1);
            return formatResponse("200", "text/plain", message);
        }
        static string formatCodeOperation(string requestOperation)
        {
            string responseCode;

            if (requestOperation == "200")
                responseCode = "HTTP/1.1 200 OK\r\n";
            else if (requestOperation == "400")
                responseCode = "HTTP/1.1 400 Bad Request\r\n";
            else
                responseCode = "HTTP/1.1 404 Not Found\r\n";

            return responseCode;
        }
        static string formatContentType(string contentType)
        {
            string responseContentType;
            if (contentType == "text/plain")
                responseContentType = "Content-Type: text/plain\r\n";
            else
                responseContentType = "Content-Type: application/octet-stream\r\n";

            return responseContentType;
        }
        static string formatResponse(string requestOperation, string requestContentType, string message)
        {
            string responseCode = formatCodeOperation(requestOperation);
            string responseContentType = formatContentType(requestContentType);

            string responseTemplate =
            $"{responseCode}" +
            $"{responseContentType}" +
            $"Content-Length: {message.Length}\r\n\r\n" +
            $"{message}";

            return responseTemplate;

        }
        static string FormatEasyResponse(string requestOperation)
        {
            string responseCode;

            if (requestOperation == "200")
                responseCode = "HTTP/1.1 200 OK\r\n\r\n";
            else if (requestOperation == "201")
                responseCode = "HTTP/1.1 201 Created\r\n\r\n";
            else if (requestOperation == "400")
                responseCode = "HTTP/1.1 400 Bad Request\r\n\r\n";
            else
                responseCode = "HTTP/1.1 404 Not Found\r\n\r\n";

            return responseCode;
        }
    }
}