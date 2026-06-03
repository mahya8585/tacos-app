using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(TacosApp.Web.Startup))]

namespace TacosApp.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // SignalR を OWIN パイプラインにマップ
            app.MapSignalR();
        }
    }
}
