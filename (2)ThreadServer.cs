using System.Net.Sockets;
using System.Net;
using System.Text;

namespace _2_Pizza_Thread
{
    internal class Handler
    {
        private const int BUFFER_SIZE = 1024;
        private readonly TcpClient client;

        public Handler(TcpClient client)    // 각 스레드마다 클라이언트 소켓을 부여받는다.
        {
            this.client = client;
        }

        public void Run()   // 스레드마다 서버의 역할 수행
        {
            IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Connected to {clientEndPoint}");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                // 스레드 마다 반복문을 돌면서 Steam을 읽고 씀
                while (true)
                {
                    // 받기
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                    if (bytesRead == 0) break;

                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string response;

                    // 파싱 가능한지
                    if (int.TryParse(received, out int order))
                    {
                        response = $"Thank you for ordering {order} pizzas!\n";
                    }
                    else
                    {
                        response = "Wrong number of pizzas, please try again\n";
                    }

                    Console.WriteLine($"Sending message to {clientEndPoint}");
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    // 쓰기
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            finally
            {
                Console.WriteLine($"Connection with {clientEndPoint} has been closed");
                client.Close();
            }
        }
    }

    internal class ThreadServer
    {
        private const int PORT = 12345;
        private readonly TcpListener server;

        public ThreadServer()   // 서버 소켓 세팅
        {
            try
            {
                // 새 주소 생성
                IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);
                Console.WriteLine($"Starting up at: {localAddress}");
                // 서버 생성 및 시작
                server = new TcpListener(localAddress);
                server.Start();
            }
            // 문제 발생할 시 서버 정지
            catch (SocketException)
            {
                server?.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }

        public void Start()
        {
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                while (true)    
                {   
                    // 클라 대기
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine($"Client connection request from {client.Client.RemoteEndPoint}");

                    // 새 핸들 생성
                    Handler handler = new Handler(client);
                    // 새 스레드 생성
                    Thread thread = new Thread(new ThreadStart(handler.Run));
                    thread.Start();
                } // 클라이언트 요청이 들어올 때마다 새로운 스레드를 생성한다.
            }
            finally
            {
                server.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }
        static void Main()
        {
            ThreadServer server = new ThreadServer();
            server.Start();
        }
    }
}
