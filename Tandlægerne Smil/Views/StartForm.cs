﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tandlægerne_Smil.Controllers;
using Tandlægerne_Smil.Controllers.DbController;
using Tandlægerne_Smil.Models;

namespace Tandlægerne_Smil.Views
{
    public partial class StartForm : Form
    {
        #region Console-Debugger

        [DllImport("kernel32.dll")] // Næste 6 linjer er for at skjule konsollen
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SwHide = 0;
        private const int SwShow = 5;

        #endregion Console-Debugger

        //test
        //private Global _global = new Global();

        private readonly Controller _controller = new Controller(); // Så vores view kan snakke med controlleren

        public StartForm()
        {
            InitializeComponent();
        }

        private void opretTestPatient_Click(object sender, EventArgs e)
        {
            RefreshPatientView();
        }

        public void RefreshPatientView()
        {
            listViewPatienter.Items.Clear();

            //var patientList = Db.PatientDbs.ToList();
            //var Db2 = new smildb();
            using (var db = new smildb())
            {
                var patientList = db.PatientDbs.ToList();

                // Db.Entry(Db.PatientDbs).Reload();
                foreach (var patientDb in patientList)
                {
                    ListViewItem lvi = new ListViewItem(patientDb.Fornavn.Replace(" ", string.Empty));
                    lvi.SubItems.Add(patientDb.Efternavn.Replace(" ", string.Empty));
                    lvi.SubItems.Add(patientDb.Telefon);
                    lvi.SubItems.Add(patientDb.PatientId.ToString());
                    lvi.Group = listViewPatienter.Groups[0];
                    listViewPatienter.Items.Add(lvi);
                }
            }
        }

        private void RefreshBookingView()
        {
            listViewDagensProgram.Items.Clear();
            using (var db = new smildb())
            {
                var dagensBookinger = db.BookingDbs
                    .Include(b => b.BehandlingslinjerDbs)
                    .Where(b => b.Tidspunkt.Day == dateTimePicker.Value.Day) // Kun den valgte dag
                    .OrderBy(b => b.Tidspunkt) // Sortere dem i rækkefølge
                    .ToList();
                foreach (var booking in dagensBookinger)
                {
                    ListViewItem list = new ListViewItem(booking.Tidspunkt.Hour.ToString() + ":" + booking.Tidspunkt.Minute.ToString());
                    var behandlinger = db.BehandlingDbs.Where(b => b.BehandlingslinjerDb.BookingId == booking.BookingId).ToList();
                    var behandlingString = "";
                    var totalAnslåetTid = 0;
                    if (behandlinger.Count > 0) // Hvis der overhovedet er nogle behandlinger tilknyttede bookingen, så man ikke får fejl
                    {
                        behandlingString = behandlinger[0].Navn;
                        totalAnslåetTid = behandlinger[0].AnslåetTid;
                    }
                    foreach (var behandling in behandlinger.Skip(1)) // Spring den første over, og tilføje alle behandlinger (hvis der er nogle)
                    {
                        behandlingString += ", " + behandling.Navn;
                        totalAnslåetTid += behandling.AnslåetTid;
                    }
                    list.SubItems.Add(booking.AnsatDb.Fornavn + " " + booking.AnsatDb.Efternavn);
                    list.SubItems.Add(totalAnslåetTid.ToString());
                    list.SubItems.Add(booking.BehandlingsrumDb.RumNavn);
                    list.SubItems.Add(booking.PatientDb.Fornavn + " " + booking.PatientDb.Efternavn);
                    list.SubItems.Add(behandlingString);
                    listViewDagensProgram.Items.Add(list);
                }

                //var bookingList = db.BookingDbs;
                //var lokaleList = db.BehandlingsrumDbs;
                //var lægeList = db.AnsatDbs;
                //var patientList = db.PatientDbs;
                //var Behandling = db.BehandlingDbs;
                //var behandlingslinje = db.BehandlingslinjerDbs;
                //var join = from b in bookingList
                //           join br in lokaleList
                //               on b.LokaleId equals br.RumId
                //           join a in lægeList
                //               on b.LægeId equals a.AnsatId
                //           join p in patientList
                //               on b.PatientId equals p.PatientId
                //           join bl in behandlingslinje
                //            on b.BookingId equals bl.BookingId
                //           join bh in Behandling
                //           on bl.BehandlingId equals bh.BehandlingId
                //           select new
                //           {
                //               b.Tidspunkt,
                //               br.RumNavn,
                //               a.Fornavn,
                //               patientnavn = p.Fornavn,
                //               behandlingsNavn = bh.Navn,
                //               patrientid = p.PatientId,
                //               bookingId = b.BookingId
                //           };
                //var sortQuery = (from r in join
                //                 where (r.Tidspunkt.Day == dateTimePicker.Value.Day)
                //                 select r).ToList();
                //sortQuery.GroupBy(i => i.bookingId).Select(g => new {Id = g.Key});
                //list.GroupBy(i => i.Id).Select(g => new { Id = g.Key, Total = g.Sum(i => i.Quantity) });
                //var dagensBookinger = (from r in sortQuery
                //                       where
                //    )
                //var behandlingerString = "";
                //var index = 0;
                //foreach (var behandling in sortQuery)
                //{
                //    behandlingerString += behandling.behandlingsNavn;
                //    behandlingerString += ", ";
                //}
                //foreach (var r in sortQuery)
                //{
                //    ListViewItem list = new ListViewItem(r.Tidspunkt.ToString());
                //    list.SubItems.Add(r.RumNavn);
                //    list.SubItems.Add(r.Fornavn);
                //    list.SubItems.Add(r.patientnavn);
                //    //list.SubItems.Add(behandlingerString);
                //    listViewDagensProgram.Items.Add(list);
                //}
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshBookingView();
        }

        private void listViewDagensProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void buttonOpretPatient_Click(object sender, EventArgs e)
        {
            PatientOpret OP = new PatientOpret(this);
            OP.Show();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void omToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"Dette program er udviklet til Tandlægerne Smil af:
Nikolaj Kiil, Kasper Skov, Patrick Korsgaard & Paul Wittig", @"Version 0.0.1");
        }

        private void AfslutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void StartForm_Load(object sender, EventArgs e)
        {
            RefreshPatientView();
            //new Task(RefreshBookingView).Start();
            RefreshBookingView();
        }

        private void VisKonsolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var handle = GetConsoleWindow();
            if (gemVisKonsolToolStripMenuItem.Checked)
            {
                ShowWindow(handle, SwHide);
                gemVisKonsolToolStripMenuItem.Checked = false;
            }
            else
            {
                ShowWindow(handle, SwShow);
                gemVisKonsolToolStripMenuItem.Checked = true;
            }
        }

        private void buttonUdskrivDagensBookninger_Click(object sender, EventArgs e)
        {
            _controller.Book.GemDagensProgram(this.dateTimePicker);
        }

        private void buttonRedigerePatient_Click(object sender, EventArgs e)
        {
            try
            {
                int PatientID = Convert.ToInt32(listViewPatienter.SelectedItems[0].SubItems[3].Text);
                PatientRedigere PR = new PatientRedigere(PatientID, this);
                PR.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Vælg en patient",
                    "Fejl",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        //*******************************************FAKTURA**********************************************

        #region faktura

        private void tabLiveView_Click(object sender, EventArgs e)
        {
        }

        private void udskrivFaktura_Click(object sender, EventArgs e)
        {
            try
            {
                string patientnavn = listView_Faktura.SelectedItems[0].SubItems[1].Text;
                int patientid = int.Parse(listView_Faktura.SelectedItems[0].SubItems[2].Text);
                int fakturaNR = int.Parse(listView_Faktura.SelectedItems[0].SubItems[0].Text);
                _controller.Faktura.UdskrivFaktura(fakturaNR, patientid, patientnavn);
            }
            catch (Exception)
            {
                MessageBox.Show("Vælg en Faktura",
                     "Fejl",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error);
            }
        }

        private void button1_Click_1(object sender, EventArgs e) //Søg
        {
            try
            {
                listView_Faktura.Items.Clear();
                _controller.Faktura.HentFaktura(int.Parse(textBox_PatientID.Text), listView_Faktura);
            }
            catch
            {
                MessageBox.Show("Fejl i Patient ID prøv igen");
            }
        }

        private void flowLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {
        }

        private void listView_Faktura_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void button_VisAllePatienter_Click(object sender, EventArgs e)
        {
            int idNummer = 0;
            try
            {
                listView_Faktura.Items.Clear();
                foreach (var item in listView_Faktura.Items.ToString())
                {
                    _controller.Faktura.HentFaktura(idNummer, listView_Faktura);
                    idNummer++;
                }
            }
            catch
            {
                MessageBox.Show("Kunne ikke hente data");
            }
        }

        private void tabFaktura_Click(object sender, EventArgs e)
        {
        }

        private void button_VisAlleFolk_Click(object sender, EventArgs e)
        {
            FakturaPatienter F = new FakturaPatienter(this);
            F.Show();
        }

        private void listViewPatienter_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void listViewVenteværelse_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void textBox_PatientID_TextChanged(object sender, EventArgs e)
        {
        }

        private void button_VisDetaljer_Click(object sender, EventArgs e)
        {
            try //viser faktura detaljer
            {
                _controller.Faktura.HentOplysningerPåValgteFakatura(
                int.Parse(listView_Faktura.SelectedItems[0].SubItems[0].Text), listView_FakturaDetaljer);
                //Sender faktura nr. + listviewet faktura detaljer så vi kan tilføje linjer i faktura klassen (Y)
            }
            catch (Exception)
            {
                MessageBox.Show("Vælg en Faktura",
                     "Fejl",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error);
            }
        }

        #endregion faktura

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            RefreshBookingView();
        }

        private void buttonOpretBooking_Click(object sender, EventArgs e)
        {
            try
            {
                int PatientID = Convert.ToInt32(listViewPatienter.SelectedItems[0].SubItems[3].Text);
                BookingOpretRedigere bookingOpretRedigere = new BookingOpretRedigere(PatientID, this);
                bookingOpretRedigere.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Vælg en patient",
                     "Fejl",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
        }
    }
}