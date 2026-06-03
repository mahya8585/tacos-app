namespace TacosApp.Web.Models.Api
{
    /// <summary>管理アプリから注文ステータスを更新するリクエスト</summary>
    public class UpdateStatusRequest
    {
        /// <summary>新しいステータス値（0=注文受付, 1=調理中, 2=配達中, 3=配達完了）</summary>
        public int Status { get; set; }
    }
}
