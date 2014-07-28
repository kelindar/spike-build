using Spike.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Spike.Samples.WinRT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Message> _Messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages { get { return _Messages; } }

        public TcpChannel Channel = new TcpChannel();

        public MainPage()
        {
            InitializeComponent();            
            DataContext = this;

            Channel.MyChatMessagesInform += (channel, packet) => {
                Messages.Add(new Message() { Text = packet.Message });
            };

            Channel.Connected += (channel) => {
                channel.JoinMyChat();
            };

            Channel.Connect("54.88.210.109", 80);

            //int.TryParse("", var out x); 
        }



        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            //Messages.Add(new Message() { Text = TextBoxMessage.Text });
            Channel.SendMyChatMessage(TextBoxMessage.Text);
        }
    }
}
