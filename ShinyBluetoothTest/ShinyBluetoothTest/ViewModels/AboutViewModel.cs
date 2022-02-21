using System;
using System.IO;
using System.Windows.Input;

using ProtoBuf;

using Shiny;
using ShinyBluetoothTest.EventArgs;
using ShinyBluetoothTest.Interfaces;
using ShinyBluetoothTest.Models;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace ShinyBluetoothTest.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            ConnectCommand = new Command(async () => {
                var btService = ShinyHost.Resolve<ITransportLayer>();

                btService.OnReceivedData += ReceiveDataAsync;

                await btService.ConnectAsync();
            });
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand ConnectCommand { get; }
        public ICommand OpenWebCommand { get; }

        private async void ReceiveDataAsync(object sender, DataReceivedEventArgs args)
        {
            var dialogService = ShinyHost.Resolve<IDialogService>();
            TestRequest meshMessage;

            using (var stream = new MemoryStream(args.Data))
            {
                meshMessage = Serializer.Deserialize<TestRequest>(stream);
            }

            System.Diagnostics.Debug.WriteLine(meshMessage.Data);

            await dialogService.DisplayAlert("Message received", meshMessage.Data, "OK");
        }
    }
}
