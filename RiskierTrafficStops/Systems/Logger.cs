﻿using Rage;
using System;

// Rohit said he wanted credit. So credit to Rohit for the code https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/Logger.cs

namespace RiskierTrafficStops.Systems
{
    internal static class Logger
    {
        private const string DefaultInfo = "[{0}] RiskierTrafficStops: {1}";

        internal static void Error(Exception ex, string location)
        {
            Game.LogTrivial(string.Format(DefaultInfo, "ERROR", ex.ToString()));
            if (Settings.AutoLogEnabled)
            {
                PostToDiscord.LogToDiscord(ex, location);
            }
        }

        internal static void Debug(string msg)
        {
            Game.LogTrivial(string.Format(DefaultInfo, "DEBUG", msg));
        }
    }
}