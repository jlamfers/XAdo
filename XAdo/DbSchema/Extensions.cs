using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XAdo.Core;
using XAdo.Core.Interface;

namespace XAdo.DbSchema
{
   public static class Extensions
   {
      private class DbSchemaGetter
      {
         private readonly XAdoDbContext _context;
         private readonly DbSchemaReader _reader;
         private DbSchema _schema;
         private readonly object _syncRoot = new object();

         public DbSchemaGetter(XAdoDbContext context, DbSchemaReader reader)
         {
            _context = context;
            _reader = reader;
         }

         public DbSchema GetSchema()
         {
            if (_schema == null)
            {
               lock (_syncRoot)
               {
                  if (_schema != null) return _schema;
                  _schema = _reader.ReadSchema(_context.ConnectionString, _context.ProviderName);
               }
            }
            return _schema;
         }
      }

      public static IXAdoContextInitializer EnableDbSchema(this IXAdoContextInitializer self)
      {
         if (!self.CanResolve(typeof(DbSchemaReader)))
         {
            self.BindSingleton<DbSchemaReader, DbSchemaReader>();
         }
         self.BindSingleton<DbSchemaGetter, DbSchemaGetter>();
         
         self.OnInitialized(ctx => Task.Run(() => ctx.GetInstance<DbSchemaGetter>().GetSchema()));

         return self;
      }
      public static IXAdoContextInitializer EnableDbProvider(this IXAdoContextInitializer self)
      {
         if (!self.CanResolve(typeof(DbSchemaReader)))
         {
            self.BindSingleton<DbSchemaReader, DbSchemaReader>();
         }
         return self;
      }

      public static DbSchema GetDbSchema(this IXAdoDbSession self)
      {
         try
         {
            return self.Context.GetInstance<DbSchemaGetter>().GetSchema();
         }
         catch (Exception ex)
         {
            throw new XAdoException("Cannot resolve DbSchema, Did you invoke 'cfg.EnableDbSchema()' at the initializer?",ex);
         }
      }
      public static DbProviderInfo GetDbProviderInfo(this IXAdoDbSession self)
      {
         try
         {
            return self.Context.GetInstance<DbSchemaReader>().ReadProviderInfo(self.Context.ConnectionString,self.Context.ProviderName);
         }
         catch (Exception ex)
         {
            throw new XAdoException("Cannot resolve DbProviderInfo, Did you invoke 'cfg.EnableDbProvider()' at the initializer?", ex);
         }
      }

      public static IList<DbTableItem> SortInsertOrder(this IList<DbTableItem> self)
      {
         var graph = self.Select(table =>
            new TopologicalSortNode<DbTableItem>
            {
               Item = table,
               DependsOn = table.Columns.Where(c => c.References != null && !c.References.Table.Equals(c.Table)).Select(c => c.References.Table).ToList()
            }).ToList();

         return TopologicalSort.Sort(graph).ToList();
      }
      public static IList<DbTableItem> SortDeleteOrder(this IList<DbTableItem> self)
      {
         var graph = self.Select(table =>
            new TopologicalSortNode<DbTableItem>
            {
               Item = table,
               DependsOn = table.ChildTables.Where(child => !child.Equals(table)).ToList()
            }).ToList();

         return TopologicalSort.Sort(graph).ToList();
      }

   }
}
