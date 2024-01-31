using JRPC_Client; // JRPC2
using Newtonsoft.Json; // JSON.NET
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using XDevkit; // XBOX 360 SDK

namespace GTASanAndreasTesting
{
    public partial class Form1 : Form
    {
        bool dbgConnection = false;
        bool oneHitKill = false;
        public uint TP_ADDRESS = 0;
        public uint HEALTH_ADDRESS = 0;

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
                MessageBox.Show("No locations to load.");
            }
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
                if (dbgConnection)
                {
                    xbdbg.DebugTarget.RemoveAllBreakpoints();
                    xbdbg.DebugTarget.DisconnectAsDebugger();
                }
            }
            catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "" && textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "")
                SaveCoords(textBox4.Text, float.Parse(textBox1.Text), float.Parse(textBox2.Text), float.Parse(textBox3.Text));
            else
                MessageBox.Show("Please fill all the fields!");
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

        private void button5_Click(object sender, EventArgs e)
        {
            LoadLocationNames();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string locationNameToLoad = comboBox1.SelectedItem.ToString();
            LoadCoordinates(locationNameToLoad);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (dbgConnection)
                {
                    if (TP_ADDRESS != 0)
                    {
                        xbdbg.WriteFloat(TP_ADDRESS, float.Parse(textBox1.Text)); 
                        xbdbg.WriteFloat(TP_ADDRESS + 0x4, float.Parse(textBox2.Text)); 
                        xbdbg.WriteFloat(TP_ADDRESS + 0x8, float.Parse(textBox3.Text));
                    }
                    else
                    {
                        MessageBox.Show("Address not yet found. Try again.");
                    }
                }
                else
                {
                    MessageBox.Show("Debug Monitor not connected!");
                }
            }
            catch
            {
                MessageBox.Show("Failed to teleport to position!");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (dbgConnection)
                {
                    uint entry = 0x822FD778; // this is the breakpoint address for the function that handles damage
                    uint freeMemAddr = 0x83070050; // this is just a random address in free memory that seems to be 0 all the time
                    uint entryHook = 0x48D728D8; // this branches to free memory where the new custom function will be written
                    byte[] hook = new byte[] { 0x2C, 0x08, 0x00, 0x31, 0x41, 0x82, 0x00, 0x08, 0x4B, 0x28, 0xD7, 0x28, 0x4B, 0x28, 0xD7, 0x30 }; // this is the new custom function that will be written to free memory
                    if (!oneHitKill)
                    {
                        xbdbg.WriteUInt32(entry, entryHook);
                        xbdbg.SetMemory(freeMemAddr, hook);
                        oneHitKill = true;
                    }
                    else
                    {
                        xbdbg.WriteUInt32(entry, 0x2B0A0000);
                        oneHitKill = false;
                    }
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

        private byte[] hexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length) // convert the hex string to byte array
                             .Where(x => x % 2 == 0) // split the hex string into 2 byte chunks
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)) 
                             .ToArray(); 
        }
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if (dbgConnection)
                {
                    // the function compares registers to find the correct address, then writes the address to free memory for external use
                    /*
                     * cmpwi %r26, 0x2 
   bne not_equal_r26

   cmpwi %r30, 0x1
   bne not_equal_r26
   
   lis %r6, 0x4187
   ori %r6, %r6, 0x0060
   cmpw %r6, %r31
   bne not_equal_r26

   bl custom_function

   custom_function:
   mr %r3, %r11
   lis %r14, 0x8307
   ori %r15, %r14, 0x0114
   stw %r3, 0(%r15)
   stfs %f6, 0x30(%r11)
   b (StorePosition - 0x34) - FreeMemory
   
   not_equal_r26:
   stfs %f6, 0x30(%r11)
   b (StorePosition - 0x3C) - FreeMemory
                     */

                    // the third check has not been fully tested so i'm not sure if that value will be dynamic after rebooting 
                    string hex = "2C1A0002408200382C1E0001408200303CC0418760C600607C06F80040820020480000057D635B783DC0830761CF0114906F0000D0CB00304B335414D0CB00304B33540C"; // new ppc function we will hook. loads the addresses into free memory for external comparison and retrieval 
                    byte[] bytes = hexStringToByteArray(hex); // convert the hex string to byte array. / seemed quicker than formatting manually
                    xbdbg.WriteUInt32(0x823A54AC, 0x48CCABB8); // inject the code to branch to the new custom function when executed
                    xbdbg.SetMemory(0x83070064, bytes); // inject the custom ppc function into free memory / i have no idea where the games actual free mem is so this is just an area that seems to be 0 all the time
                    timer2.Start(); // start timer to loop through the addresses, comparing the values to the visual representation of your coordinates, when found it will stop and store the address
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if (dbgConnection)
                {
                    uint custFunc = 0x83070114; // address in free memory where the custom function will store the address
                    uint visualaddr = 0x82C69B84; // static address for float coordinates used for comparing 
                    uint check1addr = xbdbg.ReadUInt32(custFunc) + 0x30; // get the value stored in free memory and add 0x30 to get the address of the float coordinates
                    float check1 = xbdbg.ReadFloat(check1addr); // read the float coordinates
                    float check2 = xbdbg.ReadFloat(visualaddr); // read the static float coordinates

                    if (check1 == check2) // if the coordinates are the same, then we have found the address
                    {
                        TP_ADDRESS = check1addr; // set the address to the address of the float coordinates
                        timer2.Stop(); // stop the timer
                    }
                }
                else
                {
                    timer2.Stop();
                    MessageBox.Show("Debug Monitor not connected!");
                }
            }
            catch(Exception ex) 
            {
                timer2.Stop();
                MessageBox.Show($"{ex.Message}");
            }
        }
    }
}
