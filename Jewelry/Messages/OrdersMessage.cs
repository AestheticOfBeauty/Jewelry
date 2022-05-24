using Jewelry.Model;
using System.Collections.Generic;

namespace Jewelry.Messages
{
    public class OrdersMessage : IMessage
    {
        public OrdersMessage(List<Order> orders)
        {
            Orders = orders;
        }

        public List<Order> Orders { get; set; }
    }
}
