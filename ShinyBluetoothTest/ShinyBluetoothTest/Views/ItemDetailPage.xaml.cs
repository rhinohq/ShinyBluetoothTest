using System.ComponentModel;
using Xamarin.Forms;
using ShinyBluetoothTest.ViewModels;

namespace ShinyBluetoothTest.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}
