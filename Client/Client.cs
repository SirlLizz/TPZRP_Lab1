using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    public class Client
    {
        public void SendMessageFromSocket(int port)
        {
            // Устанавливаем удаленную точку для сокета
            IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            sender.Connect(ipEndPoint);

            ConsoleWork(sender);

            // Освобождаем сокет
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }


        private void ConsoleWork(Socket sender)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];

            Console.Write("Введите путь до картинки: ");
            string path = Console.ReadLine();

            while (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден, повторите ввод");
                path = Console.ReadLine();
            }

            Image image = Image.FromFile(path);

            image = AddNoise(image);

            MemoryStream ms = new MemoryStream();
            // Save to memory using the Jpeg format 
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            // read to end 
            byte[] bmpBytes = ms.GetBuffer();
            image.Dispose();
            ms.Close();

            Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());

            Console.WriteLine("\nОтправлено пакетов на сервер: ", SendVarData(sender, bmpBytes));

            int bytesRec = sender.Receive(bytes);

            Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));

            Console.WriteLine("\nХотите передать еще картинку?(Y/N)\n");

            string message = Console.ReadLine();
            sender.Send(Encoding.UTF8.GetBytes(message));

            if (message == "Y")
            {
                ConsoleWork(sender);
            }
        }

        private int SendVarData(Socket s, byte[] data)
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            byte[] datasize = BitConverter.GetBytes(size);
            s.Send(datasize);
            while (total < size)
            {
                int sent = s.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }
            return total;
        }

        private Image AddNoise(Image img)
        {
            var rnd = new Random();

            Bitmap btp = (Bitmap)img;

            for (int x = 0; x < btp.Width; x++)
                for (int y = 0; y < btp.Height; y++)
                {
                    int q = rnd.Next(100);
                    if (q <= 20) btp.SetPixel(x, y, Color.Gray);
                }
            return btp;
        }

    }
}
