using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace TacosApp.Web.Filters
{
    /// <summary>
    /// ASP.NET Core Web API 用 API キー認証フィルター。
    /// リクエストヘッダー "X-Api-Key" を appsettings.json の ApiKey と照合する。
    /// </summary>
    public class ApiKeyAuthFilter : IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string? configuredKey = _configuration["ApiKey"];

            // ApiKey が未設定の場合はサーバー側の設定ミスとして 500 を返す
            if (string.IsNullOrEmpty(configuredKey) ||
                configuredKey == "CHANGE_THIS_IN_PRODUCTION_USE_STRONG_RANDOM_KEY")
            {
                context.Result = new ObjectResult("API キーが設定されていません。appsettings.json を確認してください。")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var values))
            {
                context.Result = new UnauthorizedObjectResult("X-Api-Key ヘッダーが必要です。");
                return;
            }

            string provided = values.ToString();

            // 定数時間比較でタイミング攻撃を防ぐ
            if (!ConstantTimeEquals(configuredKey, provided))
            {
                context.Result = new UnauthorizedObjectResult("無効な API キーです。");
            }
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
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
