using System.Web.Optimization;

namespace TacosApp.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jQuery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            // Bootstrap JS
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.js"));

            // SignalR
            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                "~/Scripts/jquery.signalR-{version}.js"));

            // アプリ独自スクリプト（カート）
            bundles.Add(new ScriptBundle("~/bundles/tacos-cart").Include(
                "~/Scripts/tacos-cart.js"));

            // アプリ独自スクリプト（注文ステータス）
            bundles.Add(new ScriptBundle("~/bundles/tacos-status").Include(
                "~/Scripts/tacos-status.js"));

            // CSS
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css"));
        }
    }
}
