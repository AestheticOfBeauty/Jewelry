using Jewelry.Model;

namespace Jewelry.Messages
{
    public class UserMessage : IMessage
    {
        public UserMessage(User user)
        {
            User = user;
        }

        public User User { get; set; }
    }
}
