using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using static dyn_mining_pool.Distributor;

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
                cmd = new SqliteCommand("create table share (share_id integer primary key, share_timestamp integer, share_wallet text, share_hash text, share_status text) ", conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'reward'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table reward (reward_id integer primary key, reward_timestamp integer, reward_height integer, reward_amount integer) ", conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'payout'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table payout (payout_id integer primary key, payout_timestamp integer, payout_wallet text, payout_amount integer) ", conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'setting'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table setting (setting_key text, setting_value text) ", conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand("insert into setting (setting_key, setting_value) values (@p1,@p2)", conn);
                cmd.Parameters.Add(new SqliteParameter("@p1", "last_payout_run"));
                cmd.Parameters.Add(new SqliteParameter("@p2", (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
                cmd.ExecuteNonQuery();


            }

        }


        public static void SaveShare ( string wallet, string hash )
        {

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation);
            conn.Open();
            var cmd = new SqliteCommand("insert into share (share_timestamp, share_wallet, share_hash, share_status) values (@p1,@p2,@p3,@p4)", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
            cmd.Parameters.Add(new SqliteParameter("@p2", wallet));
            cmd.Parameters.Add(new SqliteParameter("@p3", hash));
            cmd.Parameters.Add(new SqliteParameter("@p4", "pending"));
            cmd.ExecuteNonQuery();
        }

        public static string GetSetting (string key)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation);
            conn.Open();
            var cmd = new SqliteCommand("select setting_value from setting where setting_key = @p1", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", key));
            return (string)cmd.ExecuteScalar();

        }

        public static void UpdateSetting (string key, string value)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation);
            conn.Open();
            var cmd = new SqliteCommand("update setting set setting_value = @p1 where setting_key = @p2", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", value));
            cmd.Parameters.Add(new SqliteParameter("@p2", key));
            cmd.ExecuteNonQuery();

        }

        public static List<miningShare> CountShares(Int64 endTime)
        {
            List<miningShare> result = new List<miningShare>();

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation);
            conn.Open();
            var cmd = new SqliteCommand("select share_wallet, count(1) from share where share_status = 'pending' and share_timestamp < " + endTime + " group by share_wallet", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string wallet = reader[0].ToString();
                UInt32 shares = (UInt32)Convert.ToInt32 ( reader[1].ToString());
                result.Add(new miningShare(wallet, shares));
            }

            cmd = new SqliteCommand("update share set share_status = 'paid' where share_status = 'pending' and share_timestamp < " + endTime, conn);
            cmd.ExecuteNonQuery();

            return result;
        }

    }
}
