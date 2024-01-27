using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json; // JSON.NET
using XDevkit; // XBOX 360 SDK
using JRPC_Client; // JRPC2

namespace GTASanAndreasTesting
{
    public partial class Form1 : Form
    {
        bool dbgConnection = false;
        public uint TP_ADDRESS = 0;

        public IXboxManager xbm;
        public XboxConsole xbdbg;

        private bool DbgConnect()
        {
            try
            {
                xbm = new XboxManager();
                xbdbg = xbm.OpenConsole(xbm.DefaultConsole);
                xbdbg.OpenConnection(null);
                dbgConnection = true;
                return true;
            }
            catch
            {
                dbgConnection = false;
                return false;
            }
        }

        private bool EntryExists(List<Dictionary<string, object>> existingLocations, string locationName)
        {
            // Check if an entry with the same locationName already exists in the list
            return existingLocations.Any(entry => entry.ContainsKey("locationName") && entry["locationName"].ToString() == locationName);
        }

        private void SaveCoords(string locationName, float x, float y, float z)
        {
            // Create a new dictionary to store coordinates and location name
            Dictionary<string, object> coords = new Dictionary<string, object>();

            // Add the location name to the dictionary
            coords.Add("locationName", locationName);

            // Add the coordinates to the dictionary
            coords.Add("x", x);
            coords.Add("y", y);
            coords.Add("z", z);

            // Define the common file name for all entries
            string fileName = "all_locations.json";

            // Check if the file already exists
            if (System.IO.File.Exists(fileName))
            {
                // Read the existing content of the file
                string existingJson = System.IO.File.ReadAllText(fileName);

                // Deserialize the existing content into a list of dictionaries
                List<Dictionary<string, object>> existingLocations = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(existingJson);

                // Check if an entry with the same locationName already exists
                if (!EntryExists(existingLocations, locationName))
                {
                    // Add the new coordinates to the list
                    existingLocations.Add(coords);

                    // Convert the updated list to a JSON string
                    string updatedJson = JsonConvert.SerializeObject(existingLocations, Formatting.Indented);

                    // Overwrite the file with the updated content
                    System.IO.File.WriteAllText(fileName, updatedJson);
                }
            }
            else
            {
                // Create a new list for the first entry
                List<Dictionary<string, object>> locations = new List<Dictionary<string, object>>();
                locations.Add(coords);

                // Convert the list to a JSON string
                string json = JsonConvert.SerializeObject(locations, Formatting.Indented);

                // Write the JSON string to a new file
                System.IO.File.WriteAllText(fileName, json);
            }
        }

        private void LoadCoordinates(string locationName)
        {
            // Define the common file name for all entries
            string fileName = "all_locations.json";

            // Check if the file exists
            if (System.IO.File.Exists(fileName))
            {
                // Read the existing content of the file
                string existingJson = System.IO.File.ReadAllText(fileName);

                // Deserialize the existing content into a list of dictionaries
                List<Dictionary<string, object>> existingLocations = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(existingJson);

                // Find the entry with the specified locationName
                Dictionary<string, object> selectedEntry = existingLocations.FirstOrDefault(entry =>
                    entry.ContainsKey("locationName") && entry["locationName"].ToString() == locationName);

                if (selectedEntry != null)
                {
                    // Display coordinates in the respective TextBoxes
                    textBox1.Text = selectedEntry.ContainsKey("x") ? selectedEntry["x"].ToString() : "";
                    textBox2.Text = selectedEntry.ContainsKey("y") ? selectedEntry["y"].ToString() : "";
                    textBox3.Text = selectedEntry.ContainsKey("z") ? selectedEntry["z"].ToString() : "";
                }
                else
                {
                    MessageBox.Show("Location not found in the file.");
                }
            }
            else
            {
                MessageBox.Show("File not found.");
            }
        }

        private void LoadLocationNames()
        {
            // Define the common file name for all entries
            string fileName = "all_locations.json";

            // Check if the file exists
            if (System.IO.File.Exists(fileName))
            {
                // Read the existing content of the file
                string existingJson = System.IO.File.ReadAllText(fileName);

                // Deserialize the existing content into a list of dictionaries
                List<Dictionary<string, object>> existingLocations = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(existingJson);

                // Retrieve all 'locationName' values
                List<string> locationNames = existingLocations
                    .Where(entry => entry.ContainsKey("locationName"))
                    .Select(entry => entry["locationName"].ToString())
                    .ToList();

                // Populate the ComboBox with location names
                comboBox1.DataSource = locationNames;
            }
            else
            {
                MessageBox.Show("File not found.");
            }
        }

        private void WriteLine(string text)
        {
            richTextBox1.AppendText(text + "\n");
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (DbgConnect())
                {
                    MessageBox.Show("Connected to Debug Monitor!");
                }
                else
                {
                    MessageBox.Show("Failed to connect to Debug Monitor!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (dbgConnection)
                {
                    uint addr = 0x823A54AC;
                    xbdbg.DebugTarget.ConnectAsDebugger("xdk", XboxDebugConnectFlags.Force);
                    xbdbg.DebugTarget.SetBreakpoint(addr);
                    try
                    {
                        xbdbg.OnStdNotify += (EventType, EventInfo) =>
                        {
                            if (EventType == XboxDebugEventType.ExecutionBreak && EventInfo.Info.Address == addr)
                            {
                                EventInfo.Info.Thread.TopOfStack.GetRegister64(XboxRegisters64.r26, out long r26);
                                EventInfo.Info.Thread.TopOfStack.GetRegister64(XboxRegisters64.r30, out long r30);
                                EventInfo.Info.Thread.TopOfStack.GetRegister64(XboxRegisters64.r11, out long r11);

                                if (r26 == 0x2 && r30 == 0x1)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        WriteLine("Current Ptr is: 0x" + r11.ToString("X"));
                                        xbdbg.DebugTarget.RemoveAllBreakpoints();
                                        WriteLine("Current DMA Address is: 0x" + (r11 + 0x30).ToString("X"));
                                        TP_ADDRESS = (uint)(r11 + 0x30);
                                        float pos_x = xbdbg.ReadFloat(TP_ADDRESS);
                                        float pos_y = xbdbg.ReadFloat(TP_ADDRESS + 0x4);
                                        float pos_z = xbdbg.ReadFloat(TP_ADDRESS + 0x8);
                                        WriteLine("Current Position:");
                                        WriteLine("X: " + pos_x.ToString());
                                        WriteLine("Y: " + pos_y.ToString());
                                        WriteLine("Z: " + pos_z.ToString());
                                        textBox1.Text = pos_x.ToString();
                                        textBox2.Text = pos_y.ToString();
                                        textBox3.Text = pos_z.ToString();
                                    }));
                                }
                                else
                                {
                                    Invoke(new Action(() =>
                                    {
                                        richTextBox1.AppendText("Serching...\n");
                                    }));
                                }

                                EventInfo.Info.Thread.Continue(true);
                                xbdbg.DebugTarget.Go(out bool flag2);
                                xbdbg.DebugTarget.FreeEventInfo(EventInfo.Info);
                            }
                        };
                    }
                    catch { }
                }
                else
                {
                    MessageBox.Show("Debug Monitor not connected!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (dbgConnection && TP_ADDRESS != 0 && checkBox1.Checked)
                {
                    float pos_x = xbdbg.ReadFloat(TP_ADDRESS);
                    float pos_y = xbdbg.ReadFloat(TP_ADDRESS + 0x4);
                    float pos_z = xbdbg.ReadFloat(TP_ADDRESS + 0x8);
                    textBox1.Text = pos_x.ToString();
                    textBox2.Text = pos_y.ToString();
                    textBox3.Text = pos_z.ToString();
                }
            }
            catch
            {
                timer1.Stop();
                MessageBox.Show("Failed to read position!");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                xbdbg.DebugTarget.RemoveAllBreakpoints();
                xbdbg.DebugTarget.DisconnectAsDebugger();
            }
            catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveCoords(textBox4.Text, float.Parse(textBox1.Text), float.Parse(textBox2.Text), float.Parse(textBox3.Text));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string locationNameToLoad = textBox4.Text;
            LoadCoordinates(locationNameToLoad);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadLocationNames();
        }
    }
}
