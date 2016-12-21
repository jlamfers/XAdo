using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XAdo.SqlObjects.Search.Typed;

namespace XAdo.SqlObjects.Search.Dynamic
{
   public static class Extensions
   {
      #region Lookup
      private static readonly MethodInfo
         _methodCompareTo = typeof(string).GetMethod("CompareTo", new[] { typeof(string) }),
         _methodContains = typeof(string).GetMethod("Contains", new[] { typeof(string) }),
         _methodStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
         _methodEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

      private static readonly Dictionary<Operator, ExpressionType> 
         _operatorLookup = new Dictionary<Operator, ExpressionType>
         {
            {Operator.Eq,ExpressionType.Equal},
            {Operator.Neq,ExpressionType.NotEqual},
            {Operator.Lt,ExpressionType.LessThan},
            {Operator.Lte,ExpressionType.LessThanOrEqual},
            {Operator.Gt,ExpressionType.GreaterThan},
            {Operator.Gte,ExpressionType.GreaterThanOrEqual}
         };

      private static readonly Dictionary<Operator, MethodInfo>
         _methodLookup = new Dictionary<Operator, MethodInfo>
         {
            {Operator.Contains,_methodContains},
            {Operator.StartsWith,_methodStartsWith},
            {Operator.EndsWith,_methodEndsWith},
            {Operator.NotContains,_methodContains},
            {Operator.NotStartsWith,_methodStartsWith},
            {Operator.NotEndsWith,_methodEndsWith}
         };

      private static readonly HashSet<Operator>
         _notOperators = new HashSet<Operator>
         {
            Operator.NotContains,
            Operator.NotStartsWith,
            Operator.NotEndsWith,
         };
      private static readonly Dictionary<string, string>
         _operatorSynonymns = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
         {
            {"<","Lt"},
            {"<=","Lte"},
            {">","Gt"},
            {">=","Gte"},
            {"=","Eq"},
            {"==","Eq"},
            {"!=","Neq"},
            {"<>","Neq"},
            {"like","Contains"},
            {"not like","NotContains"},
            {"not contains","NotContains"},
            {"not startswith","NotStartsWith"},
            {"not endswith","NotEndsWith"},

            {"ct","Contains"},
            {"nct","NotContains"},
            {"sw","StartsWith"},
            {"nsw","NotStartsWith"},
            {"ew","EndsWith"},
            {"new","NotEndsWith"}
         };
      #endregion

      #region OrderBy

      private static readonly HashSet<string> _descending = new HashSet<string>(new[]{"D","DESC"},StringComparer.InvariantCultureIgnoreCase);

      public static OrderByFieldNameList ToOrderByFieldNameList(this string sortExpression)
      {
         if (string.IsNullOrWhiteSpace(sortExpression))
         {
            return null;
         }
         var result = new OrderByFieldNameList();
         foreach (var p in ParseSortColumns(sortExpression))
         {
            var descending = p.Length > 1 && _descending.Contains(p[1]);
            result.Add(p[0], descending);
         }
         return result;
      }
      public static OrderByFieldExpressionList<T> ToOrderByFieldExpressionList<T>(this OrderByFieldNameList self)
      {
         if (self == null) throw new ArgumentNullException("self");
         var result = new OrderByFieldExpressionList<T>();
         foreach (var s in self)
         {
            var p = Expression.Parameter(typeof(T), "p");
            result.Add(Expression.Lambda(GetMemberExpression(p, s.FieldName),p), s.Descending);
         }
         return result;
      }

      private static List<string[]> ParseSortColumns(string sortExpression)
      {
         var list = new List<string[]>();
         var item = new string[] {null, null};
         var sb = new StringBuilder();
         foreach (var ch in sortExpression)
         {
            if (ch == ' ') continue;

            if (ch == '~')
            {
               Accept(list, sb, ref item);
            }
            else
            {
               sb.Append(ch);
            }
         }
         Accept(list, sb, ref item);
         return list;
      }

      private static void Accept(List<string[]> list, StringBuilder sb, ref string[] item)
      {
         var v = sb.ToString();
         sb.Length = 0;
         if (item[0] == null)
         {
            list.Add(item);
            item[0] = GetStringValue(v);
         }
         else if (item[1] == null && v.Length == 1 && "aAdD".IndexOf(v[0]) >= 0)
         {
            item[1] = v;
         }
         else
         {
            item = new string[] { null, null };
            item[0] = GetStringValue(v);
            list.Add(item);
         }
      }

      #endregion

      #region Criterium
      internal static Operator GetOperator(this Criterium self)
      {
         if (self.Operator == null) return Operator.Eq;

         string actual;
         var op = _operatorSynonymns.TryGetValue(self.Operator, out actual)
            ? actual
            : self.Operator;
         return (Operator)Enum.Parse(typeof(Operator), op, true);
      }
      public static Expression ToPredicate(this Criterium self, ParameterExpression p)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (p == null) throw new ArgumentNullException("p");
         var op = self.GetOperator();
         ExpressionType expressionType;
         var memberExpression = GetMemberExpression(p, self.Field);
         var memberType = memberExpression.Type;
         if (_operatorLookup.TryGetValue(op, out expressionType))
         {
            if (memberType != typeof(string) || op == Operator.Eq || op == Operator.Neq)
            {
               return Expression.MakeBinary(expressionType,
                  memberExpression,
                  Expression.Convert(Expression.Constant(GetValue(memberType, self.Value)), memberType));
            }
            return Expression.MakeBinary(expressionType,
               Expression.Call(memberExpression, _methodCompareTo, Expression.Constant(self.Value)),
               Expression.Constant(0));
         }
         MethodInfo method;
         if (_methodLookup.TryGetValue(op, out method))
         {
            var callExpression = Expression.Call(memberExpression,
               method, Expression.Constant(GetValue(memberType, self.Value)));
            return _notOperators.Contains(op)
               ? (Expression)Expression.Not(callExpression)
               : callExpression;

         }
         throw new InvalidOperationException("Unknown operator in filter: " + self.Operator);
      }

      #endregion

      #region Filter
      public static Filter Add(this Filter self, string fieldName, string @operator, object value)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (fieldName == null) throw new ArgumentNullException("fieldName");
         if (@operator == null) throw new ArgumentNullException("operator");
         self.Criteria.Add(new Criterium
         {
            Field = fieldName,
            Operator = @operator,
            Value = FormatToString(value)
         });
         return self;
      }
      public static Expression<Func<T, bool>> ToPredicate<T>(this Filter self)
      {
         if (self == null) throw new ArgumentNullException("self");
         var p = Expression.Parameter(typeof(T), "p");
         Expression body = null;
         foreach (var e in self
            .Criteria
            .Select(c => Tuple.Create(c.Field + c.Operator, c.ToPredicate(p)))
            .GroupBy(t => t.Item1)
            .Select(g => DisjunctSameFields(g.Select(t => t.Item2).ToArray())))
         {
            body = body == null ? e : Expression.MakeBinary(ExpressionType.AndAlso, body, e);
         }
         return Expression.Lambda<Func<T, bool>>(body, p);
      }
      public static Filter ToFilter(this string filter)
      {
         //example 1: Name~neq~Pete~Name~eq~John 
         //    => Name != "Pete" && Name=="John" 
         //       by default logical conjunction
         //
         //example 2: Name~sw~Pete~Name~sw~John 
         //    => Name.StartsWith("Pete") || Name.StartsWith("John") 
         //       logical disjunction since fields and operators are the same
         //
         //example 3: Name~sw~Pete~Date~gt~2000-01-01~Name~sw~John 
         //    => (Name.StartsWith("Pete") || Name.StartsWith("John")) && Date > DateTime.Parse("2000-01-01",CultureInfo.InvariantCulture) 
         //       logical disjunction on name since fields and operators are the same here, conjunction with Date
         //
         //example 4: Name~Pete~Name~John 
         //    => Name == "Pete" || Name=="John" 
         //       disjunct since fields and operators are the same, operator defaults to equals (==)
         if (filter == null) throw new ArgumentNullException("filter");
         var criteria = new List<Criterium>();
         var criterium = new Criterium();
         var sb = new StringBuilder();
         foreach (var ch in filter)
         {
            if (ch == '~')
            {
               var v = sb.ToString().Trim();
               sb.Length = 0;
              

               if (criterium.Field == null)
               {
                  criterium.Field = v;
               }
               else if (criterium.Operator == null)
               {
                  string op;
                  op = _operatorSynonymns.TryGetValue(v, out op) ? op : v;
                  Operator op2;
                  if (Enum.TryParse(op, true, out op2))
                  {
                     criterium.Operator = op2.ToString();
                  }
                  else
                  {
                     criterium.Value = GetStringValue(v);
                     criteria.Add(criterium);
                     criterium = new Criterium();
                  }
               }
               else
               {
                  criterium.Value = GetStringValue(v);
                  criteria.Add(criterium);
                  criterium = new Criterium();
               }
            }
            else
            {
               sb.Append(ch);
            }
         }
         if (sb.Length > 0)
         {
            criterium.Value = GetStringValue(sb.ToString().Trim());
            criteria.Add(criterium);
         }
         return new Filter {Criteria = criteria};
      }

      private static string GetStringValue(string raw)
      {
         if (string.IsNullOrEmpty(raw)) return raw;
         if (raw.Length > 1 && raw[0] == '\'' && raw[raw.Length - 1] == '\'')
         {
            return raw.Substring(1, raw.Length - 2);
         }
         return raw.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : raw;
      }

      #endregion

      #region Privates
      private static string FormatToString(object value)
      {
         if (value == null) return null;
         var type = value.GetType();
         type = Nullable.GetUnderlyingType(type) ?? type;
         return type == typeof(DateTime)
            ? ((DateTime)value).ToString("o")
            : string.Format(CultureInfo.InvariantCulture, "{0}", value);
      }

      private static Expression DisjunctSameFields(IList<Expression> expression)
      {
         return expression.Count == 1 
            ? expression[0] 
            : expression.Aggregate<Expression, Expression>(null, (current, e) => current == null ? e : Expression.OrElse(current, e));
      }

      private static object GetValue(Type type, string value)
      {
         if (type == typeof (string)) return value; 
         if (value == null) return null;
         type = Nullable.GetUnderlyingType(type) ?? type;
         return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
      }

      private static MemberExpression GetMemberExpression(ParameterExpression p, string columnName)
      {
         MemberExpression memberExpression = null;
         Expression target = p;
         var currentMemberTargetType = p.Type;
         foreach (var part in columnName.Split('.'))
         {
            var currentMember = currentMemberTargetType.GetPropertyOrFieldMember(part);
            if (currentMember == null)
            {
               throw new InvalidOperationException(string.Format(
                  "Property or field '{0}' not found", part));
            }
            memberExpression = currentMember.GetPropertyOrFieldExpression(target);
            target = memberExpression;
            currentMemberTargetType = currentMember.GetMemberType();
         }
         return memberExpression;
      }

      private static Type GetMemberType(this MemberInfo member)
      {
         return member.MemberType == MemberTypes.Property
            ? ((PropertyInfo)member).PropertyType
            : ((FieldInfo)member).FieldType;
      }

      private static MemberInfo GetPropertyOrFieldMember(this Type type, string name)
      {
         const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
         const BindingFlags flagsIgnoreCase = flags | BindingFlags.IgnoreCase;
         try
         {
            return (MemberInfo) type.GetProperty(name, flagsIgnoreCase) ?? type.GetField(name, flagsIgnoreCase);
         }
         catch (AmbiguousMatchException)
         {
            return (MemberInfo)type.GetProperty(name, flags) ?? type.GetField(name, flags);
         }

      }

      private static MemberExpression GetPropertyOrFieldExpression(this MemberInfo member, Expression target)
      {
         return member.MemberType == MemberTypes.Property ? Expression.Property(target, (PropertyInfo)member) : Expression.Field(target, (FieldInfo)member);
      }
      #endregion
   }

}
