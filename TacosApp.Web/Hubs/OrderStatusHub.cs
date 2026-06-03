using Microsoft.AspNet.SignalR;

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
        public void JoinOrderGroup(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return;
            }
            // SignalR 2 の Groups.Add は Task を返すが、レガシースタイルでは
            // 戻り値の Task を同期待ちして完了を保証する。
            Groups.Add(Context.ConnectionId, orderNumber).Wait();
        }

        /// <summary>顧客ブラウザがグループから離脱する（ページ離脱時）</summary>
        public void LeaveOrderGroup(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return;
            }
            Groups.Remove(Context.ConnectionId, orderNumber).Wait();
        }
    }
}
