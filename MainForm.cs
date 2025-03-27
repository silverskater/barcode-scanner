using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace EBScan
{
    public partial class MainForm : Form
    {
        private readonly ListViewColumnSorter _lvwColumnSorter;
        private WebBrowser _webBrowser = new WebBrowser();
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClientWithAuth;
        private readonly TokenService _tokenService;

        protected class Response
        {
            public int StatusCode { get; set; }
            public bool HasError { get; set; }
            public string Message { get; set; }
            public string ExceptionMessage { get; set; }
            public string Awb { get; set; }
            public string ClientId { get; set; }
            public bool Print { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();

            // Create an instance of a ListView column sorter and assign it to the ListView control.
            _lvwColumnSorter = new ListViewColumnSorter();
            listView.ListViewItemSorter = _lvwColumnSorter;

            // Sorting: ensure that the view is set to show details.
            listView.View = View.Details;

            Icon = Properties.Resources.barcode;
            notifyIcon.Icon = Properties.Resources.barcode;

            // Our MainForm is not created on Application.Run().
            // Fix 'Invoke or BeginInvoke cannot be called on a control until the window handle has been created'.
            if (!IsHandleCreated)
            {
                CreateControl();
                // CreateControl does not create a control handle if the control's Visible property is false.
                CreateHandle();
            }

            // Initialize HttpClient with TokenHandler.
            _tokenService = new TokenService(new HttpClient());
            var tokenHandler = new TokenHandler(_tokenService);
            _httpClientWithAuth = new HttpClient(tokenHandler)
            {
                BaseAddress = new Uri("https://api.fancourier.ro/")
            };
            // Initialize HttpClient for SendBarcodeAsync().
            _httpClient = new HttpClient();

            InitializeApp();
        }

        public void InitializeApp()
        {
            if (CheckSettings())
            {
                // Init/Reset the bearer token.
                Task.Run(async () => await _tokenService.RefreshBearerTokenAsync());
                // All settings are OK, start using the barcode scanner.
                InitializeBarcodeScanner();
            }
            else
            {
                // Show form on startup errors.
                Show();
                // Hide list of messages because there will be no messages to show.
                listView.Visible = false;
            }
        }

        private bool CheckSettings()
        {
            string msgError = "";
            if (string.IsNullOrEmpty(Properties.Settings.Default.URL))
            {
                msgError = "Missing URL!";
            }
            else if (string.IsNullOrEmpty(Properties.Settings.Default.Device))
            {
                msgError = "Barcode scanner not configured!";
            }
            else if (string.IsNullOrEmpty(Properties.Settings.Default.Printer))
            {
                msgError = "Printer not configured!";
            }
            else
            {
                // Check if the configured serial port is available.
                string[] ports = SerialPort.GetPortNames();
                if (!ports.Contains(Properties.Settings.Default.Device))
                {
                    msgError = "Barcode scanner not found.";
                }
            }
            if (!string.IsNullOrEmpty(msgError))
            {
                statusLabel.Text = $"Error: {msgError}";
                notifyIcon.Icon = Properties.Resources.barcode_error;
                notifyIcon.ShowBalloonTip(5000, "Warning", msgError, ToolTipIcon.Warning);
                return false;
            }

            // No errors.
            return true;
        }

        private void InitializeBarcodeScanner()
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

        public void AddMessage(string msg, bool isError = false)
        {
            var response = new Response();
            if (!isError)
            {
                // Send the barcode to the web server and receive a response containing the AWB.
                response = Task.Run(async () => await SendBarcodeAsync(msg)).Result;
                if (response.HasError)
                {
                    isError = true;
                }
            }
            string statusCode = response.StatusCode == 0
                ? string.Empty
                : response.StatusCode.ToString();
            // If no response, fall back to exception message which could also be empty.
            string msgResponse = string.IsNullOrEmpty(response.Message)
                ? response.ExceptionMessage
                : response.Message;
            if (isError)
            {
                string helpText = !string.IsNullOrEmpty(response.ExceptionMessage)
                    ? response.ExceptionMessage
                    : response.HasError
                        ? response.Message
                        : msg;
                statusLabel.Text = $"Error: {helpText}";
                notifyIcon.Icon = Properties.Resources.barcode_error;
                notifyIcon.ShowBalloonTip(5000, "ERROR", helpText, ToolTipIcon.Error);
            }
            // Add a new line to list view.
            string status = isError ? "ERROR" : "OK";
            string[] values = { DateTime.Now.ToString(), status, msg, statusCode, response.Awb, response.ClientId, msgResponse };
            var row = new ListViewItem(values);
            // Add newest first (to the top).
            listView.Items.Insert(0, row);
            // Limit list to the last 1000 items.
            if (listView.Items.Count > 1000)
            {
                listView.Items[1000].Remove();
            }
            // Update last column width on data changes.
            ResizeForm();
            if (response.Print)
            {
                // Try to print the AWB.
                Task.Run(async () => await PrintShippingLabelAsync(response.Awb, response.ClientId));
            }
        }

        private async Task<Response> SendBarcodeAsync(string barcode)
        {
            var response = new Response();
            if (string.IsNullOrEmpty(Properties.Settings.Default.URL))
            {
                return response;
            }
            string jsonData;
            // Fetch the corresponding AWB Tracking Number from the custom API endpoint.
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(Properties.Settings.Default.URL)
                      .Append("?barcode=")
                      .Append(WebUtility.UrlEncode(barcode))
                      .Append("&user=")
                      .Append(Properties.Settings.Default.ID.ToString());
            var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());
            if (!string.IsNullOrEmpty(Properties.Settings.Default.AuthUsername) && !string.IsNullOrEmpty(Properties.Settings.Default.AuthPassword))
            {
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(
                    $"{Properties.Settings.Default.AuthUsername}:{Properties.Settings.Default.AuthPassword}"
                ));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            }
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                var httpResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);
                jsonData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                response.StatusCode = (int)httpResponse.StatusCode;
            }
            catch (HttpRequestException ex)
            {
                response.HasError = true;
                response.ExceptionMessage = ex.Message;
                return response;
            }
            // Process the API response
            var js = new JavaScriptSerializer();
            try
            {
                dynamic data = js.Deserialize<dynamic>(jsonData);
                response.HasError = data.ContainsKey("error") ? data["error"] : true;
                response.Message = data.ContainsKey("message") ? data["message"] : string.Empty;
                response.Awb = data.ContainsKey("awb") ? data["awb"] : string.Empty;
                response.ClientId = data.ContainsKey("fan_client_id") ? data["fan_client_id"].ToString() : string.Empty;
                response.Print = !string.IsNullOrEmpty(response.Awb) && !string.IsNullOrEmpty(response.ClientId) && data.ContainsKey("print") && data["print"];
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        private void ByeError()
        {
            statusLabel.Text = "";
            notifyIcon.Icon = Properties.Resources.barcode;
        }

        private async Task PrintShippingLabelAsync(string awb, string clientId)
        {
            string html = "";
            try
            {
                html = await FetchShippingLabelHtmlAsync(awb, clientId);
            }
            catch (Exception ex)
            {
                // Use the UI thread.
                Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show($"{ex.GetType()}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return;
            }
            if (string.IsNullOrEmpty(html))
            {
                // Use the UI thread.
                Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show("Empty AWB response!");
                }));
                return;
            }
            // Adjust the received HTML for printing on an A6 sticker printer.
            // Lastest IE rendering engine for CSS3 flex support.
            html = html.Replace("<body", "<head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" /></head><body");
            // Add custom CSS at the end of the '<style>' block inside of '<div id="print-area">'.
            int index = html.IndexOf("</style>", html.IndexOf("id=\"print-area\""));
            if (index > 0)
            {
                html = html.Insert(index, @"body {transform: none !important;} #awb_epod_table { width: 94mm; height: 13cm; padding-bottom: 0.1cm } #footer_row td { text-align: center} .footer_container { padding-left: 0px; } .footer_container img { width: 100% !important }");
            }
            // Internet Explorer print settings.
            string keyName = @"Software\Microsoft\Internet Explorer\PageSetup";
            using (var key = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (key != null)
                {
                    key.SetValue("PageOrientation", 2);
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
            // Use the UI thread.
            Invoke((MethodInvoker)(() =>
            {
                // Reinitialize browser if it has been disposed (after encountering an error).
                if (_webBrowser.IsDisposed)
                {
                    _webBrowser = new WebBrowser();
                }
                _webBrowser.DocumentText = html;
                _webBrowser.Parent = this;
                _webBrowser.ScriptErrorsSuppressed = true;
                _webBrowser.DocumentCompleted += (browser, webBrowserEvent) =>
                {
                    // @fixme This is a workaround for using the selected printer in IE.
                    string originalDefaultPrinterName = GetDefaultPrinter();
                    SetDefaultPrinter(Properties.Settings.Default.Printer);
                    // Print the document now that it is fully loaded.            
                    ((WebBrowser)browser).Print();
                    // Dispose the WebBrowser now that the task is complete. 
                    ((WebBrowser)browser).Dispose();
                    if (GetDefaultPrinter() != originalDefaultPrinterName)
                    {
                        SetDefaultPrinter(originalDefaultPrinterName);
                    }
                };
            }));
        }

        private async Task<string> FetchShippingLabelHtmlAsync(string awb, string clientId, bool retryForbidden = true)
        {
            // GET AWB Print in HTML format.
            string url = $"/awb/label?clientId={clientId}&awbs[]={awb}&pdf=0&&language=ro";
            using (var response = await _httpClientWithAuth.GetAsync(url).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else if (retryForbidden && response.StatusCode.Equals(HttpStatusCode.Forbidden))
                {
                    // Refresh the bearer token and retry the request one more time.
                    await _tokenService.RefreshBearerTokenAsync().ConfigureAwait(false);
                    return await FetchShippingLabelHtmlAsync(awb, clientId, false);
                }
                else
                {
                    throw new Exception($"HTTP Status Code {response.StatusCode} from: '{url}'.");
                }
            }
        }

        private static string GetDefaultPrinter()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Printer");
            var searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in searcher.Get().Cast<ManagementObject>())
            {
                if (((bool?)mo["Default"]) ?? false)
                {
                    return mo["Name"] as string;
                }
            }

            return null;
        }

        private static bool SetDefaultPrinter(string defaultPrinter)
        {
            using (var objectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
            {
                using (var objectCollection = objectSearcher.Get())
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

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
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
            var aboutBox = new AboutBox();
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

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Scanner read runs in a different thread, use thread-safe way to access form components.
            Invoke((MethodInvoker)(() => AddMessage(serialPort.ReadExisting())));
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            // Scanner read runs in a different thread, use thread-safe way to access form components.
            Invoke((MethodInvoker)(() => AddMessage($"Device error: {e.EventType}", true)));
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                _lvwColumnSorter.Order = _lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _lvwColumnSorter.SortColumn = e.Column;
                _lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listView.Sort();
        }

        private void ResendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                AddMessage(item.SubItems[2].Text);
            }
        }

        private void PrintAWBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Capture the selected items on the UI thread.
            List<ListViewItem> selectedItems = new List<ListViewItem>();
            Invoke((MethodInvoker)(() =>
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(item.SubItems[4].Text) && !string.IsNullOrEmpty(item.SubItems[5].Text))
                    {
                        selectedItems.Add(item);
                    }
                }
            }));
            // Process the selected items on a background thread.
            Task.Run(async () =>
            {
                foreach (var item in selectedItems)
                {
                    await PrintShippingLabelAsync(item.SubItems[4].Text, item.SubItems[5].Text);
                }
            });
        }
    }
}
