﻿/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Account.cs
 * The form used by the user to set the FTP/SFTP account info
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using FTPboxLib;
using System.Diagnostics;

namespace FTPbox.Forms
{
    public partial class Account : Form
    {
        
        Client client;

        static string AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);      

        public Account()
        {
            InitializeComponent();
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            Log.Write(l.Debug, AppPath);
            bool ftporsftp;
            bool ftps = false;
            bool ftpes = true;

            if (cMode.SelectedIndex == 0)
            {
                ftporsftp = true;
                if (cEncryption.SelectedIndex != 0)
                {
                    ftps = true;
                    if (cEncryption.SelectedIndex == 1)
                    {
                        Log.Write(l.Debug, "FTPS Explicit");
                        ftpes = true;
                    }
                    else
                    {
                        Log.Write(l.Debug, "FTPS Implicit");
                        ftpes = false;
                    }
                }
            }
            else
            {
                ftporsftp = false;
            }

            Profile.AddAccount(tHost.Text, tUsername.Text, tPass.Text, Convert.ToInt32(nPort.Value));
            if (ftporsftp && ftps)
                Profile.Protocol = FtpProtocol.FTPS;
            else if (ftporsftp)
                Profile.Protocol = FtpProtocol.FTP;
            else
                Profile.Protocol = FtpProtocol.SFTP;

            if (!ftps)
                Profile.FtpsInvokeMethod = FtpsMethod.None;
            else if (ftpes)
                Profile.FtpsInvokeMethod = FtpsMethod.Explicit;
            else
                Profile.FtpsInvokeMethod = FtpsMethod.Implicit;

            try
            {
                ((fMain)this.Tag).SetTray(fMain.MessageType.Connecting);

                client = new Client();
                client.Connect();
                Log.Write(l.Debug, "Connected: {0}", client.isConnected);

                client.Disconnect();

                Profile.AddAccount(tHost.Text, tUsername.Text, tPass.Text, Convert.ToInt32(nPort.Value));

                if (ftporsftp && ftps)
                    Profile.Protocol = FtpProtocol.FTPS;
                else if (ftporsftp)
                    Profile.Protocol = FtpProtocol.FTP;
                else
                    Profile.Protocol = FtpProtocol.SFTP;

                if (!ftps)
                    Profile.FtpsInvokeMethod = FtpsMethod.None;
                else if (ftpes)
                    Profile.FtpsInvokeMethod = FtpsMethod.Explicit;
                else
                    Profile.FtpsInvokeMethod = FtpsMethod.Implicit;

                ((fMain)this.Tag).SetTray(fMain.MessageType.Ready);

                ((fMain)this.Tag).SaveProfile();
                //Paths fnewdir = new Paths();
                //fnewdir.Tag = this.Tag;
                this.Hide();
                //fnewdir.ShowDialog();
                
                //this.Close();

            }
            catch (Exception ex)
            {
                ((fMain)this.Tag).SetTray(fMain.MessageType.Nothing);

                MessageBox.Show("Could not connect to FTP server. Check your account details and try again."
                    + Environment.NewLine + " Error message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (ftporsftp)
                   Log.Write(l.Debug, "Connected: {0}", client.isConnected);
            }

            if (ftporsftp)
                Log.Write(l.Debug, client.WorkingDirectory); 
        }            

        private void Account_Load(object sender, EventArgs e)
        {
            cEncryption.SelectedIndex = 0;
            cMode.SelectedIndex = 0;
            Set_Language(((fMain)this.Tag).lang());
        }

        private void cMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cMode.SelectedIndex != 1)
            {
                nPort.Value = 21;
                cEncryption.Enabled = true;
            }
            else
            {
                nPort.Value = 22;
                cEncryption.SelectedIndex = 0;
                cEncryption.Enabled = false;
            }
        }

        /// <summary>
        /// set the language of the form's labels etc
        /// </summary>
        /// <param name="lan"></param>
        private void Set_Language(string lan)
        {
            Log.Write(l.Info, "Setting lang: {0}", lan);
            this.Text = "FTPbox | " + ((fMain)this.Tag).languages.Get(lan + "/new_account/new_ftp", "New FTP Account");
            gDetails.Text = ((fMain)this.Tag).languages.Get(lan + "/new_account/login_details", "FTP login details");
            labMode.Text = ((fMain)this.Tag).languages.Get(lan + "/main_form/mode", "Protocol") + ":";
            labEncryption.Text = ((fMain)this.Tag).languages.Get(lan + "/new_account/encryption", "Encryption") + ":";
            labHost.Text = ((fMain)this.Tag).languages.Get(lan + "/main_form/host", "Host") + ":";
            labPort.Text = ((fMain)this.Tag).languages.Get(lan + "/main_form/port", "Port") + ":";
            labUN.Text = ((fMain)this.Tag).languages.Get(lan + "/main_form/username", "Username") + ":";
            labPass.Text = ((fMain)this.Tag).languages.Get(lan + "/main_form/password", "Password") + ":";
            bDone.Text = ((fMain)this.Tag).languages.Get(lan + "/new_account/done", "Done");
        }

        private void Account_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                Log.Write(l.Info, "Killing the process.....");
                //System.Threading.Thread.Sleep(1000);
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }
        }

        public void KillTheProcess()
        {
            ((fMain)this.Tag).ExitedFromTray = true;
            Log.Write(l.Info, "Killing the process...");
            try
            {
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }
            catch
            {
                Application.Exit();
            }
        }
    }
}
