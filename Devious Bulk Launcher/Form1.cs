﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace Devious_Bulk_Launcher
{
    public partial class Form1 : Form
    {
        //Declare variables
        private Thread Rows_Selected_Thread;
        private ThreadStart Rows_Selected_Thread_Start;
        private Thread Delayed_Launch_Thread;
        private ThreadStart Delayed_Launch_Thread_Start;
        private List<string> Client_Start_Parameters;
        public static string Client_Executable_Directory;
        public static string Theme;
        public static string Client_Launch_Seconds;
        public static bool Refresh_UI;
		
        public Form1()
        {
            InitializeComponent();

            //Set initial values
            Rows_Selected_Thread_Start = new ThreadStart(Update_Rows_Selected);
            Rows_Selected_Thread = new Thread(Rows_Selected_Thread_Start);
            Client_Executable_Directory = "";
            Theme = "";
            Client_Launch_Seconds = "";
            Refresh_UI = false;

            //Set events
            this.FormClosing += Form_Closing;
            this.Button_Uncheck_All.Click += Button_Uncheck_All_Click;
            this.Button_Settings.Click += Button_Settings_Click;
            this.Button_Add.Click += Button_Add_Click;
            this.Button_Launch.Click += Button_Launch_Click;
            this.Button_Remove.Click += Button_Remove_Click;
            this.Button_Save_Config.Click += Button_Save_Config_Click;

            //Load settings file
            Load_Settings_File();

            //Load configuration file
            Load_Configuration_File();

            //Start Rows Selected thread
            Rows_Selected_Thread.Start();
        }

        private void Update_Rows_Selected()
        {
            //Declare variable
            int Checked_Count;

            //Set initial value
            Checked_Count = 0;

            //Repeat process every 0.5 seconds
            while (true)
            {
                try
                {
                    //Reset Checked Count
                    Checked_Count = 0;

                    //Loop through each row and calculate total number of checked rows
                    foreach (DataGridViewRow Grid_Row in Grid_View.Rows)
                    {
                        if (Grid_Row.Cells[0].GetEditedFormattedValue(Grid_Row.Index, DataGridViewDataErrorContexts.Formatting) != null)
                        {
                            if ((bool)Grid_Row.Cells[0].GetEditedFormattedValue(Grid_Row.Index, DataGridViewDataErrorContexts.Formatting) == true)
                            {Checked_Count++;}
                        }
                    }

                    //Update label with new count
                    Invoke(new Action(() => { Label_Rows_Checked.Text = "Rows selected: " + Checked_Count.ToString(); }));

                    //Suspend thread
                    System.Threading.Thread.Sleep(500);
                }
                catch(Exception Exception){}
            }
        }

        private void Load_Configuration_File()
        {
            //Declare variable
            string[] Row;

            //Check for configuration file existence
            if (System.IO.File.Exists("Devious_Bulk_Launcher_Configuration.txt") == true)
            {
                //Loop through each line in the configuration file
                foreach (string Line in System.IO.File.ReadAllLines("Devious_Bulk_Launcher_Configuration.txt"))
                {
                    //Validation checks
                    if (Line.Trim().Length > 0 && Line.Contains("|"))
                    {
                        //Split line by delimiter
                        Row = Line.Split(new string[] { "|||||" }, StringSplitOptions.None);

                        //Append line values to the data grid view
                        Grid_View.Rows.Add(false, Row[0], Row[1], Row[2], Row[3], Row[4], Row[5], Row[6], Row[7]);
                    }

                }

            }
            
        }

        private void Load_Settings_File()
        {
            //Declare variable
            string[] Settings_File_Content;

            //Load/Create settings
            if (System.IO.File.Exists("Devious_Bulk_Launcher_Settings.txt") == true)
            {
                //Get user settings file
                Settings_File_Content = System.IO.File.ReadAllText("Devious_Bulk_Launcher_Settings.txt").Split(new string[]{"|||||"}, StringSplitOptions.None);

                //Assign user settings
                Client_Executable_Directory = Settings_File_Content[0].Trim();
                Theme = Settings_File_Content[1].Trim();
                Client_Launch_Seconds = Settings_File_Content[2].Trim();
            }
            else if (System.IO.File.Exists("Devious_Bulk_Launcher_Settings.txt") == false)
            {
                //Create and append default settings to new file
                System.IO.File.AppendAllText("Devious_Bulk_Launcher_Settings.txt", "|||||Dark|||||45|||||");

                //Assign default settings
                Client_Executable_Directory = "";
                Theme = "Dark";
                Client_Launch_Seconds = "45";
            }
        }

        private void Form_Closing(object sender, FormClosingEventArgs Form_Closing_Event_Args)
        {
            //Declare variable
            DialogResult Dialog_Result;

            //Display message box to user and capture input
            Dialog_Result = MessageBox.Show("Would you like to save your configuration file?", "Application Exit", MessageBoxButtons.YesNoCancel);

            //Save settings
            Save_Settings();

            //Determine action based on captured input
            if (Dialog_Result == DialogResult.Yes)
            {
                //Save configuration
                Button_Save_Config_Click(null, null);

                //Stop threads
                Rows_Selected_Thread.Abort();
                try{Delayed_Launch_Thread.Abort();}catch(Exception Exception){}

                //Display message box to user notifying of successful save
                MessageBox.Show("Configuration file saved successfully!", "Application Exit", MessageBoxButtons.OK);
            }
            else if (Dialog_Result == DialogResult.Cancel)
            {
                //Cancel application exit
                Form_Closing_Event_Args.Cancel = true;
            }
        }

        private void Button_Uncheck_All_Click(object sender, EventArgs e)
        {
            //Loop through each row and uncheck all
            foreach (DataGridViewRow Grid_Row in Grid_View.Rows)
            {Grid_Row.Cells[0].Value = false;}
        }

        private void Button_Save_Config_Click(object sender, EventArgs e)
        {
            //Declare variable
            string Line_To_Append;

            //Set initial value
            Line_To_Append = "";

            //Check if Configuration file exists
            if (System.IO.File.Exists("Devious_Bulk_Launcher_Configuration.txt") == true)
            {
                //Delete file
                System.IO.File.Delete("Devious_Bulk_Launcher_Configuration.txt");
            }

            //Loop through each row
            foreach (DataGridViewRow Grid_Row in Grid_View.Rows)
            {
                //Attempt to append Configuration file
                try
                {
                    //If the row is blank remove it and if not we append it to the Configuration file
                    if (Grid_Row.Cells[0].Value == null && Grid_Row.Cells[1].Value == null && Grid_Row.Cells[2].Value == null && Grid_Row.Cells[3].Value == null && Grid_Row.Cells[4].Value == null && Grid_Row.Cells[5].Value == null && Grid_Row.Cells[6].Value == null && Grid_Row.Cells[7].Value == null && Grid_Row.Cells[8].Value == null)
                    {
                        //Remove row
                        Grid_View.Rows.RemoveAt(Grid_Row.Index);
                    }
                    else
                    {
                        //Concatenate Line to append
                        for (int Loop = 1; Loop <= 8; Loop++)
                        {
                            if (Loop == 8)
                            {
                                if (Grid_Row.Cells[Loop].Value != null)
                                {Line_To_Append += Grid_Row.Cells[Loop].Value.ToString().Replace("|", "").Trim();}
                            }
                            else
                            {
                                if (Grid_Row.Cells[Loop].Value != null)
                                {Line_To_Append += Grid_Row.Cells[Loop].Value.ToString().Replace("|", "").Trim() + "|||||";}
                                else
                                {Line_To_Append += "|||||";}
                            }
                        }    

                        //Append line to Configuration file
                        System.IO.File.AppendAllText("Devious_Bulk_Launcher_Configuration.txt", Line_To_Append + Environment.NewLine);

                        //Reset Line to append
                        Line_To_Append = "";
                    }

                }
                catch (Exception Exception){}
            }

            //Validate this is a "Save Config" button click
            if (e != null)
            {
                //Notify user that configuration file has been saved
                MessageBox.Show("Configuration file saved!");
            }

        }

        public static void Save_Settings()
        {
            //Check if settings file exists
            if (System.IO.File.Exists("Devious_Bulk_Launcher_Settings.txt") == true)
            {
                //Delete file
                System.IO.File.Delete("Devious_Bulk_Launcher_Settings.txt");
            }

            //Create and append settings to new file
            System.IO.File.AppendAllText("Devious_Bulk_Launcher_Settings.txt", Client_Executable_Directory + "|||||" + Theme + "|||||" + Client_Launch_Seconds + "|||||");
        }

        private void Button_Add_Click(object sender, EventArgs e)
        {
            //Add a new row to the data grid view
            Grid_View.Rows.Add();            
        }

        private void Button_Launch_Click(object sender, EventArgs e)
        {
            //Declare variables
            List<string> Client_Start_Parameters;
            string Concatenated_Parameters;
            DialogResult Dialog_Result;

            //Set initial values
            Client_Start_Parameters = new List<string>();
            Concatenated_Parameters = "";

            //Return if the client executable path has not been set
            if (Client_Executable_Directory == "")
            {MessageBox.Show("Please set client executable directory in settings!");return;}

            //Loop through each row
            foreach (DataGridViewRow Grid_Row in Grid_View.Rows)
            {
                //Check if the select field is checked and start process if so
                try
                {
                    //Validation checks
                    if (Grid_Row.Cells[0].Value != null)
                    {
                        if ((bool)Grid_Row.Cells[0].Value == true && Grid_Row.Cells[1].Value.ToString().Trim() != "" && Grid_Row.Cells[2].Value.ToString().Trim() != "")
                        {
                            //Login
                            Concatenated_Parameters += "-login " + Grid_Row.Cells[1].Value.ToString() + ":" + Grid_Row.Cells[2].Value.ToString() + " ";

                            //Proxy
                            if (Grid_Row.Cells[3].Value.ToString().Trim() != "" && Grid_Row.Cells[4].Value.ToString().Trim() != "")
                            {
                                if (Grid_Row.Cells[5].Value.ToString().Trim() != "" && Grid_Row.Cells[6].Value.ToString().Trim() != "")
                                {
                                    //IP/Port/Username/Password
                                    Concatenated_Parameters += "-proxy " + Grid_Row.Cells[3].Value.ToString() + ":" + Grid_Row.Cells[4].Value.ToString() + ":" + Grid_Row.Cells[5].Value.ToString() + ":" + Grid_Row.Cells[6].Value.ToString() + " ";
                                }
                                else
                                {
                                    //IP/Port
                                    Concatenated_Parameters += "-proxy " + Grid_Row.Cells[3].Value.ToString() + ":" + Grid_Row.Cells[4].Value.ToString() + " ";
                                }
                            }

                            //World #
                            if (Grid_Row.Cells[7].Value.ToString().Trim() != "")
                            {Concatenated_Parameters += "-world " + Grid_Row.Cells[7].Value.ToString() + " ";}

                            //Start process dynamically with given parameters
                            Client_Start_Parameters.Add(@"/C java -jar """ + Client_Executable_Directory + @""" " + Concatenated_Parameters.Trim());

                            //Reset
                            Concatenated_Parameters = "";
                        }
                    }
                }
                catch(Exception Exception){}                
            }

            //Validate 1 or more accounts have been selected
            if (Client_Start_Parameters.Count > 0)
            {
                //Display message box to user and capture input depending on # of selections
                if (Client_Start_Parameters.Count > 1)
                {Dialog_Result = MessageBox.Show(Client_Start_Parameters.Count + " accounts have been selected. Are you sure you would like to launch?", "Application Launch", MessageBoxButtons.YesNo);}
                else
                {Dialog_Result = MessageBox.Show(Client_Start_Parameters.Count + " account has been selected. Are you sure you would like to launch?", "Application Launch", MessageBoxButtons.YesNo);}

                //Start Delayed Launch Thread if we have received confirmation from user
                if (Dialog_Result == DialogResult.Yes)
                {
                    //Inititalize thread
                    Delayed_Launch_Thread_Start = new ThreadStart(() => Delayed_Launch(Client_Start_Parameters));
                    Delayed_Launch_Thread = new Thread(Delayed_Launch_Thread_Start);

                    //Start Delayed Launch Thread
                    Delayed_Launch_Thread.Start();
                }
            }
            else
            {MessageBox.Show("Please select 1 or more accounts before attempting to launch.");}

        }

        private void Delayed_Launch(List<string> Client_Start_Parameters)
        {
            //Loop through each Start Parameter
            foreach (string Start_Parameter in Client_Start_Parameters)
            {
                //Start process with given parameters
                System.Diagnostics.Process.Start("cmd.exe", Start_Parameter);

                //Dynamic delay(seconds)
                System.Threading.Thread.Sleep(1000 * Convert.ToInt32(Client_Launch_Seconds));
            }
        }

        private void Button_Remove_Click(object sender, EventArgs e)
        {
            //Loop through each row in the data grid view
            foreach (DataGridViewRow Grid_Row in Grid_View.Rows)
            {
                //Validate the row's checkbox is not null
                if (Grid_Row.Cells[0].Value != null)
                {
                    //Validate the row's checkbox is checked
                    if ((bool)Grid_Row.Cells[0].Value == true)
                    {
                        //Remove row at given index
                        Grid_View.Rows.RemoveAt(Grid_Row.Index);
                    }                        
                }
            }

            //Refresh
            Grid_View.Refresh();
        }

        private void Button_Settings_Click(object sender, EventArgs e)
        {
            new Settings_Form().Show();
        }

        protected override void OnPaint(PaintEventArgs e)
        {     
            //Refresh UI if requested
            if (Refresh_UI == true)
            {
                this.Refresh();
                Button_Save_Config.Hide();
                Button_Save_Config.Show();
                Button_Uncheck_All.Hide();
                Button_Uncheck_All.Show();
                Button_Add.Hide();
                Button_Add.Show();
                Button_Launch.Hide();
                Button_Launch.Show();
                Button_Remove.Hide();
                Button_Remove.Show();
                Refresh_UI = false;
            }

            try
            {
                if (Form1.Theme == "Dark")
                {
                    base.OnPaintBackground(e);

                    LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.DarkRed, Color.DarkBlue, 90F);

                    e.Graphics.FillRectangle(brush, this.ClientRectangle);

                    Label_Rows_Checked.ForeColor = Color.WhiteSmoke;
                    Grid_View.BackgroundColor = Color.Black;
                }
                else if (Form1.Theme == "Light")
                {
                    base.OnPaintBackground(e);

                    LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.White, Color.LightSkyBlue, 90F);

                    e.Graphics.FillRectangle(brush, this.ClientRectangle);

                    Label_Rows_Checked.ForeColor = Color.Black;
                    Grid_View.BackgroundColor = Color.WhiteSmoke;
                }
            }
            catch (Exception Exception) { }
        }

    }

}
