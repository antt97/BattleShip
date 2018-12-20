using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MvcApp.Models {
  public class User {
    public string UserName { get; set; }
    public int Stage { get; set; }
    public int Position0 { get; set; }
    public int Position1 { get; set; }
    public long RoomId { get; set; }
  }
}