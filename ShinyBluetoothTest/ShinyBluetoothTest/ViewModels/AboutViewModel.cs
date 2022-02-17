using System;
using System.Windows.Input;

using Shiny;

using ShinyBluetoothTest.Interfaces;

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

                await btService.ConnectAsync();
            });
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand ConnectCommand { get; }
        public ICommand OpenWebCommand { get; }
    }
}
