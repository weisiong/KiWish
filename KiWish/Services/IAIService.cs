using System.Threading.Tasks;

namespace KiWish.Services
{
    public interface IAIService
    {
        string Name { get; }
        Task<string> GetResponseAsync(string message, string systemPrompt);
    }
}
