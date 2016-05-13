﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Tandlægerne_Smil.Controllers.DbController;

namespace Tandlægerne_Smil.Models
{
    internal class Patient : Global
    {
        public void OpretPatient(TextBox textBoxNavn, TextBox textBoxEfternavn, TextBox textBoxCPR, TextBox textBoxAdresse, TextBox textBoxPostnummer, TextBox textBoxTelefon) // Opret test patient her, denne metode bør slettes
        {
            var patient = new PatientDb // Opret Patient
            {
                Fornavn = textBoxNavn.Text,
                Efternavn = textBoxEfternavn.Text,
                Cpr = textBoxCPR.Text,
                Adresse = textBoxAdresse.Text,
                Postnummer = Convert.ToInt16(textBoxPostnummer.Text),
                Telefon = textBoxTelefon.Text
            };
            bool postNummerCheck = Db.PostnummerDbs.Any(p => p.Postnr == patient.Postnummer); // DETTE ER DET SYGESTE SHIT
            if (!postNummerCheck)
            {
                MessageBox.Show("Postnummer findes ikke i databasen.");
            }
            else
            {
                try
                {
                    Db.PatientDbs.Add(patient); // Tilføj patienten til tabellen
                    LogSqlQuery(); // Udskriv sql-query til konsol
                    Db.SaveChanges(); // Gem ændringerne i db (async gør det i baggrunden vha. en separat tråd)
                    MessageBox.Show("Patient oprettet");
                }
                catch (Exception dbEx) // System.Data.Entity.Validation.DbEntityValidationException dbEx,
                {
                    MessageBox.Show("Fejl i input",
                        "Fejl",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Exception raise = dbEx;
                    MessageBox.Show(dbEx.ToString());
                    throw raise;
                }
            }
        }

        public void RefreshPatientView(ListView listViewPatient)
        {
            var patientList = Db.PatientDbs.ToList();
            var index = 0;
            foreach (var patientDb in patientList)
            {
                ListViewItem lvi = new ListViewItem(patientList[index].Fornavn.Replace(" ", string.Empty));
                lvi.SubItems.Add(patientList[index].Efternavn.Replace(" ", string.Empty));
                lvi.SubItems.Add(patientList[index].Telefon);
                listViewPatient.Items.Add(lvi);
                listViewPatient.Items[index].Group = listViewPatient.Groups[0];
                index++;
            }
        }
    }
}