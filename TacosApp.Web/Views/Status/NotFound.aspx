<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.master" Inherits="System.Web.Mvc.ViewPage" ResponseEncoding="utf-8" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center mt-5">
        <div class="col-md-6 text-center">
            <div style="font-size: 4rem;">🌮❓</div>
            <h2>注文が見つかりません</h2>
            <p class="text-muted"><%: ViewBag.Error %></p>
            <a href="<%: Url.Action("Index", "Home") %>" class="btn btn-taco">メニューへ戻る</a>
        </div>
    </div>
</asp:Content>