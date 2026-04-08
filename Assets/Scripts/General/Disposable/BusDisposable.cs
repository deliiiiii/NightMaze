using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace General
{
    [DebuggerStepThrough]
    public static class BusDisposable
    {
        static readonly HashSet<string> muteSet = new();
        public static IDisposable MuteScope(string eventOuterClassName)
            => new MuteToken(eventOuterClassName);
        public static bool IsMute(string eventOuterClassName) 
            => muteSet.Any(eventOuterClassName.Contains);
        sealed class MuteToken : IDisposable
        {
            readonly string eventOuterClassName;
            public MuteToken(string eventOuterClassName)
            {
                this.eventOuterClassName = eventOuterClassName;
                muteSet.Add(eventOuterClassName);
            }
            public void Dispose()
            {
                muteSet.Remove(eventOuterClassName);
            }
        }
    }
}