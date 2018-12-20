using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MvcApp.Models {
  public class Room {
    public long RoomId { get; set; }
    public DateTime DateCreate { get; set; } = DateTime.Now;
    public DateTime DateUpdate { get; set; }
    public int Status { get; set; }
    public string UserName { get; set; }
    public List<Member> Members { get; set; }
  }
}