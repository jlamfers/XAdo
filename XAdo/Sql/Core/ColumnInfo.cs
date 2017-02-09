namespace XAdo.Sql.Core
{
   public class ColumnInfo
   {
      public ColumnInfo(string name, string @alias)
      {
         IsKey = name.EndsWith("*");
         if (IsKey)
         {
            name = name.Substring(0, name.Length - 1).TrimEnd();
         }
         Alias = string.IsNullOrEmpty(alias) ? name : alias;
         Name = name;
      }

      public string Name { get; private set; }
      public string Alias { get; private set; }
      public bool IsKey { get; private set; }
   }
}