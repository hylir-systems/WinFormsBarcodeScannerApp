namespace WinFormsBarcodeScannerApp
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox _txtBackendLabel;
        private System.Windows.Forms.TextBox _txtBackendUrl;
        private System.Windows.Forms.TextBox _txtA4Label;
        private System.Windows.Forms.TextBox _txtA4Directory;
        private System.Windows.Forms.Button _btnBrowse;
        private System.Windows.Forms.Button _btnReset;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnSave;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this._txtBackendLabel = new System.Windows.Forms.TextBox();
            this._txtBackendUrl = new System.Windows.Forms.TextBox();
            this._txtA4Label = new System.Windows.Forms.TextBox();
            this._txtA4Directory = new System.Windows.Forms.TextBox();
            this._btnBrowse = new System.Windows.Forms.Button();
            this._btnReset = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _txtBackendLabel
            // 
            this._txtBackendLabel.BackColor = System.Drawing.SystemColors.Control;
            this._txtBackendLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtBackendLabel.ForeColor = System.Drawing.Color.Black;
            this._txtBackendLabel.Multiline = true;
            this._txtBackendLabel.ReadOnly = true;
            this._txtBackendLabel.TabStop = false;
            this._txtBackendLabel.Text = "后端服务地址：";
            this._txtBackendLabel.Location = new System.Drawing.Point(14, 14);
            this._txtBackendLabel.Size = new System.Drawing.Size(120, 22);
            // 
            // _txtBackendUrl
            // 
            this._txtBackendUrl.Location = new System.Drawing.Point(14, 38);
            this._txtBackendUrl.Size = new System.Drawing.Size(612, 28);
            this._txtBackendUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _txtA4Label
            // 
            this._txtA4Label.BackColor = System.Drawing.SystemColors.Control;
            this._txtA4Label.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtA4Label.ForeColor = System.Drawing.Color.Black;
            this._txtA4Label.Multiline = true;
            this._txtA4Label.ReadOnly = true;
            this._txtA4Label.TabStop = false;
            this._txtA4Label.Text = "A4 保存目录：";
            this._txtA4Label.Location = new System.Drawing.Point(14, 82);
            this._txtA4Label.Size = new System.Drawing.Size(120, 22);
            // 
            // _txtA4Directory
            // 
            this._txtA4Directory.Location = new System.Drawing.Point(14, 106);
            this._txtA4Directory.Size = new System.Drawing.Size(430, 28);
            this._txtA4Directory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _btnBrowse
            // 
            this._btnBrowse.Text = "浏览...";
            this._btnBrowse.UseVisualStyleBackColor = true;
            this._btnBrowse.Click += new System.EventHandler(this.BtnBrowseOnClick);
            this._btnBrowse.Location = new System.Drawing.Point(454, 104);
            this._btnBrowse.Size = new System.Drawing.Size(90, 30);
            this._btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _btnReset
            // 
            this._btnReset.Text = "恢复默认";
            this._btnReset.UseVisualStyleBackColor = true;
            this._btnReset.Click += new System.EventHandler(this.BtnResetOnClick);
            this._btnReset.Location = new System.Drawing.Point(14, 260);
            this._btnReset.Size = new System.Drawing.Size(100, 30);
            // 
            // _btnCancel
            // 
            this._btnCancel.Text = "取消";
            this._btnCancel.UseVisualStyleBackColor = true;
            this._btnCancel.Location = new System.Drawing.Point(414, 260);
            this._btnCancel.Size = new System.Drawing.Size(100, 30);
            this._btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _btnSave
            // 
            this._btnSave.Text = "保存";
            this._btnSave.UseVisualStyleBackColor = true;
            this._btnSave.Click += new System.EventHandler(this.BtnSaveOnClick);
            this._btnSave.Location = new System.Drawing.Point(526, 260);
            this._btnSave.Size = new System.Drawing.Size(100, 30);
            this._btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // SettingsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 320);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "系统设置";
            // 
            // Controls
            // 
            this.Controls.Add(this._txtBackendLabel);
            this.Controls.Add(this._txtBackendUrl);
            this.Controls.Add(this._txtA4Label);
            this.Controls.Add(this._txtA4Directory);
            this.Controls.Add(this._btnBrowse);
            this.Controls.Add(this._btnReset);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnSave);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}


