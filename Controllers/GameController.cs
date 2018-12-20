using MvcApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;

namespace BattleShip.Controllers {
  public class GameController : Controller {
    protected IHubContext context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

    [System.Web.Mvc.Authorize] // Phải đăng nhập mới được xem
    public ActionResult Index() { // Danh sách các phòng thi đấu
      var user = UserProvider.GetUser(User.Identity.Name);
      if(user.Stage != 0) // Người này đang thi đấu thì redirect vào phòng đang đấu
        return RedirectToAction("Combat/" + user.RoomId);

      List<Room> rooms = RoomProvider.GetRoomsPending();
      
      // Kiểm tra các phòng hết giờ thì cho kết thúc trận đấu
      foreach(var item in rooms) {
        if(item.DateUpdate.AddSeconds(60) < DateTime.Now)
          Helper.EndCombat(item);
      }

      return View(rooms);
    }

    [System.Web.Mvc.Authorize] // Phải đăng nhập mới được xem
    public ActionResult Combat(long id) { // Phòng đấu có id = roomId
      var user = UserProvider.GetUser(User.Identity.Name);
      Room room = RoomProvider.GetRoomById(id);
      if(room == null) { // Phòng này không tồn tại
        user.Stage = 0;
        UserProvider.UpdateUser(user);
        return RedirectToAction("Index");
      }
      // Danh sách 2 người trong phòng đấu này
      room.Members = MemberProvider.GetMembersByRoomId(room.RoomId);
      // Trường hợp phòng bị xóa (0) hoặc phòng đã kết thúc (3)
      if(room.Status == 0 || room.Status == 3) {
        foreach(var item in room.Members) {
          // Redirect 2 người này về trang chính
          var u = UserProvider.GetUser(item.UserName);
          u.Stage = 0;
          UserProvider.UpdateUser(u);
        }
        
      }
      if(room.Status == 0) // Phòng đã xóa
        return RedirectToAction("Index");
      
      // Nếu là phòng "đang chờ" và người tham gia khác người tạo phòng
      // thì thêm "người đó" vào phòng và bắt đầu trận đấu
      if(room.Status == 1 && User.Identity.Name.ToLower() != room.UserName) {
        // Thêm người chơi này vào phòng
        user.Stage = 1;
        user.RoomId = room.RoomId;
        UserProvider.UpdateUser(user);
        MemberProvider.InsertMember(room.RoomId, User.Identity.Name);

        // Cập nhật phòng trạng thái đang thi đấu (Status = 2)
        room.Status = 2;
        room.DateUpdate = DateTime.Now;
        RoomProvider.UpdateRoom(room);
        room.Members = MemberProvider.GetMembersByRoomId(room.RoomId);
        // Random ma trận tàu của 2 người chơi
        foreach(var item in room.Members) {
          item.Matrix = Helper.RandomMatrix();
          if(item.UserName == room.UserName) // Chủ phòng được bắn đầu tiên
            item.IsTurn = true;
          MemberProvider.UpdateMember(item);
        }
        // Socket gửi tới những người trong phòng RoomId tải lại trang "onLoad"
        context.Clients.Group(room.RoomId.ToString()).onLoad();
      }
      if(user.Stage != 1) // Trường hợp người này đang ở trang chính
        return RedirectToAction("Index");
      return View(room);
    }
    
    // Người chơi bấm các phím số, gửi lên server với biến key
    public void Press(string userName, int key) {
      var user = UserProvider.GetUser(userName);
      if(user.Stage == 0) { // Trường hợp người chơi ở trang chính
        List<Room> rooms = RoomProvider.GetRoomsPending();
        if(key == 3) { // Tạo phòng thi đấu mới
          long roomId = RoomProvider.InsertRoom(userName);
          MemberProvider.InsertMember(roomId, userName);
          // Đổi trang thái người chơi sang phòng thi đấu
          user.Stage = 1;
          user.RoomId = roomId;
          UserProvider.UpdateUser(user);
          // Socket những người ở trang chính có sự thay đổi phòng "onLoad"
          context.Clients.All.onLoad();
          // Socket người chơi vào phòng thi đấu
          context.Clients.User(userName).onJoin(roomId);
        }else if(key == 5) { // Vào phòng đang chọn
          if(rooms.Count() == 0) 
            return; 
          // Nếu phòng ít hơn vị trí đang chọn thì --
          while(user.Position0 > rooms.Count() - 1)
            user.Position0--;
          // Cập nhật người chơi vào phòng chơi có roomId = rooms[user.Position0].RoomId
          user.Stage = 1;
          user.RoomId = rooms[user.Position0].RoomId;
          UserProvider.UpdateUser(user);
          // Socket người chơi vào phòng thi đấu
          context.Clients.User(userName).onJoin(user.RoomId);
        } else if(key == 4) { // Trường hợp di chuyển qua phòng bên trái
          if(user.Position0 == 0)
            return;
          user.Position0--;
          UserProvider.UpdateUser(user);
          // Socket người chơi đã thay đổi vị trí phòng chọn
          context.Clients.User(userName).onLoad();
        } else if(key == 6) { // Trường hợp di chuyển qua phòng bên phải
          if(user.Position0 >= rooms.Count() - 1)
            user.Position0 = rooms.Count() - 1;
          else
            user.Position0++;
          UserProvider.UpdateUser(user);
          // Socket người chơi đã thay đổi vị trí phòng chọn
          context.Clients.All.onLoad();
        } else if(key == 1) { // Trường hợp muốn về trang chính
          user.Stage = 0;
          UserProvider.UpdateUser(user);
          context.Clients.User(userName).onBack();
        }
      } else if(user.Stage == 1) { // Người chơi đang ở phòng thi đấu
        int sizeMatrix = Helper.Size * Helper.Size;
        if(key == 6)  // Di chuyển vị trí sang phải
          user.Position1 = (user.Position1 + 1) % sizeMatrix;
        else if(key == 2) // Di chuyển vị trí sang xuống
          user.Position1 = (user.Position1 + Helper.Size) % sizeMatrix;
        else if(key == 4) // Di chuyển vị trí sang trái
          user.Position1 = (user.Position1 - 1 + sizeMatrix) % sizeMatrix;
        else if(key == 8) // Di chuyển vị trí sang lên
          user.Position1 = (user.Position1 - Helper.Size + sizeMatrix) % sizeMatrix;
        else if(key == 1) { // Trường hợp muốn về trang chính
          user.Stage = 0;
          UserProvider.UpdateUser(user);
          context.Clients.User(userName).onBack();
        }
        if(key == 5) // Trường hợp người chơi bắn
          Pick(user.RoomId, user.UserName, user.Position1);
        else {
          UserProvider.UpdateUser(user);
          // Socket người chơi thay đổi vị trí chọn hiện tại
          context.Clients.User(userName).onLoad();
        } 
      }
    }

    // Người chơi userName bắn phòng id tại vị trí pick
    public int Pick(long id, string userName, int pick) { // id is RoomId
      if(pick < 0 || pick > Helper.Size * Helper.Size - 1) // Kiểm tra vị trí
        return -1;
      var room = RoomProvider.GetRoomById(id);
      if(room == null || room.Status != 2) // Kiểm tra phòng
        return -2;
      room.Members = MemberProvider.GetMembersByRoomId(room.RoomId);
      var my = room.Members.Where(d => d.UserName == userName.ToLower()).FirstOrDefault();
      if(my == null || my.IsTurn == false) // Người chơi không thuộc hoặc chưa tới lượt
        return -3;
      // reval là class của đối thủ
      var reval = room.Members.Where(d => d.UserName != userName.ToLower()).FirstOrDefault();
      if(reval.Matrix[pick] != '1' && reval.Matrix[pick] != '2') // Trường hợp không phải ô được bắn
        return -4;
      // CẬP NHẬT TRẠNG THÁI ĐÃ BẮN
      my.IsTurn = false;
      MemberProvider.UpdateMember(my);
      if(reval.Matrix[pick] == '1') // Nếu ô này không phải là tàu
        reval.Matrix = Helper.replaceAt(reval.Matrix, pick, '3');
      else if(reval.Matrix[pick] == '2') // Nếu ô này là tàu
        reval.Matrix = Helper.replaceAt(reval.Matrix, pick, '4');
      reval.IsTurn = true; // Lượt chơi dành cho đối thủ
      // Kiểm tra xem tàu đối thủ đã bị bắn hết chưa?
      bool isWin = true;
      for(int i = 0; i < Helper.Size; i++) {
        for(int j = 0; j < Helper.Size; j++) {
          if(reval.Matrix[i * Helper.Size + j] == '2')
            isWin = false;
        }
      }
      if(isWin == true) { // Tàu đối thủ đã bị bắn hết, cập nhật result = 1 (win)
        my.Result = 1;
        MemberProvider.UpdateMember(my);
        reval.Result = -1;
        room.Status = 3;
        RoomProvider.UpdateRoom(room);
        // Socket thông báo phòng này đã kết thúc
        context.Clients.Group(room.RoomId.ToString()).onEnd(my.UserName);
      }
      MemberProvider.UpdateMember(reval);
      // Socket vị trí đã bị bắn cho 2 người chơi
      context.Clients.Group(room.RoomId.ToString()).onBoom();
      return 2;
    }

    [HttpPost]
    public int End(long id) { // Kết thúc trận đấu khi hết giờ sau 60s
      var room = RoomProvider.GetRoomById(id);
      if(room == null || room.Status != 2)
        return -1;
      if(room.DateUpdate.AddSeconds(60) > DateTime.Now) 
        return -2;
      Helper.EndCombat(room); 
      // Người chiến thắng
      var win = room.Members.Where(d => d.Result == 1).FirstOrDefault();
      context.Clients.Group(room.RoomId.ToString()).onEnd(win.UserName);
      var user = UserProvider.GetUser(win.UserName);
      user.Stage = 0;
      UserProvider.UpdateUser(user);
      // Người bị thua
      var lose = room.Members.Where(d => d.Result == -1).FirstOrDefault();
      user = UserProvider.GetUser(lose.UserName);
      user.Stage = 0;
      UserProvider.UpdateUser(user);
      return 2;
    }
  }
}