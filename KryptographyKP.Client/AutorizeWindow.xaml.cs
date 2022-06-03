using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KryptographyKP.Client.ViewModels;

namespace KryptographyKP.Client
{
    /// <summary>
    /// Логика взаимодействия для AutorizeWindow.xaml
    /// </summary>
    public partial class AutorizeWindow : Window
    {
        public AutorizeWindow()
        {
            InitializeComponent();
        }

        private async void button_ClickAsync(object sender, RoutedEventArgs e)
        {
            HttpClientHandler unsafeHandler = new HttpClientHandler();
            GrpcChannel _channel = GrpcChannel.ForAddress("http://localhost:5055", new GrpcChannelOptions
            {
                HttpHandler = unsafeHandler
            });
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            CryptographyServer.CryptographyServerClient client = new CryptographyServer.CryptographyServerClient(_channel);
            string valid_name = "";

            if (textbox.Text == "")
                MessageBox.Show("Введите имя пользователя.");
            else
            {
                try
                {
                    valid_name = (await client.SayHelloAsync(new HelloRequest { Name = textbox.Text })).Message;
                }
                catch
                {
                    MessageBox.Show("Сервер недоступен");
                    return;
                }
                if (valid_name == "Wrong name!")
                    MessageBox.Show("Некорректное имя пользователя. Введите другое.");
                else if (valid_name == "More than two")
                {
                    MessageBox.Show("В системе уже есть два пользователя. Дождитесь выхода кого-то из них.");
                }
                else
                {
                    
                    MainWindow Chat = new MainWindow(textbox.Text);


                    Chat.Show();
                    this.Visibility = Visibility.Collapsed;
                }

            }
        }


 
    }
}
