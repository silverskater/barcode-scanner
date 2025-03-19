using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace EBScan
{
    public partial class MainForm : Form
    {
        private readonly ListViewColumnSorter lvwColumnSorter;

        private readonly HttpClient httpClient = new HttpClient(new HttpClientHandler { UseProxy = false} );

        private WebBrowser wb = new WebBrowser();

        protected class Response
        {
            public int statusCode = 0;
            public bool responseError = false;
            public string responseMessage = "";
            public string exceptionMessage = "";
            public string awb = "";
            public string clientId = "";
            public bool print = false;
        }

        public MainForm()
        {
            InitializeComponent();

            // Create an instance of a ListView column sorter and assign it
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView.ListViewItemSorter = lvwColumnSorter;

            // Sorting: ensure that the view is set to show details.
            listView.View = View.Details;

            Icon = Properties.Resources.barcode;
            notifyIcon.Icon = Properties.Resources.barcode;

            // Fixes 'Invoke or BeginInvoke cannot be called on a control until 
            // the window handle has been created' (the form is not created on 
            // Application.Run()).
            if (!IsHandleCreated)
            {
                CreateControl();
                // CreateControl does not create a control handle if the 
                // control's Visible property is false.
                CreateHandle();
            }

            // Only start if all the settings are OK.
            if (CheckSettings())
            {
                InitScanner();
            }
            else
            {
                // Show form on startup errors.
                Show();
                // Hide list when device can not be used.
                listView.Visible = false;
            }
        }

        public bool CheckSettings()
        {
            string msgError = "";
            if (Properties.Settings.Default.URL == String.Empty)
            {
                msgError = "Missing URL!";
            }
            else if (Properties.Settings.Default.Device == String.Empty)
            {
                msgError = "Device not configured!";
            }
            else if (Properties.Settings.Default.Printer == String.Empty)
            {
                msgError = "Printer not configured!";
            }
            else
            {
                // Check if the configured serial port is available.
                string[] ports = SerialPort.GetPortNames();
                if (!ports.Contains(Properties.Settings.Default.Device))
                {
                    msgError = "Device not found.";
                }
            }
            if (msgError != String.Empty)
            {
                statusLabel.Text = "Error: " + msgError;
                notifyIcon.Icon = Properties.Resources.barcode_error;
                notifyIcon.ShowBalloonTip(5000, "Warning", msgError, ToolTipIcon.Warning);
                //MessageBox.Show(msgError, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            // No errors.
            return true;
        }

        public void InitScanner()
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                serialPort.PortName = Properties.Settings.Default.Device;
                serialPort.Open();
                // Only show the list when the device is ready.
                listView.Visible = true;
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message, true);
            }
        }

        /// <summary>
        /// Adds barcode scan (+prints AWB) or error message to the list.
        /// </summary>
        /// <param name="msg">The barcode or error message</param>
        /// <param name="isError">True if error message</param>
        private void AddMessage(string msg, bool isError = false)
        {
            Response serverResponse = new Response();
            if (!isError)
            {
                // Send the barcode to the web server and receive a response containing the AWB.
                serverResponse = SendBarcode(msg);
                if (serverResponse.responseError)
                {
                    isError = true;
                }
            }
            string statusCode = serverResponse.statusCode == 0
                ? String.Empty
                : serverResponse.statusCode.ToString();
            // If no response, fall back to exception message which could also be empty.
            string msgResponse = serverResponse.responseMessage == String.Empty
                ? serverResponse.exceptionMessage
                : serverResponse.responseMessage;
            if (isError)
            {
                string tipText = serverResponse.exceptionMessage != String.Empty
                    ? serverResponse.exceptionMessage
                    : serverResponse.responseError
                        ? serverResponse.responseMessage
                        : msg;
                statusLabel.Text = "Error: " + tipText;
                notifyIcon.Icon = Properties.Resources.barcode_error;
                notifyIcon.ShowBalloonTip(5000, "ERROR", tipText, ToolTipIcon.Error);
            }
            // Add new line to list view.
            string status = isError ? "ERROR" : "OK";
            string[] values = { DateTime.Now.ToString(), status, msg, statusCode, serverResponse.awb, serverResponse.clientId, msgResponse };
            ListViewItem row = new ListViewItem(values);
            // Add newest first (to the top).
            listView.Items.Insert(0, row);
            // Limit list to the last 1000 items.
            if (listView.Items.Count > 1000)
            {
                listView.Items[1000].Remove();
            }
            // Update last column width on data changes.
            ResizeForm();
            // AWB printing.
            if (serverResponse.print)
            {
                // Try to print the AWB.
                PrintAwb(serverResponse.awb, serverResponse.clientId);
            }
        }

        private Response SendBarcode(string barcode)
        {
            Response response = new Response();
            if (Properties.Settings.Default.URL == String.Empty)
            {
                return response;
            }
            // Build and access URL.
            string url = Properties.Settings.Default.URL
                + "?barcode=" + WebUtility.UrlEncode(barcode)
                + "&user=" + Properties.Settings.Default.ID.ToString();
            string jsonData = "{}";
            using (WebClient wc = new WebClient())
            {
                if (Properties.Settings.Default.AuthUsername != String.Empty && Properties.Settings.Default.AuthPassword != String.Empty)
                {
                    string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(
                        Properties.Settings.Default.AuthUsername + ":" + Properties.Settings.Default.AuthPassword
                    ));
                    wc.Headers.Add(HttpRequestHeader.Authorization, "Basic " + encoded);
                }
                wc.Headers.Add(HttpRequestHeader.Accept, "application/json");
                try
                {
                    jsonData = wc.DownloadString(url);
                    response.statusCode = (int) HttpStatusCode.OK;
                }
                catch (WebException ex)
                {
                    response.responseError = true;
                    HttpWebResponse res = (HttpWebResponse) ex.Response;
                    if (res != null)
                    {
                        response.statusCode = (int) res.StatusCode;
                    }
                    response.exceptionMessage = ex.Message;
                    // Exit on error.
                    return response;
                }
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            try
            {
                dynamic data = js.Deserialize<dynamic>(jsonData);
                response.responseError = data["error"];
                response.responseMessage = data["message"];
                response.awb = data["awb"];
                response.clientId = data["fan_client_id"];
                response.print = !response.awb.Equals(String.Empty) && !response.clientId.Equals(String.Empty) && data["print"];
            }
            catch (Exception ex) 
            {
                response.responseError = true;
                response.exceptionMessage = ex.Message;
            }
            return response;
        }

        private void ByeError()
        {
            statusLabel.Text = "";
            notifyIcon.Icon = Properties.Resources.barcode;
        }

        private void PrintAwb(string awb, string clientId)
        {
            //var currentContext = TaskScheduler.FromCurrentSynchronizationContext();
            string html = "";
            try
            {
                html = Task.Run(async () =>
                {
                    return (await GetAwbDocAsync(awb, clientId));
                }).Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().ToString() + ": " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (html == String.Empty)
            {
                MessageBox.Show("Empty AWB response!");
                return;
            }
            // Adjust sizes to fit the A6 printer. Header: Logo: 2.3cm, Barcode: 4.6cm.
            string logoWidth = "2.3";
            string barcodeWidth = "4.6";
            string style = "<style> #awb_epod_table { height: 12.5cm; } #awb_epod_rand_3 { height: 2.6cm; } .epod_sectiune_destinatar { height: 2.5cm; } .epod_dest_agentie { width: 5.7cm; } .epod_destinatar { width: 5.7cm; height: 56px;  } .epod_dest_judet { height: 0.5cm; }"
                + "#awb_epod_rand_3, .epod_sectiune_destinatar { height: 2cm; }"
                + ".epod_dest_cod_sortare_vizuala { display: inline; width: 2.7cm; } .epod_tranzit { float: right; width: 2.7cm; } .epod_destinatar { float: left; } .epod_dest_adresa { height: 0.4cm; }"
                + ".epod_dest_telefon { clear: both; }"
                //+ ".epod_dest_telefon, .epod_dest_localitate, .epod_dest_judet{ display: none; float: none; }"
                + ".epod_sectiune_fan { width: " + logoWidth + "cm; } .epod_sigla_img img { width: 100%; height: auto; } "
                + ".epod_awb_number_1 { display: inline; } "
                + ".epod_awb_number { padding-left: 0cm; } "
                //+ ".epod_sectiune_fan { width: " + numericUpDown1.Value.ToString() + "cm; } "
                //+ ".epod_eticheta_nr { width: 0.6cm; } "
                + " </style>";
            html = html.Replace("</head>", style + Environment.NewLine + "</head>");
            html = html.Replace("<img style=\"width:5cm;", "<img style=\"width:" + barcodeWidth + "cm;");
            // Internet Explorer print settings.
            string keyName = @"Software\Microsoft\Internet Explorer\PageSetup";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (key != null)
                {
                    key.SetValue("PageOrientation", 2);
                    //key.SetValue("font", "Verdana");
                    key.SetValue("footer", "");
                    key.SetValue("header", "");
                    key.SetValue("margin_top", "0");
                    key.SetValue("margin_left", "0");
                    key.SetValue("margin_right", "0");
                    key.SetValue("margin_bottom", "0");
                    key.SetValue("Print_Background", "false");
                    key.SetValue("Shrink_To_Fit", "false");
                }
            }
            // Reinitialize browser if it has been disposed (after error).
            if (wb.IsDisposed)
            {
                wb = new WebBrowser();
            }
            wb.DocumentText = html;
            wb.Parent = this;
            wb.ScriptErrorsSuppressed = true;
            wb.DocumentCompleted +=
               new WebBrowserDocumentCompletedEventHandler(PrintAwbDocument);
        }

        private void PrintAwbDocument(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string originalDefaultPrinterName = GetDefaultPrinterName();

            SetDefaultPrinter(Properties.Settings.Default.Printer);
            // Print the document now that it is fully loaded.            
            ((WebBrowser)sender).Print();

            // Dispose the WebBrowser now that the task is complete. 
            ((WebBrowser)sender).Dispose();
            if (GetDefaultPrinterName() != originalDefaultPrinterName)
            {
                SetDefaultPrinter(originalDefaultPrinterName);
            }
        }

        public static string GetDefaultPrinterName()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Printer");
            var searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in searcher.Get())
            {
                if (((bool?)mo["Default"]) ?? false)
                {
                    return mo["Name"] as string;
                }
            }

            return null;
        }

        public static bool SetDefaultPrinter(string defaultPrinter)
        {
            using (ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
            {
                using (ManagementObjectCollection objectCollection = objectSearcher.Get())
                {
                    foreach (ManagementObject mo in objectCollection)
                    {
                        if (string.Compare(mo["Name"].ToString(), defaultPrinter, true) == 0)
                        {
                            mo.InvokeMethod("SetDefaultPrinter", null, null);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private async Task<string> GetAwbDocAsync(string awb, string clientId)
        {
            string url = "https://www.selfawb.ro/view_awb_integrat.php";
            var values = new Dictionary<string, string>
            {
                { "nr", awb },
                { "username", "" },
                { "client_id", clientId },
                { "user_pass", "" },
                { "page", "A6" },
                { "ln", "ro" },
            };
            var content = new FormUrlEncodedContent(values);
            using (var response = await httpClient
                .PostAsync(url, content)
                //.ConfigureAwait(false)
                )
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void ResizeForm()
        {
            int lastColIndex = listView.Columns.Count - 1;
            listView.Columns[lastColIndex].Width = -2;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            ResizeForm();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            ResizeForm();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            Focus();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            ByeError();
        }

        private void StatusStrip_Click(object sender, EventArgs e)
        {
            ByeError();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Scanner read runs in a different thread, use thread-safe way to access form components.
            this.Invoke((MethodInvoker)(() => AddMessage(serialPort.ReadExisting())));
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            // Scanner read runs in a different thread, use thread-safe way to access form components.
            this.Invoke((MethodInvoker)(() => AddMessage("Device error: " + e.EventType.ToString())));
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView.Sort();
        }

        private void ResendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                AddMessage(item.SubItems[2].Text);
            }
        }

        private void printAWBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                PrintAwb(item.SubItems[4].Text, item.SubItems[5].Text);
            }
        }
    }
}
