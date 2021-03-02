using Redirxn.TeamKitty.ViewModels;
using Splat;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SessionPage : ContentPage
    {
        public SessionPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;
        }

        internal SessionViewModel ViewModel { get; set; } = Locator.Current.GetService<SessionViewModel>();

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ViewModel.Init();
            LoadToolBarItems();
        }

        private void LoadToolBarItems()
        {
            var sIs = ViewModel.GetStockItems();

            ToolbarItems.Clear();

            foreach (var s in sIs)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = s,
                    Order = ToolbarItemOrder.Secondary
                });
                foreach (var o in ViewModel.GetOptionsFor(s))
                {
                    ToolbarItems.Add(new ToolbarItem
                    {
                        Text = o,
                        Order = ToolbarItemOrder.Secondary
                    });
                }
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Add a " + s + " option",
                    Order = ToolbarItemOrder.Secondary,
                    Command = ViewModel.AddOptionCommand,
                    CommandParameter = s,
                });
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Remove a " + s + " option",
                    Order = ToolbarItemOrder.Secondary,
                    Command = ViewModel.RemoveOptionCommand,
                    CommandParameter = s,
                });
            }
        }
    }
}