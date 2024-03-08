using System;
using System.Collections.Generic;

namespace Re_Hippocamp.Serializable
{
    [Serializable]
    public class HLogs
    {

        public List<HLog> Logs = new List<HLog>();

    }

    [Serializable]
    public class HLog
    {
        public int Color = 0;
        public int BColor = 0;
        public DateTime Date;
        public string Ram = "";
        public string Content = "";
        public string MemberName, SourceFile = "";
        public int Line = 0;
    }
}
