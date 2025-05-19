using System.Net.Sockets;
using System.Net;
using System.Text;

internal class Server
{
    // 버퍼 사이즈
    private const int BUFFER_SIZE = 1024;
    // IP주소, 포트번호
    private static readonly IPEndPoint ADDRESS = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
    // 클라 리스너
    private TcpListener serverSocket;

    public Server()     // 서버 소켓 생성
    {
        try
        {
            // 현재 Address로 TCPListener를 생성이 가능한지 체크한다.
            // 그리고 시작
            Console.WriteLine($"Starting up at: {ADDRESS}");
            serverSocket = new TcpListener(ADDRESS);
            serverSocket.Start();
        }
        catch (SocketException)
        {
            // 만일 문제가 생겼다면 멈춰줌
            Console.WriteLine("\nServer failed to start.");
            serverSocket?.Stop();
        }
    }

    public TcpClient Accept()   // 클라이언트 서버 접속 대기 및 클라이언트 소켓 반환
    {
        TcpClient client = serverSocket.AcceptTcpClient();
        IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        Console.WriteLine($"Connected to {clientEndPoint}");
        return client;
    }

    public void Serve(TcpClient client)     // 클라가 보내는 데이터 수신 및 서버의 응답 송신
    {
        // 입력 받기
        NetworkStream stream = client.GetStream();
        // 버퍼로 변환하기 위한 배열
        byte[] buffer = new byte[BUFFER_SIZE];

        try
        {
            // 끝나기 전까지 계속 반복 => 다른 Client 못 받아옴
            while (true)
            {
                // Stream to Byte
                int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                if (bytesRead == 0) break;

                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string response;

                // 파씽 작업
                if (int.TryParse(receivedData, out int order))
                {
                    // 가능하면 다음과 같이
                    response = $"Thank you for ordering {order} pizzas!\n";
                }
                else
                {
                    // 불가능하면 다음과 같이
                    response = "Wrong number of pizzas, please try again\n";
                }

                // 마지막으로 Sending을 완료하고 인코딩 후 Data를 Stream에 씀
                Console.WriteLine($"Sending message to {client.Client.RemoteEndPoint}");
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);
            }
        }
        finally
        {
            // 예외 처리 부분
            Console.WriteLine($"Connection with {client.Client.RemoteEndPoint} has been closed");
            client.Close();
        }
    }

    public void Start() // 클라 연결 대기를 위한 loop문
    {
        Console.WriteLine("Server listening for incoming connections");

        try
        {
            while (true)
            {
                // 현재 하나의 client만 통신할 수 있음
                // Why? 당연히 client를 하나를 처리하는 도중에 다른 것 작업을 할 수 없기 때문에
                // Serve를 들어가는 순간 While문에 묶이기 때문에 다른 클라가 들어올 수가 없음
                TcpClient client = Accept();
                Serve(client);
            }
        }
        finally
        {
            // 문제 생기면 즉시 정지
            serverSocket.Stop();
            Console.WriteLine("\nServer stopped.");
        }
    }

    static void Main(string[] args) // Main문 서버 시작
    {
        Server server = new Server();
        server.Start();
    }
}