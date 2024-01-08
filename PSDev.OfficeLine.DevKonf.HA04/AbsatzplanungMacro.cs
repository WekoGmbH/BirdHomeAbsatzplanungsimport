#region Copyright (c) 2002-2018, Sage GmbH

//Copyright (c) 2018, Sage GmbH (http://www.sage.de).
//Alle Rechte vorbehalten.
//Weitergabe und Vervielfältigung dieser Moduls oder von Teilen daraus sind, zu welchem Zweck
//und in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die
//Sage GmbH nicht gestattet. In diesem Modul enthaltene Informationen können
//ohne vorherige Ankündigung geändert werden. Die Module werden im Rahmen des Developer Programms
//den Teilnehmern als As-Is Basis zur Verfügung gestellt und stellen keinen Anspruch auf Vollständigkeit dar.

#endregion Copyright (c) 2002-2018, Sage GmbH

using Sagede.OfficeLine.Shared.RealTimeData.MacroProcess; // required for Extension of MacroProcessBase
using Sagede.Shared.RealTimeData.Common;
using System;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace WEKO.BirdHome.Absatzplanungimport.RealTimeData
{
    public class AbsatzplanungMacro : MacroProcessBase
     
    {
        /// <summary>
        /// Name des Macroprozesses
        /// </summary>
        protected override string Name
        {
            get
            {
                return "AbsatzplanungMakro";
            }
        }

        /// <summary>
        /// Execute Methode wird aus dem Makro heraus über "AufrufenDLL" angesprochen
        /// MakroParameter 1: Vollständiger DLL Name inklusive .dll, z.B. PSDEV.OfficeLine.Examples.RealTimeData.dll
        /// MakroParameter 2: Vollständiger Name der anzusprechenden Klasse, z.B. PSDEV.OfficeLine.Examples.RealTimeData.SampleMacroProcess
        /// MakroParameter 3: Zu übergebende Strukturfelder in Makro Syntax inkl. [ ], z.B. [Kto];[VorID] - mit Semikolon getrennt
        /// </summary>
        /// <param name="parameters">Collection von Datenstrukturfeldern die in die Macroprozessimplementierung hineingereicht wird</param>
        /// <param name="cancel">Rückgabe ob Macrofunktionalität abgebrochen wurde true / false</param>
        /// <param name="cancelMessage">Fehlermeldung falls Macrofunktionalität abgebrochen wurde</param>
        /// <returns></returns>
        ///
        protected override NamedParameters Execute(NamedParameters parameters, ref bool cancel, ref string cancelMessage)
        {
            try
            {
                var blobPath = parameters.First(p => p.Name == "BlobPath").Value;
                var blobPathFile = parameters.First(p => p.Name == "BlobPathFile").Value;
                var importDatei = parameters.First(p => p.Name == "Importdatei").Value;
               
               

        var blobProvider = new BlobProvider(base.Mandant.Credential.Name, base.Mandant.Credential.Password);
                var content = blobProvider.GetBlob(blobPathFile);
                var importList = new ImportAbsatzplanungList();



                /*importList.InitializeByBytes(base.Mandant, content);
                importList.ExecuteImport();*/
               /*string importdateineu = "C:\\blobstorage\\data\\Containers\\Data\\OLDemoReweAbfD\\123\\BatchFiles\\Prognose November 2018.xlsx";*/
                    string importdateineu = importDatei;

                /*FOS BEGIN*/
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                Excel.Range range;

                int rCnt = 0;
                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(importdateineu, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                range = xlWorkSheet.UsedRange;

                //Gehe das ganze Zabellenblatt durch
                for (rCnt = 1; rCnt <= range.Rows.Count; rCnt++)
                {
                    //Hier haben wir Zugriff auf jede Zeile
                    if ((range.Cells[rCnt, 1] as Excel.Range).Value2 != null)
                    {
                        try
                        {
                            string sZelleSpalte1 = (string)(range.Cells[rCnt, 1] as Excel.Range).Value2;
                            string sZelleSpalte2 = (string)(range.Cells[rCnt, 2] as Excel.Range).Value2;
                        }
                        catch { }
                    }
                }

                xlWorkBook.Close(true, null, null);
                xlApp.Quit();

                /*FOS ENDE*/

                blobProvider.DeleteBlob(importDatei, blobPath);

                cancel = false;
                cancelMessage = String.Empty;
                return parameters;
            }
            catch (Exception ex)
            {
                cancel = true;
                cancelMessage = ex.Message;
                return parameters;
            }
        }

        /// <summary>
        /// Vorbereitung der Ausführung
        /// </summary>
        protected override void Prepare()
        {
        }
    }
}


