using Azure.ResourceManager.ApiManagement.Models;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager;
using Azure;
using Azure.Core;
using Newtonsoft.Json.Linq;

namespace Microsoft.OpenAIRateLimiter.UI
{
    public class APIMService
    {
        private readonly ArmClient _client;

        private readonly IConfiguration _config;

        public APIMService(ArmClient client, IConfiguration configuration)
        {
            _client = client;
            _config = configuration;
        }

        public async Task<string> CreateProduct(string prodName, string apiUrl = "", string apiKey = "")
        {
            string subscriptionId = _config["SubscriptionId"];
            string resourceGroup = _config["ResourceGroup"];
            string apimName = _config["APIMServiceName"];


            var prodResource = await CreateProduct(subscriptionId, resourceGroup, apimName, prodName);

            string apiId = "azure-openai-service-api";

            var res = await AddProductToAPI(prodResource, apiId);


            ApiManagementProductData resourceData = prodResource.Data;

            var subKey = await GetProductSubscription(subscriptionId, resourceGroup, apimName, prodName);

            if(!string.IsNullOrWhiteSpace(apiUrl) && !string.IsNullOrWhiteSpace(apiKey) && prodResource.HasData) 
                await CreateProductPolicy(subscriptionId, resourceGroup, apimName, prodResource.Data.Name, apiUrl, apiKey);

            return subKey;

        }

        private async Task<ApiManagementProductResource> CreateProduct(string subscriptionId, string resourceGroup, string apimName, string prodName)
        {

            var apiManagementServiceResourceId = ApiManagementServiceResource.CreateResourceIdentifier(subscriptionId, resourceGroup, apimName);
            var apiManagementService = _client.GetApiManagementServiceResource(apiManagementServiceResourceId);

            // get the collection of this ApiManagementProductResource
            var prodCollection = apiManagementService.GetApiManagementProducts();

            // invoke the operation
            var data = new ApiManagementProductData()
            {
                DisplayName = prodName,
                Description = "This is a Product for The Cost Gateway Pattern",
                IsSubscriptionRequired = true,
                State = ApiManagementProductState.Published,
            };

            ArmOperation<ApiManagementProductResource> lro = await prodCollection.CreateOrUpdateAsync(WaitUntil.Completed, prodName, data);
            ApiManagementProductResource prodResource = lro.Value;

            return prodResource;

        }

        private async Task<ProductApiData> AddProductToAPI(ApiManagementProductResource prodResource, string serviceName)
        {
            return await prodResource.CreateOrUpdateProductApiAsync(serviceName);
        }

        private async Task<string> GetProductSubscription(string subscriptionId, string resourceGroup, string apimName, string productName)
        {


            var apiManagementServiceResourceId = ApiManagementServiceResource.CreateResourceIdentifier(subscriptionId, resourceGroup, apimName);
            var apiManagementService = _client.GetApiManagementServiceResource(apiManagementServiceResourceId);

            ApiManagementSubscriptionCollection collection2 = apiManagementService.GetApiManagementSubscriptions();

            string sid = "";
            await foreach (ApiManagementSubscriptionResource item in collection2.GetAllAsync())
            {
                if (item.Data.Scope.Contains($"/products/{productName}"))
                {
                    sid = item.Data.Name;
                    break;
                }

                //SubscriptionContractData resourceData2 = item.Data;
            }

            var apiManagementSubscriptionResourceId = ApiManagementSubscriptionResource.CreateResourceIdentifier(subscriptionId, resourceGroup, apimName, sid);
            var apiManagementSubscription = _client.GetApiManagementSubscriptionResource(apiManagementSubscriptionResourceId);

            // invoke the operation
            var secrets = await apiManagementSubscription.GetSecretsAsync();

            return secrets.Value.PrimaryKey;

        }

        private async Task<bool> CreateProductPolicy(string subscriptionId, string resourceGroup, string apimName, string productId, string apiUrl, string apiKey)
        {

            //Create Policy
            var policy = $"<policies>  <inbound>  <base />  <set-backend-service base-url=\"{apiUrl}\" /> <set-header name=\"api-key\" exists-action=\"override\"><value>{apiKey}</value> </set-header> </inbound>  <backend>    <base />  </backend>  <outbound>    <base />  </outbound> </policies>";
            //var policy = $"<policies>  <inbound>  <base />  </inbound>  <backend>    <base />  </backend>  <outbound>    <base />  </outbound> </policies>";

            ResourceIdentifier apiManagementProductResourceId = ApiManagementProductResource.CreateResourceIdentifier(subscriptionId, resourceGroup, apimName, productId);
            ApiManagementProductResource apiManagementProduct = _client.GetApiManagementProductResource(apiManagementProductResourceId);

            // get the collection of this ApiManagementProductPolicyResource
            ApiManagementProductPolicyCollection collection = apiManagementProduct.GetApiManagementProductPolicies();

            // invoke the operation
            
            var data = new PolicyContractData()
            {
                Value = policy,
                Format = PolicyContentFormat.Xml,
            };
            ArmOperation<ApiManagementProductPolicyResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, PolicyName.Policy, data);
            //ApiManagementProductPolicyResource result = lro.Value;

            return lro.Value.HasData;
        }

    }
}
