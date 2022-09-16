namespace OnlineFaturaMobileClient
{
    partial class LoginFRM
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginFRM));
            this.label1 = new System.Windows.Forms.Label();
            this.UserTXT = new System.Windows.Forms.TextBox();
            this.PassTXT = new System.Windows.Forms.TextBox();
            this.inputPanel1 = new Microsoft.WindowsCE.Forms.InputPanel(this.components);
            this.KlavyePB = new System.Windows.Forms.PictureBox();
            this.imageList1 = new System.Windows.Forms.ImageList();
            this.imageList2 = new System.Windows.Forms.ImageList();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ProgramLogoPB = new System.Windows.Forms.PictureBox();
            this.KlavyeBackLBL = new System.Windows.Forms.Label();
            this.imageList3 = new System.Windows.Forms.ImageList();
            this.TamamPB = new System.Windows.Forms.PictureBox();
            this.IptalPB = new System.Windows.Forms.PictureBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.LoginPNL = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox10 = new System.Windows.Forms.PictureBox();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.imageList4 = new System.Windows.Forms.ImageList();
            this.PrintScreenPB = new System.Windows.Forms.PictureBox();
            this.LoginCheckPB = new System.Windows.Forms.PictureBox();
            this.label45 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.LoginPNL.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.DimGray;
            this.label1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(81, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 30);
            this.label1.Text = "Kullanıcı Adı";
            // 
            // UserTXT
            // 
            this.UserTXT.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.UserTXT.Location = new System.Drawing.Point(234, 32);
            this.UserTXT.MaxLength = 10;
            this.UserTXT.Name = "UserTXT";
            this.UserTXT.Size = new System.Drawing.Size(170, 41);
            this.UserTXT.TabIndex = 0;
            this.UserTXT.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.UserTXT_KeyPress);
            // 
            // PassTXT
            // 
            this.PassTXT.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PassTXT.Location = new System.Drawing.Point(234, 86);
            this.PassTXT.MaxLength = 10;
            this.PassTXT.Name = "PassTXT";
            this.PassTXT.PasswordChar = '*';
            this.PassTXT.Size = new System.Drawing.Size(170, 41);
            this.PassTXT.TabIndex = 1;
            this.PassTXT.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PassTXT_KeyPress);
            // 
            // KlavyePB
            // 
            this.KlavyePB.BackColor = System.Drawing.Color.White;
            this.KlavyePB.Image = ((System.Drawing.Image)(resources.GetObject("KlavyePB.Image")));
            this.KlavyePB.Location = new System.Drawing.Point(384, 195);
            this.KlavyePB.Name = "KlavyePB";
            this.KlavyePB.Size = new System.Drawing.Size(60, 50);
            this.KlavyePB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.KlavyePB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.KlavyePB_MouseDown);
            this.KlavyePB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.KlavyePB_MouseUp);
            // 
            // imageList1
            // 
            this.imageList1.ImageSize = new System.Drawing.Size(64, 64);
            this.imageList1.Images.Clear();
            this.imageList1.Images.Add(((System.Drawing.Image)(resources.GetObject("resource"))));
            this.imageList1.Images.Add(((System.Drawing.Image)(resources.GetObject("resource1"))));
            // 
            // imageList2
            // 
            this.imageList2.ImageSize = new System.Drawing.Size(64, 64);
            this.imageList2.Images.Clear();
            this.imageList2.Images.Add(((System.Drawing.Image)(resources.GetObject("resource2"))));
            this.imageList2.Images.Add(((System.Drawing.Image)(resources.GetObject("resource3"))));
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Times New Roman", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.label4.ForeColor = System.Drawing.Color.Navy;
            this.label4.Location = new System.Drawing.Point(102, 448);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(275, 32);
            this.label4.Text = "Süleyman GÜNEL © 2012";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Times New Roman", 10F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(102, 374);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(275, 64);
            this.label3.Text = "Online Fatura Kesme Programı";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgramLogoPB
            // 
            this.ProgramLogoPB.Image = ((System.Drawing.Image)(resources.GetObject("ProgramLogoPB.Image")));
            this.ProgramLogoPB.Location = new System.Drawing.Point(10, 395);
            this.ProgramLogoPB.Name = "ProgramLogoPB";
            this.ProgramLogoPB.Size = new System.Drawing.Size(64, 64);
            this.ProgramLogoPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ProgramLogoPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ProgramLogoPB_MouseDown);
            // 
            // KlavyeBackLBL
            // 
            this.KlavyeBackLBL.Location = new System.Drawing.Point(380, 192);
            this.KlavyeBackLBL.Name = "KlavyeBackLBL";
            this.KlavyeBackLBL.Size = new System.Drawing.Size(68, 55);
            // 
            // imageList3
            // 
            this.imageList3.ImageSize = new System.Drawing.Size(64, 64);
            this.imageList3.Images.Clear();
            this.imageList3.Images.Add(((System.Drawing.Icon)(resources.GetObject("resource4"))));
            this.imageList3.Images.Add(((System.Drawing.Icon)(resources.GetObject("resource5"))));
            // 
            // TamamPB
            // 
            this.TamamPB.BackColor = System.Drawing.Color.White;
            this.TamamPB.Image = ((System.Drawing.Image)(resources.GetObject("TamamPB.Image")));
            this.TamamPB.Location = new System.Drawing.Point(107, 127);
            this.TamamPB.Name = "TamamPB";
            this.TamamPB.Size = new System.Drawing.Size(80, 80);
            this.TamamPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.TamamPB.GotFocus += new System.EventHandler(this.TamamPB_GotFocus);
            this.TamamPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TamamPB_MouseDown);
            this.TamamPB.LostFocus += new System.EventHandler(this.TamamPB_LostFocus);
            this.TamamPB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TamamPB_MouseUp);
            // 
            // IptalPB
            // 
            this.IptalPB.BackColor = System.Drawing.Color.White;
            this.IptalPB.Image = ((System.Drawing.Image)(resources.GetObject("IptalPB.Image")));
            this.IptalPB.Location = new System.Drawing.Point(266, 127);
            this.IptalPB.Name = "IptalPB";
            this.IptalPB.Size = new System.Drawing.Size(80, 80);
            this.IptalPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.IptalPB.GotFocus += new System.EventHandler(this.IptalPB_GotFocus);
            this.IptalPB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IptalPB_MouseDown);
            this.IptalPB.LostFocus += new System.EventHandler(this.IptalPB_LostFocus);
            this.IptalPB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.IptalPB_MouseUp);
            // 
            // label12
            // 
            this.label12.BackColor = System.Drawing.Color.White;
            this.label12.Location = new System.Drawing.Point(114, 229);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(91, 28);
            this.label12.Text = "Tamam";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label13
            // 
            this.label13.BackColor = System.Drawing.Color.White;
            this.label13.Location = new System.Drawing.Point(272, 229);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(91, 28);
            this.label13.Text = "İptal";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label11
            // 
            this.label11.BackColor = System.Drawing.Color.Red;
            this.label11.Location = new System.Drawing.Point(10, 10);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(459, 258);
            // 
            // label43
            // 
            this.label43.BackColor = System.Drawing.Color.Gray;
            this.label43.Location = new System.Drawing.Point(13, 13);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(459, 258);
            // 
            // label44
            // 
            this.label44.BackColor = System.Drawing.Color.Silver;
            this.label44.Location = new System.Drawing.Point(14, 14);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(459, 258);
            // 
            // LoginPNL
            // 
            this.LoginPNL.BackColor = System.Drawing.Color.White;
            this.LoginPNL.Controls.Add(this.pictureBox3);
            this.LoginPNL.Controls.Add(this.pictureBox4);
            this.LoginPNL.Controls.Add(this.pictureBox10);
            this.LoginPNL.Controls.Add(this.pictureBox8);
            this.LoginPNL.Controls.Add(this.IptalPB);
            this.LoginPNL.Controls.Add(this.label15);
            this.LoginPNL.Controls.Add(this.label2);
            this.LoginPNL.Controls.Add(this.label14);
            this.LoginPNL.Controls.Add(this.TamamPB);
            this.LoginPNL.Controls.Add(this.KlavyePB);
            this.LoginPNL.Controls.Add(this.KlavyeBackLBL);
            this.LoginPNL.Location = new System.Drawing.Point(13, 13);
            this.LoginPNL.Name = "LoginPNL";
            this.LoginPNL.Size = new System.Drawing.Size(453, 252);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(61, 107);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(6, 6);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pictureBox4.Location = new System.Drawing.Point(61, 73);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(6, 6);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            // 
            // pictureBox10
            // 
            this.pictureBox10.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox10.Image")));
            this.pictureBox10.Location = new System.Drawing.Point(61, 53);
            this.pictureBox10.Name = "pictureBox10";
            this.pictureBox10.Size = new System.Drawing.Size(6, 6);
            this.pictureBox10.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            // 
            // pictureBox8
            // 
            this.pictureBox8.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox8.Image")));
            this.pictureBox8.Location = new System.Drawing.Point(61, 19);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(6, 6);
            this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            // 
            // label15
            // 
            this.label15.BackColor = System.Drawing.Color.DimGray;
            this.label15.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label15.Location = new System.Drawing.Point(68, 79);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(153, 30);
            this.label15.Text = "Parola";
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.DimGray;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.DarkBlue;
            this.label2.Location = new System.Drawing.Point(61, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 40);
            // 
            // label14
            // 
            this.label14.BackColor = System.Drawing.Color.DimGray;
            this.label14.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold);
            this.label14.ForeColor = System.Drawing.Color.DarkBlue;
            this.label14.Location = new System.Drawing.Point(61, 19);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(160, 40);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(6, 8);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(29, 23);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            // 
            // imageList4
            // 
            this.imageList4.ImageSize = new System.Drawing.Size(80, 80);
            this.imageList4.Images.Clear();
            this.imageList4.Images.Add(((System.Drawing.Image)(resources.GetObject("resource6"))));
            this.imageList4.Images.Add(((System.Drawing.Image)(resources.GetObject("resource7"))));
            this.imageList4.Images.Add(((System.Drawing.Image)(resources.GetObject("resource8"))));
            this.imageList4.Images.Add(((System.Drawing.Image)(resources.GetObject("resource9"))));
            // 
            // PrintScreenPB
            // 
            this.PrintScreenPB.Location = new System.Drawing.Point(405, 275);
            this.PrintScreenPB.Name = "PrintScreenPB";
            this.PrintScreenPB.Size = new System.Drawing.Size(68, 55);
            this.PrintScreenPB.Visible = false;
            // 
            // LoginCheckPB
            // 
            this.LoginCheckPB.Image = ((System.Drawing.Image)(resources.GetObject("LoginCheckPB.Image")));
            this.LoginCheckPB.Location = new System.Drawing.Point(412, 406);
            this.LoginCheckPB.Name = "LoginCheckPB";
            this.LoginCheckPB.Size = new System.Drawing.Size(45, 45);
            this.LoginCheckPB.Visible = false;
            // 
            // label45
            // 
            this.label45.Font = new System.Drawing.Font("Times New Roman", 7F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.label45.ForeColor = System.Drawing.Color.Red;
            this.label45.Location = new System.Drawing.Point(144, 560);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(145, 25);
            this.label45.Text = "DİYARGAZ A.Ş.";
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label10.Location = new System.Drawing.Point(4, 557);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(150, 25);
            this.label10.Text = "Lisanslı Kullanıcı:";
            // 
            // LoginFRM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(480, 588);
            this.Controls.Add(this.label45);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.PrintScreenPB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.PassTXT);
            this.Controls.Add(this.UserTXT);
            this.Controls.Add(this.ProgramLogoPB);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.LoginPNL);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label43);
            this.Controls.Add(this.label44);
            this.Controls.Add(this.LoginCheckPB);
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(0, 52);
            this.Name = "LoginFRM";
            this.Text = "Online Fatura";
            this.Load += new System.EventHandler(this.LoginFRM_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.LoginFRM_Closing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LoginFRM_KeyDown);
            this.LoginPNL.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox UserTXT;
        private System.Windows.Forms.TextBox PassTXT;
        private Microsoft.WindowsCE.Forms.InputPanel inputPanel1;
        private System.Windows.Forms.PictureBox KlavyePB;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox ProgramLogoPB;
        private System.Windows.Forms.Label KlavyeBackLBL;
        private System.Windows.Forms.ImageList imageList3;
        private System.Windows.Forms.PictureBox TamamPB;
        private System.Windows.Forms.PictureBox IptalPB;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.Panel LoginPNL;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ImageList imageList4;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox10;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.PictureBox PrintScreenPB;
        private System.Windows.Forms.PictureBox LoginCheckPB;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label label10;
    }
}