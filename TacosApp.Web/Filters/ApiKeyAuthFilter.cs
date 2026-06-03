using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace TacosApp.Web.Filters
{
    /// <summary>
    /// Web API 用 API キー認証フィルター。
    /// リクエストヘッダー "X-Api-Key" を Web.config の ApiKey と照合する。
    /// </summary>
    public class ApiKeyAuthFilter : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string configuredKey = ConfigurationManager.AppSettings["ApiKey"];

            // ApiKey が未設定の場合はサーバー側の設定ミスとして 500 を返す
            if (string.IsNullOrEmpty(configuredKey) ||
                configuredKey == "CHANGE_THIS_IN_PRODUCTION_USE_STRONG_RANDOM_KEY")
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    "API キーが設定されていません。Web.config を確認してください。");
                return;
            }

            IEnumerable<string> values;
            if (!actionContext.Request.Headers.TryGetValues("X-Api-Key", out values))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized,
                    "X-Api-Key ヘッダーが必要です。");
                return;
            }

            string provided = string.Join("", values);

            // 定数時間比較でタイミング攻撃を防ぐ
            if (!ConstantTimeEquals(configuredKey, provided))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized,
                    "無効な API キーです。");
            }
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}
