namespace WinFormsBarcodeScannerApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Title bar (HBox): left / filler / right
        private System.Windows.Forms.TextBox _txtTitleBarBg;
        private System.Windows.Forms.PictureBox _picAppIcon;
        private System.Windows.Forms.TextBox _txtMainTitle;
        private System.Windows.Forms.TextBox _txtSubTitle;
        private System.Windows.Forms.PictureBox _picCameraIcon;
        private System.Windows.Forms.TextBox _txtDeviceName;
        private System.Windows.Forms.TextBox _txtSystemStatus;
        private System.Windows.Forms.Button _btnSettings;

        // Main content (2 columns): left (main) + right (fixed width)
        private System.Windows.Forms.Label _lblLiveHeader;
        private System.Windows.Forms.Label _lblProgressHeader;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.TextBox _txtProgressStage;

        private System.Windows.Forms.Label _lblHistoryHeader;
        private System.Windows.Forms.Label _lblLogHeader;
        private System.Windows.Forms.TextBox _txtUiLog;

        private System.Windows.Forms.PictureBox _pictureBox;
        private System.Windows.Forms.ComboBox _comboCameras;
        private System.Windows.Forms.Button _btnStart;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnRecognize;
        private System.Windows.Forms.TextBox _txtResult;

        // 历史列表（需要手动添加到 Designer）
        private System.Windows.Forms.ListView _historyListView;
        private System.Windows.Forms.ImageList _historyImageList;

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            _txtTitleBarBg = new System.Windows.Forms.TextBox();
            _picAppIcon = new System.Windows.Forms.PictureBox();
            _txtMainTitle = new System.Windows.Forms.TextBox();
            _txtSubTitle = new System.Windows.Forms.TextBox();
            _picCameraIcon = new System.Windows.Forms.PictureBox();
            _txtDeviceName = new System.Windows.Forms.TextBox();
            _txtSystemStatus = new System.Windows.Forms.TextBox();
            _btnSettings = new System.Windows.Forms.Button();
            _lblLiveHeader = new System.Windows.Forms.Label();
            _comboCameras = new System.Windows.Forms.ComboBox();
            _btnStart = new System.Windows.Forms.Button();
            _btnStop = new System.Windows.Forms.Button();
            _btnRecognize = new System.Windows.Forms.Button();
            _pictureBox = new System.Windows.Forms.PictureBox();
            _lblProgressHeader = new System.Windows.Forms.Label();
            _progressBar = new System.Windows.Forms.ProgressBar();
            _txtProgressStage = new System.Windows.Forms.TextBox();
            _lblHistoryHeader = new System.Windows.Forms.Label();
            _lblLogHeader = new System.Windows.Forms.Label();
            _txtUiLog = new System.Windows.Forms.TextBox();
            _txtResult = new System.Windows.Forms.TextBox();
            _historyListView = new System.Windows.Forms.ListView();
            _historyImageList = new System.Windows.Forms.ImageList(components);
            ((System.ComponentModel.ISupportInitialize)_picAppIcon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_picCameraIcon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_pictureBox).BeginInit();
            SuspendLayout();
            // 
            // _txtTitleBarBg
            // 
            _txtTitleBarBg.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _txtTitleBarBg.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _txtTitleBarBg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _txtTitleBarBg.ForeColor = System.Drawing.Color.White;
            _txtTitleBarBg.Location = new System.Drawing.Point(0, 0);
            _txtTitleBarBg.Margin = new System.Windows.Forms.Padding(4);
            _txtTitleBarBg.Multiline = true;
            _txtTitleBarBg.Name = "_txtTitleBarBg";
            _txtTitleBarBg.ReadOnly = true;
            _txtTitleBarBg.Size = new System.Drawing.Size(1000, 83);
            _txtTitleBarBg.TabIndex = 0;
            _txtTitleBarBg.TabStop = false;
            // 
            // _picAppIcon
            // 
            _picAppIcon.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _picAppIcon.Image = (System.Drawing.Image)resources.GetObject("_picAppIcon.Image");
            _picAppIcon.Location = new System.Drawing.Point(12, 15);
            _picAppIcon.Margin = new System.Windows.Forms.Padding(4);
            _picAppIcon.Name = "_picAppIcon";
            _picAppIcon.Size = new System.Drawing.Size(42, 45);
            _picAppIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            _picAppIcon.TabIndex = 1;
            _picAppIcon.TabStop = false;
            // 
            // _txtMainTitle
            // 
            _txtMainTitle.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _txtMainTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _txtMainTitle.ForeColor = System.Drawing.Color.White;
            _txtMainTitle.Location = new System.Drawing.Point(66, 13);
            _txtMainTitle.Margin = new System.Windows.Forms.Padding(4);
            _txtMainTitle.Multiline = true;
            _txtMainTitle.Name = "_txtMainTitle";
            _txtMainTitle.ReadOnly = true;
            _txtMainTitle.Size = new System.Drawing.Size(318, 29);
            _txtMainTitle.TabIndex = 2;
            _txtMainTitle.TabStop = false;
            _txtMainTitle.Text = "回单采集终端";
            // 
            // _txtSubTitle
            // 
            _txtSubTitle.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _txtSubTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _txtSubTitle.ForeColor = System.Drawing.Color.White;
            _txtSubTitle.Location = new System.Drawing.Point(66, 43);
            _txtSubTitle.Margin = new System.Windows.Forms.Padding(4);
            _txtSubTitle.Multiline = true;
            _txtSubTitle.Name = "_txtSubTitle";
            _txtSubTitle.ReadOnly = true;
            _txtSubTitle.Size = new System.Drawing.Size(318, 24);
            _txtSubTitle.TabIndex = 3;
            _txtSubTitle.TabStop = false;
            _txtSubTitle.Text = "Receipt Capture Agent";
            // 
            // _picCameraIcon
            // 
            _picCameraIcon.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _picCameraIcon.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _picCameraIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _picCameraIcon.Location = new System.Drawing.Point(1644, 25);
            _picCameraIcon.Margin = new System.Windows.Forms.Padding(4);
            _picCameraIcon.Name = "_picCameraIcon";
            _picCameraIcon.Size = new System.Drawing.Size(22, 23);
            _picCameraIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _picCameraIcon.TabIndex = 4;
            _picCameraIcon.TabStop = false;
            // 
            // _txtDeviceName
            // 
            _txtDeviceName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _txtDeviceName.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _txtDeviceName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _txtDeviceName.ForeColor = System.Drawing.Color.White;
            _txtDeviceName.Location = new System.Drawing.Point(1425, 25);
            _txtDeviceName.Margin = new System.Windows.Forms.Padding(4);
            _txtDeviceName.Multiline = true;
            _txtDeviceName.Name = "_txtDeviceName";
            _txtDeviceName.ReadOnly = true;
            _txtDeviceName.Size = new System.Drawing.Size(220, 24);
            _txtDeviceName.TabIndex = 5;
            _txtDeviceName.TabStop = false;
            _txtDeviceName.Text = "未选择设备";
            // 
            // _txtSystemStatus
            // 
            _txtSystemStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _txtSystemStatus.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _txtSystemStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _txtSystemStatus.ForeColor = System.Drawing.Color.White;
            _txtSystemStatus.Location = new System.Drawing.Point(1712, 25);
            _txtSystemStatus.Margin = new System.Windows.Forms.Padding(4);
            _txtSystemStatus.Multiline = true;
            _txtSystemStatus.Name = "_txtSystemStatus";
            _txtSystemStatus.ReadOnly = true;
            _txtSystemStatus.Size = new System.Drawing.Size(68, 24);
            _txtSystemStatus.TabIndex = 6;
            _txtSystemStatus.TabStop = false;
            _txtSystemStatus.Text = "就绪";
            // 
            // _btnSettings
            // 
            _btnSettings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _btnSettings.BackColor = System.Drawing.Color.FromArgb(57, 70, 100);
            _btnSettings.FlatAppearance.BorderSize = 0;
            _btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnSettings.Font = new System.Drawing.Font("Segoe UI Symbol", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            _btnSettings.ForeColor = System.Drawing.Color.White;
            _btnSettings.Location = new System.Drawing.Point(1815, 11);
            _btnSettings.Margin = new System.Windows.Forms.Padding(4);
            _btnSettings.Name = "_btnSettings";
            _btnSettings.Size = new System.Drawing.Size(123, 45);
            _btnSettings.TabIndex = 7;
            _btnSettings.Text = "⚙设置";
            _btnSettings.UseVisualStyleBackColor = false;
            _btnSettings.Click += BtnSettingsOnClick;
            // 
            // _lblLiveHeader
            // 
            _lblLiveHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            _lblLiveHeader.ForeColor = System.Drawing.Color.Black;
            _lblLiveHeader.Location = new System.Drawing.Point(15, 91);
            _lblLiveHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _lblLiveHeader.Name = "_lblLiveHeader";
            _lblLiveHeader.Size = new System.Drawing.Size(110, 45);
            _lblLiveHeader.TabIndex = 8;
            _lblLiveHeader.Text = "实时影像";
            _lblLiveHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _comboCameras
            // 
            _comboCameras.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _comboCameras.Location = new System.Drawing.Point(134, 96);
            _comboCameras.Margin = new System.Windows.Forms.Padding(4);
            _comboCameras.Name = "_comboCameras";
            _comboCameras.Size = new System.Drawing.Size(305, 32);
            _comboCameras.TabIndex = 9;
            _comboCameras.SelectedIndexChanged += ComboCamerasOnSelectedIndexChanged;
            // 
            // _btnStart
            // 
            _btnStart.Location = new System.Drawing.Point(452, 91);
            _btnStart.Margin = new System.Windows.Forms.Padding(4);
            _btnStart.Name = "_btnStart";
            _btnStart.Size = new System.Drawing.Size(110, 45);
            _btnStart.TabIndex = 10;
            _btnStart.Text = "预览";
            _btnStart.UseVisualStyleBackColor = true;
            _btnStart.Click += BtnStartOnClick;
            // 
            // _btnStop
            // 
            _btnStop.Location = new System.Drawing.Point(572, 91);
            _btnStop.Margin = new System.Windows.Forms.Padding(4);
            _btnStop.Name = "_btnStop";
            _btnStop.Size = new System.Drawing.Size(110, 45);
            _btnStop.TabIndex = 11;
            _btnStop.Text = "重置";
            _btnStop.UseVisualStyleBackColor = true;
            _btnStop.Click += BtnStopOnClick;
            // 
            // _btnRecognize
            // 
            _btnRecognize.Location = new System.Drawing.Point(692, 91);
            _btnRecognize.Margin = new System.Windows.Forms.Padding(4);
            _btnRecognize.Name = "_btnRecognize";
            _btnRecognize.Size = new System.Drawing.Size(110, 45);
            _btnRecognize.TabIndex = 12;
            _btnRecognize.Text = "识别";
            _btnRecognize.UseVisualStyleBackColor = true;
            _btnRecognize.Click += BtnRecognizeOnClick;
            // 
            // _pictureBox
            // 
            _pictureBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _pictureBox.Location = new System.Drawing.Point(15, 147);
            _pictureBox.Margin = new System.Windows.Forms.Padding(4);
            _pictureBox.Name = "_pictureBox";
            _pictureBox.Size = new System.Drawing.Size(1403, 917);
            _pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            _pictureBox.TabIndex = 13;
            _pictureBox.TabStop = false;
            // 
            // _lblProgressHeader
            // 
            _lblProgressHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            _lblProgressHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _lblProgressHeader.ForeColor = System.Drawing.Color.Black;
            _lblProgressHeader.Location = new System.Drawing.Point(15, 731);
            _lblProgressHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _lblProgressHeader.Name = "_lblProgressHeader";
            _lblProgressHeader.Size = new System.Drawing.Size(73, 61);
            _lblProgressHeader.TabIndex = 14;
            _lblProgressHeader.Text = "进度";
            _lblProgressHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _progressBar
            // 
            _progressBar.Location = new System.Drawing.Point(98, 736);
            _progressBar.Margin = new System.Windows.Forms.Padding(4);
            _progressBar.Name = "_progressBar";
            _progressBar.Size = new System.Drawing.Size(631, 21);
            _progressBar.TabIndex = 15;
            // 
            // _txtProgressStage
            // 
            _txtProgressStage.BackColor = System.Drawing.Color.White;
            _txtProgressStage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _txtProgressStage.ForeColor = System.Drawing.Color.Black;
            _txtProgressStage.Location = new System.Drawing.Point(98, 763);
            _txtProgressStage.Margin = new System.Windows.Forms.Padding(4);
            _txtProgressStage.Multiline = true;
            _txtProgressStage.Name = "_txtProgressStage";
            _txtProgressStage.ReadOnly = true;
            _txtProgressStage.Size = new System.Drawing.Size(630, 26);
            _txtProgressStage.TabIndex = 16;
            _txtProgressStage.TabStop = false;
            _txtProgressStage.TextChanged += _txtProgressStage_TextChanged;
            // 
            // _lblHistoryHeader
            // 
            _lblHistoryHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            _lblHistoryHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _lblHistoryHeader.ForeColor = System.Drawing.Color.Black;
            _lblHistoryHeader.Location = new System.Drawing.Point(1425, 83);
            _lblHistoryHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _lblHistoryHeader.Name = "_lblHistoryHeader";
            _lblHistoryHeader.Size = new System.Drawing.Size(464, 29);
            _lblHistoryHeader.TabIndex = 17;
            _lblHistoryHeader.Text = "扫描历史";
            _lblHistoryHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblLogHeader
            // 
            _lblLogHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            _lblLogHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _lblLogHeader.ForeColor = System.Drawing.Color.Black;
            _lblLogHeader.Location = new System.Drawing.Point(1425, 707);
            _lblLogHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _lblLogHeader.Name = "_lblLogHeader";
            _lblLogHeader.Size = new System.Drawing.Size(464, 29);
            _lblLogHeader.TabIndex = 19;
            _lblLogHeader.Text = "操作日志";
            _lblLogHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _txtUiLog
            // 
            _txtUiLog.Location = new System.Drawing.Point(1425, 763);
            _txtUiLog.Margin = new System.Windows.Forms.Padding(4);
            _txtUiLog.Multiline = true;
            _txtUiLog.Name = "_txtUiLog";
            _txtUiLog.ReadOnly = true;
            _txtUiLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            _txtUiLog.Size = new System.Drawing.Size(464, 289);
            _txtUiLog.TabIndex = 20;
            // 
            // _txtResult
            // 
            _txtResult.Location = new System.Drawing.Point(15, 800);
            _txtResult.Margin = new System.Windows.Forms.Padding(4);
            _txtResult.Multiline = true;
            _txtResult.Name = "_txtResult";
            _txtResult.ReadOnly = true;
            _txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            _txtResult.Size = new System.Drawing.Size(713, 119);
            _txtResult.TabIndex = 21;
            // 
            // _historyListView
            // 
            _historyListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _historyListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _historyListView.FullRowSelect = true;
            _historyListView.GridLines = true;
            _historyListView.Location = new System.Drawing.Point(1425, 116);
            _historyListView.Margin = new System.Windows.Forms.Padding(4);
            _historyListView.MultiSelect = false;
            _historyListView.Name = "_historyListView";
            _historyListView.Size = new System.Drawing.Size(464, 572);
            _historyListView.TabIndex = 22;
            _historyListView.UseCompatibleStateImageBehavior = false;
            _historyListView.View = System.Windows.Forms.View.Tile;
            // 
            // _historyImageList
            // 
            _historyImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            _historyImageList.ImageSize = new System.Drawing.Size(64, 48);
            _historyImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1953, 1251);
            Controls.Add(_historyListView);
            Controls.Add(_btnSettings);
            Controls.Add(_picAppIcon);
            Controls.Add(_txtMainTitle);
            Controls.Add(_txtSubTitle);
            Controls.Add(_picCameraIcon);
            Controls.Add(_txtDeviceName);
            Controls.Add(_txtSystemStatus);
            Controls.Add(_lblLiveHeader);
            Controls.Add(_comboCameras);
            Controls.Add(_btnStart);
            Controls.Add(_btnStop);
            Controls.Add(_btnRecognize);
            Controls.Add(_pictureBox);
            Controls.Add(_lblProgressHeader);
            Controls.Add(_progressBar);
            Controls.Add(_txtProgressStage);
            Controls.Add(_lblHistoryHeader);
            Controls.Add(_lblLogHeader);
            Controls.Add(_txtUiLog);
            Controls.Add(_txtTitleBarBg);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "WinForms Barcode Scanner (AForge + Aspose)";
            FormClosing += OnFormClosing;
            Load += OnLoad;
            Resize += OnResize;
            ((System.ComponentModel.ISupportInitialize)_picAppIcon).EndInit();
            ((System.ComponentModel.ISupportInitialize)_picCameraIcon).EndInit();
            ((System.ComponentModel.ISupportInitialize)_pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}


