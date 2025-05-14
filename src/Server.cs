using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Schema;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

var socket = server.AcceptSocket(); // wait for client

byte[] buffer = new byte[4096];

int received = socket.Receive(buffer, SocketFlags.None);

string requestString = Encoding.UTF8.GetString(buffer, 0, received);

string[] lines = requestString.Split("\r\n");

string[] requestLine = lines[0].Split(" ");

string path = requestLine[1];

string status;

if(validateStatus(path))
{
    status = "HTTP/1.1 200 OK";
}
else
{
    status = "HTTP/1.1 404 Not Found";
    socket.Send(System.Text.Encoding.UTF8.GetBytes(status));
}

if(requestLine[1].Contains("/echo/"))
{
    string content = requestLine[1].Substring(requestLine[1].IndexOf("/echo/"));

    int contentLength = content.Length;

    string result = $"{status}\r\nContent-Type: text/plain\r\nContent-Length: {contentLength}\r\n\r\n{content}";
}






socket.Send(System.Text.Encoding.UTF8.GetBytes(status));


bool validateStatus(string URL)
{
    if (URL.Contains(".html") || URL == "/")
    {
        return true;
    }
    else
    {
        return false;
    }
}