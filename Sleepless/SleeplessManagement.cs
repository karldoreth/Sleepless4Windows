using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;

namespace Sleepless
{
    class SleeplessManagement
    {
        //Globale Objekte
        Timer MyTimer;
        NotifyIcon MyTray;

        public SleeplessManagement()
        {
            inittray();

            MyTimer = new Timer();
            MyTimer.Interval = 5000;
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();
        }
        
        private void MyTimer_Tick(object sender, EventArgs e)
        {
            CountMonitors();
        }

        //TraySteuerung
        
        public void inittray()
        {
            //Tray Icon vorbereiten
            MyTray = new NotifyIcon();
            MyTray.Text = "Sleepless Application";
            MyTray.Icon = new Icon("Grey.ico");
            MyTray.Visible = true;

            //Aktion für Linksklick
            //MyTray.Click += CountMonitors;

            //Context Menü für Funktionen
            ContextMenu CM = new ContextMenu();

            CM.MenuItems.Add("Close", ExitClick);
            CM.MenuItems.Add("Force Standby", StandbyClick);

            MyTray.ContextMenu = CM;
        }

        private void ExitClick(Object sender, EventArgs e)
        {
            MyTray.Visible = false;
            Application.Exit();
        }

        private static void StandbyClick(Object sender, EventArgs e)
        {
            var psi = new ProcessStartInfo("rundll32.exe", "powrprof.dll,SetSuspendState");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        void CountMonitors()
        {
            int AnzahlMonitore = 0;
            string query = "select * from WmiMonitorBasicDisplayParams";
            using (var wmiSearcher = new ManagementObjectSearcher("\\root\\wmi", query))
            {
                var results = wmiSearcher.Get();
                AnzahlMonitore = results.Count;
            }
            if (AnzahlMonitore > 1)
            {
                //Console.WriteLine("Sleepless Mode");
                SetEnergyScheme(0);
                MyTray.Text = "Mode: Sleepless!";
                MyTray.Icon = new Icon("Red.ico");
            }
            else
            {
                //Console.WriteLine("Sleep Mode");
                SetEnergyScheme(1);
                MyTray.Text = "Mode: Suspend!";
                MyTray.Icon = new Icon("Green.ico");
            }
        }

        static void SetEnergyScheme(int TheValue)
        {
            //string EnergieSchema = "381b4222-f694-41f0-9685-ff5bb260df2e"; //Ausbalanciert
            //string Untergruppe = "4f971e89-eebd-4455-a8de-9e59040e7347"; //Netzschalter und Zuklappen
            //string Einstellung = "5ca83367-6e45-459f-a27b-476b1d01c936"; //Zuklappen
            //string Wert = TheValue.ToString(); //0 = Nichts Unternehmen, 1 = Energie Sparen
            //string Argumente = String.Format("/SETACVALUEINDEX {0} {1} {2} {3}", EnergieSchema, Untergruppe, Einstellung, Wert); //SETDCVALUEINDEX regelt nur Wechselstromberieb

            string Argumente = String.Format("/setacvalueindex scheme_current sub_buttons lidaction {0}", TheValue);
            var psi = new ProcessStartInfo("powercfg", Argumente);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);

            string Argumente2 = String.Format("/setactive {0}", "scheme_current"); 

            var psi2 = new ProcessStartInfo("powercfg", Argumente2);
            psi2.CreateNoWindow = true;
            psi2.UseShellExecute = false;
            Process.Start(psi2);
        }
    }
}
