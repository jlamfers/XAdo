using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace XAdo.Studio.Core
{
   public partial class ConectionManagerDialog : Form
   {
      public ConectionManagerDialog()
      {
         InitializeComponent();
         foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
         {
            comboBoxItems.Items.Add(cs.Name);
         }
      }

      public ConectionManagerDialog Select(string name)
      {
         if (name != null)
         {
            SelectedName = name;
            comboBoxItems.SelectedItem = name;
         }
         return this;
      }

      private void btnOk_Click(object sender, System.EventArgs e)
      {
         SelectedName = (string)comboBoxItems.SelectedItem;
         DialogResult = DialogResult.OK;
         Close();
      }

      public string SelectedName { get; private set; }

   }
}
