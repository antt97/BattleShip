using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MvcApp.Models {
  public class RoomProvider {
    public static Room Room(SqlDataReader reader) {
      return new Room {
        RoomId = (long)reader["RoomId"],
        DateCreate = DateTime.Parse(reader["DateCreate"].ToString()),
        DateUpdate = DateTime.Parse(reader["DateUpdate"].ToString()),
        Status = (int)reader["Status"],
        UserName = reader["UserName"].ToString().ToLower()
      };
    }

    protected static List<Room> Rooms(SqlDataReader reader) {
      List<Room> list = new List<Room>();
      while (reader.Read()) {
        list.Add(Room(reader));
      }
      return list;
    }

    public static long InsertRoom(string userName) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("InsertRoom", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userName.ToLower();
        cn.Open();
        int ret = cmd.ExecuteNonQuery();
        if(ret > 0)
          return (long)cmd.Parameters["@RoomId"].Value;
        return 0;
      }
    }


    public static List<Room> GetRoomsPending() {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("GetRoomsPending", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        return Rooms(reader);
      }
    }
    public static Room GetRoomById(long roomId) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("GetRoomById", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Value = roomId;
        cn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        if(reader.Read())
          return Room(reader);
        return null;
      }
    }

    public static void UpdateRoom(Room room) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("UpdateRoom", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Value = room.RoomId;
        cmd.Parameters.Add("@Status", SqlDbType.Int).Value = room.Status;
        cn.Open();
        cmd.ExecuteReader();
      }
    }

    //public static List<Data> GetDatasByDateTime(DateTime start, DateTime end) {
    //  using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
    //    SqlCommand cmd = new SqlCommand("GetDatasByDateTime", cn);
    //    cmd.CommandType = CommandType.StoredProcedure;
    //    cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = start;
    //    cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = end;
    //    cn.Open();
    //    SqlDataReader reader = cmd.ExecuteReader();
    //    return Datas(reader);
    //  }
    //}
    //public static void DeleteDatas() {
    //  using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
    //    SqlCommand cmd = new SqlCommand("DeleteDatas", cn);
    //    cmd.CommandType = CommandType.StoredProcedure;
    //    cn.Open();
    //    cmd.ExecuteReader();
    //  }
    //}

  }
}