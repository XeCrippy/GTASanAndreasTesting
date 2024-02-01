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
        bool connection = false;
        bool infAmmo = false;
        bool oneHitKill = false;
        bool tpHook = false;
        public uint TP_ADDRESS = 0;
        public uint HEALTH_ADDRESS = 0;

        private uint ammo1 = 0x82629E38;
        private uint ammo2 = 0x82629DE8;
        private uint ammo_off = 0x396BFFFF;
        private uint ammo_on = 0x396B0000;
        private uint money1 = 0x82F31608;
        private uint money2 = 0x82F3160C;
        private uint vehicleAmmo = 0x826267C0;

        private const uint fat = 0x82F2B88C;//tu0=0x82F2B80C;
        private uint stamina = fat + 0x4;
        private uint muscle = fat + 0x8;
        private uint respect1 = fat + 0xAC;
        private uint respect2 = fat + 0xBC;
        private uint sex_appeal = fat + 0xEC;
        private uint driving_skill = fat - 0x334;

        private const uint pistolSkill = 0x82F2B94C;//tu0=0x82F2B8CC; //823CB3F0
        private uint silencedPistolSkill = pistolSkill + 0x4;
        private uint desertEagleSkill = pistolSkill + 0x8;
        private uint shotgunSkill = pistolSkill + 0xC;
        private uint sawnOffShotgunSkill = pistolSkill + 0x10;
        private uint combatShotgunSkill = pistolSkill + 0x14;
        private uint machinePistolSkill = pistolSkill + 0x18;
        private uint smgSkill = pistolSkill + 0x1C;
        private uint ak47Skill = pistolSkill + 0x20;
        private uint m4Skill = pistolSkill + 0x24;
        private uint rifleSkill = pistolSkill + 0x28;

        private const uint bulletsFired = 0x82F2B4D0;//tu0=0x82F2B450;
        private uint kgsOfExplosivesUsed = bulletsFired + 0x4;
        private uint bulletsThatHit = bulletsFired + 0x8;
        private uint hospitalVisits = bulletsFired + 0x24;

        private uint peopleKilled = 0x82F2B4BC;//tu0=0x82F2B43C; // 4000 = The Los Santos Slayer 20G
        private uint passangersDroppedOff = 0x82F2B4B0; // 50 = Yes I Speak English
        private uint sexAppeal = 0x82F2B978;//tu0=0x82F2B8F8; // 9999 = Chick magnet 100G
        private uint respect = 0x82F2B948;//tu0=0x82F2B8C8; // 9999 = Original Gangster 100G
        private uint timesBusted = 0x82F2B4EC;//tu0=0x82F2B46C; // 49 + 1 = Serial Offender 20G
        private uint taxiFares = 0x82D8C8B0;//tu0=0x82D8C830; // set to 50 while on taxi mission = Yes I Speak English 30G

        IXboxConsole xbox;

        private bool Connect()
        {
            try
            {
                if (xbox.Connect(out xbox))
                {
                    connection = true;
                    return true;
                }
                else
                {
                    connection = false;
                    return false;
                }
            }
            catch
            {
                connection = false;
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
                if (Connect())
                {
                    MessageBox.Show("Connected to: " + xbox.Name);
                }
                else
                {
                    MessageBox.Show("Failed to connect to default console");
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
                if (connection && TP_ADDRESS != 0 && checkBox1.Checked)
                {
                    float pos_x = xbox.ReadFloat(TP_ADDRESS);
                    float pos_y = xbox.ReadFloat(TP_ADDRESS + 0x4);
                    float pos_z = xbox.ReadFloat(TP_ADDRESS + 0x8);
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
                if (connection)
                {
                    xbox.DebugTarget.RemoveAllBreakpoints();
                    xbox.DebugTarget.DisconnectAsDebugger();
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
                if (connection)
                {
                    TP_ADDRESS = xbox.ReadUInt32(0x83070100) + 0x30; // get the address of the float coordinates

                    if (TP_ADDRESS != 0)
                    {
                        xbox.WriteFloat(TP_ADDRESS, float.Parse(textBox1.Text));
                        xbox.WriteFloat(TP_ADDRESS + 0x4, float.Parse(textBox2.Text));
                        xbox.WriteFloat(TP_ADDRESS + 0x8, float.Parse(textBox3.Text));
                    }
                    else
                    {
                        MessageBox.Show("Address not yet found. Try again.");
                    }
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
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
                if (connection)
                {
                    uint entry = 0x822FE5C4; // this is the breakpoint address for the function that handles damage
                    uint freeMemAddr = 0x83070044; // this is just a random address in free memory that seems to be 0 all the time
                    uint entryHook = 0x48D71A80; // this branches to free memory where the new custom function will be written
                    string hex = "2C0800312C090042418200182C0800692C0900074182000CEDAD68284B28E568600000004B28E560";
                    string hex2 = "2C0800312C0900424182001C2C080069418200142C0800314182000CEDAD68284B28E4B4600000004B28E4AC";
                    byte[] hook = hexStringToByteArray(hex); // this is the new custom function that will be written to free memory
                    byte[] hook2 = hexStringToByteArray(hex2);
                    if (!oneHitKill)
                    {
                        xbox.WriteUInt32(entry, entryHook);
                        xbox.SetMemory(freeMemAddr, hook);

                        xbox.WriteUInt32(0x822FE540, 0x48D71B30); // inject the code to branch to the new custom function when executed
                        xbox.SetMemory(0x83070070, hook2);
                        oneHitKill = true;
                    }
                    else
                    {
                        byte[] clear = new byte[hook.Length + hook2.Length]; // clear the memory that was written to with the custom function
                        xbox.WriteUInt32(entry, 0xEDAD6028); // restore the original function
                        xbox.WriteUInt32(0x822FE540, 0xD01F0540); // restore the original function
                        xbox.SetMemory(freeMemAddr, clear); // clear the memory that was written to with the custom function
                        oneHitKill = false;
                    }
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
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
                if (connection)
                {
                    string hex = "2C1A0002408200342C1E00014082002C3DE0800061EF00007C0F38004082001C480000053DC0830761CE0100916E0000D0CB00304B3364D4D0CB00304B3364CC"; // new ppc function we will hook. loads the addresses into free memory for external comparison and retrieval 
                    byte[] bytes = hexStringToByteArray(hex); // convert the hex string to byte array. / seemed quicker than formatting manually
                    if (!tpHook)
                    {
                        xbox.WriteUInt32(0x823A6504, 0x48CC9AFC); // inject the code to branch to the new custom function when executed
                        xbox.SetMemory(0x83070000, bytes); // inject the custom ppc function into free memory / i have no idea where the games actual free mem is so this is just an area that seems to be 0 all the time
                        tpHook = true;
                    }
                    else
                    {
                        byte[] clear = new byte[bytes.Length]; // clear the memory that was written to with the custom function
                        xbox.WriteUInt32(0x823A6504, 0xD0CB0030); // restore the original function
                        xbox.SetMemory(0x83070000, clear); // clear the memory that was written to with the custom function
                        tpHook = false;
                    }
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
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
                if (connection)
                {
                    if (!infAmmo)
                    {
                        xbox.WriteUInt32(ammo1, ammo_on);
                        xbox.WriteUInt32(ammo2, ammo_on);
                        xbox.WriteUInt32(vehicleAmmo, ammo_on);
                        infAmmo = true;
                    }
                    else
                    {
                        xbox.WriteUInt32(ammo1, ammo_off);
                        xbox.WriteUInt32(ammo2, ammo_off);
                        xbox.WriteUInt32(vehicleAmmo, ammo_off);
                        infAmmo = false;
                    }
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection)
                {
                    xbox.WriteFloat(fat, 0);
                    xbox.WriteFloat(stamina, 1000);
                    xbox.WriteFloat(muscle, 1000);
                    xbox.WriteFloat(respect1, 1000);
                    xbox.WriteFloat(respect2, 1000);
                    xbox.WriteFloat(sex_appeal, 1000);
                    xbox.WriteFloat(driving_skill, 1000);
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection)
                {
                    xbox.WriteUInt32(money1, 999999999);
                    xbox.WriteUInt32(money2, 999999999);
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection)
                {
                    xbox.WriteFloat(pistolSkill, 999f);
                    xbox.WriteFloat(silencedPistolSkill, 999f);
                    xbox.WriteFloat(desertEagleSkill, 999f);
                    xbox.WriteFloat(shotgunSkill, 999f);
                    xbox.WriteFloat(sawnOffShotgunSkill, 999f);
                    xbox.WriteFloat(combatShotgunSkill, 999f);
                    xbox.WriteFloat(machinePistolSkill, 999f);
                    xbox.WriteFloat(smgSkill, 999f);
                    xbox.WriteFloat(ak47Skill, 999f);
                    xbox.WriteFloat(m4Skill, 999f);
                    xbox.WriteFloat(rifleSkill, 999f);
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection)
                {
                    xbox.WriteUInt32(peopleKilled, 40000u);
                    xbox.WriteFloat(sexAppeal, 2000f);
                    xbox.WriteFloat(respect, 2000f);
                    xbox.WriteUInt32(timesBusted, 49u);
                    xbox.WriteUInt32(taxiFares, 49u);
                }
                else
                {
                    MessageBox.Show("You are not connected to your console");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
