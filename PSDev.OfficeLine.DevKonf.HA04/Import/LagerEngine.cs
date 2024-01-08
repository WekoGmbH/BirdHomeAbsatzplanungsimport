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
using Sagede.OfficeLine.Data;
using Sagede.OfficeLine.Data.Entities.Main;
using Sagede.OfficeLine.Engine;
using Sagede.OfficeLine.Shared;
using Sagede.OfficeLine.Wawi.LagerEngine;
using System;

namespace WEKO.BirdHome.Absatzplanungimport
{
    public class LagerEngine : LagerplatzBuchung, IDisposable
    {
        private Mandant _mandant;

        private LagerplatzBuchungBewegungsart _bewegungsart;
        private Lagerplatz _herkunftslagerplatz;

        private Lagerplatz _ziellagerplatz;
        private ArtikelItem _artikelItem;

        private ArtikelVariantenItem _artikelVariantenItem;

        private LagerJob _lagerjob;

        /// <summary>
        /// Herkunftslagerplatz der Buchung
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Lagerplatz Herkunfslagerplatz
        {
            get { return _herkunftslagerplatz; }
            set { _herkunftslagerplatz = value; }
        }

        /// <summary>
        /// Ziellagerplatz der Buchung
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Lagerplatz Ziellagerplatz
        {
            get { return _ziellagerplatz; }
            set { _ziellagerplatz = value; }
        }

        /// <summary>
        /// Lagerjob für die Buchung
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public LagerJob LagerJob
        {
            get { return _lagerjob; }
            set { _lagerjob = value; }
        }

        public ArtikelItem ArtikelItem
        {
            get { return _artikelItem; }
        }

        public new string Artikelnummer
        {
            get { return base.Artikelnummer; }
            set
            {
                _artikelItem = _mandant.MainDevice.Entities.Artikel.GetItem(value, _mandant.Id);
                if (_artikelItem == null)
                {
                    throw new Exception("Artikel nicht in Datenbank vorhanden.");
                }
                base.Artikelnummer = value;
            }
        }

        public new int AuspraegungsHandle
        {
            get { return base.AuspraegungsHandle; }
            set
            {
                QueryParameterList parameterList = new QueryParameterList();
                parameterList.Add(new ClauseParameter
                {
                    FieldName = "Mandant",
                    Value = _mandant.Id
                });
                parameterList.Add(new ClauseParameter
                {
                    FieldName = "AuspraegungID",
                    Value = value
                });
                _artikelVariantenItem = _mandant.MainDevice.Entities.ArtikelVarianten.GetItem(parameterList);
                if (_artikelVariantenItem == null)
                {
                    throw new Exception("Artikelvariante nicht vorhanden.");
                }
                base.AuspraegungsHandle = value;
            }
        }

        public new ApplicationStateInfo Errors
        {
            get { return base.Errors; }
            set { base.Errors = value; }
        }

        public new int BuchungsHandle
        {
            get { return base.BuchungsHandle; }
        }

        /// <summary>
        /// Konstruktor der Klasse
        /// </summary>
        /// <param name="mandant"></param>
        /// <param name="initialesApplikationsdatum"></param>
        /// <param name="initialesMandantenJahr"></param>
        /// <param name="bewegungsart"></param>
        /// <remarks></remarks>

        public LagerEngine(Mandant mandant, System.DateTime initialesApplikationsdatum, short initialesMandantenJahr, LagerJob lagerJob, LagerplatzBuchungBewegungsart bewegungsart) : this(mandant, initialesApplikationsdatum, initialesMandantenJahr, bewegungsart)
        {
            _lagerjob = lagerJob;
            base.SerienNummernCollection = new SeriennummernEintragCollection();
            base.ChargenCollection = new ChargenEintragCollection();
        }

        public LagerEngine(Mandant mandant, System.DateTime initialesApplikationsdatum, short initialesMandantenJahr, LagerplatzBuchungBewegungsart bewegungsart) : base(mandant, initialesApplikationsdatum, initialesMandantenJahr)
        {
            _mandant = mandant;
            _bewegungsart = bewegungsart;
            _lagerjob = createNewLagerjob();
            base.Bewegungsdatum = initialesApplikationsdatum;
            base.SerienNummernCollection = new SeriennummernEintragCollection();
            base.ChargenCollection = new ChargenEintragCollection();

            switch (bewegungsart)
            {
                case LagerplatzBuchungBewegungsart.BewegungsartInterneUmbuchung:
                    base.Bewegungsart = "IU";
                    break;

                case LagerplatzBuchungBewegungsart.BewegungsartManuelleEntnahme:
                    base.Bewegungsart = "EM";
                    break;

                case LagerplatzBuchungBewegungsart.BewegungsartManuellerZugang:
                    base.Bewegungsart = "ZM";
                    break;
            }
        }

        public bool Validate()
        {
            var bestaendeAktualisieren = true;
            var lagerfcts = new Sagede.OfficeLine.Wawi.LagerEngine.LagerFunctions(_mandant, base.ApplikationsDatum, base.MandantenJahr);

            if (this.MengeBasis == 0 && this.MengeLager != 0)
            {
                // Basismenge berechnen
                this.MengeBasis = this.MengeLager * _artikelItem.UmrechnungsFaktorLMEValue;
            }

            switch (_bewegungsart)
            {
                case LagerplatzBuchungBewegungsart.BewegungsartInterneUmbuchung:
                    return base.Validate(_herkunftslagerplatz, _ziellagerplatz, ref bestaendeAktualisieren, true, false);

                case LagerplatzBuchungBewegungsart.BewegungsartManuelleEntnahme:
                    if (_herkunftslagerplatz == null)
                    {
                        this.setHerkunftslagerplatz(lagerfcts.Hauptlagerplatz(this.Artikelnummer));
                    }
                    return base.Validate(_herkunftslagerplatz, null, ref bestaendeAktualisieren, true, false);

                case LagerplatzBuchungBewegungsart.BewegungsartManuellerZugang:
                    if (_ziellagerplatz == null)
                    {
                        this.setZiellagerPlatz(lagerfcts.Hauptlagerplatz(this.Artikelnummer));
                    }
                    return base.Validate(null, _ziellagerplatz, ref bestaendeAktualisieren, true, false);

                default:
                    throw new Exception("Fehlerhafte Bewegungsart.");
            }
        }

        public bool setZiellagerPlatz(int platzID)
        {
            try
            {
                _ziellagerplatz = setLagerplatz(platzID);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool setHerkunftslagerplatz(int platzID)
        {
            try
            {
                _herkunftslagerplatz = setLagerplatz(platzID);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Lagerplatz setLagerplatz(int platzID)
        {
            Sagede.OfficeLine.Data.Entities.Main.LagerplaetzeItem lagerplatzItem = null;
            Lagerplatz lagerPlatz = new Lagerplatz(_mandant, base.ApplikationsDatum);

            lagerplatzItem = _mandant.MainDevice.Entities.Lagerplaetze.GetItem(platzID, _mandant.Id);

            if (lagerplatzItem == null)
            {
                throw new Exception(string.Format("Lagerplatz {0} nicht in Datenbank vorhanden.", platzID));
            }

            lagerplatzItem = _mandant.MainDevice.Entities.Lagerplaetze.GetItem(platzID, _mandant.Id);
            lagerPlatz.Breite = lagerplatzItem.BreiteValue;
            lagerPlatz.Dimension1 = lagerplatzItem.Dimension1Value;
            lagerPlatz.Dimension2 = lagerplatzItem.Dimension2Value;
            lagerPlatz.Dimension3 = lagerplatzItem.Dimension3Value;
            lagerPlatz.Hoehe = lagerplatzItem.HoeheValue;
            lagerPlatz.Inventurmethode = lagerplatzItem.InventurmethodeValue;
            lagerPlatz.IstGesperrt = lagerplatzItem.IstGesperrt;
            lagerPlatz.IstGesperrtInventur = lagerplatzItem.IstGesperrtInventur;
            lagerPlatz.Kurzbezeichnung = lagerplatzItem.Kurzbezeichnung;
            lagerPlatz.Laenge = lagerplatzItem.LaengeValue;
            lagerPlatz.Lagerkennung = lagerplatzItem.Lagerkennung;
            lagerPlatz.LetztesInventurjahr = lagerplatzItem.LetztesInventurjahrValue;
            lagerPlatz.LetztesInventurUebernahmedatum = lagerplatzItem.LetztesInventurUebernahmedatumValue;
            lagerPlatz.LetztesInventurZaehldatum = lagerplatzItem.LetztesInventurZaehldatumValue;
            lagerPlatz.PlatzBezeichnung = lagerplatzItem.Platzbezeichnung;
            lagerPlatz.PlatzHandle = lagerplatzItem.PlatzID;
            lagerPlatz.Status = lagerplatzItem.StatusValue;
            lagerPlatz.Tragkraft = lagerplatzItem.TragkraftValue;
            lagerPlatz.Volumen = lagerplatzItem.VolumenValue;

            return lagerPlatz;
        }

        public bool Save()
        {
            return saveInternal();
        }

        /// <summary>
        /// Methode dient als zusätzliche Validierungsmethode, um Zugriff gegen die
        /// Office Line LagerEngine zu validieren und zu kapseln.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool validateInternal()
        {
            if (this.MengeBasis == 0 && this.MengeLager != 0)
            {
                // Basismenge berechnen
                this.MengeBasis = this.MengeLager * _artikelItem.UmrechnungsFaktorLMEValue;
            }

            if (_artikelItem.Nachweispflicht < 2)
            {
                // Seriennummer-Collection darf nicht instanziiert sein, wenn
                // kein Seriennummer-Artikel
                this.SerienNummernCollection = null;
            }

            if (_artikelItem.Chargenpflicht < 2)
            {
                // Chargen-Colletion darf nicht instanziiert sein, wenn kein
                // Chargen-Artikel
                this.ChargenCollection = null;
            }

            switch (_bewegungsart)
            {
                case LagerplatzBuchungBewegungsart.BewegungsartInterneUmbuchung:

                    break;

                case LagerplatzBuchungBewegungsart.BewegungsartManuelleEntnahme:

                    break;

                case LagerplatzBuchungBewegungsart.BewegungsartManuellerZugang:

                    break;
            }
            return true;
        }

        /// <summary>
        /// Methode dient als zusätzliche Berechnungsmethode, um bei Zugriffen gegen die
        /// Office Line LagerEngine die Konsistenz der Daten sicherzustellen.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool calculateInternal()
        {
            switch (_bewegungsart)
            {
                case LagerplatzBuchungBewegungsart.BewegungsartInterneUmbuchung:
                    calculateInternalBewegungsartInterneUmbuchung();
                    break;

                case LagerplatzBuchungBewegungsart.BewegungsartManuelleEntnahme:
                    calculateInternalBewegungsartEntnahmeManuell();
                    break;

                case LagerplatzBuchungBewegungsart.BewegungsartManuellerZugang:
                    calculateInternalBewegungsartZugangManuell();
                    break;
            }
            return true;
        }

        private void calculateInternalBewegungsartZugangManuell()
        {
            // TODO: MEK berechnen, wenn nicht durch Client erfolgt
            if (base.MittlererEKPreis == 0)
            {
                base.MittlererEKPreis = ConversionHelper.ToDecimal(_artikelVariantenItem.MittlererEK);
                base.Einzelpreis = ConversionHelper.ToDecimal(_artikelVariantenItem.MittlererEK);
            }
            if (base.Einstandspreis == 0)
            {
                base.Einstandspreis = base.MengeBasis * base.MittlererEKPreis;
            }
        }

        private void calculateInternalBewegungsartEntnahmeManuell()
        {
        }

        private void calculateInternalBewegungsartInterneUmbuchung()
        {
        }

        private bool saveInternal()
        {
            var transaktionLevel = _mandant.MainDevice.GenericConnection.TransactionLevel;
            bool saveOk = false;

            try
            {
                validateInternal();
                calculateInternal();

                if (transaktionLevel == 0)
                {
                    _mandant.MainDevice.GenericConnection.BeginTransaction();
                }

                if (!LagerJob.SaveJob())
                {
                    throw new LagerJobSaveException("Lagerjob konnte nicht erzeugt werden.");
                }

                switch (_bewegungsart)
                {
                    case LagerplatzBuchungBewegungsart.BewegungsartInterneUmbuchung:
                        saveOk = base.Save(ref _herkunftslagerplatz, ref _ziellagerplatz, _lagerjob);
                        break;

                    case LagerplatzBuchungBewegungsart.BewegungsartManuelleEntnahme:
                        _ziellagerplatz = null;
                        saveOk = base.Save(ref _herkunftslagerplatz, ref _ziellagerplatz, _lagerjob, true, false, true, true, true);
                        break;

                    case LagerplatzBuchungBewegungsart.BewegungsartManuellerZugang:
                        _herkunftslagerplatz = null;
                        saveOk = base.Save(ref _herkunftslagerplatz, ref _ziellagerplatz, _lagerjob);
                        break;
                }

                if (saveOk)
                {
                    if (transaktionLevel == 0)
                    {
                        _mandant.MainDevice.GenericConnection.CommitTransaction();
                    }
                }
                else
                {
                    throw new Exception("Fehler beim Speichern der Lagerbuchung.");
                }

                return saveOk;
            }
            catch (Exception)
            {
                if (transaktionLevel == 0)
                {
                    _mandant.MainDevice.GenericConnection.RollbackTransaction();
                }
                return false;
            }
        }

        private LagerJob createNewLagerjob()
        {
            LagerJob lagerjob = new LagerJob(_mandant, base.ApplikationsDatum, base.MandantenJahr);
            lagerjob.Benutzer = _mandant.Benutzer.Name;
            lagerjob.Erfassungsdatum = base.ApplikationsDatum;
            lagerjob.Memo = "Lagerbuchungen";
            lagerjob.Standardtext = "Lagerbuchungen";
            return lagerjob;
        }

        // So ermitteln Sie überflüssige Aufrufe
        private bool disposedValue = false;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }

                // TODO: Eigenen Zustand freigeben (nicht verwaltete Objekte).
                // TODO: Große Felder auf NULL festlegen.
            }
            this.disposedValue = true;
        }

        #region " IDisposable Support "

        // Dieser Code wird von Visual Basic hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie oben in Dispose(ByVal disposing As Boolean) Bereinigungscode ein.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion " IDisposable Support "
    }
}