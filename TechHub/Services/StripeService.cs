using Stripe;
using Microsoft.Extensions.Configuration;

namespace TeachHub.Services
{
    public class StripeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _environment;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            _environment = configuration["Stripe:Environment"];
        }

        public async Task<string> CreateCharge(string token, float amount)
        {
            // Simulate a charge in test mode
            await Task.Delay(500); // Simulate API call delay
            return $"test_ch_{Guid.NewGuid().ToString("N")}";
        }
    }
}