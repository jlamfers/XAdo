using System;
using System.Linq;
using NUnit.Framework;
using XAdo.Core;

namespace XAdo.UnitTest
{
   [TestFixture]
   public class DtoTest
   {
      /*
             * */
      [Test]
      public void TestDto()
      {
         var fields = new[]
         {
             "Id",
             "FirstName",
             "Address.Id",
             "Address.Street",
             "Address.City",
             "Address.AddressType.Ide",
             "Address.AddressType.Name",
             "PostalAddress.Id",
             "PostalAddress.Street",
             "PostalAddress.City",
             "PostalAddress.AddressType.Id",
             "PostalAddress.AddressType.Name",
             "LastName",
         };
         fields = new[]
         {
             "Id",
             "FirstName",
             "Address.Id",
             "LastName",
             "PostalAddress.Id"
         };
         var types = fields.Select(f => typeof(string)).ToArray();

         var type = AnonymousTypeHelper.GetOrCreateType(fields, types);
         var obj = Activator.CreateInstance(type);

      }
   }
}
