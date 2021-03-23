using System;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace EBScan
{
    public partial class MainForm : Form
    {
        private readonly ListViewColumnSorter lvwColumnSorter;
        
        protected class Response
        {
            public int statusCode = 0;
            public bool responseError = false;
            public string responseMessage = "";
            public string exceptionMessage = "";
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

            if (CheckSettings())
            {
                InitDevice();
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
                msgError = "Invalid URL!";
            }
            else if (Properties.Settings.Default.Device == String.Empty)
            {
                msgError = "Device not configured!";
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
                MessageBox.Show(msgError, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }
            // No errors.
            return true;
        }

        public void InitDevice()
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

        private void AddMessage(string msg, bool isError = false)
        {
            Response serverResponse = new Response();
            if (!isError)
            {
                // Send the barcode to the web server and receive a response.
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
            string status = isError ? "ERROR" : "OK";
            string[] values = { DateTime.Now.ToString(), status, msg, statusCode, msgResponse };
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

        private void ResendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                AddMessage(item.SubItems[2].Text);
            }
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
    }
}
