using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XAdo.Examples;

namespace XAdo.Examples
{

   public partial class DbPerson
   {
      public DbAddress Addres { get; internal set; }
   }

   public partial class DbAddress
   {
      public DbAddressType AddresType { get; internal set; }
   }

   [TestClass]
   public class UnitTests
   {

      [TestMethod]
      public void AllTablesCanBeSelected()
      {
         var tasks = new List<Task>();
         foreach (
            var t in
               Assembly.GetExecutingAssembly()
                  .GetTypes()
                  .Where(t => typeof (DbTable).IsAssignableFrom(t) && t != typeof (DbTable)))
         {
            Debug.WriteLine("testing " + t.Name);
            tasks.Add(TableCanBeSelectedByType(t));
         }
         Task.WaitAll(tasks.ToArray());
         Debug.WriteLine("done...");
      }

      public async Task TableCanBeSelectedByType(Type tableType)
      {
         var m = GetType().GetMethod("TableCanBeSelected");
         m = m.MakeGenericMethod(tableType);
         await (Task)m.Invoke(null, new object[0]);
      }

      public static async Task TableCanBeSelected<T>() where T : DbTable, new()
      {
         var name = new T().GetTabeName();
         var columns = string.Join(", ",typeof (T).GetProperties().Select(p => string.Format("[{1}] as [{0}]",p.Name, p.GetCustomAttribute<ColumnAttribute>() != null ? p.GetCustomAttribute<ColumnAttribute>().Name.Replace("\\","") : p.Name )).ToArray());
         var sql = string.Format("SELECT TOP(1) {0} FROM {1}",columns,name);
         using (var ctx = DbContext.Northwind.CreateSession())
         {
            var typedPersonList = ctx.Query<T>(sql);
            var untypedPersonList = ctx.Query(sql);
            typedPersonList = await ctx.QueryAsync<T>(sql);
            untypedPersonList = await ctx.QueryAsync(sql);
         }
         using (var ctx = DbContext.NorthwindEmitted.CreateSession())
         {
            var untypedPersonList = ctx.Query(sql);
            untypedPersonList = await ctx.QueryAsync(sql);
         }
      }


      [TestMethod]
      public void GraphLoadExample()
      {
         var sql = @"SELECT      TOP(100)  
Person.Person.*, Person.Address.*, Person.AddressType.*
FROM             Person.Address INNER JOIN
                         Person.BusinessEntityAddress ON Person.Address.AddressID = Person.BusinessEntityAddress.AddressID INNER JOIN
                         Person.BusinessEntity ON Person.BusinessEntityAddress.BusinessEntityID = Person.BusinessEntity.BusinessEntityID INNER JOIN
                         Person.Person ON Person.BusinessEntity.BusinessEntityID = Person.Person.BusinessEntityID INNER JOIN
                         Person.AddressType ON Person.BusinessEntityAddress.AddressTypeID = Person.AddressType.AddressTypeID";
         using (var ctx = DbContext.Northwind.CreateSession())
         {
               var list = ctx.Query<DbPerson, DbAddress, DbAddressType, DbPerson>(sql, (p, a, t) =>
               {
                  a.AddresType = t;
                  p.Addres = a;
                  return p;
               });
            //1500 ms

            /*
Debug Trace:
1700
1948
1783
             */
            //var sw = new Stopwatch();
            //sw.Start();
            //var pl = ctx.Query(sql);
            //sw.Stop();
            //Debug.WriteLine(sw.ElapsedMilliseconds);
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
            {
               //5800
               var lists = ctx.Query<DbPerson, DbAddress, DbAddressType, DbPerson>(sql, (p, a, t) =>
               {
                  a.AddresType = t;
                  p.Addres = a;
                  return p;
               });
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);

            //sw = new Stopwatch();
            //sw.Start();
            //pl = ctx.Query(sql);
            //sw.Stop();
            //Debug.WriteLine(sw.ElapsedMilliseconds);
         }
         
      }
   }
}
