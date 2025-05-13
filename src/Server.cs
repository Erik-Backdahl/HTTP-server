using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

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

bool isValidPath = false;

string response = "HTTP/1.1 200 OK\r\n\r\n";

if(path.Contains(".html") || path == "/")
{
    isValidPath = true;
}
if(isValidPath){
    response = "HTTP/1.1 200 OK\r\n\r\n";
}
else{
    response = "HTTP/1.1 404 Not Found\r\n\r\n";
}

socket.Send(System.Text.Encoding.UTF8.GetBytes(response));

//LOLOLOLOLOLOLOL1
//LOLOLOLOLOLOLOL2
//LOLOLOLOLOLOLOL3
//LOLOLOLOLOLOLOL4
//LOLOLOLOLOLOLOL5
//LOLOLOLOLOLOLOL6
//LOLOLOLOLOLOLOL7