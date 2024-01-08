#region Copyright (c) 2002-2018, Sage GmbH

//Copyright (c) 2018, Sage GmbH (http://www.sage.de).
//Alle Rechte vorbehalten.
//Weitergabe und Vervielfältigung dieser Moduls oder von Teilen daraus sind, zu welchem Zweck
//und in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die
//Sage GmbH nicht gestattet. In diesem Modul enthaltene Informationen können
//ohne vorherige Ankündigung geändert werden. Die Module werden im Rahmen des Developer Programms
//den Teilnehmern als As-Is Basis zur Verfügung gestellt und stellen keinen Anspruch auf Vollständigkeit dar.

#endregion Copyright (c) 2002-2018, Sage GmbH

using Sagede.Core.Logging;
using System;

namespace WEKO.BirdHome.Absatzplanungimport
{
    /// <summary>
    /// Stellt eine, für diese Assembly,
    /// exclusive Instanz des Loggers zur Verfügung.
    /// </summary>
    public static class TraceLog
    {
        private static ILogger _logger;
        private static readonly object LockObject = new object();

        /// <summary>
        /// Liefert die Instanz des Loggers.
        /// </summary>
        internal static ILogger Logger
        {
            get
            {
                lock (LockObject)
                {
                    return _logger ?? (_logger = LogManager.GetLogger("PSDev", "OfficeLine.DevKonf.HA04"));
                }
            }
        }

        /// <summary>
        /// Loggt Debug-Informationen im TraceLog-Manager
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogVerbose(string message, params object[] args)
        {
            //Logger.LogVerboseFormat(message, args);
            Logger.Verbose(string.Format(message, args));
        }

        /// <summary>
        /// Loggt Exceptions im Tracelog-Manager
        /// </summary>
        /// <param name="ex"></param>
        public static void LogException(Exception ex)
        {
            Logger.Error(String.Format("{0},{1},{2}", ex.Message, Environment.NewLine, ex.StackTrace));
        }

        /// <summary>
        /// Loggt die Dauer von Aufrufen
        /// </summary>
        public static void LogTime(string measurement, long start)
        {
            Logger.LogTime(measurement, start);
        }
    }
}