using Jewelry.Events;
using Jewelry.Messages;
using Jewelry.Pages.Products;
using Jewelry.Services;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Jewelry.Pages.Authentication
{
    public partial class AuthenticationPage : Page
    {
        private readonly PageService _navigation;
        private readonly CaptchaService _captchaService;
        private readonly DatabaseService _databaseService;
        private readonly EventBus _eventBus;
        private readonly MessageBus _messageBus;
        private int _failedAuthenticationAttemps = 0;

        public AuthenticationPage(CaptchaService captchaService, DatabaseService databaseService,
            PageService navigation, EventBus eventBus, MessageBus messageBus)
        {
            InitializeComponent();
            _navigation = navigation;
            _captchaService = captchaService;
            _databaseService = databaseService;
            _eventBus = eventBus;
            _messageBus = messageBus;

            LoginTextBox.Text = "loginDEirs2018";
            PasswordTextBox.Text = "hkJGHJK";
        }
        private void AuthenticationAsGuestButton_Click(object sender, RoutedEventArgs e)
        {
            _eventBus.Publish(new LoginAsGuestEvent());
            _navigation.Navigate(Dependency.Resolve<ProductsPage>());
        }
        private void AuthenticationAsUserButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;
            var password = PasswordTextBox.Text;
            var captchaCode = CaptchaCodeTextBox.Text;
            if (_failedAuthenticationAttemps == 0)
            {
                Authenticate(login, password);
            }
            else
            {
                AuthenticateWithCaptcha(login, password, captchaCode);
            }
        }
        private async void Authenticate(string login, string password)
        {
            var user = _databaseService.GetCloudContext().Users
                .FirstOrDefault(x => x.Login == login && x.Password == password);
            if (user is null)
            {
                _failedAuthenticationAttemps += 1;
                MessageBox.Show("Введите правильные данные для входа",
                                "Неверные данные",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                CaptchaCodeTextBox.Visibility = Visibility.Visible;
                CaptchaImage.Visibility = Visibility.Visible;
                CaptchaImage.Source = _captchaService.GenerateCaptchaImage();
            }
            else
            {
                _messageBus.SendTo<ProductsPage>(new UserMessage(user));
                _eventBus.Publish(new LoginAsUserEvent());
                _navigation.Navigate(Dependency.Resolve<ProductsPage>());
            }
        }
        private async void AuthenticateWithCaptcha(string login, string password, string captchaCode)
        {
            if (captchaCode == _captchaService.CaptchaCode)
            {
                Authenticate(login, password);
            }
            else
            {
                MessageBox.Show("Введите правильно captcha для входа",
                                "Неверные данные",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                await DisableButtonsFor10Seconds();
            }
        }
        private async Task DisableButtonsFor10Seconds()
        {
            AuthenticationAsUserButton.IsEnabled = false;
            AuthenticationAsGuestButton.IsEnabled = false;
            await Task.Delay(10000);
            AuthenticationAsUserButton.IsEnabled = true;
            AuthenticationAsGuestButton.IsEnabled = true;
        }
    }
}
