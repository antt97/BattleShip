using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BattleShip.Startup))]
namespace BattleShip {
  public partial class Startup {
    public void Configuration(IAppBuilder app) {
      ConfigureAuth(app);
      app.MapSignalR();

    }
  }
}
