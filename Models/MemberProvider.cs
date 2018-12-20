using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MvcApp.Models {
  public class MemberProvider {
    protected static Member Member(SqlDataReader reader) {
      return new Member {
        MemberId = (long)reader["MemberId"],
        RoomId = (long)reader["RoomId"],
        UserName = reader["UserName"].ToString().ToLower(),
        Matrix = reader["Matrix"].ToString(),
        IsTurn = (bool)reader["IsTurn"],
        Result = (int)reader["Result"],
      };
    }

    protected static List<Member> Members(SqlDataReader reader) {
      List<Member> list = new List<Member>();
      while (reader.Read()) {
        list.Add(Member(reader));
      }
      return list;
    }

    public static long InsertMember(long roomId, string userName) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("InsertMember", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@MemberId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Value = roomId;
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userName.ToLower();
        cn.Open();
        int ret = cmd.ExecuteNonQuery();
        if(ret > 0)
          return (long)cmd.Parameters["@MemberId"].Value;
        return 0;
      }
    }
    public static List<Member> GetMembersByRoomId(long roomId) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("GetMembersByRoomId", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Value = roomId;
        cn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        return Members(reader);
      }
    }
    //public static Member GetMemberByRoomId(long roomId, string userName) {
    //  using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
    //    SqlCommand cmd = new SqlCommand("GetMemberByRoomId", cn);
    //    cmd.CommandType = CommandType.StoredProcedure;
    //    cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Value = roomId;
    //    cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userName.ToLower();
    //    cn.Open();
    //    SqlDataReader reader = cmd.ExecuteReader();
    //    if(reader.Read())
    //      return Member(reader);
    //    return null;
    //  }
    //}

    public static void UpdateMember(Member member) {
      using(SqlConnection cn = new SqlConnection(Helper.ConnectString)) {
        SqlCommand cmd = new SqlCommand("UpdateMember", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@MemberId", SqlDbType.BigInt).Value = member.MemberId;
        cmd.Parameters.Add("@RoomId", SqlDbType.BigInt).Value = member.RoomId;
        cmd.Parameters.Add("@Matrix", SqlDbType.NVarChar).Value = member.Matrix;
        cmd.Parameters.Add("@IsTurn", SqlDbType.Bit).Value = member.IsTurn;
        cmd.Parameters.Add("@Result", SqlDbType.Int).Value = member.Result;
        cn.Open();
        cmd.ExecuteNonQuery();
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