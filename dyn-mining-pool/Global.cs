﻿using System;
using System.Collections.Generic;
using System.Text;

namespace dyn_mining_pool
{
    public class Global
    {

        public static bool Shutdown = false;
        public static string FullNodeRPC = "http://10.1.0.29:6433/";
        public static string FullNodeUser = "user";
        public static string FullNodePass = "123456";
        public static string DatabaseLocation = @"C:\pool\mining_pool.db";

        public static string AlgoProgram;
        public static string PrevBlockHash;
        public static string CurrPoolTarget;
        public static string CurrBlockTarget;

        public static int feePercent = 2;

        public static string miningWallet = "dy1qqsyj5s9t8eqtzn9x8twfnelxj7am9q9dntt55y";
        public static string profitWallet = "dy1qqsyj5s9t8eqtzn9x8twfnelxj7am9q9dntt55y";


        public static uint randSeed;
        public static Object randLock = new Object();
        public static uint randomNum(uint x)
        {
            lock (randLock)
            {
                randSeed += x;
                return randSeed;
            }
        }

        public static void updateRand(uint x)
        {
            lock (randLock)
            {
                randSeed += x;
            }
        }

    }
}
