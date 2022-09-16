namespace OnlineFaturaMobileClient
{
    partial class EvetHayirFRM
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
            this.RightBTN = new System.Windows.Forms.Button();
            this.LeftBTN = new System.Windows.Forms.Button();
            this.MesajLBL = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // RightBTN
            // 
            this.RightBTN.Location = new System.Drawing.Point(228, 169);
            this.RightBTN.Name = "RightBTN";
            this.RightBTN.Size = new System.Drawing.Size(144, 40);
            this.RightBTN.TabIndex = 3;
            this.RightBTN.Text = "Hayır";
            this.RightBTN.Click += new System.EventHandler(this.RightBTN_Click);
            // 
            // LeftBTN
            // 
            this.LeftBTN.Location = new System.Drawing.Point(26, 169);
            this.LeftBTN.Name = "LeftBTN";
            this.LeftBTN.Size = new System.Drawing.Size(144, 40);
            this.LeftBTN.TabIndex = 2;
            this.LeftBTN.Text = "Evet";
            this.LeftBTN.Click += new System.EventHandler(this.LeftBTN_Click);
            // 
            // MesajLBL
            // 
            this.MesajLBL.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.MesajLBL.ForeColor = System.Drawing.Color.Red;
            this.MesajLBL.Location = new System.Drawing.Point(26, 27);
            this.MesajLBL.Name = "MesajLBL";
            this.MesajLBL.Size = new System.Drawing.Size(348, 126);
            this.MesajLBL.Text = "İnternet bağlantısı sağlanamadı, lütfen bağlantı türünüzü doğrulayın";
            // 
            // EvetHayirFRM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(400, 230);
            this.Controls.Add(this.MesajLBL);
            this.Controls.Add(this.RightBTN);
            this.Controls.Add(this.LeftBTN);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(0, 52);
            this.Name = "EvetHayirFRM";
            this.Text = "Lütfen Seçiniz";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button RightBTN;
        private System.Windows.Forms.Button LeftBTN;
        private System.Windows.Forms.Label MesajLBL;
    }
}