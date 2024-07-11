using ServiceReference2;
namespace EyeMezzexz.Services
{
    public class WebServiceClient
    {
        private readonly BarcodeWebServiceSoapClient _client;

        public WebServiceClient()
        {
            _client = new BarcodeWebServiceSoapClient(BarcodeWebServiceSoapClient.EndpointConfiguration.BarcodeWebServiceSoap);
        }

        public async Task<bool> CheckLoginDetailAsync(string email, string password)
        {
            return await _client.CheckLoginDetailAsync(email, password);
        }
        public async Task<GetLoginDetailResponseGetLoginDetailResult> GetLoginDetailAsync(string email, string password)
        {
            return await _client.GetLoginDetailAsync(email, password);
        }
    }
}
    
