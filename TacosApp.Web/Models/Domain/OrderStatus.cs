namespace TacosApp.Web.Models.Domain
{
    /// <summary>注文ステータス</summary>
    public enum OrderStatus
    {
        /// <summary>注文受付</summary>
        Received = 0,
        /// <summary>調理中</summary>
        Preparing = 1,
        /// <summary>配達中</summary>
        OnDelivery = 2,
        /// <summary>配達完了</summary>
        Delivered = 3
    }
}
