using Jewelry.Messages;
using Jewelry.Model;
using Jewelry.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jewelry.Windows
{
    public partial class OrderWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly EventBus _eventBus;
        private readonly MessageBus _messageBus;
        private readonly ImageService _imageService;
        private User _currentUser;
        private List<Order> _orders = new List<Order>();


        public OrderWindow(DatabaseService databaseService, EventBus eventBus, MessageBus messageBus, ImageService imageService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _eventBus = eventBus;
            _messageBus = messageBus;
            _imageService = imageService;

            _messageBus.Receive<UserMessage>(this, message =>
            {
                if (_currentUser != null)
                {
                    _currentUser = message.User;
                    UserCredentialsLabel.Content = _currentUser.Credentials;
                }
            });
            _messageBus.Receive<OrdersMessage>(this, message =>
            {
                _orders.Clear();
                _orders.AddRange(message.Orders);
                _orders.Distinct();
                OrdersListView.ItemsSource = _orders;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
            _orders.Clear();
            OrdersListView.ItemsSource = null;
            OrdersListView.Items.Clear();
            Debug.WriteLine(_orders.Count);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            var text = e.Text;
            int amount;
            if (int.TryParse(e.Text, out amount))
            {
                var dataContext = (sender as TextBox).DataContext as Order;
                Debug.WriteLine(dataContext.Product.Name);
                Debug.WriteLine(dataContext.ProductsAmount);
            }
        }
    }
}
