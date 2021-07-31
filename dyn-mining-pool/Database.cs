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

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();

            var cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'share'", conn);
            Int64 exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table share (share_id integer primary key, share_timestamp integer, share_wallet text, share_hash text, share_status text, currentdiff integer, currentnethash integer) ", conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand("create index share_idx1 on share (share_status, share_timestamp)", conn);
                cmd.ExecuteNonQuery();

            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'reward'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table reward (reward_id integer primary key, reward_timestamp integer, reward_height integer, reward_hash text, reward_status text) ", conn);
                cmd.ExecuteNonQuery();
                cmd = new SqliteCommand("create index reward_idx on reward(reward_status)", conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'payout'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table payout (payout_id integer primary key, payout_timestamp integer, payout_wallet text, payout_amount integer) ", conn);
                cmd.ExecuteNonQuery();
            }


            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'pending_payout'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("create table pending_payout (pending_payout_id integer primary key, pending_payout_wallet text, pending_payout_amount integer) ", conn);
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

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'hash_rate'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("CREATE TABLE hash_rate (hashrate_id integer primary key,poolhashrate integer,pooldiff integer, cur_networkdiff integer, cur_networkhash integer,tot_sharecount integer,range_min_time integer,range_max_time integer,insert_range_00 integer,insert_range_59 integer,currenttimestamp integer)", conn);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqliteCommand("select count(name) from sqlite_master where type = 'table' and name = 'hashflag'", conn);
            exists = (Int64)cmd.ExecuteScalar();

            if (exists == 0)
            {
                cmd = new SqliteCommand("CREATE TABLE hashflag (flag_key text, insert_timestamp integer,  next_insert_timestamp integer) ", conn);
                cmd.ExecuteNonQuery();

                cmd = new SqliteCommand("insert into hashflag select \"last_insert_run\", @p1, @p2", conn);
                cmd.Parameters.Add(new SqliteParameter("@p1", (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds)));
                cmd.Parameters.Add(new SqliteParameter("@p2", (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds + 60)));
                cmd.ExecuteNonQuery();

            }

        }


        public static void SaveShare(string wallet, string hash, string currentdiff, string currentnethash)
        {

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("insert into share (share_timestamp, share_wallet, share_hash, share_status, currentdiff, currentnethash) values (@p1,@p2,@p3,@p4,@p5,@p6)", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
            cmd.Parameters.Add(new SqliteParameter("@p2", wallet));
            cmd.Parameters.Add(new SqliteParameter("@p3", hash));
            cmd.Parameters.Add(new SqliteParameter("@p4", "pending"));
            cmd.Parameters.Add(new SqliteParameter("@p5", currentdiff));
            cmd.Parameters.Add(new SqliteParameter("@p6", currentnethash));
            cmd.ExecuteNonQuery();
        }

        public static string GetSetting(string key)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("select setting_value from setting where setting_key = @p1", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", key));
            return (string)cmd.ExecuteScalar();

        }

        public static void UpdateSetting(string key, string value)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("update setting set setting_value = @p1 where setting_key = @p2", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", value));
            cmd.Parameters.Add(new SqliteParameter("@p2", key));
            cmd.ExecuteNonQuery();

        }

        public static List<miningShare> CountShares(Int64 endTime)
        {
            List<miningShare> result = new List<miningShare>();

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("select share_wallet, count(1) from share where share_status = 'pending' and share_timestamp < " + endTime + " group by share_wallet", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string wallet = reader[0].ToString();
                UInt32 shares = (UInt32)Convert.ToInt32(reader[1].ToString());
                result.Add(new miningShare(wallet, shares));
            }

            cmd = new SqliteCommand("update share set share_status = 'paid' where share_status = 'pending' and share_timestamp < " + endTime, conn);
            cmd.ExecuteNonQuery();

            return result;
        }


        public static void SaveReward(string blockHash)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("insert into reward (reward_timestamp, reward_height, reward_hash, reward_status) values (@p1,@p2,@p3,@p4)", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
            cmd.Parameters.Add(new SqliteParameter("@p2", blockHash));
            cmd.Parameters.Add(new SqliteParameter("@p3", 1));
            cmd.Parameters.Add(new SqliteParameter("@p4", "pending"));
            cmd.ExecuteNonQuery();
        }


        public static void SavePayout(string wallet, UInt64 amount)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("insert into payout (payout_timestamp, payout_wallet, payout_amount) values (@p1,@p2,@p3)", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
            cmd.Parameters.Add(new SqliteParameter("@p2", wallet));
            cmd.Parameters.Add(new SqliteParameter("@p3", amount));
            cmd.ExecuteNonQuery();
        }

        public static void ClearPendingRewards()
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("update reward set reward_status = 'paid' where reward_status = 'pending'", conn);
            cmd.ExecuteNonQuery();

        }


        public static UInt64 pendingPayouts()
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("select ifnull(sum(pending_payout_amount),0) from pending_payout", conn);
            return Convert.ToUInt64(cmd.ExecuteScalar().ToString());
        }

        public static void SavePendingPayout(string wallet, UInt64 amount)
        {
            if (pendingWalletExists(wallet))
            {
                var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
                conn.Open();
                var cmd = new SqliteCommand("update pending_payout set pending_payout_amount = pending_payout_amount + " + amount + " where pending_payout_wallet = @p1", conn);
                cmd.Parameters.Add(new SqliteParameter("@p1", wallet));
                cmd.ExecuteNonQuery();
            }
            else
            {
                var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
                conn.Open();
                var cmd = new SqliteCommand("insert into pending_payout (pending_payout_wallet, pending_payout_amount) values (@p1,@p2)", conn);
                cmd.Parameters.Add(new SqliteParameter("@p1", wallet));
                cmd.Parameters.Add(new SqliteParameter("@p2", amount));
                cmd.ExecuteNonQuery();
            }

        }

        static bool pendingWalletExists(string wallet)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("select count(1) from pending_payout where pending_payout_wallet = @p1", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", wallet));
            return (Convert.ToUInt32(cmd.ExecuteScalar().ToString()) > 0);

        }


        public static List<pendingPayout> GetPendingPayouts()
        {
            List<pendingPayout> result = new List<pendingPayout>();

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("select pending_payout_wallet, pending_payout_amount from pending_payout", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string wallet = reader[0].ToString();
                UInt64 amount = (UInt64)Convert.ToInt64(reader[1].ToString());
                result.Add(new pendingPayout(wallet, amount));
            }

            return result;
        }

        public static void DeletePendingPayout(string wallet)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("delete from pending_payout where pending_payout_wallet = @p1", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", wallet));
            cmd.ExecuteNonQuery();
        }

        public static Int64 Getflag(string key)
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("select next_insert_timestamp from hashflag where flag_key = @p1", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", key));
            return (Int64)cmd.ExecuteScalar();
        }

        public static void SaveHashrate()
        {
            Int64 unixNowMOD = (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds % 60);
            Int64 unixNowDIFF = (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds - unixNowMOD);
            Int64 minus60 = (Int64)(unixNowDIFF - 60);
            Int64 minus1 = (Int64)(unixNowDIFF - 1);
            Int64 lasthour = (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds - 3600);

            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();

            var cmd = new SqliteCommand("select min(share_id) from share where share_timestamp >= @p5 ", conn);
            cmd.Parameters.Add(new SqliteParameter("@p5", lasthour));
            Int64 shareidval = (Int64)cmd.ExecuteScalar();

            cmd = new SqliteCommand("insert into hash_rate (poolhashrate,pooldiff,cur_networkdiff,cur_networkhash,tot_sharecount,range_min_time,range_max_time,insert_range_00,insert_range_59,currenttimestamp) select ((avg(currentdiff)/256) * count(share_hash) * 4294967296)/60, (currentdiff)/256, avg(currentdiff), avg(currentnethash), " +
                "count(share_hash),min(share_timestamp), max(share_timestamp), @p1,@p2,@p3 from share where share_timestamp between @p1 and @p2 and share_id >= @p4", conn);
            cmd.Parameters.Add(new SqliteParameter("@p3", (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds)));
            cmd.Parameters.Add(new SqliteParameter("@p1", minus60));
            cmd.Parameters.Add(new SqliteParameter("@p2", minus1));
            cmd.Parameters.Add(new SqliteParameter("@p4", shareidval));
            cmd.ExecuteNonQuery();
        }

        public static void UpdateHashflag()
        {
            var conn = new SqliteConnection("Filename=" + Global.DatabaseLocation());
            conn.Open();
            var cmd = new SqliteCommand("UPDATE hashflag SET insert_timestamp = @p1, next_insert_timestamp = @p2  WHERE flag_key = 'last_insert_run'", conn);
            cmd.Parameters.Add(new SqliteParameter("@p1", (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds)));
            cmd.Parameters.Add(new SqliteParameter("@p2", (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds + 60)));
            cmd.ExecuteNonQuery();
        }


    }
}
