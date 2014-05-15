namespace _3DViewer
{
    partial class FormRobot_multiObj
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.trackBar4 = new System.Windows.Forms.TrackBar();
            this.trackBar5 = new System.Windows.Forms.TrackBar();
            this.trackBar6 = new System.Windows.Forms.TrackBar();
            this.glControl1 = new OpenTK.GLControl();
            this.panel_ik = new System.Windows.Forms.Panel();
            this.btnDecrease = new System.Windows.Forms.Button();
            this.btnIncrease = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.yaw = new System.Windows.Forms.RadioButton();
            this.pitch = new System.Windows.Forms.RadioButton();
            this.roll = new System.Windows.Forms.RadioButton();
            this.z = new System.Windows.Forms.RadioButton();
            this.y = new System.Windows.Forms.RadioButton();
            this.x = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.Coord_panel = new System.Windows.Forms.Panel();
            this.rdBtn1 = new System.Windows.Forms.RadioButton();
            this.rdBtn2 = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar6)).BeginInit();
            this.panel_ik.SuspendLayout();
            this.Coord_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 254F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.glControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel_ik, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.button1, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.03846F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.96154F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(618, 465);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.trackBar1);
            this.flowLayoutPanel2.Controls.Add(this.trackBar2);
            this.flowLayoutPanel2.Controls.Add(this.trackBar3);
            this.flowLayoutPanel2.Controls.Add(this.trackBar4);
            this.flowLayoutPanel2.Controls.Add(this.trackBar5);
            this.flowLayoutPanel2.Controls.Add(this.trackBar6);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(367, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(248, 282);
            this.flowLayoutPanel2.TabIndex = 6;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(3, 3);
            this.trackBar1.Maximum = 180;
            this.trackBar1.Minimum = -180;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(197, 45);
            this.trackBar1.TabIndex = 8;
            // 
            // trackBar2
            // 
            this.trackBar2.Location = new System.Drawing.Point(3, 54);
            this.trackBar2.Maximum = 110;
            this.trackBar2.Minimum = -110;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(197, 45);
            this.trackBar2.TabIndex = 9;
            // 
            // trackBar3
            // 
            this.trackBar3.Location = new System.Drawing.Point(3, 105);
            this.trackBar3.Maximum = 245;
            this.trackBar3.Minimum = -65;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Size = new System.Drawing.Size(197, 45);
            this.trackBar3.TabIndex = 10;
            // 
            // trackBar4
            // 
            this.trackBar4.Location = new System.Drawing.Point(3, 156);
            this.trackBar4.Maximum = 360;
            this.trackBar4.Minimum = -360;
            this.trackBar4.Name = "trackBar4";
            this.trackBar4.Size = new System.Drawing.Size(197, 45);
            this.trackBar4.TabIndex = 11;
            // 
            // trackBar5
            // 
            this.trackBar5.Location = new System.Drawing.Point(3, 207);
            this.trackBar5.Maximum = 125;
            this.trackBar5.Minimum = -125;
            this.trackBar5.Name = "trackBar5";
            this.trackBar5.Size = new System.Drawing.Size(197, 45);
            this.trackBar5.TabIndex = 12;
            // 
            // trackBar6
            // 
            this.trackBar6.Location = new System.Drawing.Point(3, 258);
            this.trackBar6.Maximum = 360;
            this.trackBar6.Minimum = -360;
            this.trackBar6.Name = "trackBar6";
            this.trackBar6.Size = new System.Drawing.Size(197, 45);
            this.trackBar6.TabIndex = 13;
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(3, 3);
            this.glControl1.Name = "glControl1";
            this.tableLayoutPanel1.SetRowSpan(this.glControl1, 3);
            this.glControl1.Size = new System.Drawing.Size(358, 438);
            this.glControl1.TabIndex = 1;
            this.glControl1.VSync = false;
            // 
            // panel_ik
            // 
            this.panel_ik.Controls.Add(this.Coord_panel);
            this.panel_ik.Controls.Add(this.btnDecrease);
            this.panel_ik.Controls.Add(this.btnIncrease);
            this.panel_ik.Controls.Add(this.label2);
            this.panel_ik.Controls.Add(this.label1);
            this.panel_ik.Controls.Add(this.yaw);
            this.panel_ik.Controls.Add(this.pitch);
            this.panel_ik.Controls.Add(this.roll);
            this.panel_ik.Controls.Add(this.z);
            this.panel_ik.Controls.Add(this.y);
            this.panel_ik.Controls.Add(this.x);
            this.panel_ik.Location = new System.Drawing.Point(367, 291);
            this.panel_ik.Name = "panel_ik";
            this.panel_ik.Size = new System.Drawing.Size(239, 95);
            this.panel_ik.TabIndex = 7;
            // 
            // btnDecrease
            // 
            this.btnDecrease.Font = new System.Drawing.Font("Arial Rounded MT Bold", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDecrease.Location = new System.Drawing.Point(90, 50);
            this.btnDecrease.Name = "btnDecrease";
            this.btnDecrease.Size = new System.Drawing.Size(62, 42);
            this.btnDecrease.TabIndex = 9;
            this.btnDecrease.Text = "-";
            this.btnDecrease.UseVisualStyleBackColor = true;
            this.btnDecrease.Click += new System.EventHandler(this.btnDecrease_Click);
            // 
            // btnIncrease
            // 
            this.btnIncrease.Font = new System.Drawing.Font("Arial Rounded MT Bold", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIncrease.Location = new System.Drawing.Point(90, 0);
            this.btnIncrease.Name = "btnIncrease";
            this.btnIncrease.Size = new System.Drawing.Size(62, 44);
            this.btnIncrease.TabIndex = 8;
            this.btnIncrease.Text = "+";
            this.btnIncrease.UseVisualStyleBackColor = true;
            this.btnIncrease.Click += new System.EventHandler(this.btnIncrease_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Rotate";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Trans";
            // 
            // yaw
            // 
            this.yaw.AutoSize = true;
            this.yaw.Location = new System.Drawing.Point(62, 79);
            this.yaw.Name = "yaw";
            this.yaw.Size = new System.Drawing.Size(14, 13);
            this.yaw.TabIndex = 5;
            this.yaw.TabStop = true;
            this.yaw.UseVisualStyleBackColor = true;
            // 
            // pitch
            // 
            this.pitch.AutoSize = true;
            this.pitch.Location = new System.Drawing.Point(62, 56);
            this.pitch.Name = "pitch";
            this.pitch.Size = new System.Drawing.Size(14, 13);
            this.pitch.TabIndex = 4;
            this.pitch.TabStop = true;
            this.pitch.UseVisualStyleBackColor = true;
            // 
            // roll
            // 
            this.roll.AutoSize = true;
            this.roll.Location = new System.Drawing.Point(62, 29);
            this.roll.Name = "roll";
            this.roll.Size = new System.Drawing.Size(14, 13);
            this.roll.TabIndex = 3;
            this.roll.TabStop = true;
            this.roll.UseVisualStyleBackColor = true;
            // 
            // z
            // 
            this.z.AutoSize = true;
            this.z.Location = new System.Drawing.Point(20, 77);
            this.z.Name = "z";
            this.z.Size = new System.Drawing.Size(36, 17);
            this.z.TabIndex = 2;
            this.z.TabStop = true;
            this.z.Text = "  z";
            this.z.UseVisualStyleBackColor = true;
            // 
            // y
            // 
            this.y.AutoSize = true;
            this.y.Location = new System.Drawing.Point(20, 52);
            this.y.Name = "y";
            this.y.Size = new System.Drawing.Size(36, 17);
            this.y.TabIndex = 1;
            this.y.TabStop = true;
            this.y.Text = "  y";
            this.y.UseVisualStyleBackColor = true;
            // 
            // x
            // 
            this.x.AutoSize = true;
            this.x.Location = new System.Drawing.Point(20, 27);
            this.x.Name = "x";
            this.x.Size = new System.Drawing.Size(36, 17);
            this.x.TabIndex = 0;
            this.x.TabStop = true;
            this.x.Text = "  x";
            this.x.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(367, 392);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(68, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Coord_panel
            // 
            this.Coord_panel.Controls.Add(this.rdBtn2);
            this.Coord_panel.Controls.Add(this.rdBtn1);
            this.Coord_panel.Dock = System.Windows.Forms.DockStyle.Right;
            this.Coord_panel.Location = new System.Drawing.Point(158, 0);
            this.Coord_panel.Name = "Coord_panel";
            this.Coord_panel.Size = new System.Drawing.Size(81, 95);
            this.Coord_panel.TabIndex = 10;
            this.Coord_panel.TabStop = true;
            // 
            // rdBtn1
            // 
            this.rdBtn1.AutoSize = true;
            this.rdBtn1.Location = new System.Drawing.Point(17, 20);
            this.rdBtn1.Name = "rdBtn1";
            this.rdBtn1.Size = new System.Drawing.Size(50, 17);
            this.rdBtn1.TabIndex = 0;
            this.rdBtn1.TabStop = true;
            this.rdBtn1.Text = "world";
            this.rdBtn1.UseVisualStyleBackColor = true;
            // 
            // rdBtn2
            // 
            this.rdBtn2.AutoSize = true;
            this.rdBtn2.Location = new System.Drawing.Point(17, 52);
            this.rdBtn2.Name = "rdBtn2";
            this.rdBtn2.Size = new System.Drawing.Size(42, 17);
            this.rdBtn2.TabIndex = 1;
            this.rdBtn2.TabStop = true;
            this.rdBtn2.Text = "tool";
            this.rdBtn2.UseVisualStyleBackColor = true;
            // 
            // FormRobot_multiObj
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 465);
            this.Controls.Add(this.tableLayoutPanel1);
            this.KeyPreview = true;
            this.Name = "FormRobot_multiObj";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar6)).EndInit();
            this.panel_ik.ResumeLayout(false);
            this.panel_ik.PerformLayout();
            this.Coord_panel.ResumeLayout(false);
            this.Coord_panel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.TrackBar trackBar4;
        private System.Windows.Forms.TrackBar trackBar5;
        private System.Windows.Forms.TrackBar trackBar6;
        private System.Windows.Forms.Panel panel_ik;
        private System.Windows.Forms.Button btnDecrease;
        private System.Windows.Forms.Button btnIncrease;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton yaw;
        private System.Windows.Forms.RadioButton pitch;
        private System.Windows.Forms.RadioButton roll;
        private System.Windows.Forms.RadioButton z;
        private System.Windows.Forms.RadioButton y;
        private System.Windows.Forms.RadioButton x;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel Coord_panel;
        private System.Windows.Forms.RadioButton rdBtn2;
        private System.Windows.Forms.RadioButton rdBtn1;

    }
}