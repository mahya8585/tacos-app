namespace TacosApp.Web.Services
{
    /// <summary>
    /// Admin API → Blazor Server コンポーネントへのリアルタイム通知ブリッジ（Singleton）。
    /// SignalR クライアントなしで OrderStatus.razor を更新するために使用。
    /// </summary>
    public class OrderStatusNotificationService
    {
        /// <summary>引数: orderNumber, statusValue, statusLabel</summary>
        public event Action<string, int, string>? OnStatusUpdated;

        public void NotifyStatusUpdate(string orderNumber, int status, string message)
        {
            OnStatusUpdated?.Invoke(orderNumber, status, message);
        }
    }
}
