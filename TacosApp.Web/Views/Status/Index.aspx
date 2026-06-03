<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.master" Inherits="System.Web.Mvc.ViewPage" ResponseEncoding="utf-8" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
        <div class="col-md-8">
            <h2 class="taco-heading mb-2">📡 配達状況</h2>
            <p class="text-muted mb-4">
                注文番号: <strong><%: ViewBag.OrderNumber %></strong>
                （<%: ViewBag.CustomerName %> 様）
            </p>

            <div class="status-steps mb-4">
                <div class="step-item" id="step0">
                    <div class="step-circle">1</div>
                    <div class="step-label">注文受付</div>
                </div>
                <div class="step-line"></div>
                <div class="step-item" id="step1">
                    <div class="step-circle">2</div>
                    <div class="step-label">調理中</div>
                </div>
                <div class="step-line"></div>
                <div class="step-item" id="step2">
                    <div class="step-circle">3</div>
                    <div class="step-label">配達中</div>
                </div>
                <div class="step-line"></div>
                <div class="step-item" id="step3">
                    <div class="step-circle">4</div>
                    <div class="step-label">配達完了</div>
                </div>
            </div>

            <div class="alert status-alert" id="statusMessage" role="alert">
                <strong id="statusText"></strong>
            </div>

            <div class="text-center mt-4">
                <p class="text-muted small" id="connectionStatus">接続中...</p>
            </div>

            <div class="text-center mt-2">
                <a href="<%: Url.Action("Index", "Home") %>" class="btn btn-outline-secondary btn-sm">メニューへ戻る</a>
            </div>
        </div>
    </div>

    <script>
        var orderNumber = '<%: ViewBag.OrderNumber %>';
        var currentStatus = <%: ViewBag.CurrentStatus %>;
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsContent" runat="server">
    <%: System.Web.Optimization.Scripts.Render("~/bundles/signalr") %>
    <%: System.Web.Optimization.Scripts.Render("~/bundles/tacos-status") %>
</asp:Content>