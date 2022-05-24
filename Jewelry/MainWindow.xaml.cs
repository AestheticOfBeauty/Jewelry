using Jewelry.Pages.Authentication;
using Jewelry.Services;
using System.Windows;

namespace Jewelry
{
    public partial class MainWindow : Window
    {
        private readonly PageService _navigation;

        public MainWindow(PageService service)
        {
            InitializeComponent();
            DataContext = this;
            _navigation = service;

            _navigation.OnPageChanged += (page) => PageSource.Content = page;
            _navigation.Navigate(Dependency.Resolve<AuthenticationPage>());
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
