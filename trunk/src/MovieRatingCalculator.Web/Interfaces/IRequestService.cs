using System.Web;

namespace MovieRatingCalculator.Web.Interfaces
{
    public interface IRequestService
    {
        string GetClientIpAddress(HttpRequestBase request);
        bool IsPrivateIpAddress(string ipAddress);
    }
}
