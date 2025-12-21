using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SurveyApp.Hubs
{
    /// <summary>
    /// SignalR Hub - Gerçek zamanlı bildirimler için
    /// Kullanım alanları:
    /// 1. Yeni anket yanıtı bildirileri
    /// 2. Anket sonuçları canlı güncelleme
    /// 3. Admin'e anlık istatistikler
    /// </summary>
    public class SurveyHub : Hub
    {
        /// <summary>
        /// Kullanıcı bağlandığında çalışır
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            Console.WriteLine($"✅ SignalR bağlantı: {userId ?? "Anonim"} - ConnectionId: {Context.ConnectionId}");

            // Kullanıcıyı gruplara ekle
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                Console.WriteLine($"   → Admin grubuna eklendi");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Kullanıcı bağlantıyı kestiğinde
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name;
            Console.WriteLine($"❌ SignalR bağlantı kesildi: {userId ?? "Anonim"}");

            if (exception != null)
            {
                Console.WriteLine($"   Hata: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Yeni yanıt bildirileri - Admin'e gönder
        /// </summary>
        public async Task NotifyNewResponse(int surveyId, string surveyTitle, int responseCount)
        {
            await Clients.Group("Admins").SendAsync("ReceiveNewResponse", new
            {
                surveyId,
                surveyTitle,
                responseCount,
                timestamp = DateTime.Now
            });

            Console.WriteLine($"📩 Bildirim gönderildi: {surveyTitle} - {responseCount} yanıt");
        }

        /// <summary>
        /// Anket istatistiklerini canlı güncelle
        /// </summary>
        public async Task UpdateSurveyStats(int surveyId, object stats)
        {
            // Belirli bir anketi izleyen herkese gönder
            await Clients.Group($"Survey_{surveyId}").SendAsync("ReceiveSurveyStats", stats);
        }

        /// <summary>
        /// Belirli bir anketi izlemeye başla
        /// </summary>
        public async Task JoinSurveyGroup(int surveyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Survey_{surveyId}");
            Console.WriteLine($"👁️ Kullanıcı {surveyId} numaralı anketi izliyor");
        }

        /// <summary>
        /// Anket izlemeyi bırak
        /// </summary>
        public async Task LeaveSurveyGroup(int surveyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Survey_{surveyId}");
        }

        /// <summary>
        /// Test mesajı gönder
        /// </summary>
        [Authorize]
        public async Task SendTestMessage(string message)
        {
            var UserName = Context.User?.Identity?.Name ?? "Anonim";
            await Clients.All.SendAsync("ReceiveTestMessage", UserName, message);
        }
    }
}