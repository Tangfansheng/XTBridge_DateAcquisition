using System;
using ConsoleXTBridge.service;
using ConsoleXTBridge.utils;
using MySql.Data.MySqlClient;
namespace ConsoleXTBridge
{
    class Program
    {
        private Anchor_Syn anchor_Syn = new Anchor_Syn();
        int TimesCalled = 0;

        void Reader(object state) {
            //可以定义若干个读取模块
            anchor_Syn.UpdateData();
            Console.WriteLine("{0} {1} keep running.", (string)state, ++TimesCalled);
        }
        static void Main(string[] args)
        {
            Program p = new Program();
            System.Threading.Timer readerTimer = new System.Threading.Timer(p.Reader, "Processing timer event", 2000, 2000);
            Console.WriteLine("Timer started.");
            Console.ReadLine();
        }
    }
}
