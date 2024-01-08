#region Copyright (c) 2002-2018, Sage GmbH

//Copyright (c) 2018, Sage GmbH (http://www.sage.de).
//Alle Rechte vorbehalten.
//Weitergabe und Vervielfältigung dieser Moduls oder von Teilen daraus sind, zu welchem Zweck
//und in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die
//Sage GmbH nicht gestattet. In diesem Modul enthaltene Informationen können
//ohne vorherige Ankündigung geändert werden. Die Module werden im Rahmen des Developer Programms
//den Teilnehmern als As-Is Basis zur Verfügung gestellt und stellen keinen Anspruch auf Vollständigkeit dar.

#endregion Copyright (c) 2002-2018, Sage GmbH

using WEKO.BirdHome.Absatzplanungimport;
using Sagede.OfficeLine.Engine;
using Sagede.OfficeLine.Shared;
using System;
using System.Linq;

namespace WEKO.BirdHome.Absatzplanungimport
{
    public class ImportAbsatzplanung
    {
        private Mandant _mandant;
        private ApplicationStateInfo _errors;
        private ParameterBag _bag;

        /// <summary>
        /// Konstruktor der Klasse
        /// </summary>
        /// <param name="mandant">Mandanten-Objekt</param>
        public ImportAbsatzplanung(Mandant mandant)
        {
            _mandant = mandant;
            initClass();
        }

        /// <summary>
        /// Initialisierung der Klassen-Eigenschaften
        /// </summary>
        private void initClass()
        {
            _errors = new ApplicationStateInfo();
            _bag = new ParameterBag();
            Artikelnummer = string.Empty;
            Monat1 = string.Empty;
            Monat2 = string.Empty;
            Monat3 = string.Empty;
            Monat4 = string.Empty;
            Monat5 = string.Empty;
            Monat6 = string.Empty;
            Monat7 = string.Empty;
        }

        /// <summary>
        /// Artikelnummer
        /// </summary>
        public string Artikelnummer { get; set; }

        /// <summary>
        /// Monat1
        /// </summary>
        public string Monat1 { get; set; }

        /// <summary>
        /// Monat2
        /// </summary>
        public string Monat2 { get; set; }

        /// <summary>
        /// Monat3
        /// </summary>
        public string Monat3 { get; set; }

        /// <summary>
        /// Moant4
        /// </summary>
        public string Monat4 { get; set; }

        /// <summary>
        /// Moant5
        /// </summary>
        public string Monat5 { get; set; }

        /// <summary>
        /// Moant6
        /// </summary>
        public string Monat6 { get; set; }

        /// <summary>
        /// Moant7
        /// </summary>
        public string Monat7 { get; set; }

        /// <summary>
        /// Aktueller Lagerbestand in Sage 100
        /// </summary>
        public decimal Lagerbestand { get; set; }

        /// <summary>
        /// Ziellagerbestand in Sage 100
        /// </summary>
        public decimal Ziellagerbestand { get; set; }

        /// <summary>
        /// Einstandspreis für Zubuchungen
        /// </summary>
        public decimal Einstandspreis { get; set; }

        /// <summary>
        /// Error-Collection
        /// </summary>
        ///
        public ApplicationStateInfo Errors
        {
            get { return _errors; }
            set { _errors = value; }
        }

        /// <summary>
        /// ParameterBag der Klasse
        /// </summary>
        public ParameterBag Bag
        {
            get { return _bag; }
            set { _bag = value; }
        }

        /// <summary>
        /// Load-Methode der Klasse
        /// </summary>
        /// <param name="artikelnummer">Artikelnummer</param>
        /// <returns></returns>
        public bool Load(string artikelnummer)
        {
            try
            {
                var item = _mandant.MainDevice.Entities.Artikel.GetItem(artikelnummer, _mandant.Id);

                if (item == null) throw new DataNotFoundException($"Artikel { artikelnummer } nicht vorhanden.");

                Artikelnummer = item.Artikelnummer;
                return true;
            }
            catch (Exception ex)
            {
                _errors.AppendException(ex);
                return false;
            }
        }

        /// <summary>
        /// Save-Methode der Klasse
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                // TODO: Template noch konfigurierbar machen
                var templateArtikelnummer = "00200050";
                var templateArtikelItem = _mandant.MainDevice.Entities.Artikel.GetItem(templateArtikelnummer, _mandant.Id);
                var artikelItem = _mandant.MainDevice.Entities.Artikel.GetItem(Artikelnummer, _mandant.Id);

                // Validierungen
                if (templateArtikelItem == null)
                {
                    throw new DataNotFoundException($"Template-Artikel {templateArtikelnummer} nicht korrekt definiert.");
                }

               

                // Transaktion beginnen
                _mandant.MainDevice.GenericConnection.BeginTransaction();

              

                if (artikelItem == null)
                {
                    artikelItem = _mandant.MainDevice.Entities.Artikel.CreateItem();

                    // Bei neuen Artikel - Templatewerte vorbelegen
                    // Templatefields
                    artikelItem.Basismengeneinheit = templateArtikelItem.Basismengeneinheit;
                    artikelItem.Lagermengeneinheit = templateArtikelItem.Lagermengeneinheit;
                    artikelItem.Steuerklasse = templateArtikelItem.Steuerklasse;
                    artikelItem.Artikelart = templateArtikelItem.Artikelart;
                    artikelItem.Besteuerungsart = templateArtikelItem.Besteuerungsart;
                    artikelItem.Bewertungssatz = templateArtikelItem.Bewertungssatz;
                    artikelItem.GemeinkostenID = templateArtikelItem.GemeinkostenID;
                    artikelItem.Bezugskostenzuschlag = templateArtikelItem.Bezugskostenzuschlag;
                    artikelItem.IstBestellartikel = templateArtikelItem.IstBestellartikel;
                    artikelItem.IstEinmalartikel = templateArtikelItem.IstEinmalartikel;
                    artikelItem.IstErsatzteil = templateArtikelItem.IstErsatzteil;
                    artikelItem.IstFavorit = templateArtikelItem.IstFavorit;
                    artikelItem.IstFertigungsartikel = templateArtikelItem.IstFertigungsartikel;
                    artikelItem.IstProvisionierbar = templateArtikelItem.IstProvisionierbar;
                    artikelItem.IstRabattfaehig = templateArtikelItem.IstRabattfaehig;
                    artikelItem.IstUnterbaugruppe = templateArtikelItem.IstUnterbaugruppe;
                    artikelItem.IstVerkaufsartikel = templateArtikelItem.IstVerkaufsartikel;
                    artikelItem.IstVerschleissteil = templateArtikelItem.IstVerschleissteil;
                    artikelItem.IstBestellartikel = templateArtikelItem.IstBestellartikel;
                    artikelItem.PlatzID = templateArtikelItem.PlatzID;
                    artikelItem.PreispflegeEK = templateArtikelItem.PreispflegeEK;
                    artikelItem.Rabattgruppe = templateArtikelItem.Rabattgruppe;
                    artikelItem.SachkontoEK = templateArtikelItem.SachkontoEK;
                    artikelItem.SachkontoVK = templateArtikelItem.SachkontoVK;
                    artikelItem.SachkontoWB = templateArtikelItem.SachkontoWB;
                    artikelItem.SachkontoWZ = templateArtikelItem.SachkontoWZ;
                }

                artikelItem.Mandant = _mandant.Id;
                artikelItem.Artikelnummer = Artikelnummer;
               

                var result = artikelItem.TrySave();

                if (result.IsSucceeded)
                {
                    _bag.StringValues["RowVersion"] = BitConverter.ToDouble(artikelItem.Timestamp, 0).ToString();
                }
                else
                {
                    throw new DataUpdateException($"Fehler beim Speichern von Artikel { Artikelnummer }.", result.ExceptionOccurred);
                }

                

                // Transaktion Commit
                _mandant.MainDevice.GenericConnection.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                // Transaktion Rollback
                _mandant.MainDevice.GenericConnection.RollbackTransaction();
                _errors.AppendException(ex);
                return false;
            }
        }

        /// <summary>
        /// Delete-Methode der Klasse
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            try
            {
                var item = _mandant.MainDevice.Entities.Artikel.GetItem(Artikelnummer, _mandant.Id);

                if (item == null)
                {
                    throw new DataNotFoundException($"Artikel { Artikelnummer } nicht vorhanden.");
                }

                var result = item.TryDelete();

                if (result.IsSucceeded)
                {
                    return true;
                }
                else
                {
                    throw new DataUpdateException($"Fehler beim Löschen von Artikel { Artikelnummer }.", result.ExceptionOccurred);
                }
            }
            catch (Exception ex)
            {
                _errors.AppendException(ex);
                return false;
            }
        }
    }
}