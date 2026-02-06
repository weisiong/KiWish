using System.IO;
using System.Threading.Tasks;

namespace KiWish.Services
{
    public class ProductKnowledgeService
    {
        private string _content = string.Empty;
        private readonly string _filePath;

        public ProductKnowledgeService(IConfiguration configuration)
        {
            // Get the content root path from configuration and construct the correct path
            var contentRootPath = configuration["ContentRootPath"] ?? AppContext.BaseDirectory;
            _filePath = Path.Combine(contentRootPath, "wwwroot", "kiwish_product_info.md");
            System.Console.WriteLine($"Product info file path: {_filePath}");
        }

        public async Task<string> GetProductInfoAsync()
        {
            if (!string.IsNullOrEmpty(_content))
            {
                return _content;
            }

            if (File.Exists(_filePath))
            {
                _content = await File.ReadAllTextAsync(_filePath);
            }
            else
            {
                _content = "Product information not found.";
            }

            return _content;
        }
    }
}
