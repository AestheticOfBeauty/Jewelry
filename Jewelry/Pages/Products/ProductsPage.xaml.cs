using Jewelry.Commands;
using Jewelry.Events;
using Jewelry.Messages;
using Jewelry.Model;
using Jewelry.Pages.Authentication;
using Jewelry.Services;
using Jewelry.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jewelry.Pages.Products
{
    public partial class ProductsPage : Page
    {
        private readonly PageService _navigation;
        private readonly DatabaseService _databaseService;
        private readonly EventBus _eventBus;
        private readonly MessageBus _messageBus;
        private readonly ImageService _imageService;
        private User _currentUser;
        private List<Product> _selectedProducts = new List<Product>();

        public ProductsPage(PageService navigation, DatabaseService databaseService,
            EventBus eventBus, ImageService imageService, MessageBus messageBus)
        {
            InitializeComponent();
            DataContext = this;
            _navigation = navigation;
            _databaseService = databaseService;
            _eventBus = eventBus;
            _imageService = imageService;
            _messageBus = messageBus;

            DiscountsComboBox.ItemsSource = new List<DiscountItem>
            {
                new DiscountItem(Discount.AllDiscounts, "Все диапазоны"),
                new DiscountItem(Discount.From0To10, "0-9,99%"),
                new DiscountItem(Discount.From10To15, "10-14,99%"),
                new DiscountItem(Discount.From15, "15% и более")
            };

            SortingComboBox.ItemsSource = new List<PriceSortingItem>
            {
                new PriceSortingItem(ListSortDirection.Ascending, "Цена по возрастанию"),
                new PriceSortingItem(ListSortDirection.Descending, "Цена по убыванию")
            };

            _messageBus.Receive<UserMessage>(this, message =>
            {
                _currentUser = message.User;                
            });

            _eventBus.Subscribe<LoginAsUserEvent>(@event =>
            {
                _currentUser = _databaseService.GetLocalContext().Users.Find(_currentUser.Id);
                UserCredentialsLabel.Content = _currentUser.Credentials;

                if (_currentUser.Role.Name == "Администратор")
                {
                    AddProductButton.Visibility = Visibility.Visible;
                }
            });
            _eventBus.Subscribe<LoginAsGuestEvent>(@event =>
            {
                UserCredentialsLabel.Content = "Учётная запись Гостя";
                AddProductButton.Visibility = Visibility.Collapsed;
            });

            _eventBus.Subscribe<ProductChangedEvent>(@event =>
            {
                ProductsListView.ItemsSource = FilterAndSortProducts();
            });

            var products = _databaseService.GetCloudContext().Products.ToList();
            products.ForEach(product => product.Image = !File.Exists(product.FullPhotoSource) ?
                _imageService.GetImageFromPath($@"{AppDomain.CurrentDomain.BaseDirectory}Images\picture.png")
                : _imageService.GetImageFromPath($@"{AppDomain.CurrentDomain.BaseDirectory}Images\{product.PhotoSource}"));
            ProductsListView.ItemsSource = products;
        }

        public ICommand OpenProductContextMenu => new DelegateCommand<Product>(product =>
        {
            _selectedProducts.Add(product);
            OrderButton.Visibility = Visibility.Visible;

        }, (product) => (_currentUser is null || _currentUser.Role.Name == "Клиент") && !_selectedProducts.Contains(product));
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _navigation.Navigate(Dependency.Resolve<AuthenticationPage>());
        }
        private void TextFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ProductsListView.ItemsSource = FilterAndSortProducts();
        }
        private void DiscountFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductsListView.ItemsSource = FilterAndSortProducts();
        }
        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductsListView.ItemsSource = FilterAndSortProducts();
        }
        private List<Product> FilterAndSortProducts()
        {
            var products = _databaseService.GetCloudContext().Products.AsEnumerable();
            var productsAmount = products.Count();
            var textFilter = TextFilterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                products = products.Where(product => product.Name.ToLower().Contains(textFilter.ToLower()));
            }

            var discountOption = (DiscountsComboBox.SelectedItem as DiscountItem);
            if (discountOption != null)
            {
                switch (discountOption.Discount)
                {
                    case Discount.From0To10:
                        products = products.Where(product => product.DiscountAmount >= 0 && product.DiscountAmount < 10);
                        break;
                    case Discount.From10To15:
                        products = products.Where(product => product.DiscountAmount >= 10 && product.DiscountAmount < 15);
                        break;
                    case Discount.From15:
                        products = products.Where(product => product.DiscountAmount >= 15);
                        break;
                    case Discount.AllDiscounts:
                    default:
                        break;
                }
            }

            var sortingDirection = (SortingComboBox.SelectedItem as PriceSortingItem);
            if (sortingDirection != null)
            {
                if (sortingDirection.ListSortDirection == ListSortDirection.Descending)
                {
                    products = products.OrderByDescending(product => product.ActualCost);
                }
                else
                {
                    products = products.OrderBy(product => product.ActualCost);
                }
            }

            products.ToList().ForEach(product => product.Image = product.PhotoSource is null ?
                _imageService.GetImageFromPath($@"{AppDomain.CurrentDomain.BaseDirectory}Images\picture.png")
                : _imageService.GetImageFromPath($@"{AppDomain.CurrentDomain.BaseDirectory}Images\{product.PhotoSource}"));
            CurrentProductsRationToActualLabel.Content = $"{products.Count()} из {productsAmount}";
            return products.ToList();
        }
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role.Name == "Администратор")
            {
                _eventBus.Publish(new AddingNewProductEvent());
                Dependency.Resolve<ProductWindow>().Show();
            }
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser.Role.Name == "Администратор")
            {
                var selectedProduct = ProductsListView.SelectedItem as Product;
                if (selectedProduct != null)
                {
                    _messageBus.SendTo<ProductWindow>(new ProductMessage(selectedProduct));
                    Dependency.Resolve<ProductWindow>().Show();
                }
            }
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            var orders = new List<Order>();
            var newReceiveCode = _databaseService.GetCloudContext().Orders.Select(o => o.ReceiveCode).Max() + 1;
            foreach (Product product in _selectedProducts)
            {
                orders.Add(new Order
                {
                    ProductsAmount = 1,
                    Product = product,
                    ReceiveCode = newReceiveCode
                });
            }
            _messageBus.SendTo<OrderWindow>(new UserMessage(_currentUser));
            _messageBus.SendTo<OrderWindow>(new OrdersMessage(orders));
            Dependency.Resolve<OrderWindow>().Show();
            OrderButton.Visibility = Visibility.Hidden;
            _selectedProducts.Clear();
        }
    }

    public class DiscountItem
    {
        public DiscountItem(Discount discount, string name)
        {
            Discount = discount;
            Name = name;
        }

        public Discount Discount { get; set; }
        public string Name { get; set; }
    }
    public enum Discount
    {
        AllDiscounts = 0,
        From0To10 = 1,
        From10To15 = 2,
        From15 = 3
    }

    public class PriceSortingItem
    {
        public PriceSortingItem(ListSortDirection listSortDirection, string name)
        {
            ListSortDirection = listSortDirection;
            Name = name;
        }

        public ListSortDirection ListSortDirection { get; set; }
        public string Name { get; set; }
    }
    public enum PriceSorting
    {
        Ascending = 0,
        Descending = 1
    }
}
