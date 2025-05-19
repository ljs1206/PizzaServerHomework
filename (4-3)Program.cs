using _4_Pizza_Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_Pizza_Event
{
    internal class Program
    {
        static void Main()
        {
            // 생성 후 계속 반복
            EventLoop eventLoop = new EventLoop();
            EventServer server = new EventServer(eventLoop);
            server.Start();
            eventLoop.RunForever();
        }
    }
}
