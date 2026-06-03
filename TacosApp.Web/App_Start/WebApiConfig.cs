using System.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;

namespace TacosApp.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // CORS を設定（管理アプリのオリジンのみ許可）
            var adminOrigin = ConfigurationManager.AppSettings["AdminAppOrigin"] ?? "http://localhost:8080";
            var corsPolicy = new EnableCorsAttribute(
                origins: adminOrigin,
                headers: "Content-Type,X-Api-Key",
                methods: "GET,PUT"
            );
            config.EnableCors(corsPolicy);

            // Web API ルーティング
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // JSON を既定のフォーマットにする
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
