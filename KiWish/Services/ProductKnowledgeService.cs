using System.IO;
using System.Threading.Tasks;

namespace KiWish.Services
{
    public class ProductKnowledgeService
    {
        private string _content = string.Empty;
        private readonly string _filePath;

        public ProductKnowledgeService()
        {
            // Assuming the file is at the specified path relative to the solution or absolute
            // In a real app, I'd inject IWebHostEnvironment to get the path, but here I'll use the fixed path provided in instructions
            // or relative to the app execution.
            // The prompt gave: c:\Temp\KiWish_DotNet_20260204_1730\Specs\kiwish_product_info.md
            _filePath = @"c:\Temp\KiWish_DotNet_20260204_1730\Specs\kiwish_product_info.md";
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
