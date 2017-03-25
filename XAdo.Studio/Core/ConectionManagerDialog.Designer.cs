namespace XAdo.Studio.Core
{
   partial class ConectionManagerDialog
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
         this.comboBoxItems = new System.Windows.Forms.ComboBox();
         this.btnOk = new System.Windows.Forms.Button();
         this.btnCancel = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // comboBoxItems
         // 
         this.comboBoxItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxItems.FormattingEnabled = true;
         this.comboBoxItems.Location = new System.Drawing.Point(12, 12);
         this.comboBoxItems.Name = "comboBoxItems";
         this.comboBoxItems.Size = new System.Drawing.Size(262, 21);
         this.comboBoxItems.TabIndex = 0;
         // 
         // btnOk
         // 
         this.btnOk.Location = new System.Drawing.Point(199, 39);
         this.btnOk.Name = "btnOk";
         this.btnOk.Size = new System.Drawing.Size(75, 23);
         this.btnOk.TabIndex = 1;
         this.btnOk.Text = "OK";
         this.btnOk.UseVisualStyleBackColor = true;
         this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
         // 
         // btnCancel
         // 
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(118, 39);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(75, 23);
         this.btnCancel.TabIndex = 2;
         this.btnCancel.Text = "Cancel";
         this.btnCancel.UseVisualStyleBackColor = true;
         // 
         // ConectionManagerDialog
         // 
         this.AcceptButton = this.btnOk;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(289, 74);
         this.Controls.Add(this.btnCancel);
         this.Controls.Add(this.btnOk);
         this.Controls.Add(this.comboBoxItems);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "ConectionManagerDialog";
         this.Text = "Connect";
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ComboBox comboBoxItems;
      private System.Windows.Forms.Button btnOk;
      private System.Windows.Forms.Button btnCancel;
   }
}