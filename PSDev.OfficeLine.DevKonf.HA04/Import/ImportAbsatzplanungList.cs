#region Copyright (c) 2002-2018, Sage GmbH

//Copyright (c) 2018, Sage GmbH (http://www.sage.de).
//Alle Rechte vorbehalten.
//Weitergabe und Vervielfältigung dieser Moduls oder von Teilen daraus sind, zu welchem Zweck
//und in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die
//Sage GmbH nicht gestattet. In diesem Modul enthaltene Informationen können
//ohne vorherige Ankündigung geändert werden. Die Module werden im Rahmen des Developer Programms
//den Teilnehmern als As-Is Basis zur Verfügung gestellt und stellen keinen Anspruch auf Vollständigkeit dar.

#endregion Copyright (c) 2002-2018, Sage GmbH

using Sagede.Core.Tools;
using Sagede.OfficeLine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WEKO.BirdHome.Absatzplanungimport
{
    public class ImportAbsatzplanungList : List<ImportAbsatzplanung>
    {
        private Mandant _mandant;

        public void InitializeByBytes(Mandant mandant, Byte[] content)
        {
            try
            {
                _mandant = mandant;
                this.Clear();
                string result = System.Text.Encoding.Default.GetString(content);
                var list = Regex.Split(result, "\r\n").ToList();

                foreach (var line in list)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var absatzplanung = line.Split(';');
                        this.Add(new ImportAbsatzplanung(mandant)
                        {
                            Artikelnummer = absatzplanung[8],
                            Monat1 = absatzplanung[13],
                            Monat2 = absatzplanung[14],
                            Monat3 = absatzplanung[15],
                            Monat4 = absatzplanung[16],
                            Monat5 = absatzplanung[17],
                            Monat6 = absatzplanung[18],
                            Monat7 = absatzplanung[19]
                            /*Ziellagerbestand = ConversionHelper.ToDecimal(artikel[4], true)*/
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLog.LogException(ex);
                throw;
            }
        }

        public void ExecuteImport()
        {
            this.ForEach(a =>
            {
                if (!a.Save())
                {
                    throw new Exception("Fehler beim Import " + a.Errors.GetDescriptionSummary());
                }
            });
        }
    }
}