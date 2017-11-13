using Risk.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Risk.Networking.Server
{
  public class RiskServer
  {

    private ManualResetEvent _allDone = new ManualResetEvent(false);

    private string _hostNameOrAddress;

    private int _port;

    private int _maxLengthConQueue;

    public ICollection<string> Players { get; set; }

    public RiskServer(): this("localhost", 1100, 100) { }

    public RiskServer(int port): this("localhost", port, 100) { }

    public RiskServer(string hostNameOrAddress, int port): this(hostNameOrAddress, port, 100) { }

    public RiskServer(string hostNameOrAddress, int port, int maxLengthConQueue)
    {
      _hostNameOrAddress = hostNameOrAddress;
      _port = port;
      _maxLengthConQueue = maxLengthConQueue;
      Players = new HashSet<string>();
      
      Debug.WriteLine("**Server inicialization: host={0} port={1} maxLengthConQueue={2} OK", _hostNameOrAddress, _port, _maxLengthConQueue);
    }

    public void Start()
    {
      Debug.WriteLine("**Server: Starting listenig");

      IPAddress ipHost = Dns.GetHostEntry(_hostNameOrAddress).AddressList[0];
      IPEndPoint localEnd = new IPEndPoint(ipHost, _port);

      Socket listener = new Socket(ipHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

      try
      {
        listener.Bind(localEnd);
        listener.Listen(_maxLengthConQueue);

        while(true)
        {
          _allDone.Reset();

          listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

          _allDone.WaitOne();
        }
      }
      catch(Exception e)
      {
        Debug.WriteLine("**Server ERROR: " + e.StackTrace);
      }
    }

    public void AcceptCallback(IAsyncResult result)
    {
      Debug.WriteLine("**Server: NEW CLIENT");

      _allDone.Set();

      Socket listener = (Socket)result.AsyncState;
      Socket handler = listener.EndAccept(result);

      Client client = new Client(handler, this, new ResponseFactory());
      client.StartAsync();
    }

    public void ReadCallback(IAsyncResult result)
    {
      string content = string.Empty;

      Player state = (Player)result.AsyncState;
      Socket handler = state.connection;

      int bytesRead = handler.EndReceive(result);

      if (bytesRead > 0)
      {
        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

        content = state.sb.ToString();

        Send(handler, content);

        Debug.WriteLine("**Server received: " + content);

        if (content.IndexOf("<EOF>") <= -1)
        {
          handler.BeginReceive(state.buffer, 0, Player.bufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        else
        {
          Send(handler, content);

          handler.Shutdown(SocketShutdown.Both);
          handler.Close();
        }

      }
    }

    public void Send(Socket handler, string data)
    {
      byte[] byteData = Encoding.ASCII.GetBytes(data);

      handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), handler);
    }

    public void SendCallBack(IAsyncResult result)
    {
      try
      {
        Socket handler = (Socket)result.AsyncState;

        int byteSent = handler.EndSend(result);

        Debug.WriteLine(byteSent);
      }
      catch(Exception e)
      {
        Debug.WriteLine("**Server ERROR: " + e.StackTrace);
      }
    }
  }
}
