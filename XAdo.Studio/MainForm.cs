using System;
using System.Windows.Forms;
using XAdo.Quobs;
using XAdo.Quobs.Core;
using XAdo.Studio.Core;

namespace XAdo.Studio
{
   public partial class MainForm : Form
   {
      private ConnectionManager _connectionManager = new ConnectionManager();

      public MainForm()
      {
         InitializeComponent();

         txtMain.KeyDown += OnKeyDown;
         txtModel.KeyDown += OnKeyDown;
         txtSql.KeyDown += OnKeyDown;
      }

      private void connectToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _connectionManager.Choose();
      }

      private void toolStripMenuItem1_Click(object sender, EventArgs e)
      {
         Build();
      }

      private void Build()
      {
         try
         {
            var sql = txtMain.Text;
            var context =
               new QuobsContext(
                  cfg =>
                     cfg.SetConnectionString(_connectionManager.ConnectionString.ConnectionString,
                        _connectionManager.ConnectionString.ProviderName));
            using (var session = context.CreateSession())
            {
               var resource = session.GetSqlResource(sql);
               //txtModel.Text = resource.SqlSelectTemplate;
               //var type = resource.GetEntityType(session);
               var codeBuilder = new CodeBuilder(session);
               txtModel.Text = codeBuilder.Generate(sql, "Xado.Generated");
               txtSql.Text = resource.SqlSelectTemplate;

            }
         }
         catch (Exception ex)
         {
            HandleException(ex);
         }
      }

      private void OnKeyDown(object sender, KeyEventArgs e)
      {
         if (e.Control)
         {
            if (e.KeyCode == Keys.A)
            {
               if (sender != null)
                  ((TextBox) sender).SelectAll();
            }
            if (e.KeyCode == Keys.C)
            {
               if (sender != null)
                  ((TextBox)sender).Copy();
            }
            if (e.KeyCode == Keys.V)
            {
               if (sender != null)
                  ((TextBox)sender).Paste();
            }
         }
      }

            private static void HandleException(Exception ex)
      {
         MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK,
            //MessageBoxIcon.Warning // for Warning  
            MessageBoxIcon.Error // for Error 
            //MessageBoxIcon.Information  // for Information
            //MessageBoxIcon.Question // for Question
      );
      }




      
   }
}
