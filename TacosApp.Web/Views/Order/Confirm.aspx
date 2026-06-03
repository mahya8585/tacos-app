<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.master" Inherits="System.Web.Mvc.ViewPage" ResponseEncoding="utf-8" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="TacosApp.Web.Models.ViewModels" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <% var vm = (OrderConfirmViewModel)Model; %>
    <div class="row justify-content-center">
        <div class="col-md-8">
            <h2 class="taco-heading mb-4">📋 注文内容の確認</h2>

            <div class="card mb-4 shadow-sm">
                <div class="card-header bg-taco text-white">お届け先情報</div>
                <div class="card-body">
                    <dl class="row mb-0">
                        <dt class="col-sm-3">お名前</dt>
                        <dd class="col-sm-9"><%: vm.CustomerName %></dd>
                        <dt class="col-sm-3">電話番号</dt>
                        <dd class="col-sm-9"><%: vm.Phone %></dd>
                        <dt class="col-sm-3">住所</dt>
                        <dd class="col-sm-9"><%: vm.DeliveryAddress %></dd>
                        <% if (!string.IsNullOrEmpty(vm.DeliveryNote)) { %>
                        <dt class="col-sm-3">備考</dt>
                        <dd class="col-sm-9"><%: vm.DeliveryNote %></dd>
                        <% } %>
                    </dl>
                </div>
            </div>

            <div class="card mb-4 shadow-sm">
                <div class="card-header bg-taco text-white">ご注文内容</div>
                <div class="card-body p-0">
                    <table class="table mb-0">
                        <thead class="thead-light">
                            <tr>
                                <th>商品名</th>
                                <th class="text-center">数量</th>
                                <th class="text-right">小計</th>
                            </tr>
                        </thead>
                        <tbody>
                            <% foreach (var item in vm.Items) { %>
                            <tr>
                                <td>
                                    <%: item.MenuName %>
                                    <% if (item.Toppings.Count > 0) { %>
                                    <div>
                                        <% foreach (var t in item.Toppings) { %>
                                        <span class="badge badge-light">+<%: t.Name %></span>
                                        <% } %>
                                    </div>
                                    <% } %>
                                </td>
                                <td class="text-center"><%: item.Quantity %></td>
                                <td class="text-right">¥<%: item.SubTotal.ToString("N0") %></td>
                            </tr>
                            <% } %>
                        </tbody>
                        <tfoot>
                            <tr class="table-active font-weight-bold">
                                <td colspan="2" class="text-right">合計（代金引換）</td>
                                <td class="text-right">¥<%: vm.Total.ToString("N0") %></td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>

            <% using (Html.BeginForm("PlaceOrder", "Order", FormMethod.Post)) { %>
                <%: Html.AntiForgeryToken() %>
                <div class="d-flex justify-content-between">
                    <a href="<%: Url.Action("Checkout", "Order") %>" class="btn btn-outline-secondary">← 修正する</a>
                    <button type="submit" class="btn btn-taco btn-lg">注文を確定する 🌮</button>
                </div>
            <% } %>
        </div>
    </div>
</asp:Content>