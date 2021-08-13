namespace IAGrim.UI {
    partial class SettingsWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelBox5 = new PanelBox();
            this.labelNumItems = new System.Windows.Forms.Label();
            this.labelLastUpdated = new System.Windows.Forms.Label();
            this.labelLastPatch = new System.Windows.Forms.Label();
            this.buttonDonate = new FirefoxButton();
            this.panelBox1 = new PanelBox();
            this.buttonMigratePostgres = new FirefoxButton();
            this.buttonViewLogs = new FirefoxButton();
            this.contextMenuStrip1.SuspendLayout();
            this.panelBox5.SuspendLayout();
            this.panelBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.ContextMenuStrip = this.contextMenuStrip1;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.Location = new System.Drawing.Point(503, 340);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(75, 33);
            this.linkLabel1.TabIndex = 20;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Help";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(103, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // panelBox5
            // 
            this.panelBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelBox5.Controls.Add(this.labelNumItems);
            this.panelBox5.Controls.Add(this.labelLastUpdated);
            this.panelBox5.Controls.Add(this.labelLastPatch);
            this.panelBox5.Font = new System.Drawing.Font("Segoe UI Semibold", 20F);
            this.panelBox5.HeaderHeight = 40;
            this.panelBox5.Location = new System.Drawing.Point(12, 278);
            this.panelBox5.Name = "panelBox5";
            this.panelBox5.NoRounding = false;
            this.panelBox5.Size = new System.Drawing.Size(478, 95);
            this.panelBox5.TabIndex = 9;
            this.panelBox5.Tag = "iatag_ui_gddb_title";
            this.panelBox5.Text = "Grim Dawn Database";
            this.panelBox5.TextLocation = "8; 5";
            // 
            // labelNumItems
            // 
            this.labelNumItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelNumItems.AutoSize = true;
            this.labelNumItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelNumItems.Location = new System.Drawing.Point(3, 48);
            this.labelNumItems.Name = "labelNumItems";
            this.labelNumItems.Size = new System.Drawing.Size(35, 13);
            this.labelNumItems.TabIndex = 4;
            this.labelNumItems.Text = "label2";
            // 
            // labelLastUpdated
            // 
            this.labelLastUpdated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelLastUpdated.AutoSize = true;
            this.labelLastUpdated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelLastUpdated.Location = new System.Drawing.Point(3, 61);
            this.labelLastUpdated.Name = "labelLastUpdated";
            this.labelLastUpdated.Size = new System.Drawing.Size(35, 13);
            this.labelLastUpdated.TabIndex = 3;
            this.labelLastUpdated.Text = "label1";
            // 
            // labelLastPatch
            // 
            this.labelLastPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelLastPatch.AutoSize = true;
            this.labelLastPatch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelLastPatch.Location = new System.Drawing.Point(3, 74);
            this.labelLastPatch.Name = "labelLastPatch";
            this.labelLastPatch.Size = new System.Drawing.Size(35, 13);
            this.labelLastPatch.TabIndex = 5;
            this.labelLastPatch.Text = "label1";
            // 
            // buttonDonate
            // 
            this.buttonDonate.EnabledCalc = true;
            this.buttonDonate.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonDonate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(68)))), ((int)(((byte)(80)))));
            this.buttonDonate.Location = new System.Drawing.Point(19, 89);
            this.buttonDonate.Name = "buttonDonate";
            this.buttonDonate.Size = new System.Drawing.Size(192, 32);
            this.buttonDonate.TabIndex = 5;
            this.buttonDonate.Tag = "iatag_ui_donatenow";
            this.buttonDonate.Text = "Donate Now!";
            this.buttonDonate.Click += new System.EventHandler(this.buttonDonate_Click);
            // 
            // panelBox1
            // 
            this.panelBox1.Controls.Add(this.buttonDonate);
            this.panelBox1.Controls.Add(this.buttonMigratePostgres);
            this.panelBox1.Controls.Add(this.buttonViewLogs);
            this.panelBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 20F);
            this.panelBox1.HeaderHeight = 40;
            this.panelBox1.Location = new System.Drawing.Point(12, 12);
            this.panelBox1.Name = "panelBox1";
            this.panelBox1.NoRounding = false;
            this.panelBox1.Size = new System.Drawing.Size(230, 246);
            this.panelBox1.TabIndex = 1;
            this.panelBox1.Tag = "iatag_ui_actions_title";
            this.panelBox1.Text = "Actions";
            this.panelBox1.TextLocation = "8; 5";
            // 
            // buttonMigratePostgres
            // 
            this.buttonMigratePostgres.EnabledCalc = true;
            this.buttonMigratePostgres.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonMigratePostgres.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(68)))), ((int)(((byte)(80)))));
            this.buttonMigratePostgres.Location = new System.Drawing.Point(0, 0);
            this.buttonMigratePostgres.Name = "buttonMigratePostgres";
            this.buttonMigratePostgres.Size = new System.Drawing.Size(0, 0);
            this.buttonMigratePostgres.TabIndex = 0;
            // 
            // buttonViewLogs
            // 
            this.buttonViewLogs.EnabledCalc = true;
            this.buttonViewLogs.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonViewLogs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(68)))), ((int)(((byte)(80)))));
            this.buttonViewLogs.Location = new System.Drawing.Point(19, 51);
            this.buttonViewLogs.Name = "buttonViewLogs";
            this.buttonViewLogs.Size = new System.Drawing.Size(192, 32);
            this.buttonViewLogs.TabIndex = 6;
            this.buttonViewLogs.Tag = "iatag_ui_viewlogs";
            this.buttonViewLogs.Text = "View Logs";
            this.buttonViewLogs.Click += new System.EventHandler(this.buttonViewLogs_Click);
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 382);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.panelBox5);
            this.Controls.Add(this.panelBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsWindow_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panelBox5.ResumeLayout(false);
            this.panelBox5.PerformLayout();
            this.panelBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PanelBox panelBox1;
        private System.Windows.Forms.Label labelLastUpdated;
        private System.Windows.Forms.Label labelNumItems;
        private System.Windows.Forms.Label labelLastPatch;
        private PanelBox panelBox5;
        private FirefoxButton buttonDonate;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private FirefoxButton buttonMigratePostgres;
        private FirefoxButton buttonViewLogs;
    }
}