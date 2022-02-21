using Microsoft.Extensions.DependencyInjection;

using Shiny;
using ShinyBluetoothTest.Interfaces;
using ShinyBluetoothTest.Services;

namespace ShinyBluetoothTest
{
    public class Startup : ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services, IPlatform platform)
        {
            services.UseBleClient();
            services.UseBleHosting();

            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ITransportLayer, BluetoothService>();
            services.AddSingleton<BluetoothClientService>();
            services.AddSingleton<BluetoothHostingService>();
        }
    }
}
