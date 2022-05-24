using Jewelry.Pages.Authentication;
using Jewelry.Pages.Orders;
using Jewelry.Pages.Products;
using Jewelry.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Jewelry.Services
{
    public static class Dependency
    {
        private static IServiceProvider _service;

        static Dependency()
        {
            var services = new ServiceCollection();

            services.AddTransient<AuthenticationPage>();
            services.AddTransient<OrdersPage>();
            services.AddSingleton<ProductsPage>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ProductWindow>();
            services.AddSingleton<OrderWindow>();
            
            services.AddTransient<CaptchaService>();
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<PageService>();
            services.AddSingleton<ImageService>();
            services.AddSingleton<EventBus>();
            services.AddSingleton<MessageBus>();

            _service = services.BuildServiceProvider();

            foreach (ServiceDescriptor service in services)
            {
                _service.GetRequiredService(service.ServiceType);
            }
        }

        public static T Resolve<T>() => _service.GetRequiredService<T>();
    }
}
