namespace WinFormsBarcodeScannerApp
{
    partial class ImageDetailForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox _txtOrderNo;
        private System.Windows.Forms.TextBox _txtTime;
        private System.Windows.Forms.TextBox _txtUrl;
        private System.Windows.Forms.PictureBox _picture;
        private System.Windows.Forms.Button _btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this._txtOrderNo = new System.Windows.Forms.TextBox();
            this._txtTime = new System.Windows.Forms.TextBox();
            this._txtUrl = new System.Windows.Forms.TextBox();
            this._picture = new System.Windows.Forms.PictureBox();
            this._btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._picture)).BeginInit();
            this.SuspendLayout();
            // 
            // _txtOrderNo
            // 
            this._txtOrderNo.Multiline = true;
            this._txtOrderNo.ReadOnly = true;
            this._txtOrderNo.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._txtOrderNo.Location = new System.Drawing.Point(12, 12);
            this._txtOrderNo.Size = new System.Drawing.Size(976, 24);
            this._txtOrderNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _txtTime
            // 
            this._txtTime.Multiline = true;
            this._txtTime.ReadOnly = true;
            this._txtTime.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._txtTime.Location = new System.Drawing.Point(12, 42);
            this._txtTime.Size = new System.Drawing.Size(976, 24);
            this._txtTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _txtUrl
            // 
            this._txtUrl.Multiline = true;
            this._txtUrl.ReadOnly = true;
            this._txtUrl.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._txtUrl.Location = new System.Drawing.Point(12, 72);
            this._txtUrl.Size = new System.Drawing.Size(976, 24);
            this._txtUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _picture
            // 
            this._picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 填充式适应（等比缩放，完整显示整张图）
            this._picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._picture.TabStop = true;
            this._picture.Location = new System.Drawing.Point(12, 106);
            this._picture.Size = new System.Drawing.Size(976, 580);
            this._picture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // _btnClose
            // 
            this._btnClose.Text = "关闭";
            this._btnClose.UseVisualStyleBackColor = true;
            this._btnClose.Location = new System.Drawing.Point(878, 700);
            this._btnClose.Size = new System.Drawing.Size(110, 34);
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // ImageDetailForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 750);
            this.Name = "ImageDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "图片详情";
            // 
            // Controls
            // 
            this.Controls.Add(this._txtOrderNo);
            this.Controls.Add(this._txtTime);
            this.Controls.Add(this._txtUrl);
            this.Controls.Add(this._picture);
            this.Controls.Add(this._btnClose);
            ((System.ComponentModel.ISupportInitialize)(this._picture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}


