//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Data;
//using Mono.Data.SqliteClient;

//public class DataBaseCreator : MonoBehaviour {

//	// Use this for initialization
//    string sql;
//    string _strDBName = "URI=file:SQLite17.db";
//    IDbConnection _connection;
//    IDbCommand _command;
//    void Start()
//    {
//        CreateDataBase();
//        InsertMatchData(4, "11.11.2017-14:28", "123", "AI", 5, 1, 2, 3, 4);
//        ReadDataBase("SELECT powers FROM match");


//    }

//    public void CreateDataBase()
//    {

//        _connection = new SqliteConnection(_strDBName);
//        _command = _connection.CreateCommand();

//        _connection.Open();

//        sql = "CREATE TABLE match (id INT, datetime VARCHAR(20), length VARCHAR(20), winner VARCHAR(20), powers INT, playerID_1 INT, playerID_2 INT, playerID_3 INT, playerID_4 INT)";
//        _command.CommandText = sql;
//        _command.ExecuteNonQuery();

//        sql = "CREATE TABLE player (id INT, powers INT, accuracy INT, damagedone INT, damagedoneprojectile1 INT, damagedoneprojectile2 INT, damagedoneprojectile3 INT, damagedoneprojectile4 INT )";
//        _command.CommandText = sql;
//        _command.ExecuteNonQuery();


//    }
//    public void InsertMatchData(int id, string datetime, string length, string winner, int powers, int playerID1, int playerID2, int playerID3, int playerID4)
//    {
//        string values = "('" +id + "','"+ datetime + "','" + length +"','" + winner + "'," + powers.ToString() + ","+ playerID1.ToString() +"," + playerID2.ToString() + "," + playerID3.ToString() + "," + playerID4.ToString() + ")";
//        Debug.Log(values);
//        sql = "INSERT INTO match (id, datetime, length, winner, powers, playerID_1, playerID_2, playerID_3, playerID_4) values " + values;
//        _command.CommandText = sql;
//        _command.ExecuteNonQuery();

//    }
//    public void ReadDataBase(string SQLcommand)
//    {
//        string cs = "URI=file:SQLite17.db";

//        using (SqliteConnection con = new SqliteConnection(cs))
//        {
//            con.Open();

//            string stm = SQLcommand;

//            using (SqliteCommand cmd = new SqliteCommand(stm, con))
//            {
//                using (SqliteDataReader rdr = cmd.ExecuteReader())
//                {
//                    while (rdr.Read())
//                    {
//                        for(int i =0; i < rdr.FieldCount; i++)
//                        Debug.Log(rdr.GetString(i));
//                    }
//                }
//            }

//            con.Close();
//        }

//    }

//    //public void InsertPlayerData()
//    //{
//    //    sql = "insert into highscores (name, score) values ('And I', 9001)";
//    //    _command.CommandText = sql;
//    //    _command.ExecuteNonQuery();
//    //}

//}
