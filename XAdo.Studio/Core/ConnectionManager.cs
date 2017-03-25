using System.Configuration;
using System.Windows.Forms;

namespace XAdo.Studio.Core
{
   public class ConnectionManager
   {

      public ConnectionManager()
      {
         if(ConfigurationManager.ConnectionStrings.Count > 0)
         {
            ConnectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.ConnectionStrings.Count - 1];
         }
      }

      public string Choose()
      {
         var dialog = new ConectionManagerDialog().Select(ConnectionString != null ? ConnectionString.Name : null);
         var result = dialog.ShowDialog();
         if (result == DialogResult.OK)
         {
            ConnectionString = ConfigurationManager.ConnectionStrings[dialog.SelectedName];
            return dialog.SelectedName;
         }
         return null;
      }

      public ConnectionStringSettings ConnectionString { get;private set; }

   }
}
