#region Copyright (c) 2002-2018, Sage GmbH

//Copyright (c) 2018, Sage GmbH (http://www.sage.de).
//Alle Rechte vorbehalten.
//Weitergabe und Vervielfältigung dieser Moduls oder von Teilen daraus sind, zu welchem Zweck
//und in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die
//Sage GmbH nicht gestattet. In diesem Modul enthaltene Informationen können
//ohne vorherige Ankündigung geändert werden. Die Module werden im Rahmen des Developer Programms
//den Teilnehmern als As-Is Basis zur Verfügung gestellt und stellen keinen Anspruch auf Vollständigkeit dar.

#endregion Copyright (c) 2002-2018, Sage GmbH

using System;

namespace WEKO.BirdHome.Absatzplanungimport
{
    /// <summary>
    /// Exception für nicht gefunden Datensätze
    /// </summary>
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException()
        {
        }

        public DataNotFoundException(string message) : base(message)
        {
        }

        public DataNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DataNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}