using Stripe;
using Stripe.Checkout;

namespace KiWish.Services
{
    public class StripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ProductKnowledgeService _productKnowledgeService;

        public StripeService(IConfiguration configuration, ProductKnowledgeService productKnowledgeService)
        {
            _configuration = configuration;
            _productKnowledgeService = productKnowledgeService;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<Stripe.Product> GetOrCreateKiWishProductAsync()
        {
            // First try to find an existing KiWish product
            var productService = new ProductService();
            var options = new ProductListOptions
            {
                Active = true
            };
            
            var products = await productService.ListAsync(options);
            var existingProduct = products.Data.FirstOrDefault(p => 
                p.Name.Equals("KiWish Detox", StringComparison.OrdinalIgnoreCase) || 
                p.Name.Contains("KiWish", StringComparison.OrdinalIgnoreCase));
            
            if (existingProduct != null)
            {
                return existingProduct;
            }
            
            // If no existing product found, create a new one
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7001"; // Default to dev URL
            var imageUrl = $"{baseUrl}/ProductImage.jpeg";
            
            // Get product information from ProductKnowledgeService
            var productInfo = await _productKnowledgeService.GetProductInfoAsync();
            
            var productOptions = new ProductCreateOptions
            {
                Name = "KiWish Detox",
                Description = GetProductDescriptionFromInfo(productInfo),
                Images = new List<string> { imageUrl },
                Type = "good",
                Active = true,
                Metadata = new Dictionary<string, string>
                {
                    { "product_type", "detox" },
                    { "brand", "KiWish" }
                }
            };
            
            return await productService.CreateAsync(productOptions);
        }

        public async Task<Price> CreatePriceForProductAsync(string productId, decimal unitAmount, string currency = "usd", string nickname = "Default Price")
        {
            var priceService = new PriceService();
            
            var priceOptions = new PriceCreateOptions
            {
                Product = productId,
                Currency = currency,
                UnitAmount = (long)(unitAmount * 100), // Convert to cents
                Nickname = nickname
            };
            
            return await priceService.CreateAsync(priceOptions);
        }

        public async Task<List<Stripe.Product>> GetProductsAsync(string? productIds = null)
        {
            var productService = new ProductService();
            var options = new ProductListOptions
            {
                Active = true
            };
            
            if (!string.IsNullOrEmpty(productIds))
            {
                // If specific product IDs are provided, search for those specifically
                var productIdsList = productIds.Split(',').Select(id => id.Trim()).ToList();
                options.Ids = productIdsList;
            }
            
            var products = await productService.ListAsync(options);
            return products.Data.ToList();
        }

        public async Task<Stripe.Product> GetProductByIdAsync(string productId)
        {
            var productService = new ProductService();
            return await productService.GetAsync(productId);
        }

        public async Task<List<Price>> GetPricesForProductAsync(string productId)
        {
            var priceService = new PriceService();
            var options = new PriceListOptions
            {
                Product = productId,
                Active = true
            };
            
            var prices = await priceService.ListAsync(options);
            return prices.Data.ToList();
        }

        public async Task<string> CreateCheckoutSessionForKiWishProductAsync(string successUrl, string cancelUrl, int quantity = 1, decimal unitPrice = 49.99m, string currency = "usd")
        {
            // For this implementation, we'll use the most basic API that's available in most versions
            // We'll create a checkout session without the complex nested ProductData that's causing issues
            var baseUrl = new System.Uri(successUrl).GetLeftPart(System.UriPartial.Authority);
            var imageUrl = $"{baseUrl}/ProductImage.jpeg";
            
            // Get product information from ProductKnowledgeService
            var productInfo = await _productKnowledgeService.GetProductInfoAsync();
            
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
                            Currency = currency,
                            UnitAmount = (long)(unitPrice * 100), // Convert to cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "KiWish Detox",
                                Description = GetProductDescriptionFromInfo(productInfo), // Use product info from service
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

        // Helper method to extract product description from product info
        private string GetProductDescriptionFromInfo(string productInfo)
        {
            // Extract the product description from the markdown content
            // Look for the first line that contains the product description
            var lines = productInfo.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Contains("Stubborn Fat Reduction") || trimmedLine.Contains("Detox Drink"))
                {
                    return trimmedLine;
                }
            }
            
            // Default description if not found in product info
            return "KiWish Detox Product - Advanced Detox Solution";
        }

        public async Task<Session> GetCheckoutSessionAsync(string sessionId)
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId);
        }
    }
}
