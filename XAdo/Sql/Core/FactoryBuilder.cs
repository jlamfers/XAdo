using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

   public static class FactoryBuilder
   {
      private static MethodInfo _isDbNull = MemberInfoFinder.GetMethodInfo<IDataRecord>(r => r.IsDBNull(0));

      public static Expression<Func<IDataRecord, T>> BuildFactory<T>(this SelectInfo info)
      {
         var p = Expression.Parameter(typeof (IDataRecord), "row");
         var expression = info.GetRefExpression(typeof (T), "", p, new HashSet<string>(), false);
         return Expression.Lambda<Func<IDataRecord, T>>(expression, p);
      }

      private static Expression GetRefExpression(this SelectInfo info, Type refType, string path, ParameterExpression p, HashSet<string> handledPathes, bool optional )
      {
         var ctor = refType.GetConstructor(Type.EmptyTypes);
         if (ctor == null)
         {
            throw new InvalidOperationException("Type " + refType.Name + " has no public default constructor");
         }
         var members = info
            .Columns
            .Where(m => path.Length == 0 || m.Path==path || m.Path.StartsWith(path+"."))
            .OrderBy(m => m.Path)
            .ThenBy(m => m.Index)
            .ToArray();

         var expressions = new List<MemberBinding>();
         foreach (var m in members)
         {
            if (m.Path == path)
            {
               try
               {
                  expressions.Add(GetRecordGetter(refType.GetFieldOrProperty(m.Name), m.Index, p, m.NotNull));
               }
               catch (Exception ex)
               {
                  throw new Exception("Invalid member reference: " + refType.Name+"."+m.Name+", map: " + m.Map+" (verify your mapping)",ex);
               }
            }
            else
            {
               if (!handledPathes.Contains(m.Path))
               {
                  try
                  {
                     var refMember = refType.GetFieldOrProperty(m.Path.Split('.').Last());
                     var newExpression = info.GetRefExpression(refMember.GetMemberType(), m.Path, p, handledPathes,
                        m.IsOuterJoinColumn);
                     expressions.Add(Expression.Bind(refMember, newExpression));
                     handledPathes.Add(m.Path);
                  }
                  catch (Exception ex)
                  {
                     throw new Exception("Invalid member reference: " + refType.Name + "." + m.Name + ", map: " + m.Map + " (verify your mapping)", ex);
                  }
               }
            }
         }
         var body = Expression.MemberInit(Expression.New(ctor), expressions);
         return path.Length == 0 || !optional ? (Expression)body : Expression.Condition(Expression.Call(p, _isDbNull, Expression.Constant(members[0].Index)), Expression.Constant(null).Convert(refType), body);
      }

      private static MemberBinding GetRecordGetter(MemberInfo m, int index, ParameterExpression p, bool isRequired)
      {
         if (m == null) throw new ArgumentNullException("m");
         var method = GetGetterMethod(m.GetMemberType(), isRequired);
         var getter = method.IsStatic ? Expression.Call(method, p, Expression.Constant(index)) : Expression.Call(p, method, Expression.Constant(index));
         return Expression.Bind(m, getter.Convert(m.GetMemberType()));
      }

      private static MethodInfo GetGetterMethod(Type type,bool isRequired)
      {
         var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
         var name = nonNullableType == typeof(Single) ? "Float" : nonNullableType.Name;
         if (isRequired)
         {
            return typeof (IDataRecord).GetMethod("Get" + name) ?? typeof (IDataRecord).GetMethod("GetValue");
         }

         if (!type.IsValueType || type.IsNullable())
         {
            name = "GetN" + name;
         }
         else
         {
            name = "Get" + name;
         }

         return typeof (DataRecordGetters).GetMethod(name) ?? typeof (DataRecordGetters).GetMethod("GetNValue");
      }

   }
}
