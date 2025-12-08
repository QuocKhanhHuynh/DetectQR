namespace DetectQRCode
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picCamera = new DetectQRCode.OCR.Utils.OverlayPictureBox();
            this.picPreprocessed = new System.Windows.Forms.PictureBox();
            this.cmbCameraDevices = new System.Windows.Forms.ComboBox();
            this.btnStartCamera = new System.Windows.Forms.Button();
            this.btnStopCamera = new System.Windows.Forms.Button();
            this.lblQRCode = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblColor = new System.Windows.Forms.Label();
            this.lblProductCode = new System.Windows.Forms.Label();
            this.lblProductTotal = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblFPS = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSaveFrame = new System.Windows.Forms.Button();
            this.btnSaveNghieng = new System.Windows.Forms.Button();
            this.btnSaveMo = new System.Windows.Forms.Button();
            this.btnSaveCong = new System.Windows.Forms.Button();
            this.btnSaveNho = new System.Windows.Forms.Button();
            this.rbCamera = new System.Windows.Forms.RadioButton();
            this.rbImportImage = new System.Windows.Forms.RadioButton();
            this.btnBrowseImage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreprocessed)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picCamera
            // 
            this.picCamera.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picCamera.BackColor = System.Drawing.Color.Black;
            this.picCamera.GuideBox = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.picCamera.IsObjectDetected = false;
            this.picCamera.Location = new System.Drawing.Point(12, 90);
            this.picCamera.Name = "picCamera";
            this.picCamera.Size = new System.Drawing.Size(960, 540);
            this.picCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCamera.TabIndex = 0;
            this.picCamera.TabStop = false;
            // 
            // picPreprocessed
            // 
            this.picPreprocessed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picPreprocessed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.picPreprocessed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreprocessed.Location = new System.Drawing.Point(990, 90);
            this.picPreprocessed.Name = "picPreprocessed";
            this.picPreprocessed.Size = new System.Drawing.Size(290, 220);
            this.picPreprocessed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreprocessed.TabIndex = 16;
            this.picPreprocessed.TabStop = false;
            // 
            // cmbCameraDevices
            // 
            this.cmbCameraDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCameraDevices.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCameraDevices.FormattingEnabled = true;
            this.cmbCameraDevices.Location = new System.Drawing.Point(132, 15);
            this.cmbCameraDevices.Name = "cmbCameraDevices";
            this.cmbCameraDevices.Size = new System.Drawing.Size(350, 31);
            this.cmbCameraDevices.TabIndex = 1;
            // 
            // btnStartCamera
            // 
            this.btnStartCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnStartCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartCamera.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartCamera.ForeColor = System.Drawing.Color.White;
            this.btnStartCamera.Location = new System.Drawing.Point(500, 12);
            this.btnStartCamera.Name = "btnStartCamera";
            this.btnStartCamera.Size = new System.Drawing.Size(120, 38);
            this.btnStartCamera.TabIndex = 2;
            this.btnStartCamera.Text = "Start Camera";
            this.btnStartCamera.UseVisualStyleBackColor = false;
            this.btnStartCamera.Click += new System.EventHandler(this.btnStartCamera_Click);
            // 
            // btnStopCamera
            // 
            this.btnStopCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnStopCamera.Enabled = false;
            this.btnStopCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopCamera.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopCamera.ForeColor = System.Drawing.Color.White;
            this.btnStopCamera.Location = new System.Drawing.Point(635, 12);
            this.btnStopCamera.Name = "btnStopCamera";
            this.btnStopCamera.Size = new System.Drawing.Size(120, 38);
            this.btnStopCamera.TabIndex = 3;
            this.btnStopCamera.Text = "Stop Camera";
            this.btnStopCamera.UseVisualStyleBackColor = false;
            this.btnStopCamera.Click += new System.EventHandler(this.btnStopCamera_Click);
            // 
            // lblQRCode
            // 
            this.lblQRCode.AutoSize = true;
            this.lblQRCode.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblQRCode.Location = new System.Drawing.Point(10, 10);
            this.lblQRCode.Name = "lblQRCode";
            this.lblQRCode.Size = new System.Drawing.Size(105, 28);
            this.lblQRCode.TabIndex = 4;
            this.lblQRCode.Text = "QR: N/A";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblSize.Location = new System.Drawing.Point(10, 45);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(95, 25);
            this.lblSize.TabIndex = 5;
            this.lblSize.Text = "Size: N/A";
            // 
            // lblColor
            // 
            this.lblColor.AutoSize = true;
            this.lblColor.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblColor.Location = new System.Drawing.Point(10, 75);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(107, 25);
            this.lblColor.TabIndex = 6;
            this.lblColor.Text = "Color: N/A";
            // 
            // lblProductCode
            // 
            this.lblProductCode.AutoSize = true;
            this.lblProductCode.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblProductCode.Location = new System.Drawing.Point(10, 105);
            this.lblProductCode.Name = "lblProductCode";
            this.lblProductCode.Size = new System.Drawing.Size(132, 25);
            this.lblProductCode.TabIndex = 7;
            this.lblProductCode.Text = "Product: N/A";
            // 
            // lblProductTotal
            // 
            this.lblProductTotal.AutoSize = true;
            this.lblProductTotal.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblProductTotal.Location = new System.Drawing.Point(10, 135);
            this.lblProductTotal.Name = "lblProductTotal";
            this.lblProductTotal.Size = new System.Drawing.Size(103, 25);
            this.lblProductTotal.TabIndex = 8;
            this.lblProductTotal.Text = "Total: N/A";
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Location = new System.Drawing.Point(12, 640);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(760, 25);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFPS
            // 
            this.lblFPS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFPS.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFPS.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblFPS.Location = new System.Drawing.Point(780, 640);
            this.lblFPS.Name = "lblFPS";
            this.lblFPS.Size = new System.Drawing.Size(190, 25);
            this.lblFPS.TabIndex = 21;
            this.lblFPS.Text = "FPS: 0.0 | Processed: 0.0";
            this.lblFPS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblQRCode);
            this.panel1.Controls.Add(this.lblSize);
            this.panel1.Controls.Add(this.lblColor);
            this.panel1.Controls.Add(this.lblProductCode);
            this.panel1.Controls.Add(this.lblProductTotal);
            this.panel1.Location = new System.Drawing.Point(990, 320);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(290, 310);
            this.panel1.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 23);
            this.label1.TabIndex = 11;
            this.label1.Text = "Camera:";
            // 
            // btnSaveFrame
            // 
            this.btnSaveFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnSaveFrame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveFrame.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveFrame.ForeColor = System.Drawing.Color.White;
            this.btnSaveFrame.Location = new System.Drawing.Point(910, 12);
            this.btnSaveFrame.Name = "btnSaveFrame";
            this.btnSaveFrame.Size = new System.Drawing.Size(90, 38);
            this.btnSaveFrame.TabIndex = 12;
            this.btnSaveFrame.Text = "Lưu";
            this.btnSaveFrame.UseVisualStyleBackColor = false;
            this.btnSaveFrame.Click += new System.EventHandler(this.btnSaveFrame_Click);
            // 
            // btnSaveNghieng
            // 
            this.btnSaveNghieng.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.btnSaveNghieng.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveNghieng.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveNghieng.ForeColor = System.Drawing.Color.White;
            this.btnSaveNghieng.Location = new System.Drawing.Point(12, 645);
            this.btnSaveNghieng.Name = "btnSaveNghieng";
            this.btnSaveNghieng.Size = new System.Drawing.Size(110, 35);
            this.btnSaveNghieng.TabIndex = 17;
            this.btnSaveNghieng.Text = "💾 Nghiêng";
            this.btnSaveNghieng.UseVisualStyleBackColor = false;
            this.btnSaveNghieng.Click += new System.EventHandler(this.btnSaveNghieng_Click);
            // 
            // btnSaveMo
            // 
            this.btnSaveMo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnSaveMo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveMo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveMo.ForeColor = System.Drawing.Color.White;
            this.btnSaveMo.Location = new System.Drawing.Point(130, 645);
            this.btnSaveMo.Name = "btnSaveMo";
            this.btnSaveMo.Size = new System.Drawing.Size(110, 35);
            this.btnSaveMo.TabIndex = 18;
            this.btnSaveMo.Text = "💾 Mờ";
            this.btnSaveMo.UseVisualStyleBackColor = false;
            this.btnSaveMo.Click += new System.EventHandler(this.btnSaveMo_Click);
            // 
            // btnSaveCong
            // 
            this.btnSaveCong.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnSaveCong.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveCong.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveCong.ForeColor = System.Drawing.Color.White;
            this.btnSaveCong.Location = new System.Drawing.Point(248, 645);
            this.btnSaveCong.Name = "btnSaveCong";
            this.btnSaveCong.Size = new System.Drawing.Size(110, 35);
            this.btnSaveCong.TabIndex = 19;
            this.btnSaveCong.Text = "💾 Cong";
            this.btnSaveCong.UseVisualStyleBackColor = false;
            this.btnSaveCong.Click += new System.EventHandler(this.btnSaveCong_Click);
            // 
            // btnSaveNho
            // 
            this.btnSaveNho.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSaveNho.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveNho.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveNho.ForeColor = System.Drawing.Color.White;
            this.btnSaveNho.Location = new System.Drawing.Point(366, 645);
            this.btnSaveNho.Name = "btnSaveNho";
            this.btnSaveNho.Size = new System.Drawing.Size(110, 35);
            this.btnSaveNho.TabIndex = 20;
            this.btnSaveNho.Text = "💾 Nhỏ";
            this.btnSaveNho.UseVisualStyleBackColor = false;
            this.btnSaveNho.Click += new System.EventHandler(this.btnSaveNho_Click);
            // 
            // rbCamera
            // 
            this.rbCamera.AutoSize = true;
            this.rbCamera.Checked = true;
            this.rbCamera.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.rbCamera.Location = new System.Drawing.Point(12, 60);
            this.rbCamera.Name = "rbCamera";
            this.rbCamera.Size = new System.Drawing.Size(97, 27);
            this.rbCamera.TabIndex = 13;
            this.rbCamera.TabStop = true;
            this.rbCamera.Text = "Camera";
            this.rbCamera.UseVisualStyleBackColor = true;
            this.rbCamera.CheckedChanged += new System.EventHandler(this.rbCamera_CheckedChanged);
            // 
            // rbImportImage
            // 
            this.rbImportImage.AutoSize = true;
            this.rbImportImage.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.rbImportImage.Location = new System.Drawing.Point(125, 60);
            this.rbImportImage.Name = "rbImportImage";
            this.rbImportImage.Size = new System.Drawing.Size(149, 27);
            this.rbImportImage.TabIndex = 14;
            this.rbImportImage.Text = "Import Image";
            this.rbImportImage.UseVisualStyleBackColor = true;
            this.rbImportImage.CheckedChanged += new System.EventHandler(this.rbImportImage_CheckedChanged);
            // 
            // btnBrowseImage
            // 
            this.btnBrowseImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnBrowseImage.Enabled = false;
            this.btnBrowseImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseImage.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnBrowseImage.ForeColor = System.Drawing.Color.White;
            this.btnBrowseImage.Location = new System.Drawing.Point(290, 56);
            this.btnBrowseImage.Name = "btnBrowseImage";
            this.btnBrowseImage.Size = new System.Drawing.Size(150, 35);
            this.btnBrowseImage.TabIndex = 15;
            this.btnBrowseImage.Text = "Browse Image...";
            this.btnBrowseImage.UseVisualStyleBackColor = false;
            this.btnBrowseImage.Click += new System.EventHandler(this.btnBrowseImage_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1300, 690);
            this.Controls.Add(this.btnSaveNho);
            this.Controls.Add(this.btnSaveCong);
            this.Controls.Add(this.btnSaveMo);
            this.Controls.Add(this.btnSaveNghieng);
            this.Controls.Add(this.lblFPS);
            this.Controls.Add(this.btnBrowseImage);
            this.Controls.Add(this.rbImportImage);
            this.Controls.Add(this.rbCamera);
            this.Controls.Add(this.btnSaveFrame);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.picPreprocessed);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnStopCamera);
            this.Controls.Add(this.btnStartCamera);
            this.Controls.Add(this.cmbCameraDevices);
            this.Controls.Add(this.picCamera);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OCR Label Detector - Camera";
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreprocessed)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DetectQRCode.OCR.Utils.OverlayPictureBox picCamera;
        public System.Windows.Forms.PictureBox picPreprocessed;
        private System.Windows.Forms.ComboBox cmbCameraDevices;
        private System.Windows.Forms.Button btnStartCamera;
        private System.Windows.Forms.Button btnStopCamera;
        private System.Windows.Forms.Label lblQRCode;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Label lblProductCode;
        private System.Windows.Forms.Label lblProductTotal;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblFPS;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSaveFrame;
        private System.Windows.Forms.Button btnSaveNghieng;
        private System.Windows.Forms.Button btnSaveMo;
        private System.Windows.Forms.Button btnSaveCong;
        private System.Windows.Forms.Button btnSaveNho;
        private System.Windows.Forms.RadioButton rbCamera;
        private System.Windows.Forms.RadioButton rbImportImage;
        private System.Windows.Forms.Button btnBrowseImage;
    }
}
