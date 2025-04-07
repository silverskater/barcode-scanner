
namespace EBScan
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.labelUrl = new System.Windows.Forms.Label();
            this.textBoxId = new System.Windows.Forms.TextBox();
            this.labelUserId = new System.Windows.Forms.Label();
            this.labelDevice = new System.Windows.Forms.Label();
            this.comboBoxDevice = new System.Windows.Forms.ComboBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxPrinter = new System.Windows.Forms.ComboBox();
            this.labelPrinter = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxFanUsername = new System.Windows.Forms.TextBox();
            this.textBoxFanPassword = new System.Windows.Forms.TextBox();
            this.labelFanUsername = new System.Windows.Forms.Label();
            this.labelFanPassword = new System.Windows.Forms.Label();
            this.radioButtonFormatHtml = new System.Windows.Forms.RadioButton();
            this.radioButtonFormatPdf = new System.Windows.Forms.RadioButton();
            this.labelFormat = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(116, 298);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 24);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "&OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(197, 298);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 24);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.Location = new System.Drawing.Point(64, 17);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(208, 20);
            this.textBoxUrl.TabIndex = 3;
            // 
            // labelUrl
            // 
            this.labelUrl.AutoSize = true;
            this.labelUrl.Location = new System.Drawing.Point(8, 20);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(29, 13);
            this.labelUrl.TabIndex = 2;
            this.labelUrl.Text = "URL";
            // 
            // textBoxId
            // 
            this.textBoxId.Location = new System.Drawing.Point(64, 213);
            this.textBoxId.Name = "textBoxId";
            this.textBoxId.Size = new System.Drawing.Size(208, 20);
            this.textBoxId.TabIndex = 7;
            // 
            // labelUserId
            // 
            this.labelUserId.AutoSize = true;
            this.labelUserId.Location = new System.Drawing.Point(8, 216);
            this.labelUserId.Name = "labelUserId";
            this.labelUserId.Size = new System.Drawing.Size(43, 13);
            this.labelUserId.TabIndex = 6;
            this.labelUserId.Text = "User ID";
            // 
            // labelDevice
            // 
            this.labelDevice.AutoSize = true;
            this.labelDevice.Location = new System.Drawing.Point(8, 242);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(41, 13);
            this.labelDevice.TabIndex = 8;
            this.labelDevice.Text = "Device";
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(64, 239);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(208, 21);
            this.comboBoxDevice.TabIndex = 9;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(60, 42);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(195, 20);
            this.textBoxPassword.TabIndex = 3;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(4, 45);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(53, 13);
            this.labelPassword.TabIndex = 2;
            this.labelPassword.Text = "Password";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(60, 16);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(195, 20);
            this.textBoxUsername.TabIndex = 1;
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Location = new System.Drawing.Point(4, 19);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(55, 13);
            this.labelUsername.TabIndex = 0;
            this.labelUsername.Text = "Username";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelUsername);
            this.groupBox1.Controls.Add(this.textBoxUsername);
            this.groupBox1.Controls.Add(this.labelPassword);
            this.groupBox1.Controls.Add(this.textBoxPassword);
            this.groupBox1.Location = new System.Drawing.Point(11, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(261, 68);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Custom barcode->AWB API HTTP Basic Auth";
            // 
            // comboBoxPrinter
            // 
            this.comboBoxPrinter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPrinter.FormattingEnabled = true;
            this.comboBoxPrinter.Location = new System.Drawing.Point(64, 266);
            this.comboBoxPrinter.Name = "comboBoxPrinter";
            this.comboBoxPrinter.Size = new System.Drawing.Size(208, 21);
            this.comboBoxPrinter.TabIndex = 11;
            // 
            // labelPrinter
            // 
            this.labelPrinter.AutoSize = true;
            this.labelPrinter.Location = new System.Drawing.Point(8, 269);
            this.labelPrinter.Name = "labelPrinter";
            this.labelPrinter.Size = new System.Drawing.Size(37, 13);
            this.labelPrinter.TabIndex = 10;
            this.labelPrinter.Text = "Printer";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelFormat);
            this.groupBox2.Controls.Add(this.radioButtonFormatHtml);
            this.groupBox2.Controls.Add(this.radioButtonFormatPdf);
            this.groupBox2.Controls.Add(this.labelFanUsername);
            this.groupBox2.Controls.Add(this.textBoxFanUsername);
            this.groupBox2.Controls.Add(this.labelFanPassword);
            this.groupBox2.Controls.Add(this.textBoxFanPassword);
            this.groupBox2.Location = new System.Drawing.Point(11, 115);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(261, 91);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "FANCourier API (leave empty to reuse from above)";
            // 
            // textBoxFanUsername
            // 
            this.textBoxFanUsername.Location = new System.Drawing.Point(60, 16);
            this.textBoxFanUsername.Name = "textBoxFanUsername";
            this.textBoxFanUsername.Size = new System.Drawing.Size(195, 20);
            this.textBoxFanUsername.TabIndex = 1;
            // 
            // textBoxFanPassword
            // 
            this.textBoxFanPassword.Location = new System.Drawing.Point(60, 42);
            this.textBoxFanPassword.Name = "textBoxFanPassword";
            this.textBoxFanPassword.PasswordChar = '*';
            this.textBoxFanPassword.Size = new System.Drawing.Size(195, 20);
            this.textBoxFanPassword.TabIndex = 3;
            // 
            // labelFanUsername
            // 
            this.labelFanUsername.AutoSize = true;
            this.labelFanUsername.Location = new System.Drawing.Point(4, 19);
            this.labelFanUsername.Name = "labelFanUsername";
            this.labelFanUsername.Size = new System.Drawing.Size(55, 13);
            this.labelFanUsername.TabIndex = 0;
            this.labelFanUsername.Text = "Username";
            // 
            // labelFanPassword
            // 
            this.labelFanPassword.AutoSize = true;
            this.labelFanPassword.Location = new System.Drawing.Point(4, 45);
            this.labelFanPassword.Name = "labelFanPassword";
            this.labelFanPassword.Size = new System.Drawing.Size(53, 13);
            this.labelFanPassword.TabIndex = 2;
            this.labelFanPassword.Text = "Password";
            // 
            // radioButtonFormatHtml
            // 
            this.radioButtonFormatHtml.AutoSize = true;
            this.radioButtonFormatHtml.Location = new System.Drawing.Point(116, 68);
            this.radioButtonFormatHtml.Name = "radioButtonFormatHtml";
            this.radioButtonFormatHtml.Size = new System.Drawing.Size(55, 17);
            this.radioButtonFormatHtml.TabIndex = 6;
            this.radioButtonFormatHtml.TabStop = true;
            this.radioButtonFormatHtml.Text = "HTML";
            this.radioButtonFormatHtml.UseVisualStyleBackColor = true;
            // 
            // radioButtonFormatPdf
            // 
            this.radioButtonFormatPdf.AutoSize = true;
            this.radioButtonFormatPdf.Location = new System.Drawing.Point(63, 68);
            this.radioButtonFormatPdf.Name = "radioButtonFormatPdf";
            this.radioButtonFormatPdf.Size = new System.Drawing.Size(46, 17);
            this.radioButtonFormatPdf.TabIndex = 5;
            this.radioButtonFormatPdf.TabStop = true;
            this.radioButtonFormatPdf.Text = "PDF";
            this.radioButtonFormatPdf.UseVisualStyleBackColor = true;
            // 
            // labelFormat
            // 
            this.labelFormat.AutoSize = true;
            this.labelFormat.Location = new System.Drawing.Point(4, 68);
            this.labelFormat.Name = "labelFormat";
            this.labelFormat.Size = new System.Drawing.Size(39, 13);
            this.labelFormat.TabIndex = 4;
            this.labelFormat.Text = "Format";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(284, 331);
            this.Controls.Add(this.labelUrl);
            this.Controls.Add(this.textBoxUrl);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.labelUserId);
            this.Controls.Add(this.textBoxId);
            this.Controls.Add(this.labelDevice);
            this.Controls.Add(this.comboBoxDevice);
            this.Controls.Add(this.labelPrinter);
            this.Controls.Add(this.comboBoxPrinter);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.Label labelUrl;
        private System.Windows.Forms.TextBox textBoxId;
        private System.Windows.Forms.Label labelUserId;
        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.ComboBox comboBoxDevice;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBoxPrinter;
        private System.Windows.Forms.Label labelPrinter;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxFanUsername;
        private System.Windows.Forms.TextBox textBoxFanPassword;
        private System.Windows.Forms.Label labelFanUsername;
        private System.Windows.Forms.Label labelFanPassword;
        private System.Windows.Forms.Label labelFormat;
        private System.Windows.Forms.RadioButton radioButtonFormatHtml;
        private System.Windows.Forms.RadioButton radioButtonFormatPdf;
    }
}