using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XAdo.Sql.Core.Common
{
   // This Annotator class lets you dynamicly (and/or additionally) annotate types and members, only if you use this Annotator
   // extension methods to retrieve all corresponding annotations (attributes).
   public static class Annotator
   {
      private class Identity
      {
         private readonly Type _type;
         private readonly MemberInfo _member;
         private readonly int _hashcode;

         public Identity(Type type, MemberInfo member)
         {
            _type = type;
            _member = member;
            unchecked
            {
               var hashcode = _type.GetHashCode();
               if (_member != null)
               {
                  hashcode = hashcode * 499 + _member.GetHashCode();
                  _hashcode = hashcode;
               }
            }
         }

         public override bool Equals(object obj)
         {
            var other = (Identity)obj;
            return _type == other._type && Equals(_member, other._member);
         }

         public override int GetHashCode()
         {
            return _hashcode;
         }
      }

      private static readonly ConcurrentDictionary<Identity, IList<Attribute>>
          Annotations = new ConcurrentDictionary<Identity, IList<Attribute>>();

      private static readonly ConcurrentDictionary<MemberInfo, IList<Attribute>>
          Cache = new ConcurrentDictionary<MemberInfo, IList<Attribute>>();

      public static void Annotate(this MemberInfo member, Attribute attribute)
      {
         if (member == null) throw new ArgumentNullException("member");
         if (attribute == null) throw new ArgumentNullException("attribute");
         Cache.Clear();
         var list = Annotations.GetOrAdd(member.GetIdentity(), t => new List<Attribute>());
         CheckUnique(attribute, list);
         list.Add(attribute);
      }

      public static bool RemoveAnnotation(MemberInfo member, Attribute attribute)
      {
         if (member == null) throw new ArgumentNullException("member");
         if (attribute == null) throw new ArgumentNullException("attribute");
         Cache.Clear();
         var identity = member.GetIdentity();
         IList<Attribute> annotation;
         return Annotations.TryGetValue(identity, out annotation) && annotation.Remove(attribute);
      }

      /// <summary>
      /// Gets all annotations from the member
      /// The dynamic added attributes are added first to the result list, the static attributes are added secondly
      /// </summary>
      /// <param name="member">The corresponding member</param>
      /// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events. The default value is: true</param>
      /// <param name="findStaticAttributes">true to search this member's static (compile time added) attributes; otherwise, false. The default value is: true</param>
      /// <returns>A list of attributes found, or an empty list of no attributes were found.</returns>
      public static IList<Attribute> GetAnnotations(this MemberInfo member, bool inherit = true, bool findStaticAttributes = true)
      {

         return Cache.GetOrAdd(member, m =>
         {
            var result = new List<Attribute>();
            while (true)
            {
               IList<Attribute> list;
               if (Annotations.TryGetValue(member.GetIdentity(), out list))
               {
                  result.AddRange(list.Where(a => a.IsAllowMultiple() || !result.Contains(a)));
               }
               if (findStaticAttributes)
               {
                  result.AddRange(
                      member.GetCustomAttributes(inherit)
                          .OfType<Attribute>()
                          .Where(a => a.IsAllowMultiple() || result.All(x => !a.GetType().IsAssignableFrom(x.GetType()))));
               }

               if (!inherit || member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Event)
               {
                  return result;
               }

               if (member.IsType())
               {
                  member = ((Type)member).BaseType;
               }
               else
               {
                  var type = member.ReflectedType;
                  MemberInfo baseMember = null;
                  while (type != null && baseMember == null)
                  {
                     if ((type = type.BaseType) != null)
                        baseMember = type.GetMember(member.Name).SingleOrDefault(x => x.MemberType != MemberTypes.Method || ((MethodInfo)m).GetBaseDefinition() == ((MethodInfo)member).GetBaseDefinition());
                  }
                  member = baseMember;
               }

               if (member == null)
               {
                  return result;
               }
            }
         });
      }

      public static IEnumerable<T> GetAnnotations<T>(this MemberInfo member, bool inherit = true, bool findStaticAttributes = true)
          where T : Attribute
      {
         if (member == null) throw new ArgumentNullException("member");
         return member.GetAnnotations().OfType<T>();
      }

      public static T GetAnnotation<T>(this MemberInfo member, bool inherit = true, bool findStaticAttributes = true)
          where T : Attribute
      {
         return member.GetAnnotations<T>().SingleOrDefault();
      }

      private static bool IsAllowMultiple(this Attribute attribute)
      {
         var att = attribute.GetType().GetCustomAttribute<AttributeUsageAttribute>();
         return att != null && att.AllowMultiple;
      }

      private static void CheckUnique(Attribute attribute, IList<Attribute> attributeList)
      {
         if (attribute == null) throw new ArgumentNullException("attribute");
         if (!attribute.IsAllowMultiple() && attributeList.Contains(attribute))
         {
            throw new Exception(string.Format("AllowMultiple==false => Attribute '{0}' already was added.", attribute.GetType()));
         }
      }

      private static bool IsType(this MemberInfo member)
      {
         return member != null &&
                (member.MemberType == MemberTypes.TypeInfo || member.MemberType == MemberTypes.NestedType);
      }

      private static Identity GetIdentity(this MemberInfo self)
      {
         if (self.IsType())
         {
            return new Identity((Type)self, null);
         }
         if (self.MemberType == MemberTypes.Method)
         {
            return new Identity(self.DeclaringType, ((MethodInfo)self).GetBaseDefinition());
         }
         return new Identity(self.DeclaringType, self);
      }
   }
}
