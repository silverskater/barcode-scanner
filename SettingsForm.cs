using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Management;
using System.Windows.Forms;

namespace EBScan
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            SetPlaceholderText(textBoxUrl, "Custom barcode->AWB API endpoint");
        }

        public void GetSettings()
        {
            textBoxUrl.Text = Properties.Settings.Default.URL;
            textBoxUsername.Text = Properties.Settings.Default.AuthUsername;
            textBoxPassword.Text = Properties.Settings.Default.AuthPassword;
            textBoxId.Text = Properties.Settings.Default.ID.ToString();
            Dictionary<String, String> comboSource = GetComPortNames();
            if (comboSource.Count > 0)
            {
                comboBoxDevice.DataSource = new BindingSource(comboSource, null);
                comboBoxDevice.ValueMember = "Key";
                comboBoxDevice.DisplayMember = "Value";
                if (!string.IsNullOrEmpty(Properties.Settings.Default.Device))
                {
                    comboBoxDevice.SelectedValue = Properties.Settings.Default.Device;
                }
            }
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                comboBoxPrinter.Items.Add(printer);
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Printer))
            {
                comboBoxPrinter.SelectedItem = Properties.Settings.Default.Printer;
            }
        }

        public void SaveSettings(string url, int userId)
        {
            // Save settings.
            Properties.Settings.Default.URL = url.Trim();
            Properties.Settings.Default.AuthUsername = textBoxUsername.Text.Trim();
            Properties.Settings.Default.AuthPassword = textBoxPassword.Text.Trim();
            Properties.Settings.Default.FanAuthUsername = textBoxFanUsername.Text.Trim();
            Properties.Settings.Default.FanAuthPassword = textBoxFanPassword.Text.Trim();
            Properties.Settings.Default.ID = userId;
            Properties.Settings.Default.Device = comboBoxDevice.SelectedItem != null
                ? ((KeyValuePair<string, string>)comboBoxDevice.SelectedItem).Key
                : String.Empty;
            Properties.Settings.Default.Printer = comboBoxPrinter.SelectedItem != null
                ? comboBoxPrinter.SelectedItem.ToString()
                : String.Empty;
            Properties.Settings.Default.Save();
        }

        private Dictionary<String, String> GetComPortNames()
        {
            Dictionary<String, String> comPorts = new Dictionary<String, String>();
            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE 'Barcode Scanner (COM%)'"))
            {
                foreach (ManagementBaseObject mo in searcher.Get())
                {
                    string name = mo["Caption"].ToString();
                    try
                    {
                        comPorts.Add(name.Substring(17, name.Length - 18), name);
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
            }

            return comPorts;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            GetSettings();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            // Validate URL.
            bool isValidUrl = Uri.TryCreate(textBoxUrl.Text, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                && !textBoxUrl.Text.Contains("?")
                && !textBoxUrl.Text.Contains("#");
            if (!isValidUrl)
            {
                MessageBox.Show("Invalid URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Validate user ID.
            int userId;
            try
            {
                userId = Convert.ToInt32(textBoxId.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("User ID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Settings are valid, save them.
            SaveSettings(textBoxUrl.Text, userId);
            // Try to (re-)start with the new configuration.
            Program.mainForm.InitializeApp();
            // We're done, close the settings form.
            Close();
        }

        private void SetPlaceholderText(TextBox textBox, string placeholder)
        {
            textBox.ForeColor = Color.Gray;
            textBox.Text = placeholder;

            textBox.Enter += (sender, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };

            textBox.Leave += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }
    }
}
