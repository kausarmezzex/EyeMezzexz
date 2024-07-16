using System.Threading.Tasks;
using ServiceReference1; // Ensure that this namespace is correctly referenced

namespace EyeMezzexz.Services
{
    public class WebServiceClient
    {
        private readonly BarcodeWebServiceSoapClient _client;

        public WebServiceClient()
        {
            _client = new BarcodeWebServiceSoapClient(BarcodeWebServiceSoapClient.EndpointConfiguration.BarcodeWebServiceSoap);
        }

        // Synchronous method
        public string GetLoginDetail(string email, string password)
        {
            return _client.GetLoginDetail(email, password);
        }

        // Asynchronous method
        public async Task<string> GetLoginDetailAsync(string email, string password)
        {
            return await _client.GetLoginDetailAsync(email, password);
        }
    }
}
