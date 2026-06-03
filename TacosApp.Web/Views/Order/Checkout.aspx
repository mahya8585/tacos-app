<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.master" Inherits="System.Web.Mvc.ViewPage" ResponseEncoding="utf-8" %>
<%@ Import Namespace="TacosApp.Web.Models.ViewModels" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <% var vm = Model as CheckoutViewModel; %>
    <div class="row justify-content-center">
        <div class="col-md-8 col-lg-6">
            <h2 class="taco-heading mb-4">📍 お届け先の入力</h2>

            <% using (Html.BeginForm("Checkout", "Order", FormMethod.Post, new { @class = "needs-validation" })) { %>
                <%: Html.AntiForgeryToken() %>

                <div class="form-group">
                    <label for="CustomerName">お名前</label>
                    <%: Html.TextBox("CustomerName", vm != null ? vm.CustomerName : "", new { @class = "form-control", placeholder = "山田 太郎" }) %>
                    <%: Html.ValidationMessage("CustomerName", "", new { @class = "text-danger small" }) %>
                </div>

                <div class="form-group">
                    <label for="Phone">電話番号</label>
                    <%: Html.TextBox("Phone", vm != null ? vm.Phone : "", new { @class = "form-control", placeholder = "090-1234-5678", type = "tel" }) %>
                    <%: Html.ValidationMessage("Phone", "", new { @class = "text-danger small" }) %>
                </div>

                <div class="form-group">
                    <label for="DeliveryAddress">配達先住所</label>
                    <%: Html.TextArea("DeliveryAddress", vm != null ? vm.DeliveryAddress : "", 3, 0, new { @class = "form-control", placeholder = "東京都渋谷区○○町1-2-3 ×× マンション 101号室" }) %>
                    <%: Html.ValidationMessage("DeliveryAddress", "", new { @class = "text-danger small" }) %>
                </div>

                <div class="form-group">
                    <label for="DeliveryNote">備考・配達メモ</label>
                    <%: Html.TextArea("DeliveryNote", vm != null ? vm.DeliveryNote : "", 2, 0, new { @class = "form-control", placeholder = "例：インターホンが壊れているため電話でご連絡ください" }) %>
                    <%: Html.ValidationMessage("DeliveryNote", "", new { @class = "text-danger small" }) %>
                </div>

                <div class="form-group mt-4">
                    <button type="submit" class="btn btn-taco btn-block">内容を確認する →</button>
                    <a href="<%: Url.Action("Cart", "Order") %>" class="btn btn-outline-secondary btn-block mt-2">カートへ戻る</a>
                </div>
            <% } %>
        </div>
    </div>
</asp:Content>