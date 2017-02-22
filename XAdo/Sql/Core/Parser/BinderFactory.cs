using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sql.Parser;
using XAdo.Core;

namespace XAdo.Sql.Core
{
   /*
   public class Address
   {
      public int Id { get; set; }
      public string Street { get; set; }
   }

   public class Person
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public Address Address { get; set; }
   }

   public static class Factory
   {
      public static Person CreatePerson(IDataRecord r)
      {
         return new Person
         {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            Address = r.IsDBNull(2)
               ? null
               : new Address
               {
                  Id = r.GetInt32(2),
                  Street = r.IsDBNull(3) ? null : r.GetString(3)
               }
         };
      }

   }
    * */

   public static class BinderFactory
   {

      private static readonly MethodInfo IsDbNull = MemberInfoFinder.GetMethodInfo<IDataRecord>(r => r.IsDBNull(0));

      public static Expression<Func<IDataRecord, T>> CreateBinder<T>(this SqlSelectInfo info)
      {
         var p = Expression.Parameter(typeof (IDataRecord), "row");
         var expression = info.GetBinderExpression(typeof (T), "", p, new HashSet<string>(), false);
         return Expression.Lambda<Func<IDataRecord, T>>(expression.Convert(typeof(T)), p);
      }
      public static Expression CreateBinder(this SqlSelectInfo info, Type entityType)
      {
         var p = Expression.Parameter(typeof(IDataRecord), "row");
         var expression = info.GetBinderExpression(entityType, "", p, new HashSet<string>(), false);
         return Expression.Lambda(expression, p);
      }

      private static Expression GetBinderExpression(this SqlSelectInfo info, Type refType, string path, ParameterExpression p, HashSet<string> handledPathes, bool optional )
      {
         var ctor = refType.GetConstructor(Type.EmptyTypes);
         if (ctor == null)
         {
            throw new InvalidOperationException("Type " + refType.Name + " has no public default constructor");
         }
         var members = info
            .Select
            .Columns
            .Where(m => path.Length == 0 || m.Map.Path==path || m.Map.Path.StartsWith(path+"."))
            .OrderBy(m => m.Map.Path)
            .ThenBy(m => m.Index)
            .ToArray();

         var expressions = new List<MemberBinding>();
         foreach (var m in members)
         {
            if (m.Map.Path == path)
            {
               try
               {
                  expressions.Add(refType.GetPropertyOrField(m.Map.Name).GetDataRecordMemberAssignmentExpression(m.Index, p, m.Meta.NotNull));
               }
               catch (Exception ex)
               {
                  throw new Exception("Invalid member reference: " + refType.Name+"."+m.Map.Name+", map: " + m.Map+" (verify your mapping)",ex);
               }
            }
            else
            {
               if (!handledPathes.Contains(m.Map.Path))
               {
                  try
                  {
                     var refMember = refType.GetPropertyOrField(m.Map.Path.Split('.').Last());
                     var newExpression = info.GetBinderExpression(refMember.GetMemberType(), m.Map.Path, p, handledPathes,
                        m.Meta.IsOuterJoinColumn);
                     expressions.Add(Expression.Bind(refMember, newExpression));
                     handledPathes.Add(m.Map.Path);
                  }
                  catch (Exception ex)
                  {
                     throw new Exception("Invalid member reference: " + refType.Name + "." + m.Map.Name + ", map: " + m.Map + " (verify your mapping)", ex);
                  }
               }
            }
         }
         var body = Expression.MemberInit(Expression.New(ctor), expressions);
         return path.Length == 0 || !optional ? (Expression)body : Expression.Condition(Expression.Call(p, IsDbNull, Expression.Constant(members[0].Index)), Expression.Constant(null).Convert(refType), body);
      }


   }
}
