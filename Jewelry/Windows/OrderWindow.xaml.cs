using Jewelry.Messages;
using Jewelry.Model;
using Jewelry.Services;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jewelry.Windows
{
    public partial class OrderWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly MessageBus _messageBus;
        private User _currentUser;
        private List<Order> _orders = new List<Order>();


        public OrderWindow(DatabaseService databaseService, MessageBus messageBus)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _messageBus = messageBus;

            _messageBus.Receive<UserMessage>(this, message =>
            {                 
                if (message.User != null)
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
                RefreshTotalPrice();
            });
            PickupPointsComboBox.ItemsSource = _databaseService.GetCloudContext().PickupPoints.ToList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
            _orders.Clear();
            OrdersListView.ItemsSource = null;
            OrdersListView.Items.Clear();
            UserCredentialsLabel.Content = "Гость";
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
                RefreshTotalPrice();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (sender as TextBox);
            var order = textBox.DataContext as Order;
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                order.ProductsAmount = int.Parse(textBox.Text);
                if (int.Parse(textBox.Text) == 0)
                {
                    _orders.Remove(order);
                    OrdersListView.ItemsSource = _orders;
                    OrdersListView.Items.Refresh();
                    
                }
                RefreshTotalPrice();
            }
            
        }
        private void RefreshTotalPrice()
        {
            var overallSum = _orders.Select(o => o.ProductsAmount * o.Product.ActualCost).Sum();
            OverallPriceLabel.Content = overallSum.ToString();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (PickupPointsComboBox.SelectedItem is null)
            {
                MessageBox.Show("Выберите пункт выдачи",
                                "Ошибка ввода",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            _orders.ForEach(order =>
            {
                order.OrderDate = DateTime.Now.Date;
                order.PickupPoint = PickupPointsComboBox.SelectedItem as PickupPoint;
            });
            if (_orders.All(order => order.Product.QuantityInStock > 3))
            {
                _orders.ForEach(order => order.DeliveryDate = DateTime.Now.AddDays(3).Date);
            }
            else
            {
                _orders.ForEach(order => order.DeliveryDate = DateTime.Now.AddDays(6).Date);
            }

            using (var pdfDocument = new PdfDocument())
            {
                pdfDocument.Info.Title = "Чек";
                var page = pdfDocument.AddPage();

                var graphics = XGraphics.FromPdfPage(page);
                var height = 0;

                var font = new XFont("Comic Sans MS", 10, XFontStyle.Regular);
                
                graphics.DrawString($"Дата заказа: {_orders.First().OrderDate.Value.Date.ToString("dd/MM/yyyy")}", font, XBrushes.Black,
                    new XRect(20, height += 10, page.Width, page.Height), XStringFormats.TopLeft);

                graphics.DrawString($"Состав заказа:",
                        font, XBrushes.Black, new XRect(20, height += 15, page.Width, page.Height), XStringFormats.TopLeft);

                foreach (var order in _orders)
                {
                    graphics.DrawString($"* {order.Product.Description}({order.Product.ArticleNumber})",
                        font, XBrushes.Black, new XRect(20, height += 15, page.Width, page.Height), XStringFormats.TopLeft);
                }
                
                graphics.DrawString($"Сумма заказа: {_orders.Select(o => o.ProductsAmount * o.Product.ActualCost).Sum()}",
                    font, XBrushes.Black, new XRect(20, height += 15, page.Width, page.Height), XStringFormats.TopLeft);

                graphics.DrawString($"Сумма скидки: {_orders.Select(o => o.ProductsAmount * o.Product.Cost).Sum() - _orders.Select(o => o.ProductsAmount * o.Product.ActualCost).Sum()}",
                    font, XBrushes.Black, new XRect(20, height += 15, page.Width, page.Height), XStringFormats.TopLeft);

                graphics.DrawString($"Пункт выдачи: {_orders.First().PickupPoint.Address}",
                    font, XBrushes.Black, new XRect(20, height += 15, page.Width, page.Height), XStringFormats.TopLeft);

                graphics.DrawString($"Номер заказа: JEW{_orders.First().ReceiveCode}", font, XBrushes.Black,
                    new XRect(20, height += 15, page.Width, page.Height), XStringFormats.TopLeft);

                if (_currentUser != null)
                {
                    graphics.DrawString($"Покупатель: {_currentUser.Credentials}", font, XBrushes.Black,
                    new XRect(20, height += 15, page.Width, page.Height), XStringFormats.TopLeft);
                }

                graphics.DrawString($"Код получения: {_orders.First().ReceiveCode}", new XFont("Comic Sans MS", 20, XFontStyle.Bold), XBrushes.Black,
                    new XRect(20, height += 20, page.Width, page.Height),
                    XStringFormats.TopCenter);

                pdfDocument.Save($@"{_orders.First().OrderDate.Value.Date.ToString("dd-MM-yyyy")}-{_orders.First().ReceiveCode}.pdf");
            }

            _databaseService.GetCloudContext().Orders.AddRange(_orders);
            _databaseService.GetCloudContext().SaveChanges();
            MessageBox.Show("Заказ успешно оформлен",
                            "Заказ",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
