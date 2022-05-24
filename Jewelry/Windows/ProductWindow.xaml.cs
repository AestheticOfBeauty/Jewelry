using Jewelry.Events;
using Jewelry.Messages;
using Jewelry.Model;
using Jewelry.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace Jewelry.Windows
{
    public partial class ProductWindow : Window
    {
        private readonly ImageService _imageService;
        private readonly EventBus _eventBus;
        private readonly MessageBus _messageBus;
        private readonly DatabaseService _databaseService;
        private Product _selectedProduct;
        private string _safeFileName = string.Empty;
        private string _fullImagePath = string.Empty;

        public ProductWindow(ImageService imageService, EventBus eventBus, MessageBus messageBus, DatabaseService databaseService)
        {
            InitializeComponent();
            _imageService = imageService;
            _eventBus = eventBus;
            _messageBus = messageBus;
            _databaseService = databaseService;

            _messageBus.Receive<ProductMessage>(this, message =>
            {
                _selectedProduct = message.Product;
                ArticleNumberTextBox.IsEnabled = false;
                ArticleNumberTextBox.Text = _selectedProduct.ArticleNumber;
                NameTextBox.Text = _selectedProduct.Name;
                CategoryComboBox.SelectedValue = _selectedProduct.Category;
                QuantityInStockTextBox.Text = _selectedProduct.QuantityInStock.ToString();
                UnitTextBox.Text = _selectedProduct.Unit;
                ManufacturerTextBox.Text = _selectedProduct.Manufacturer;
                SupplierTextBox.Text = _selectedProduct.Supplier;
                CostTextBox.Text = _selectedProduct.Cost.ToString();
                MaxDiscountTextBox.Text = _selectedProduct.MaxDiscount.ToString();
                DiscountAmountTextBox.Text = _selectedProduct.DiscountAmount.ToString();
                ImagePathTextBox.Text = _selectedProduct.PhotoSource is null ? $@"{AppDomain.CurrentDomain.BaseDirectory}Images\picture.png" : _selectedProduct.FullPhotoSource;
                DescriptionTextBox.Text = _selectedProduct.Description;
                _fullImagePath = _selectedProduct.FullPhotoSource;
                _safeFileName = _selectedProduct.PhotoSource;
            });
            _eventBus.Subscribe<AddingNewProductEvent>(@event =>
            {
                _selectedProduct = null;
                ArticleNumberTextBox.IsEnabled = true;
                ArticleNumberTextBox.Text = string.Empty;
                NameTextBox.Text = string.Empty;
                CategoryComboBox.SelectedValue = string.Empty;
                QuantityInStockTextBox.Text = string.Empty;
                UnitTextBox.Text = string.Empty;
                ManufacturerTextBox.Text = string.Empty;
                SupplierTextBox.Text = string.Empty;
                CostTextBox.Text = string.Empty;
                MaxDiscountTextBox.Text = string.Empty;
                DiscountAmountTextBox.Text = string.Empty;
                ImagePathTextBox.Text = string.Empty;
                DescriptionTextBox.Text = string.Empty;
                _fullImagePath = string.Empty;
                _safeFileName = string.Empty;
            });
            CategoryComboBox.ItemsSource = new List<string>
            {
                "Кольцо",
                "Колье",
                "Серьги",
                "Браслет",
                "Подвеска",
                "Ожерелье",
                "Брошь"
            };
        }

        private void ChooseImage(object sender, RoutedEventArgs e)
        {
            var imageOpenFileDialog = _imageService.CreateImageOpenFileDialog();
            if (imageOpenFileDialog.ShowDialog() == true)
            {
                _safeFileName = imageOpenFileDialog.SafeFileName;
                _fullImagePath = imageOpenFileDialog.FileName;
                ImagePathTextBox.Text = _fullImagePath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            var articleNumber = ArticleNumberTextBox.Text;
            if (string.IsNullOrWhiteSpace(articleNumber))
            {
                errors.AppendLine("Артикул не должен быть пустым");
            }

            var name = NameTextBox.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.AppendLine("Наименование не должно быть пустым");
            }

            var category = CategoryComboBox.SelectedValue;
            if (category is null)
            {
                errors.AppendLine("Категория не должна быть пустой");
            }

            var quantityInStock = QuantityInStockTextBox.Text;
            if (!int.TryParse(quantityInStock, out _))
            {
                errors.AppendLine("Количество на складе не должно быть пустым");
            }

            var unit = UnitTextBox.Text;
            if (string.IsNullOrWhiteSpace(unit))
            {
                errors.AppendLine("Единица измерения не должна быть пустой");
            }

            var manufacturer = ManufacturerTextBox.Text;
            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                errors.AppendLine("Производитель не должен быть пустым");
            }

            var supplier = SupplierTextBox.Text;
            if (string.IsNullOrWhiteSpace(supplier))
            {
                errors.AppendLine("Поставщик не должен быть пустым");
            }

            var cost = CostTextBox.Text;
            if (!double.TryParse(cost, out _))
            {
                errors.AppendLine("Стоимость должна быть числом");
            }

            var maxDiscount = MaxDiscountTextBox.Text;
            var discount = DiscountAmountTextBox.Text;
            if (!int.TryParse(maxDiscount, out _) || int.Parse(maxDiscount) > 20)
            {
                errors.AppendLine("Максимальная скидка не должна быть пустой и превышать 20%");
            }
            else
            {
                if (!short.TryParse(discount, out _) || short.Parse(discount) > int.Parse(maxDiscount))
                {
                    errors.AppendLine("Cкидка не должна быть пустой и превышать максимальной скидки");
                }
            }

            var imagePath = ImagePathTextBox.Text;
            if (!File.Exists(imagePath))
            {
                errors.AppendLine("Такого изображения не существует");
            }

            var description = DescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(description))
            {
                errors.AppendLine("Описание не должно быть пустым");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(),
                                "Ошибка данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else
            {
                if (_selectedProduct is null)
                {
                    _selectedProduct = new Product();
                }
                
                _selectedProduct.ArticleNumber = articleNumber;
                _selectedProduct.Name = name;
                _selectedProduct.Category = category.ToString();
                _selectedProduct.QuantityInStock = int.Parse(quantityInStock);
                _selectedProduct.PhotoSource = _safeFileName;
                _selectedProduct.Unit = unit;
                _selectedProduct.Manufacturer = manufacturer;
                _selectedProduct.Supplier = supplier;
                _selectedProduct.Cost = double.Parse(cost);
                _selectedProduct.MaxDiscount = int.Parse(maxDiscount);
                _selectedProduct.DiscountAmount = short.Parse(DiscountAmountTextBox.Text);
                _selectedProduct.Description = description;

                if (_selectedProduct.FullPhotoSource != _fullImagePath)
                {
                    Debug.WriteLine(_selectedProduct.PhotoSource);
                    Debug.WriteLine(_fullImagePath);
                    Debug.WriteLine(_selectedProduct.FullPhotoSource);
                    _imageService.SaveImage(imagePath, _selectedProduct.FullPhotoSource);
                }

                var context = _databaseService.GetCloudContext();
                context.Products.AddOrUpdate(_selectedProduct);
                context.SaveChanges();
                _eventBus.Publish(new ProductChangedEvent());
                Debug.WriteLine("hiding");
                Hide();
            }

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var context = _databaseService.GetCloudContext();
            context.Products.Remove(_selectedProduct);
            context.SaveChanges();
            _eventBus.Publish(new ProductChangedEvent());
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
