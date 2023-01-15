using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Drawing;
using System.Threading;

namespace AdminImageReciever
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //private readonly object _personCollectionLock;
        private readonly object _locker = new object();
        private ObservableCollection<Image> _imagesFromUsers;

        public ObservableCollection<Image> ImagesFromUsers
        {
            get { return _imagesFromUsers; }
            set
            {
                _imagesFromUsers = value;
                BindingOperations.EnableCollectionSynchronization(_imagesFromUsers, _locker);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ImagesFromUsers = new ObservableCollection<Image>();
            Task.Run(() =>
            {
                ServerStart();
            });
            //Thread thread = new Thread(() =>
            //{
            //    ServerStart();
            //});
            //thread.Start();

        }
        public Image byteArrayToImage(byte[] bytesArr)
        {
            using (MemoryStream memstr = new MemoryStream(bytesArr))
            {
                Image img = Image.FromStream(memstr);

                return img;
            }
        }
        public void ServerStart()
        {
            var ipAddress = IPAddress.Parse("192.168.56.1");
            var port = 80;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var ep = new IPEndPoint(ipAddress, port);
                socket.Bind(ep);
                socket.Listen(1000);
                MessageBox.Show("Server Is Running");
                while (true)
                {
                    var client = socket.Accept();

                    MessageBox.Show(client.RemoteEndPoint.ToString());

                    Task.Run(() =>
                    {
                        MessageBox.Show("Image Is Coming");

                        var bytes = new byte[1000000];
                        do
                        {
                            client.Receive(bytes);
                            //MessageBox.Show(length.ToString());
                            //Image x = byteArrayToImage(bytes);

                            using (var ms = new MemoryStream(bytes))
                            {
                                var image = Image.FromStream(ms);
                                MessageBox.Show("Image has been received");
                                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                                var filename = path + $"\\{client.RemoteEndPoint}+{Guid.NewGuid()}";
                                MessageBox.Show(filename);
                                try
                                {
                                    image.Save(path);
                                }
                                catch (Exception)
                                {
                                }
                                
                                ImagesFromUsers.Add(image);
                            }

                        } while (true);
                    });
                }
            }
        }
    }
}
