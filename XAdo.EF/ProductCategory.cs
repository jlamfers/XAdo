//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XAdo.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductCategory
    {
        public ProductCategory()
        {
            this.ProductSubcategories = new HashSet<ProductSubcategory>();
        }
    
        public int ProductCategoryID { get; set; }
        public string Name { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    
        public virtual ICollection<ProductSubcategory> ProductSubcategories { get; set; }
    }
}
