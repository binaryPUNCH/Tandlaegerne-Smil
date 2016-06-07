﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Tandlægerne_Smil.Controllers;
using Tandlægerne_Smil.Controllers.DbController;
using Tandlægerne_Smil.Models;

namespace Tandlægerne_Smil.Views
{
    public partial class BehandlingAfslut : Form
    {
        private StartForm _startForm;

        private readonly Controller _controller = new Controller(); // Så vores view kan snakke med controllerenPatient P = new Patient();

        private Global _global = new Global();
        private readonly int booking_ID;

        public BehandlingAfslut(int booking_ID, StartForm startForm)
        //modtager booking id fra tidligere valgte booking
        {
            InitializeComponent();
            this.booking_ID = booking_ID; //gemmer dem i de lokale variabler
            this._startForm = startForm; //
        }

        private void button1_Click(object sender, EventArgs e) //FJERN LINJE
        {
            try
            {
                listView_BehandlingsList.SelectedItems[0].Remove();
            }
            catch (Exception)
            {
                MessageBox.Show("Ingen behandling valgt.",
                                "Advarsel",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void BehandlingAfslut_Load(object sender, EventArgs e) //LOAD
        {
            LoadBehandlingerTilCombobox();
            LoadPatientInfo();
            LoadTidligereBehandlinger();
        }

        public void LoadTidligereBehandlinger()
        {
            using (var db = new smildb())
            {
                var behandlingslinjer = db.BehandlingslinjerDbs
                       .Where(b => b.BookingId == booking_ID) // Kun den valgte dag
                       .ToList();

                foreach (var linje in behandlingslinjer)
                {
                    ListViewItem list = new ListViewItem(linje.BehandlingDb.Navn);
                    list.SubItems.Add(linje.BehandlingDb.Pris.ToString());
                    listView_BehandlingsList.Items.Add(list);
                }
            }
        }

        public void LoadBehandlingerTilCombobox()
        {
            using (var db = new smildb())
            {
                var behandlinger = db.BehandlingDbs.ToList();

                foreach (var item in behandlinger)
                {
                    comboBox_Behandlinger.Items.Add(item.Navn);
                }
            }
        }

        public void LoadPatientInfo()
        {
            var booking = _global.Db.BookingDbs.FirstOrDefault(b => b.BookingId == booking_ID);
            var patient = _global.Db.PatientDbs.FirstOrDefault(p => p.PatientId == booking.PatientId);
            var behandlingsrum = _global.Db.BehandlingsrumDbs.ToList();
            var ansat = _global.Db.AnsatDbs.ToList();

            textBox_PatientNr.Text = patient.PatientId.ToString();
            textBox_Fornavn.Text = patient.Fornavn;
            textBox_Efternanv.Text = patient.Efternavn;
            textBox_Cpr.Text = patient.Cpr;
            textBox_Adresse.Text = patient.Adresse;
            textBox_Postnr.Text = patient.Postnummer.ToString();
            textBox_Tlfnr.Text = patient.Telefon;
        }

        private void button_TiljøjBehandling_Click(object sender, EventArgs e) //ADD behandling
        {
            var behandling = _global.Db.BehandlingDbs.FirstOrDefault(b => b.Navn == comboBox_Behandlinger.Text);

            ListViewItem lvi = new ListViewItem(behandling.Navn);
            lvi.SubItems.Add(behandling.Pris.ToString());

            listView_BehandlingsList.Items.Add(lvi);
        }

        private void button_GemOgFaktur_Click(object sender, EventArgs e)
        //knappen der afslutter behandlingen og gemmer faktura
        {
            if (listView_BehandlingsList.Items.Count == 0)
            {
                // Beder brugern om at tilføje behandlings linjer før behandlingen kan afsluttes
                MessageBox.Show("Tilføje nogle behandlingslinjer",
                    "Fejl",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                //OpdaterBehandlinger();
                /*Kalder metoden der refresher behandlings listen på valgte
                paient og gemmer dem så fakturaen nu kan blive dannet */
                using (var db = new smildb())
                {
                    var booking = db.BookingDbs.FirstOrDefault(b => b.BookingId == booking_ID);
                    var faktura = new FakturaDb
                    {
                        PatientId = booking.PatientId,
                        Betalt = false,
                        BookingId = booking_ID,
                        FakturaDato = DateTime.Now
                    };
                    booking.Ankommet = false;
                    booking.Faktureret = true;

                    var nyfaktura = db.FakturaDbs.Add(faktura);
                    db.SaveChanges();
                    List<string> navnList = listView_BehandlingsList.Items.Cast<ListViewItem>()
                                            .Select(item => item.Text)
                                            .ToList();

                    var bList = db.BehandlingslinjerDbs.ToList();
                    foreach (var test in bList)
                    {
                        if (test.BookingId == booking_ID) // TODO: for eller foreach (pas på i)
                        {
                            foreach (var navn in navnList)
                            {
                                if (test.BehandlingDb.Navn == navn)
                                    test.FakturaId = nyfaktura.FakturaId;
                            }
                        }
                    }

                    //foreach (var behandlingsNavn in )
                    //{
                    //var behandlingsNavn = listView_BehandlingsList.Items[0].Text;
                    //var linje = db.BehandlingslinjerDbs.FirstOrDefault(b => b.BookingId == booking_ID && b.BehandlingDb.Navn == TESTlIST);
                    //linje.FakturaId = nyfaktura.FakturaId;
                    //}
                    // var linje = db.BehandlingslinjerDbs.FirstOrDefault(b => b.BookingId == booking_ID && b.BehandlingDb.Navn == behandlingsNavn.T);

                    //_controller.Global.UdskrivSqlTilKonsol();
                    db.SaveChanges();

                    //Ekstra information til brugeren iform af: et navn og hvor man kan finde den ny oprettet faktura
                    string Afslutning = "Behandlingen af " + (textBox_Fornavn.Text + " " + textBox_Efternanv.Text) +
                                        " er nu afsluttet";
                    Afslutning += Environment.NewLine;
                    Afslutning += "Patientens Faktura kan findes under Patient ID: " + textBox_PatientNr.Text;
                    MessageBox.Show(Afslutning, "Færdig!");

                    _startForm.RefreshVenteværelseView();
                    //Refresh af veenteværelsets listview
                    _startForm.RefreshBookingView();
                    //refresher bookingview
                    this.Close();
                    //lukker "pop up" formen ned igen
                }
            }
        }

        public void OpdaterBehandlinger()
        {
            //Sletter tidligere behandlinger
            using (var db = new smildb())
            {
            }
        }

        private void textBox_Efternanv_TextChanged(object sender, EventArgs e)
        {
        }
    }
}