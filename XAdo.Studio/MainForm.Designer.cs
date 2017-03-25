namespace XAdo.Studio
{
   partial class MainForm
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
         this.txtMain = new System.Windows.Forms.TextBox();
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.tabSql = new System.Windows.Forms.TabControl();
         this.tabPage1 = new System.Windows.Forms.TabPage();
         this.txtModel = new System.Windows.Forms.TextBox();
         this.tabPage2 = new System.Windows.Forms.TabPage();
         this.txtSql = new System.Windows.Forms.TextBox();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.menuStrip1.SuspendLayout();
         this.tabSql.SuspendLayout();
         this.tabPage1.SuspendLayout();
         this.tabPage2.SuspendLayout();
         this.SuspendLayout();
         // 
         // txtMain
         // 
         this.txtMain.AcceptsReturn = true;
         this.txtMain.AcceptsTab = true;
         this.txtMain.Dock = System.Windows.Forms.DockStyle.Fill;
         this.txtMain.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.txtMain.Location = new System.Drawing.Point(0, 24);
         this.txtMain.Multiline = true;
         this.txtMain.Name = "txtMain";
         this.txtMain.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtMain.Size = new System.Drawing.Size(999, 231);
         this.txtMain.TabIndex = 0;
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.txtMain);
         this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.tabSql);
         this.splitContainer1.Size = new System.Drawing.Size(999, 511);
         this.splitContainer1.SplitterDistance = 255;
         this.splitContainer1.TabIndex = 1;
         // 
         // menuStrip1
         // 
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.aboutToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(999, 24);
         this.menuStrip1.TabIndex = 1;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // fileToolStripMenuItem
         // 
         this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
         this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
         this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
         this.fileToolStripMenuItem.Text = "File";
         // 
         // connectToolStripMenuItem
         // 
         this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
         this.connectToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
         this.connectToolStripMenuItem.Text = "Connect";
         this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
         // 
         // toolStripMenuItem1
         // 
         this.toolStripMenuItem1.Name = "toolStripMenuItem1";
         this.toolStripMenuItem1.Size = new System.Drawing.Size(119, 22);
         this.toolStripMenuItem1.Text = "Build";
         this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(116, 6);
         // 
         // exitToolStripMenuItem
         // 
         this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
         this.exitToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
         this.exitToolStripMenuItem.Text = "Exit";
         // 
         // aboutToolStripMenuItem
         // 
         this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
         this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
         this.aboutToolStripMenuItem.Text = "About";
         // 
         // tabSql
         // 
         this.tabSql.Controls.Add(this.tabPage1);
         this.tabSql.Controls.Add(this.tabPage2);
         this.tabSql.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tabSql.Location = new System.Drawing.Point(0, 0);
         this.tabSql.Name = "tabSql";
         this.tabSql.SelectedIndex = 0;
         this.tabSql.Size = new System.Drawing.Size(999, 252);
         this.tabSql.TabIndex = 0;
         // 
         // tabPage1
         // 
         this.tabPage1.Controls.Add(this.txtModel);
         this.tabPage1.Location = new System.Drawing.Point(4, 22);
         this.tabPage1.Name = "tabPage1";
         this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
         this.tabPage1.Size = new System.Drawing.Size(991, 226);
         this.tabPage1.TabIndex = 0;
         this.tabPage1.Text = "Model";
         this.tabPage1.UseVisualStyleBackColor = true;
         // 
         // txtModel
         // 
         this.txtModel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.txtModel.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.txtModel.Location = new System.Drawing.Point(3, 3);
         this.txtModel.Multiline = true;
         this.txtModel.Name = "txtModel";
         this.txtModel.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtModel.Size = new System.Drawing.Size(985, 220);
         this.txtModel.TabIndex = 0;
         // 
         // tabPage2
         // 
         this.tabPage2.Controls.Add(this.txtSql);
         this.tabPage2.Location = new System.Drawing.Point(4, 22);
         this.tabPage2.Name = "tabPage2";
         this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
         this.tabPage2.Size = new System.Drawing.Size(991, 226);
         this.tabPage2.TabIndex = 1;
         this.tabPage2.Text = "SQL";
         this.tabPage2.UseVisualStyleBackColor = true;
         // 
         // txtSql
         // 
         this.txtSql.Dock = System.Windows.Forms.DockStyle.Fill;
         this.txtSql.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.txtSql.Location = new System.Drawing.Point(3, 3);
         this.txtSql.Multiline = true;
         this.txtSql.Name = "txtSql";
         this.txtSql.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtSql.Size = new System.Drawing.Size(985, 220);
         this.txtSql.TabIndex = 1;
         // 
         // MainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(999, 511);
         this.Controls.Add(this.splitContainer1);
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "MainForm";
         this.Text = "Form1";
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel1.PerformLayout();
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.tabSql.ResumeLayout(false);
         this.tabPage1.ResumeLayout(false);
         this.tabPage1.PerformLayout();
         this.tabPage2.ResumeLayout(false);
         this.tabPage2.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TextBox txtMain;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.TabControl tabSql;
      private System.Windows.Forms.TabPage tabPage1;
      private System.Windows.Forms.TextBox txtModel;
      private System.Windows.Forms.TabPage tabPage2;
      private System.Windows.Forms.TextBox txtSql;
      private System.Windows.Forms.MenuStrip menuStrip1;
      private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
   }
}

