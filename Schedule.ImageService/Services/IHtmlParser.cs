using System.Threading.Tasks;

namespace Schedule.ImageService.Services
{
    public interface IHtmlParser
    {
        Task<string> ParseDay(string htmlPage, int day);
    }
}