using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MvcApp.Models {
  public class UserProvider {
    protected static User User(SqlDataReader reader) {
      return new User {
        UserName = reader["UserName"].ToString().ToLower(),
        Stage = (int)reader["Stage"],
        Position0 = (int)reader["Position0"],
        Position1 = (int)reader["Position1"],
        RoomId = (long)reader["RoomId"],
      };
    }

    protected static List<User> Users(SqlDataReader reader) {
      List<User> list = new List<User>();
      while (reader.Read()) {
        list.Add(User(reader));
      }
      return list;
    }

    public static void InsertUser(string userName) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("InsertUser", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userName.ToLower();
        cn.Open();
        cmd.ExecuteNonQuery();
      }
    }
    public static User GetUser(string userName) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("GetUser", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userName.ToLower();
        cn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        if(reader.Read())
          return User(reader);
        return null;
      }
    }

    public static void UpdateUser(User user) {
      if(user.Position0 < 0)
        user.Position0 = 0;
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("UpdateUser", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = user.UserName.ToLower();
        cmd.Parameters.Add("@Stage", SqlDbType.Int).Value = user.Stage;
        cmd.Parameters.Add("@Position0", SqlDbType.Int).Value = user.Position0;
        cmd.Parameters.Add("@Position1", SqlDbType.Int).Value = user.Position1;
        cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Value = user.RoomId;
        cn.Open();
        cmd.ExecuteNonQuery();
      }
    }

  }
}