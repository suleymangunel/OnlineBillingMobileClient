namespace OnlineFaturaMobileClient
{
    partial class BaglantiTuruFRM
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.TamamBTN = new System.Windows.Forms.Button();
            this.IptalBTN = new System.Windows.Forms.Button();
            this.DahiliRB = new System.Windows.Forms.RadioButton();
            this.HariciRB = new System.Windows.Forms.RadioButton();
            this.MesajLBL = new System.Windows.Forms.Label();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.SuspendLayout();
            // 
            // TamamBTN
            // 
            this.TamamBTN.Location = new System.Drawing.Point(28, 148);
            this.TamamBTN.Name = "TamamBTN";
            this.TamamBTN.Size = new System.Drawing.Size(144, 40);
            this.TamamBTN.TabIndex = 0;
            this.TamamBTN.Text = "Tamam";
            this.TamamBTN.Click += new System.EventHandler(this.TamamBTN_Click);
            // 
            // IptalBTN
            // 
            this.IptalBTN.Location = new System.Drawing.Point(230, 148);
            this.IptalBTN.Name = "IptalBTN";
            this.IptalBTN.Size = new System.Drawing.Size(144, 40);
            this.IptalBTN.TabIndex = 1;
            this.IptalBTN.Text = "İptal";
            this.IptalBTN.Click += new System.EventHandler(this.IptalBTN_Click);
            // 
            // DahiliRB
            // 
            this.DahiliRB.Location = new System.Drawing.Point(28, 83);
            this.DahiliRB.Name = "DahiliRB";
            this.DahiliRB.Size = new System.Drawing.Size(111, 40);
            this.DahiliRB.TabIndex = 2;
            this.DahiliRB.Text = "Dahili";
            // 
            // HariciRB
            // 
            this.HariciRB.Location = new System.Drawing.Point(263, 83);
            this.HariciRB.Name = "HariciRB";
            this.HariciRB.Size = new System.Drawing.Size(111, 40);
            this.HariciRB.TabIndex = 3;
            this.HariciRB.Text = "Harici";
            // 
            // MesajLBL
            // 
            this.MesajLBL.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.MesajLBL.ForeColor = System.Drawing.Color.Red;
            this.MesajLBL.Location = new System.Drawing.Point(28, 18);
            this.MesajLBL.Name = "MesajLBL";
            this.MesajLBL.Size = new System.Drawing.Size(348, 59);
            this.MesajLBL.Text = "İnternet bağlantısı sağlanamadı, lütfen bağlantı türünüzü doğrulayın";
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 212);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(400, 38);
            this.statusBar1.Text = "Craddle: Var / Internet: Var";
            // 
            // BaglantiTuruFRM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.ControlBox = false;
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.MesajLBL);
            this.Controls.Add(this.HariciRB);
            this.Controls.Add(this.DahiliRB);
            this.Controls.Add(this.IptalBTN);
            this.Controls.Add(this.TamamBTN);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(0, 52);
            this.Name = "BaglantiTuruFRM";
            this.Text = "Bağlantı Türü";
            this.Load += new System.EventHandler(this.BaglantiTuruFRM_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button TamamBTN;
        private System.Windows.Forms.Button IptalBTN;
        private System.Windows.Forms.RadioButton DahiliRB;
        private System.Windows.Forms.RadioButton HariciRB;
        private System.Windows.Forms.Label MesajLBL;
        private System.Windows.Forms.StatusBar statusBar1;
    }
}