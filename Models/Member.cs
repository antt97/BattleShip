using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MvcApp.Models {
  public class Member {
    public long MemberId { get; set; }
    public long RoomId { get; set; }
    public string UserName { get; set; }
    public string Matrix { get; set; }
    public bool IsTurn { get; set; }
    public int Result { get; set; }
  }
}