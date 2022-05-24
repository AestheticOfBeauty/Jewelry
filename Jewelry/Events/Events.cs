namespace Jewelry.Events
{
    public interface IEvent { }
    public class LoginAsUserEvent : IEvent { }
    public class LoginAsGuestEvent : IEvent { }
    public class ProductChangedEvent : IEvent { }
    public class AddingNewProductEvent : IEvent { }
}
