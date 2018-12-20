using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using MvcApp.Models;

namespace BattleShip {

  [HubName("GameHub")]
  public class GameHub : Hub {
    public void JoinRoom(string id) { //Socket join Room
      Groups.Add(Context.ConnectionId, id);
    }

    public override Task OnConnected() {
      return base.OnConnected();
    }

    public override Task OnDisconnected(bool stopCalled) {
      return base.OnDisconnected(stopCalled);
    }
  }
}