namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Client client = new Client();
                client.SendMessageFromSocket(80);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}