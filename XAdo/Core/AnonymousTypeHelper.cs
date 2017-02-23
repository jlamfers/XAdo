using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XAdo.Core.Cache;

namespace XAdo.Core
{
   public class AnonymousTypeHelper : DtoTypeBuilder
   {
      private class Key
      {
         private IList<string> _names;
         private IList<Type> _types;
         private readonly int _hashcode;

         public Key(IList<string> names, IList<Type> types)
         {
            _names = names;
            _types = types;
            const int prime = 2791;
            var hashcode = prime;
            unchecked
            {
               foreach (var n in names)
               {
                  hashcode = hashcode * prime + n.GetHashCode();
               }
               foreach (var t in types)
               {
                  hashcode = hashcode * prime + t.GetHashCode();
               }
            }
            _hashcode = hashcode;
         }

         public override int GetHashCode()
         {
            return _hashcode;
         }

         public override bool Equals(object obj)
         {
            var other = obj as Key;
            return other != null && other._names.SequenceEqual(_names) && other._types.SequenceEqual(_types);
         }

         public void LazyCloneArrays()
         {
            _names = _names.ToArray();
            _types = _types.ToArray();
         }

     }

      private static readonly LRUCache<object, Type>
          _cache = new LRUCache<object, Type>("LRUCache.XAdo.AnonymousTypes.Size",2000);


      public static Type GetOrCreateType(IList<string> names, IList<Type> types, string typeName = null)
      {
         return _cache.GetOrAdd((object)typeName ?? new Key(names, types), k =>
         {
            // cloning the inner arrays is needed to ensure not to get effected by outer changes, only if the particular key instance is stored into the cache
            var key = k as Key;
            if (key != null)
            {
               key.LazyCloneArrays();
            }

            return names.Any(n => n.Contains('.')) ? CreateGraphType(names, types, typeName) : CreateType(names, types, typeName);
         });
      }


      protected override string GetUniqueName()
      {
         return "<anonymous_" + NextUniqueNumber() + ">";
      }

      private static Type CreateType(IList<string> names, IList<Type> types, string typeName = null)
      {
         var b = new AnonymousTypeHelper { TypeName = typeName };
         for (var i = 0; i < names.Count; i++)
         {
            b.ImplementProperty(names[i], types[i]);
         }
         return b.CreateType();
      }

      private static Type CreateGraphType(IEnumerable<string> names, IList<Type> types, string typeName)
      {
         var tuples = names
            .Select((t, i) => Tuple.Create(t, types[i]))
            .OrderBy(t => t.Item1)
            .Select(t => Tuple.Create(t.Item1.Split('.'),t.Item2))
            .ToList();

         var nodes = ToNodes(tuples).ToArray();

         return CreateType(nodes.Select(n => n.EnsureTypeCreated().Name).ToList(), nodes.Select(n => n.Type).ToList(), typeName);
      }

      private class PropertyNode
      {
         public string Name;
         public Type Type;
         public IList<PropertyNode> Childs;

         public PropertyNode EnsureTypeCreated()
         {
            Type = Type ?? GetOrCreateType(Childs.Select(n => n.EnsureTypeCreated().Name).ToList(), Childs.Select(n => n.Type).ToList());
            return this;
         }
      }

      private static IEnumerable<PropertyNode> ToNodes(IList<Tuple<string[],Type>> tuples)
      {
         string prevName = null;
         foreach (var t in tuples)
         {
            if (t.Item1.Length == 1)
            {
               yield return new PropertyNode{Name = t.Item1[0], Type = t.Item2};
               continue;
            }
            if (prevName == t.Item1[0]) continue;
            prevName = t.Item1[0];
            yield return new PropertyNode { Name = t.Item1[0], Childs = ToNodes(NextLevel(tuples, prevName)).ToList() };
         }
      }

      private static IList<Tuple<string[], Type>> NextLevel(IEnumerable<Tuple<string[], Type>> tuples, string name)
      {
         return
            tuples.Where(tuple => tuple.Item1.Length > 1 && tuple.Item1[0] == name)
               .Select(tuple => Tuple.Create(tuple.Item1.Skip(1).ToArray(), tuple.Item2)).ToList();
      }


   }
}