using System;
using System.Collections.Generic;

public class Analytics
{
    private static UnityGameState _gs;
    public static void Setup(UnityGameState gs)
    {
        _gs = gs;
    }

    public static void Send(AnalyticsEvents analyticsEvent, string str1 = null,
        string str2 = null,        
        Dictionary<string,string> extraData = null)
    {
        //_gs.logger.Debug("Analytics: " + analyticsEvent.ToString() + ", " + str1 + ", " + str2);
    }
}
