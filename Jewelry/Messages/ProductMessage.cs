using Jewelry.Model;

namespace Jewelry.Messages
{
    public class ProductMessage : IMessage
    {
        public ProductMessage(Product product)
        {
            Product = product;
        }

        public Product Product { get; set; }
    }
}
