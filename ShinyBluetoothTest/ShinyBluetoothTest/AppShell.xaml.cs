using System;
using System.Collections.Generic;
using ShinyBluetoothTest.ViewModels;
using ShinyBluetoothTest.Views;
using Xamarin.Forms;

namespace ShinyBluetoothTest
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

        private async void OnMenuItemClicked(object sender, System.EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
