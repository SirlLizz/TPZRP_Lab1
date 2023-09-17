using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Text;
using GroupDocs.Comparison;
using GroupDocs.Comparison.Options;

namespace Server
{
    public class ClientThread
    {
        public ClientThread(Socket listenerAccept)
        {
            try
            {
                ImageWork(listenerAccept);
            }
            finally
            {
                listenerAccept.Shutdown(SocketShutdown.Both);
                listenerAccept.Close();
            }
        }

        private static void ImageWork(Socket listenerAccept)
        {
            var data = ReceiveVarData(listenerAccept);

            MemoryStream ms = new MemoryStream(data);

            Image img = Image.FromStream(ms);

            var filePath = AppDomain.CurrentDomain.BaseDirectory + "file.jpeg";
            var filterFilePath = AppDomain.CurrentDomain.BaseDirectory + "filter_file.jpeg";
            var compareFilePath = AppDomain.CurrentDomain.BaseDirectory + "compare_file.jpeg";

            img.Save(filePath, ImageFormat.Jpeg);

            var filter = new float[,]
            {
                { 0, 1, 0 },
                { 1, 3, 1 },
                { 0, 1, 0 }
            };

            Convolve.Apply(img, filter).Save(filterFilePath, ImageFormat.Jpeg);

            using (Comparer comparer = new Comparer(filePath))
            {
                CompareOptions options = new CompareOptions();
                options.GenerateSummaryPage = false;
                comparer.Add(filterFilePath);
                comparer.Compare(compareFilePath);
            }

            listenerAccept.Send(Encoding.UTF8.GetBytes("Спасибо за картинку в " + data.Length.ToString() + " бит"));

            byte[] bytes = new byte[1024];
            int bytesRec = listenerAccept.Receive(bytes);

            if (Encoding.UTF8.GetString(bytes, 0, bytesRec) == "Y")
            {
                ImageWork(listenerAccept);
            }
        }

        private static byte[] ReceiveVarData(Socket s)
        {
            int total = 0;
            byte[] datasize = new byte[4];
            s.Receive(datasize, 0, 4, 0);
            int size = BitConverter.ToInt32(datasize, 0);
            int dataleft = size;
            byte[] data = new byte[size];

            while (total < size)
            {
                int recv = s.Receive(data, total, dataleft, 0);
                if (recv == 0)
                {
                    break;
                }
                total += recv;
                dataleft -= recv;
            }
            return data;
        }
    }
}
