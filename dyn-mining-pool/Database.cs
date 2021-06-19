using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace dyn_mining_pool
{
    public class Database
    {

        public static void CreateOrOpenDatabase()
        {

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation);
            conn.Open();

            var cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'share'", conn);
            Int64 exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table share (share_id integer primary key, share_timestamp integer, share_wallet text, share_hash text) ", conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'reward'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table share (reward_id integer primary key, reward_timestamp integer, reward_height integer, reward_amount integer) ", conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'payout'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table payout (payout_id integer primary key, payout_timestamp integer, payout_wallet text, payout_amount integer) ", conn);
                cmd.ExecuteNonQuery();
            }

        }


        public static void SaveShare ( string wallet )
        {

        }

    }
}
