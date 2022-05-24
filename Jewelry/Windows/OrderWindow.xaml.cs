using Jewelry.Messages;
using Jewelry.Model;
using Jewelry.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                Debug.WriteLine(message.User is null);
                if (_currentUser != null)
                {
                    _currentUser = message.User;
                    Debug.WriteLine(_currentUser.Credentials);
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
            Hide();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button.DataContext as Order;
            if (order != null)
            {
                _orders.Remove(order);
                OrdersListView.ItemsSource = _orders;
                OrdersListView.Items.Refresh();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (sender as TextBox);
            Debug.WriteLine(textBox.Text);
            if (!string.IsNullOrWhiteSpace(textBox.Text) && int.Parse(textBox.Text) == 0)
            {
                var order = textBox.DataContext as Order;
                _orders.Remove(order);
                OrdersListView.ItemsSource = _orders;
                OrdersListView.Items.Refresh();
            }
        }
    }
}
