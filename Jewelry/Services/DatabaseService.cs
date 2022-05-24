using Jewelry.Model;
using System.Configuration;

namespace Jewelry.Services
{
    public class DatabaseService
    {
        private JewelryContext _cloudContext;
        private JewelryContext _localContext;

        public DatabaseService()
        {
            GetCloudContext();
            GetLocalContext();
        }
        public JewelryContext GetCloudContext()
        {
            if (_cloudContext == null)
            {
                var cloudStorageConnectionString = ConfigurationManager.ConnectionStrings["JewelryContextCloud"].ConnectionString;
                _cloudContext = new JewelryContext(cloudStorageConnectionString);
            }
            return _cloudContext;
        }
        public JewelryContext GetLocalContext()
        {
            if (_localContext == null)
            {
                var cloudStorageConnectionString = ConfigurationManager.ConnectionStrings["JewelryContextLocal"].ConnectionString;
                _localContext = new JewelryContext(cloudStorageConnectionString);
            }
            return _localContext;
        }
    }
}
