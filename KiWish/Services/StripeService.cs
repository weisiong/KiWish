using Stripe;
using Stripe.Checkout;

namespace KiWish.Services
{
    public class StripeService
    {
        private readonly IConfiguration _configuration;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<string> CreateCheckoutSessionForKiWishProductAsync(string successUrl, string cancelUrl, int quantity = 1, decimal unitPrice = 49.99m)
        {
            // For this implementation, we'll use the most basic API that's available in most versions
            // We'll create a checkout session without the complex nested ProductData that's causing issues
            var baseUrl = new System.Uri(successUrl).GetLeftPart(System.UriPartial.Authority);
            var imageUrl = $"{baseUrl}/ProductImage.jpeg";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        // Using the most basic line item structure that should work
                        // In practice, you would use a pre-configured price ID from your Stripe dashboard
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(unitPrice * 100), // Convert to cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "KiWish Detox",
                                Description = "KiWish Detox Product - Advanced Detox Solution",
                                Images = new List<string> { imageUrl },
                            },
                        },
                        Quantity = quantity,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "product_name", "KiWish Detox" },
                    { "quantity", quantity.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }

        public async Task<Session> GetCheckoutSessionAsync(string sessionId)
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId);
        }
    }
}
