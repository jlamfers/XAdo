using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using XAdo.Core.Impl;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class Examples
   {

      [Test]
      public async void SimpleExample_DynamicObjects()
      {
         var context = new AdoContext("AW");

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query("select FirstName,LastName from Person.Person");
            Type type = persons.First().GetType();
            Debug.WriteLine(type);
            persons = await sn.QueryAsync("select FirstName,LastName from Person.Person");
         }
      }

      [Test]
      public async void SimpleExample_EmittedDynamicTypes()
      {
         var context = new AdoContext(cfg => cfg
            .SetConnectionStringName("AW")
            .EnableEmittedDynamicTypes()
            );

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query("select FirstName,LastName from Person.Person");
            Type type = persons.First().GetType();
            Debug.WriteLine(type);
            persons = await sn.QueryAsync("select FirstName,LastName from Person.Person");

         }
      }

      [Test]
      public async void SimpleExample_WithFactory()
      {
         var context = new AdoContext(cfg => cfg
            .SetConnectionStringName("AW")
            .EnableEmittedDynamicTypes()
            );

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query("select FirstName,LastName from Person.Person", r => Tuple.Create(r.GetString(0), r.GetString(1)));
            Type type = persons.First().GetType();
            Debug.WriteLine(type);
            persons = await sn.QueryAsync("select FirstName,LastName from Person.Person", r => Tuple.Create(r.GetString(0), r.GetString(1)));
         }
      }

      [Test]
      public async void SimpleExample_WithAnonymousFactory()
      {
         var context = new AdoContext(cfg => cfg
            .SetConnectionStringName("AW")
            .EnableEmittedDynamicTypes()
            );

         using (var sn = context.CreateSession())
         {
            var persons = sn.Query("select FirstName,LastName from Person.Person", r => new{FirstName=r.GetString(0),LastName=r.GetString(0)});
            Type type = persons.First().GetType();
            Debug.WriteLine(type);
            persons = await sn.QueryAsync("select FirstName,LastName from Person.Person", r => new { FirstName = r.GetString(0), LastName = r.GetString(0) });

            // typed access:
            var name = persons.First().FirstName;
         }
      }

      public class Person
      {
         public int Id { get; set; }
         public String FirstName { get; set; }
         public string LastName { get; set; }
         public Address Addres { get; set; }
         public IList<Job> Jobs { get; set; }
      }

      public class Address
      {
         public int Id { get; set; }
         public string City { get; set; }
         public string Street { get; set; }
         public string StreetNumber { get; set; }
      }

      public class Job
      {
         public string Name { get; set; }
         public string City { get; set; }
         public double Duration { get; set; }
      }

      public class StringTokenizer
      {
         private readonly string _source;
         private int _pos;

         public StringTokenizer(string source)
         {
            _source = source;
         }
         public char PeekChar()
         {
            return !Eof() ? _source[_pos] : '\0';
         }
         public char NextChar()
         {
            return !Eof() ? _source[_pos++] : '\0';
         }

         public void SkipSpaces()
         {
            while (!Eof())
            {
               switch (PeekChar())
               {
                  case ' ':
                  case '\t':
                  case '\r':
                     NextChar();
                     continue;
                  default:
                     return;
               }
            }
         }

         public void Read(char ch)
         {
            if (PeekChar() != ch)
            {
               throw new Exception("Expected char: " + ch);
            }
            NextChar();
         }
         public bool Eof()
         {
            return _pos >= _source.Length;
         }
      }

      /*
 Id* 0
 FirstName 1 
 LastName 2 
 Address 3{
   Id* 3
   City 4
   Street 5
   Number 6
 }
 Jobs 7{
   Name* 7
   City 8
   Duration 9
 }
 */
      /* Id*0;FirstName 1;LastName 2;Address?3{Id*3;City 4;Street 5;Number 6};Jobs?7{Name*7;City 8; Duration 9};
       * */

      /*******

         SELECT 
           FirstName as "c0.1",
           LastName as c1 
         FROM Person.Person 
         --$WHERE {where}
         --$HAVING {having}
         --$ORDER BY {order}
         --$OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY
 
      **********/

      public class Map
      {
         readonly IList<Map> _childs = new List<Map>();

         public Map Parent { get; set; }
         public bool IsKey { get; set; }
         public string Name { get; set; }
         public int Index { get; set; }
         public IList<Map> Childs { get { return _childs; } }

         public Map Parse(StringTokenizer reader, Map parent = null)
         {
            parent = parent ?? new Map {Name = "<root>"};
            var name = new StringBuilder();
            var index = new StringBuilder();
            bool readIndex = false;
            bool isKey = false;
            reader.SkipSpaces();
            while (!reader.Eof())
            {
               var ch = reader.PeekChar();
               switch (ch)
               {
                  case ' ':
                     reader.NextChar();
                     readIndex = true;
                     continue;
                  case '*':
                     reader.NextChar();
                     isKey = true;
                     readIndex = true;
                     break;
                  case '\n':
                  case '{':
                     var map = new Map
                     {
                        Index = int.Parse(index.ToString()),
                        IsKey = isKey,
                        Name = name.ToString(),
                        Parent = parent
                     };
                     parent.Childs.Add(map);
                     name.Length = 0;
                     index.Length = 0;
                     readIndex = false;
                     isKey = false;
                     if (ch == '{')
                     {
                        reader.NextChar();
                        parent.Childs.Add(Parse(reader, map));
                        reader.Read('}');
                     }
                     break;
                  case '}':
                     break;

                  default:
                     if (readIndex)
                     {
                        index.Append(reader.NextChar());
                     }
                     else
                     {
                        name.Append(reader.NextChar());
                     }
                     break;

               }
               reader.SkipSpaces();
            }
            return parent;
         }
      }

      



   }
}
