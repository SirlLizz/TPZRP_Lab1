namespace Server
{
    class Startup
    {
        static void Main(string[] args)
        {
            Server server = new("127.0.0.1", 80);
            server.Start();
        }
    }
}