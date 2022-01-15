﻿namespace IAGrim.UI {
    partial class ModsDatabaseConfig {
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
            this.panelBox5 = new PanelBox();
            this.listViewMods = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewInstalls = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonForceUpdate = new FirefoxButton();
            this.buttonAddPath = new FirefoxButton();
            this.panelBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBox5
            // 
            this.panelBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBox5.Controls.Add(this.buttonAddPath);
            this.panelBox5.Controls.Add(this.listViewMods);
            this.panelBox5.Controls.Add(this.listViewInstalls);
            this.panelBox5.Controls.Add(this.buttonForceUpdate);
            this.panelBox5.Font = new System.Drawing.Font("Segoe UI Semibold", 20F);
            this.panelBox5.HeaderHeight = 40;
            this.panelBox5.Location = new System.Drawing.Point(12, 12);
            this.panelBox5.Name = "panelBox5";
            this.panelBox5.NoRounding = false;
            this.panelBox5.Size = new System.Drawing.Size(768, 404);
            this.panelBox5.TabIndex = 10;
            this.panelBox5.Tag = "iatag_ui_mods_header";
            this.panelBox5.Text = "Grim Dawn Database";
            this.panelBox5.TextLocation = "8; 5";
            // 
            // listViewMods
            // 
            this.listViewMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewMods.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader4});
            this.listViewMods.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.listViewMods.FullRowSelect = true;
            this.listViewMods.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewMods.HideSelection = false;
            this.listViewMods.Location = new System.Drawing.Point(431, 55);
            this.listViewMods.MultiSelect = false;
            this.listViewMods.Name = "listViewMods";
            this.listViewMods.Size = new System.Drawing.Size(321, 291);
            this.listViewMods.TabIndex = 6;
            this.listViewMods.UseCompatibleStateImageBehavior = false;
            this.listViewMods.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Tag = "iatag_ui_mod_database_mods_header";
            this.columnHeader2.Text = "Mod";
            this.columnHeader2.Width = 203;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Tag = "iatag_ui_mod_path";
            this.columnHeader4.Text = "Path";
            this.columnHeader4.Width = 200;
            // 
            // listViewInstalls
            // 
            this.listViewInstalls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInstalls.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3});
            this.listViewInstalls.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.listViewInstalls.FullRowSelect = true;
            this.listViewInstalls.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewInstalls.HideSelection = false;
            this.listViewInstalls.Location = new System.Drawing.Point(9, 55);
            this.listViewInstalls.MultiSelect = false;
            this.listViewInstalls.Name = "listViewInstalls";
            this.listViewInstalls.Size = new System.Drawing.Size(416, 291);
            this.listViewInstalls.TabIndex = 0;
            this.listViewInstalls.UseCompatibleStateImageBehavior = false;
            this.listViewInstalls.View = System.Windows.Forms.View.Details;
            this.listViewInstalls.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Tag = "iatag_ui_mod_database_mods_header";
            this.columnHeader1.Text = "Install";
            this.columnHeader1.Width = 203;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Tag = "iatag_ui_mod_path";
            this.columnHeader3.Text = "Path";
            this.columnHeader3.Width = 200;
            // 
            // buttonForceUpdate
            // 
            this.buttonForceUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonForceUpdate.EnabledCalc = true;
            this.buttonForceUpdate.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonForceUpdate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(68)))), ((int)(((byte)(80)))));
            this.buttonForceUpdate.Location = new System.Drawing.Point(9, 358);
            this.buttonForceUpdate.Name = "buttonForceUpdate";
            this.buttonForceUpdate.Size = new System.Drawing.Size(192, 32);
            this.buttonForceUpdate.TabIndex = 4;
            this.buttonForceUpdate.Tag = "iatag_ui_load_database";
            this.buttonForceUpdate.Text = "Load Database";
            this.buttonForceUpdate.Click += new System.EventHandler(this.buttonForceUpdate_Click);
            // 
            // buttonAddPath
            // 
            this.buttonAddPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddPath.EnabledCalc = true;
            this.buttonAddPath.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonAddPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(68)))), ((int)(((byte)(80)))));
            this.buttonAddPath.Location = new System.Drawing.Point(207, 358);
            this.buttonAddPath.Name = "buttonAddPath";
            this.buttonAddPath.Size = new System.Drawing.Size(192, 32);
            this.buttonAddPath.TabIndex = 7;
            this.buttonAddPath.Tag = "iatag_ui_add_path";
            this.buttonAddPath.Text = "Add path";
            this.buttonAddPath.Click += new System.EventHandler(this.buttonAddPath_Click);
            // 
            // ModsDatabaseConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 428);
            this.Controls.Add(this.panelBox5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ModsDatabaseConfig";
            this.Text = "ModsDatabaseConfig";
            this.Load += new System.EventHandler(this.ModsDatabaseConfig_Load);
            this.panelBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewInstalls;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private FirefoxButton buttonForceUpdate;
        private PanelBox panelBox5;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView listViewMods;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private FirefoxButton buttonAddPath;
    }
}