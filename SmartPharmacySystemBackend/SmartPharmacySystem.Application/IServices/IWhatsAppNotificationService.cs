using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices
{
    public interface IWhatsAppNotificationService
    {
        Task<bool> SendMessageAsync(string phoneNumber, string message);
        Task<WhatsAppHealthResult> CheckHealthAsync();
    }

    public class WhatsAppHealthResult
    {
        public bool IsAvailable { get; set; }
        public bool IsReady { get; set; }
        public int HttpStatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? QrCode { get; set; }
        public DateTime? QrGeneratedAt { get; set; }
        public string? LastDisconnectReason { get; set; }
        public int Retries { get; set; }
        public string? RawResponse { get; set; }
        public double ResponseTimeMs { get; set; }
    }
}
