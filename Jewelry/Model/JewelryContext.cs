using MySql.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jewelry.Model
{


    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public partial class JewelryContext : DbContext
    {
        public JewelryContext()
            : base("name=JewelryContext")
        {
        }

        public JewelryContext(string connectionString)
            : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<PickupPoint> PickupPoints { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
    }

    [Table("users")]
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Order = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public string Credentials => $"{Surname} {Name} {Patronymic}";


        public virtual Role Role { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Order { get; set; }
    }

    public partial class Role
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Role()
        {
            this.User = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> User { get; set; }
    }

    [Table("orders")]
    public partial class Order
    {
        public int Id { get; set; }
        public int ProductsAmount { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? ReceiveCode { get; set; }
        public int OrderStatusId { get; set; }
        public int? UserId { get; set; }
        public string ProductArticleNumber { get; set; }
        public int PickupPointId { get; set; }

        public virtual OrderStatus OrderStatus { get; set; }
        public virtual Product Product { get; set; }
        public virtual PickupPoint PickupPoint { get; set; }
        public virtual User User { get; set; }
    }

    [Table("pickuppoints")]
    public partial class PickupPoint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PickupPoint()
        {
            this.Order = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public int HouseNumber { get; set; }
        
        [NotMapped]
        public string Address => $"{PostalCode}, {City}, {Street}, д.{HouseNumber}";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Order { get; set; }
    }

    [Table("products")]
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.Orders = new HashSet<Order>();
        }

        [Key]
        public string ArticleNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string PhotoSource { get; set; }
        public string Manufacturer { get; set; }
        public double Cost { get; set; }
        public int QuantityInStock { get; set; }
        public short DiscountAmount { get; set; }
        public int MaxDiscount { get; set; }
        public string Unit { get; set; }
        public string Supplier { get; set; }

        [NotMapped]
        public int Amount { get; set; }
        [NotMapped]
        public string FullPhotoSource => $@"{AppDomain.CurrentDomain.BaseDirectory}Images\{PhotoSource}";
        [NotMapped]
        public double ActualCost => Cost - Cost * DiscountAmount / 100;
        [NotMapped]
        public BitmapImage Image { get; set; }
        [NotMapped]
        public SolidColorBrush DiscountColor => DiscountAmount >= 15 ?
            (SolidColorBrush)new BrushConverter().ConvertFrom("#7fff00") :
            (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Orders { get; set; }
    }

    [Table("orderstatuses")]
    public partial class OrderStatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OrderStatus()
        {
            this.Order = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Order { get; set; }
    }
}