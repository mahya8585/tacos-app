using Microsoft.AspNetCore.SignalR;

namespace TacosApp.Web.Hubs
{
    /// <summary>
    /// 注文ステータスをリアルタイムで顧客ブラウザに配信する SignalR ハブ。
    /// 顧客は注文番号のグループに参加し、管理アプリが REST API 経由で
    /// ステータスを更新すると statusUpdated イベントを受信する。
    /// </summary>
    public class OrderStatusHub : Hub
    {
        /// <summary>顧客ブラウザが注文番号グループに参加する</summary>
        public async Task JoinOrderGroup(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, orderNumber);
        }

        /// <summary>顧客ブラウザがグループから離脱する（ページ離脱時）</summary>
        public async Task LeaveOrderGroup(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return;
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderNumber);
        }
    }
}
