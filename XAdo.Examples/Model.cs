using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.SqlServer.Types;

namespace XAdo.Examples
{

   public partial class DbTable
   {
   }

   public static class DbTableExtensions
   {
      public static string GetTabeName(this DbTable table)
      {
         return table.GetType().GetCustomAttribute<TableAttribute>().Name;
      }
   }

   [Table("dbo.AWBuildVersion")]
   public partial class DbAWBuildVersion : DbTable
   {
      [Key, Required]
      public virtual Byte? SystemInformationID { get; set; }
      [Required, MaxLength(25), Column("Database Version")]
      public virtual String Database_Version { get; set; }
      [Required]
      public virtual DateTime? VersionDate { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.Department")]
   public partial class DbDepartment : DbTable
   {
      [Key, Required]
      public virtual Int16? DepartmentID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(50)]
      public virtual String GroupName { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.Employee")]
   public partial class DbEmployee : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(15)]
      public virtual String NationalIDNumber { get; set; }
      [Required, MaxLength(256)]
      public virtual String LoginID { get; set; }
      public virtual SqlHierarchyId? OrganizationNode { get; set; }
      public virtual Int16? OrganizationLevel { get; set; }
      [Required, MaxLength(50)]
      public virtual String JobTitle { get; set; }
      [Required]
      public virtual DateTime? BirthDate { get; set; }
      [Required, MaxLength(1)]
      public virtual String MaritalStatus { get; set; }
      [Required, MaxLength(1)]
      public virtual String Gender { get; set; }
      [Required]
      public virtual DateTime? HireDate { get; set; }
      [Required]
      public virtual Boolean? SalariedFlag { get; set; }
      [Required]
      public virtual Int16? VacationHours { get; set; }
      [Required]
      public virtual Int16? SickLeaveHours { get; set; }
      [Required]
      public virtual Boolean? CurrentFlag { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.EmployeeDepartmentHistory")]
   public partial class DbEmployeeDepartmentHistory : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]
      public virtual Int16? DepartmentID { get; set; }
      [Key, Required]
      public virtual Byte? ShiftID { get; set; }
      [Key, Required]
      public virtual DateTime? StartDate { get; set; }
      public virtual DateTime? EndDate { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.EmployeePayHistory")]
   public partial class DbEmployeePayHistory : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]
      public virtual DateTime? RateChangeDate { get; set; }
      [Required]
      public virtual Decimal? Rate { get; set; }
      [Required]
      public virtual Byte? PayFrequency { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.JobCandidate")]
   public partial class DbJobCandidate : DbTable
   {
      [Key, Required]
      public virtual Int32? JobCandidateID { get; set; }
      public virtual Int32? BusinessEntityID { get; set; }
      public virtual String Resume { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.Shift")]
   public partial class DbShift : DbTable
   {
      [Key, Required]
      public virtual Byte? ShiftID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual TimeSpan? StartTime { get; set; }
      [Required]
      public virtual TimeSpan? EndTime { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.Address")]
   public partial class DbAddress : DbTable
   {
      [Key, Required]
      public virtual Int32? AddressID { get; set; }
      [Required, MaxLength(60)]
      public virtual String AddressLine1 { get; set; }
      [MaxLength(60)]
      public virtual String AddressLine2 { get; set; }
      [Required, MaxLength(30)]
      public virtual String City { get; set; }
      [Required]

      public virtual Int32? StateProvinceID { get; set; }
      [Required, MaxLength(15)]
      public virtual String PostalCode { get; set; }
      public virtual SqlGeography SpatialLocation { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.AddressType")]

   public partial class DbAddressType : DbTable
   {
      [Key, Required]
      public virtual Int32? AddressTypeID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.BusinessEntity")]

   public partial class DbBusinessEntity : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.BusinessEntityAddress")]
   public partial class DbBusinessEntityAddress : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]

      public virtual Int32? AddressID { get; set; }
      [Key, Required]

      public virtual Int32? AddressTypeID { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.BusinessEntityContact")]
   public partial class DbBusinessEntityContact : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]

      public virtual Int32? PersonID { get; set; }
      [Key, Required]

      public virtual Int32? ContactTypeID { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.ContactType")]

   public partial class DbContactType : DbTable
   {
      [Key, Required]
      public virtual Int32? ContactTypeID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.CountryRegion")]

   public partial class DbCountryRegion : DbTable
   {
      [Key, Required, MaxLength(3)]
      public virtual String CountryRegionCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.EmailAddress")]
   public partial class DbEmailAddress : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]
      public virtual Int32? EmailAddressID { get; set; }
      [MaxLength(50)]
      public virtual String EmailAddress { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.Password")]
   public partial class DbPassword : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(128)]
      public virtual String PasswordHash { get; set; }
      [Required, MaxLength(10)]
      public virtual String PasswordSalt { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.Person")]

   public partial class DbPerson : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(2)]
      public virtual String PersonType { get; set; }
      [Required]
      public virtual Boolean? NameStyle { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [Required]
      public virtual Int32? EmailPromotion { get; set; }
      public virtual String AdditionalContactInfo { get; set; }
      public virtual String Demographics { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.PersonPhone")]
   public partial class DbPersonPhone : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required, MaxLength(25)]
      public virtual String PhoneNumber { get; set; }
      [Key, Required]

      public virtual Int32? PhoneNumberTypeID { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.PhoneNumberType")]

   public partial class DbPhoneNumberType : DbTable
   {
      [Key, Required]
      public virtual Int32? PhoneNumberTypeID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.StateProvince")]

   public partial class DbStateProvince : DbTable
   {
      [Key, Required]
      public virtual Int32? StateProvinceID { get; set; }
      [Required, MaxLength(3)]
      public virtual String StateProvinceCode { get; set; }
      [Required, MaxLength(3)]

      public virtual String CountryRegionCode { get; set; }
      [Required]
      public virtual Boolean? IsOnlyStateProvinceFlag { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]

      public virtual Int32? TerritoryID { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.BillOfMaterials")]
   public partial class DbBillOfMaterials : DbTable
   {
      [Key, Required]
      public virtual Int32? BillOfMaterialsID { get; set; }

      public virtual Int32? ProductAssemblyID { get; set; }
      [Required]

      public virtual Int32? ComponentID { get; set; }
      [Required]
      public virtual DateTime? StartDate { get; set; }
      public virtual DateTime? EndDate { get; set; }
      [Required, MaxLength(3)]

      public virtual String UnitMeasureCode { get; set; }
      [Required]
      public virtual Int16? BOMLevel { get; set; }
      [Required]
      public virtual Decimal? PerAssemblyQty { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.Culture")]

   public partial class DbCulture : DbTable
   {
      [Key, Required, MaxLength(6)]
      public virtual String CultureID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.Document")]

   public partial class DbDocument : DbTable
   {
      [Key, Required]
      public virtual SqlHierarchyId? DocumentNode { get; set; }
      public virtual Int16? DocumentLevel { get; set; }
      [Required, MaxLength(50)]
      public virtual String Title { get; set; }
      [Required]

      public virtual Int32? Owner { get; set; }
      [Required]
      public virtual Boolean? FolderFlag { get; set; }
      [Required, MaxLength(400)]
      public virtual String FileName { get; set; }
      [Required, MaxLength(8)]
      public virtual String FileExtension { get; set; }
      [Required, MaxLength(5)]
      public virtual String Revision { get; set; }
      [Required]
      public virtual Int32? ChangeNumber { get; set; }
      [Required]
      public virtual Byte? Status { get; set; }
      public virtual String DocumentSummary { get; set; }
      public virtual Byte[] Document { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.Illustration")]

   public partial class DbIllustration : DbTable
   {
      [Key, Required]
      public virtual Int32? IllustrationID { get; set; }
      public virtual String Diagram { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.Location")]

   public partial class DbLocation : DbTable
   {
      [Key, Required]
      public virtual Int16? LocationID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual Decimal? CostRate { get; set; }
      [Required]
      public virtual Decimal? Availability { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.Product")]

   public partial class DbProduct : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(25)]
      public virtual String ProductNumber { get; set; }
      [Required]
      public virtual Boolean? MakeFlag { get; set; }
      [Required]
      public virtual Boolean? FinishedGoodsFlag { get; set; }
      [MaxLength(15)]
      public virtual String Color { get; set; }
      [Required]
      public virtual Int16? SafetyStockLevel { get; set; }
      [Required]
      public virtual Int16? ReorderPoint { get; set; }
      [Required]
      public virtual Decimal? StandardCost { get; set; }
      [Required]
      public virtual Decimal? ListPrice { get; set; }
      [MaxLength(5)]
      public virtual String Size { get; set; }
      [MaxLength(3)]

      public virtual String SizeUnitMeasureCode { get; set; }
      [MaxLength(3)]

      public virtual String WeightUnitMeasureCode { get; set; }
      public virtual Decimal? Weight { get; set; }
      [Required]
      public virtual Int32? DaysToManufacture { get; set; }
      [MaxLength(2)]
      public virtual String ProductLine { get; set; }
      [MaxLength(2)]
      public virtual String Class { get; set; }
      [MaxLength(2)]
      public virtual String Style { get; set; }

      public virtual Int32? ProductSubcategoryID { get; set; }

      public virtual Int32? ProductModelID { get; set; }
      [Required]
      public virtual DateTime? SellStartDate { get; set; }
      public virtual DateTime? SellEndDate { get; set; }
      public virtual DateTime? DiscontinuedDate { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductCategory")]

   public partial class DbProductCategory : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductCategoryID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductCostHistory")]
   public partial class DbProductCostHistory
   {
      [Key, Required]

      public virtual Int32? ProductID { get; set; }
      [Key, Required]
      public virtual DateTime? StartDate { get; set; }
      public virtual DateTime? EndDate { get; set; }
      [Required]
      public virtual Decimal? StandardCost { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductDescription")]

   public partial class DbProductDescription : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductDescriptionID { get; set; }
      [Required, MaxLength(400)]
      public virtual String Description { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductDocument")]
   public partial class DbProductDocument : DbTable
   {
      [Key, Required]

      public virtual Int32? ProductID { get; set; }
      [Key, Required]

      public virtual SqlHierarchyId? DocumentNode { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductInventory")]
   public partial class DbProductInventory : DbTable
   {
      [Key, Required]

      public virtual Int32? ProductID { get; set; }
      [Key, Required]

      public virtual Int16? LocationID { get; set; }
      [Required, MaxLength(10)]
      public virtual String Shelf { get; set; }
      [Required]
      public virtual Byte? Bin { get; set; }
      [Required]
      public virtual Int16? Quantity { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductListPriceHistory")]
   public partial class DbProductListPriceHistory : DbTable
   {
      [Key, Required]

      public virtual Int32? ProductID { get; set; }
      [Key, Required]
      public virtual DateTime? StartDate { get; set; }
      public virtual DateTime? EndDate { get; set; }
      [Required]
      public virtual Decimal? ListPrice { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductModel")]

   public partial class DbProductModel : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductModelID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      public virtual String CatalogDescription { get; set; }
      public virtual String Instructions { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductModelIllustration")]
   public partial class DbProductModelIllustration : DbTable
   {
      [Key, Required]

      public virtual Int32? ProductModelID { get; set; }
      [Key, Required]

      public virtual Int32? IllustrationID { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductModelProductDescriptionCulture")]
   public partial class DbProductModelProductDescriptionCulture : DbTable
   {
      [Key, Required]

      public virtual Int32? ProductModelID { get; set; }
      [Key, Required]

      public virtual Int32? ProductDescriptionID { get; set; }
      [Key, Required, MaxLength(6)]

      public virtual String CultureID { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductPhoto")]

   public partial class DbProductPhoto : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductPhotoID { get; set; }
      public virtual Byte[] ThumbNailPhoto { get; set; }
      [MaxLength(50)]
      public virtual String ThumbnailPhotoFileName { get; set; }
      public virtual Byte[] LargePhoto { get; set; }
      [MaxLength(50)]
      public virtual String LargePhotoFileName { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductProductPhoto")]
   public partial class DbProductProductPhoto : DbTable
   {
      [Key, Required]

      public virtual Int32? ProductID { get; set; }
      [Key, Required]

      public virtual Int32? ProductPhotoID { get; set; }
      [Required]
      public virtual Boolean? Primary { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductReview")]
   public partial class DbProductReview : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductReviewID { get; set; }
      [Required]

      public virtual Int32? ProductID { get; set; }
      [Required, MaxLength(50)]
      public virtual String ReviewerName { get; set; }
      [Required]
      public virtual DateTime? ReviewDate { get; set; }
      [Required, MaxLength(50)]
      public virtual String EmailAddress { get; set; }
      [Required]
      public virtual Int32? Rating { get; set; }
      [MaxLength(3850)]
      public virtual String Comments { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ProductSubcategory")]

   public partial class DbProductSubcategory : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductSubcategoryID { get; set; }
      [Required]

      public virtual Int32? ProductCategoryID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.ScrapReason")]

   public partial class DbScrapReason : DbTable
   {
      [Key, Required]
      public virtual Int16? ScrapReasonID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.TransactionHistory")]
   public partial class DbTransactionHistory : DbTable
   {
      [Key, Required]
      public virtual Int32? TransactionID { get; set; }
      [Required]

      public virtual Int32? ProductID { get; set; }
      [Required]
      public virtual Int32? ReferenceOrderID { get; set; }
      [Required]
      public virtual Int32? ReferenceOrderLineID { get; set; }
      [Required]
      public virtual DateTime? TransactionDate { get; set; }
      [Required, MaxLength(1)]
      public virtual String TransactionType { get; set; }
      [Required]
      public virtual Int32? Quantity { get; set; }
      [Required]
      public virtual Decimal? ActualCost { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.TransactionHistoryArchive")]
   public partial class DbTransactionHistoryArchive : DbTable
   {
      [Key, Required]
      public virtual Int32? TransactionID { get; set; }
      [Required]
      public virtual Int32? ProductID { get; set; }
      [Required]
      public virtual Int32? ReferenceOrderID { get; set; }
      [Required]
      public virtual Int32? ReferenceOrderLineID { get; set; }
      [Required]
      public virtual DateTime? TransactionDate { get; set; }
      [Required, MaxLength(1)]
      public virtual String TransactionType { get; set; }
      [Required]
      public virtual Int32? Quantity { get; set; }
      [Required]
      public virtual Decimal? ActualCost { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.UnitMeasure")]

   public partial class DbUnitMeasure : DbTable
   {
      [Key, Required, MaxLength(3)]
      public virtual String UnitMeasureCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.WorkOrder")]

   public partial class DbWorkOrder : DbTable
   {
      [Key, Required]
      public virtual Int32? WorkOrderID { get; set; }
      [Required]

      public virtual Int32? ProductID { get; set; }
      [Required]
      public virtual Int32? OrderQty { get; set; }
      public virtual Int32? StockedQty { get; set; }
      [Required]
      public virtual Int16? ScrappedQty { get; set; }
      [Required]
      public virtual DateTime? StartDate { get; set; }
      public virtual DateTime? EndDate { get; set; }
      [Required]
      public virtual DateTime? DueDate { get; set; }

      public virtual Int16? ScrapReasonID { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.WorkOrderRouting")]
   public partial class DbWorkOrderRouting : DbTable
   {
      [Key, Required]

      public virtual Int32? WorkOrderID { get; set; }
      [Key, Required]
      public virtual Int32? ProductID { get; set; }
      [Key, Required]
      public virtual Int16? OperationSequence { get; set; }
      [Required]

      public virtual Int16? LocationID { get; set; }
      [Required]
      public virtual DateTime? ScheduledStartDate { get; set; }
      [Required]
      public virtual DateTime? ScheduledEndDate { get; set; }
      public virtual DateTime? ActualStartDate { get; set; }
      public virtual DateTime? ActualEndDate { get; set; }
      public virtual Decimal? ActualResourceHrs { get; set; }
      [Required]
      public virtual Decimal? PlannedCost { get; set; }
      public virtual Decimal? ActualCost { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Purchasing.ProductVendor")]
   public partial class DbProductVendor : DbTable
   {
      [Key, Required]

      public virtual Int32? ProductID { get; set; }
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Required]
      public virtual Int32? AverageLeadTime { get; set; }
      [Required]
      public virtual Decimal? StandardPrice { get; set; }
      public virtual Decimal? LastReceiptCost { get; set; }
      public virtual DateTime? LastReceiptDate { get; set; }
      [Required]
      public virtual Int32? MinOrderQty { get; set; }
      [Required]
      public virtual Int32? MaxOrderQty { get; set; }
      public virtual Int32? OnOrderQty { get; set; }
      [Required, MaxLength(3)]

      public virtual String UnitMeasureCode { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Purchasing.PurchaseOrderDetail")]
   public partial class DbPurchaseOrderDetail : DbTable
   {
      [Key, Required]

      public virtual Int32? PurchaseOrderID { get; set; }
      [Key, Required]
      public virtual Int32? PurchaseOrderDetailID { get; set; }
      [Required]
      public virtual DateTime? DueDate { get; set; }
      [Required]
      public virtual Int16? OrderQty { get; set; }
      [Required]

      public virtual Int32? ProductID { get; set; }
      [Required]
      public virtual Decimal? UnitPrice { get; set; }
      public virtual Decimal? LineTotal { get; set; }
      [Required]
      public virtual Decimal? ReceivedQty { get; set; }
      [Required]
      public virtual Decimal? RejectedQty { get; set; }
      public virtual Decimal? StockedQty { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Purchasing.PurchaseOrderHeader")]

   public partial class DbPurchaseOrderHeader : DbTable
   {
      [Key, Required]
      public virtual Int32? PurchaseOrderID { get; set; }
      [Required]
      public virtual Byte? RevisionNumber { get; set; }
      [Required]
      public virtual Byte? Status { get; set; }
      [Required]

      public virtual Int32? EmployeeID { get; set; }
      [Required]

      public virtual Int32? VendorID { get; set; }
      [Required]

      public virtual Int32? ShipMethodID { get; set; }
      [Required]
      public virtual DateTime? OrderDate { get; set; }
      public virtual DateTime? ShipDate { get; set; }
      [Required]
      public virtual Decimal? SubTotal { get; set; }
      [Required]
      public virtual Decimal? TaxAmt { get; set; }
      [Required]
      public virtual Decimal? Freight { get; set; }
      public virtual Decimal? TotalDue { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Purchasing.ShipMethod")]

   public partial class DbShipMethod : DbTable
   {
      [Key, Required]
      public virtual Int32? ShipMethodID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual Decimal? ShipBase { get; set; }
      [Required]
      public virtual Decimal? ShipRate { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Purchasing.Vendor")]

   public partial class DbVendor : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(15)]
      public virtual String AccountNumber { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual Byte? CreditRating { get; set; }
      [Required]
      public virtual Boolean? PreferredVendorStatus { get; set; }
      [Required]
      public virtual Boolean? ActiveFlag { get; set; }
      [MaxLength(1024)]
      public virtual String PurchasingWebServiceURL { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.CountryRegionCurrency")]
   public partial class DbCountryRegionCurrency : DbTable
   {
      [Key, Required, MaxLength(3)]

      public virtual String CountryRegionCode { get; set; }
      [Key, Required, MaxLength(3)]

      public virtual String CurrencyCode { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.CreditCard")]

   public partial class DbCreditCard : DbTable
   {
      [Key, Required]
      public virtual Int32? CreditCardID { get; set; }
      [Required, MaxLength(50)]
      public virtual String CardType { get; set; }
      [Required, MaxLength(25)]
      public virtual String CardNumber { get; set; }
      [Required]
      public virtual Byte? ExpMonth { get; set; }
      [Required]
      public virtual Int16? ExpYear { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.Currency")]

   public partial class DbCurrency : DbTable
   {
      [Key, Required, MaxLength(3)]
      public virtual String CurrencyCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.CurrencyRate")]

   public partial class DbCurrencyRate : DbTable
   {
      [Key, Required]
      public virtual Int32? CurrencyRateID { get; set; }
      [Required]
      public virtual DateTime? CurrencyRateDate { get; set; }
      [Required, MaxLength(3)]

      public virtual String FromCurrencyCode { get; set; }
      [Required, MaxLength(3)]

      public virtual String ToCurrencyCode { get; set; }
      [Required]
      public virtual Decimal? AverageRate { get; set; }
      [Required]
      public virtual Decimal? EndOfDayRate { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.Customer")]

   public partial class DbCustomer : DbTable
   {
      [Key, Required]
      public virtual Int32? CustomerID { get; set; }

      public virtual Int32? PersonID { get; set; }

      public virtual Int32? StoreID { get; set; }

      public virtual Int32? TerritoryID { get; set; }
      [MaxLength(10)]
      public virtual String AccountNumber { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.PersonCreditCard")]
   public partial class DbPersonCreditCard : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]

      public virtual Int32? CreditCardID { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesOrderDetail")]
   public partial class DbSalesOrderDetail : DbTable
   {
      [Key, Required]

      public virtual Int32? SalesOrderID { get; set; }
      [Key, Required]
      public virtual Int32? SalesOrderDetailID { get; set; }
      [MaxLength(25)]
      public virtual String CarrierTrackingNumber { get; set; }
      [Required]
      public virtual Int16? OrderQty { get; set; }
      [Required]

      public virtual Int32? ProductID { get; set; }
      [Required]

      public virtual Int32? SpecialOfferID { get; set; }
      [Required]
      public virtual Decimal? UnitPrice { get; set; }
      [Required]
      public virtual Decimal? UnitPriceDiscount { get; set; }
      public virtual Decimal? LineTotal { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesOrderHeader")]

   public partial class DbSalesOrderHeader : DbTable
   {
      [Key, Required]
      public virtual Int32? SalesOrderID { get; set; }
      [Required]
      public virtual Byte? RevisionNumber { get; set; }
      [Required]
      public virtual DateTime? OrderDate { get; set; }
      [Required]
      public virtual DateTime? DueDate { get; set; }
      public virtual DateTime? ShipDate { get; set; }
      [Required]
      public virtual Byte? Status { get; set; }
      [Required]
      public virtual Boolean? OnlineOrderFlag { get; set; }
      [MaxLength(25)]
      public virtual String SalesOrderNumber { get; set; }
      [MaxLength(25)]
      public virtual String PurchaseOrderNumber { get; set; }
      [MaxLength(15)]
      public virtual String AccountNumber { get; set; }
      [Required]

      public virtual Int32? CustomerID { get; set; }

      public virtual Int32? SalesPersonID { get; set; }

      public virtual Int32? TerritoryID { get; set; }
      [Required]

      public virtual Int32? BillToAddressID { get; set; }
      [Required]

      public virtual Int32? ShipToAddressID { get; set; }
      [Required]

      public virtual Int32? ShipMethodID { get; set; }

      public virtual Int32? CreditCardID { get; set; }
      [MaxLength(15)]
      public virtual String CreditCardApprovalCode { get; set; }

      public virtual Int32? CurrencyRateID { get; set; }
      [Required]
      public virtual Decimal? SubTotal { get; set; }
      [Required]
      public virtual Decimal? TaxAmt { get; set; }
      [Required]
      public virtual Decimal? Freight { get; set; }
      public virtual Decimal? TotalDue { get; set; }
      [MaxLength(128)]
      public virtual String Comment { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesOrderHeaderSalesReason")]
   public partial class DbSalesOrderHeaderSalesReason : DbTable
   {
      [Key, Required]

      public virtual Int32? SalesOrderID { get; set; }
      [Key, Required]

      public virtual Int32? SalesReasonID { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesPerson")]

   public partial class DbSalesPerson : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }

      public virtual Int32? TerritoryID { get; set; }
      public virtual Decimal? SalesQuota { get; set; }
      [Required]
      public virtual Decimal? Bonus { get; set; }
      [Required]
      public virtual Decimal? CommissionPct { get; set; }
      [Required]
      public virtual Decimal? SalesYTD { get; set; }
      [Required]
      public virtual Decimal? SalesLastYear { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesPersonQuotaHistory")]
   public partial class DbSalesPersonQuotaHistory : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]
      public virtual DateTime? QuotaDate { get; set; }
      [Required]
      public virtual Decimal? SalesQuota { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesReason")]

   public partial class DbSalesReason : DbTable
   {
      [Key, Required]
      public virtual Int32? SalesReasonID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(50)]
      public virtual String ReasonType { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesTaxRate")]
   public partial class DbSalesTaxRate : DbTable
   {
      [Key, Required]
      public virtual Int32? SalesTaxRateID { get; set; }
      [Required]

      public virtual Int32? StateProvinceID { get; set; }
      [Required]
      public virtual Byte? TaxType { get; set; }
      [Required]
      public virtual Decimal? TaxRate { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesTerritory")]

   public partial class DbSalesTerritory : DbTable
   {
      [Key, Required]
      public virtual Int32? TerritoryID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(3)]

      public virtual String CountryRegionCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String Group { get; set; }
      [Required]
      public virtual Decimal? SalesYTD { get; set; }
      [Required]
      public virtual Decimal? SalesLastYear { get; set; }
      [Required]
      public virtual Decimal? CostYTD { get; set; }
      [Required]
      public virtual Decimal? CostLastYear { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SalesTerritoryHistory")]
   public partial class DbSalesTerritoryHistory : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Key, Required]

      public virtual Int32? TerritoryID { get; set; }
      [Key, Required]
      public virtual DateTime? StartDate { get; set; }
      public virtual DateTime? EndDate { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.ShoppingCartItem")]
   public partial class DbShoppingCartItem : DbTable
   {
      [Key, Required]
      public virtual Int32? ShoppingCartItemID { get; set; }
      [Required, MaxLength(50)]
      public virtual String ShoppingCartID { get; set; }
      [Required]
      public virtual Int32? Quantity { get; set; }
      [Required]

      public virtual Int32? ProductID { get; set; }
      [Required]
      public virtual DateTime? DateCreated { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SpecialOffer")]

   public partial class DbSpecialOffer : DbTable
   {
      [Key, Required]
      public virtual Int32? SpecialOfferID { get; set; }
      [Required, MaxLength(255)]
      public virtual String Description { get; set; }
      [Required]
      public virtual Decimal? DiscountPct { get; set; }
      [Required, MaxLength(50)]
      public virtual String Type { get; set; }
      [Required, MaxLength(50)]
      public virtual String Category { get; set; }
      [Required]
      public virtual DateTime? StartDate { get; set; }
      [Required]
      public virtual DateTime? EndDate { get; set; }
      [Required]
      public virtual Int32? MinQty { get; set; }
      public virtual Int32? MaxQty { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.SpecialOfferProduct")]

   public partial class DbSpecialOfferProduct : DbTable
   {
      [Key, Required]

      public virtual Int32? SpecialOfferID { get; set; }
      [Key, Required]

      public virtual Int32? ProductID { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Sales.Store")]

   public partial class DbStore : DbTable
   {
      [Key, Required]

      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }

      public virtual Int32? SalesPersonID { get; set; }
      public virtual String Demographics { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.vEmployee")]
   public partial class DbvEmployee : DbTable
   {
      [Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [Required, MaxLength(50)]
      public virtual String JobTitle { get; set; }
      [MaxLength(25)]
      public virtual String PhoneNumber { get; set; }
      [MaxLength(50)]
      public virtual String PhoneNumberType { get; set; }
      [MaxLength(50)]
      public virtual String EmailAddress { get; set; }
      [Required]
      public virtual Int32? EmailPromotion { get; set; }
      [Required, MaxLength(60)]
      public virtual String AddressLine1 { get; set; }
      [MaxLength(60)]
      public virtual String AddressLine2 { get; set; }
      [Required, MaxLength(30)]
      public virtual String City { get; set; }
      [Required, MaxLength(50)]
      public virtual String StateProvinceName { get; set; }
      [Required, MaxLength(15)]
      public virtual String PostalCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String CountryRegionName { get; set; }
      public virtual String AdditionalContactInfo { get; set; }
   }
   [Table("HumanResources.vEmployeeDepartment")]
   public partial class DbvEmployeeDepartment : DbTable
   {
      [Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [Required, MaxLength(50)]
      public virtual String JobTitle { get; set; }
      [Required, MaxLength(50)]
      public virtual String Department { get; set; }
      [Required, MaxLength(50)]
      public virtual String GroupName { get; set; }
      [Required]
      public virtual DateTime? StartDate { get; set; }
   }
   [Table("HumanResources.vEmployeeDepartmentHistory")]
   public partial class DbvEmployeeDepartmentHistory : DbTable
   {
      [Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [Required, MaxLength(50)]
      public virtual String Shift { get; set; }
      [Required, MaxLength(50)]
      public virtual String Department { get; set; }
      [Required, MaxLength(50)]
      public virtual String GroupName { get; set; }
      [Required]
      public virtual DateTime? StartDate { get; set; }
      public virtual DateTime? EndDate { get; set; }
   }
   [Table("HumanResources.vJobCandidate")]
   public partial class DbvJobCandidate : DbTable
   {
      [Key, Required]
      public virtual Int32? JobCandidateID { get; set; }
      public virtual Int32? BusinessEntityID { get; set; }
      [MaxLength(30), Column("Name\\.Prefix")]
      public virtual String Name_Prefix { get; set; }
      [MaxLength(30), Column("Name\\.First")]
      public virtual String Name_First { get; set; }
      [MaxLength(30), Column("Name\\.Middle")]
      public virtual String Name_Middle { get; set; }
      [MaxLength(30), Column("Name\\.Last")]
      public virtual String Name_Last { get; set; }
      [MaxLength(30), Column("Name\\.Suffix")]
      public virtual String Name_Suffix { get; set; }
      public virtual String Skills { get; set; }
      [MaxLength(30), Column("Addr\\.Type")]
      public virtual String Addr_Type { get; set; }
      [MaxLength(100), Column("Addr\\.Loc\\.CountryRegion")]
      public virtual String Addr_Loc_CountryRegion { get; set; }
      [MaxLength(100), Column("Addr\\.Loc\\.State")]
      public virtual String Addr_Loc_State { get; set; }
      [MaxLength(100), Column("Addr\\.Loc\\.City")]
      public virtual String Addr_Loc_City { get; set; }
      [MaxLength(20), Column("Addr\\.PostalCode")]
      public virtual String Addr_PostalCode { get; set; }
      public virtual String EMail { get; set; }
      public virtual String WebSite { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("HumanResources.vJobCandidateEducation")]
   public partial class DbvJobCandidateEducation : DbTable
   {
      [Key, Required]
      public virtual Int32? JobCandidateID { get; set; }
      [Column("Edu\\.Level")]
      public virtual String Edu_Level { get; set; }
      [Column("Edu\\.StartDate")]
      public virtual DateTime? Edu_StartDate { get; set; }
      [Column("Edu\\.EndDate")]
      public virtual DateTime? Edu_EndDate { get; set; }
      [MaxLength(50), Column("Edu\\.Degree")]
      public virtual String Edu_Degree { get; set; }
      [MaxLength(50), Column("Edu\\.Major")]
      public virtual String Edu_Major { get; set; }
      [MaxLength(50), Column("Edu\\.Minor")]
      public virtual String Edu_Minor { get; set; }
      [MaxLength(5), Column("Edu\\.GPA")]
      public virtual String Edu_GPA { get; set; }
      [MaxLength(5), Column("Edu\\.GPAScale")]
      public virtual String Edu_GPAScale { get; set; }
      [MaxLength(100), Column("Edu\\.School")]
      public virtual String Edu_School { get; set; }
      [MaxLength(100), Column("Edu\\.Loc\\.CountryRegion")]
      public virtual String Edu_Loc_CountryRegion { get; set; }
      [MaxLength(100), Column("Edu\\.Loc\\.State")]
      public virtual String Edu_Loc_State { get; set; }
      [MaxLength(100), Column("Edu\\.Loc\\.City")]
      public virtual String Edu_Loc_City { get; set; }
   }
   [Table("HumanResources.vJobCandidateEmployment")]
   public partial class DbvJobCandidateEmployment : DbTable
   {
      [Key, Required]
      public virtual Int32? JobCandidateID { get; set; }
      [Column("Emp\\.StartDate")]
      public virtual DateTime? Emp_StartDate { get; set; }
      [Column("Emp\\.EndDate")]
      public virtual DateTime? Emp_EndDate { get; set; }
      [MaxLength(100), Column("Emp\\.OrgName")]
      public virtual String Emp_OrgName { get; set; }
      [MaxLength(100), Column("Emp\\.JobTitle")]
      public virtual String Emp_JobTitle { get; set; }
      [Column("Emp\\.Responsibility")]
      public virtual String Emp_Responsibility { get; set; }
      [Column("Emp\\.FunctionCategory")]
      public virtual String Emp_FunctionCategory { get; set; }
      [Column("Emp\\.IndustryCategory")]
      public virtual String Emp_IndustryCategory { get; set; }
      [Column("Emp\\.Loc\\.CountryRegion")]
      public virtual String Emp_Loc_CountryRegion { get; set; }
      [Column("Emp\\.Loc\\.State")]
      public virtual String Emp_Loc_State { get; set; }
      [Column("Emp\\.Loc\\.City")]
      public virtual String Emp_Loc_City { get; set; }
   }
   [Table("Person.vAdditionalContactInfo")]
   public partial class DbvAdditionalContactInfo : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(50)]
      public virtual String TelephoneNumber { get; set; }
      public virtual String TelephoneSpecialInstructions { get; set; }
      [MaxLength(50)]
      public virtual String Street { get; set; }
      [MaxLength(50)]
      public virtual String City { get; set; }
      [MaxLength(50)]
      public virtual String StateProvince { get; set; }
      [MaxLength(50)]
      public virtual String PostalCode { get; set; }
      [MaxLength(50)]
      public virtual String CountryRegion { get; set; }
      public virtual String HomeAddressSpecialInstructions { get; set; }
      [MaxLength(128)]
      public virtual String EMailAddress { get; set; }
      public virtual String EMailSpecialInstructions { get; set; }
      [MaxLength(50)]
      public virtual String EMailTelephoneNumber { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Person.vStateProvinceCountryRegion")]
   public partial class DbvStateProvinceCountryRegion : DbTable
   {
      [Key, Required]
      public virtual Int32? StateProvinceID { get; set; }
      [Required, MaxLength(3)]
      public virtual String StateProvinceCode { get; set; }
      [Required]
      public virtual Boolean? IsOnlyStateProvinceFlag { get; set; }
      [Required, MaxLength(50)]
      public virtual String StateProvinceName { get; set; }
      [Required]
      public virtual Int32? TerritoryID { get; set; }
      [Key, Required, MaxLength(3)]
      public virtual String CountryRegionCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String CountryRegionName { get; set; }
   }
   [Table("Production.vProductAndDescription")]
   public partial class DbvProductAndDescription : DbTable
   {
      [Required]
      public virtual Int32? ProductID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(50)]
      public virtual String ProductModel { get; set; }
      [Required, MaxLength(6)]
      public virtual String CultureID { get; set; }
      [Required, MaxLength(400)]
      public virtual String Description { get; set; }
   }
   [Table("Production.vProductModelCatalogDescription")]
   public partial class DbvProductModelCatalogDescription : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductModelID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      public virtual String Summary { get; set; }
      public virtual String Manufacturer { get; set; }
      [MaxLength(30)]
      public virtual String Copyright { get; set; }
      [MaxLength(256)]
      public virtual String ProductURL { get; set; }
      [MaxLength(256)]
      public virtual String WarrantyPeriod { get; set; }
      [MaxLength(256)]
      public virtual String WarrantyDescription { get; set; }
      [MaxLength(256)]
      public virtual String NoOfYears { get; set; }
      [MaxLength(256)]
      public virtual String MaintenanceDescription { get; set; }
      [MaxLength(256)]
      public virtual String Wheel { get; set; }
      [MaxLength(256)]
      public virtual String Saddle { get; set; }
      [MaxLength(256)]
      public virtual String Pedal { get; set; }
      public virtual String BikeFrame { get; set; }
      [MaxLength(256)]
      public virtual String Crankset { get; set; }
      [MaxLength(256)]
      public virtual String PictureAngle { get; set; }
      [MaxLength(256)]
      public virtual String PictureSize { get; set; }
      [MaxLength(256)]
      public virtual String ProductPhotoID { get; set; }
      [MaxLength(256)]
      public virtual String Material { get; set; }
      [MaxLength(256)]
      public virtual String Color { get; set; }
      [MaxLength(256)]
      public virtual String ProductLine { get; set; }
      [MaxLength(256)]
      public virtual String Style { get; set; }
      [MaxLength(1024)]
      public virtual String RiderExperience { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Production.vProductModelInstructions")]
   public partial class DbvProductModelInstructions : DbTable
   {
      [Key, Required]
      public virtual Int32? ProductModelID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      public virtual String Instructions { get; set; }
      public virtual Int32? LocationID { get; set; }
      public virtual Decimal? SetupHours { get; set; }
      public virtual Decimal? MachineHours { get; set; }
      public virtual Decimal? LaborHours { get; set; }
      public virtual Int32? LotSize { get; set; }
      [MaxLength(1024)]
      public virtual String Step { get; set; }
      [Required]
      public virtual Guid? rowguid { get; set; }
      [Required]
      public virtual DateTime? ModifiedDate { get; set; }
   }
   [Table("Purchasing.vVendorWithAddresses")]
   public partial class DbvVendorWithAddresses : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(50)]
      public virtual String AddressType { get; set; }
      [Required, MaxLength(60)]
      public virtual String AddressLine1 { get; set; }
      [MaxLength(60)]
      public virtual String AddressLine2 { get; set; }
      [Required, MaxLength(30)]
      public virtual String City { get; set; }
      [Required, MaxLength(50)]
      public virtual String StateProvinceName { get; set; }
      [Required, MaxLength(15)]
      public virtual String PostalCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String CountryRegionName { get; set; }
   }
   [Table("Purchasing.vVendorWithContacts")]
   public partial class DbvVendorWithContacts : DbTable
   {
      [Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(50)]
      public virtual String ContactType { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [MaxLength(25)]
      public virtual String PhoneNumber { get; set; }
      [MaxLength(50)]
      public virtual String PhoneNumberType { get; set; }
      [MaxLength(50)]
      public virtual String EmailAddress { get; set; }
      [Required]
      public virtual Int32? EmailPromotion { get; set; }
   }
   [Table("Sales.vIndividualCustomer")]
   public partial class DbvIndividualCustomer : DbTable
   {
      [Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [MaxLength(25)]
      public virtual String PhoneNumber { get; set; }
      [MaxLength(50)]
      public virtual String PhoneNumberType { get; set; }
      [MaxLength(50)]
      public virtual String EmailAddress { get; set; }
      [Required]
      public virtual Int32? EmailPromotion { get; set; }
      [Required, MaxLength(50)]
      public virtual String AddressType { get; set; }
      [Required, MaxLength(60)]
      public virtual String AddressLine1 { get; set; }
      [MaxLength(60)]
      public virtual String AddressLine2 { get; set; }
      [Required, MaxLength(30)]
      public virtual String City { get; set; }
      [Required, MaxLength(50)]
      public virtual String StateProvinceName { get; set; }
      [Required, MaxLength(15)]
      public virtual String PostalCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String CountryRegionName { get; set; }
      public virtual String Demographics { get; set; }
   }
   [Table("Sales.vPersonDemographics")]
   public partial class DbvPersonDemographics : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      public virtual Decimal? TotalPurchaseYTD { get; set; }
      public virtual DateTime? DateFirstPurchase { get; set; }
      public virtual DateTime? BirthDate { get; set; }
      [MaxLength(1)]
      public virtual String MaritalStatus { get; set; }
      [MaxLength(30)]
      public virtual String YearlyIncome { get; set; }
      [MaxLength(1)]
      public virtual String Gender { get; set; }
      public virtual Int32? TotalChildren { get; set; }
      public virtual Int32? NumberChildrenAtHome { get; set; }
      [MaxLength(30)]
      public virtual String Education { get; set; }
      [MaxLength(30)]
      public virtual String Occupation { get; set; }
      public virtual Boolean? HomeOwnerFlag { get; set; }
      public virtual Int32? NumberCarsOwned { get; set; }
   }
   [Table("Sales.vSalesPerson")]
   public partial class DbvSalesPerson : DbTable
   {
      [Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [Required, MaxLength(50)]
      public virtual String JobTitle { get; set; }
      [MaxLength(25)]
      public virtual String PhoneNumber { get; set; }
      [MaxLength(50)]
      public virtual String PhoneNumberType { get; set; }
      [MaxLength(50)]
      public virtual String EmailAddress { get; set; }
      [Required]
      public virtual Int32? EmailPromotion { get; set; }
      [Required, MaxLength(60)]
      public virtual String AddressLine1 { get; set; }
      [MaxLength(60)]
      public virtual String AddressLine2 { get; set; }
      [Required, MaxLength(30)]
      public virtual String City { get; set; }
      [Required, MaxLength(50)]
      public virtual String StateProvinceName { get; set; }
      [Required, MaxLength(15)]
      public virtual String PostalCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String CountryRegionName { get; set; }
      [MaxLength(50)]
      public virtual String TerritoryName { get; set; }
      [MaxLength(50)]
      public virtual String TerritoryGroup { get; set; }
      public virtual Decimal? SalesQuota { get; set; }
      [Required]
      public virtual Decimal? SalesYTD { get; set; }
      [Required]
      public virtual Decimal? SalesLastYear { get; set; }
   }
   [Table("Sales.vSalesPersonSalesByFiscalYears")]
   public partial class DbvSalesPersonSalesByFiscalYears : DbTable
   {
      public virtual Int32? SalesPersonID { get; set; }
      [MaxLength(152)]
      public virtual String FullName { get; set; }
      [Required, MaxLength(50)]
      public virtual String JobTitle { get; set; }
      [Required, MaxLength(50)]
      public virtual String SalesTerritory { get; set; }
      [Column("2002")]
      public virtual Decimal? _2002 { get; set; }
      [Column("2003")]
      public virtual Decimal? _2003 { get; set; }
      [Column("2004")]
      public virtual Decimal? _2004 { get; set; }
   }
   [Table("Sales.vStoreWithAddresses")]
   public partial class DbvStoreWithAddresses : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(50)]
      public virtual String AddressType { get; set; }
      [Required, MaxLength(60)]
      public virtual String AddressLine1 { get; set; }
      [MaxLength(60)]
      public virtual String AddressLine2 { get; set; }
      [Required, MaxLength(30)]
      public virtual String City { get; set; }
      [Required, MaxLength(50)]
      public virtual String StateProvinceName { get; set; }
      [Required, MaxLength(15)]
      public virtual String PostalCode { get; set; }
      [Required, MaxLength(50)]
      public virtual String CountryRegionName { get; set; }
   }
   [Table("Sales.vStoreWithContacts")]
   public partial class DbvStoreWithContacts : DbTable
   {
      [Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      [Required, MaxLength(50)]
      public virtual String ContactType { get; set; }
      [MaxLength(8)]
      public virtual String Title { get; set; }
      [Required, MaxLength(50)]
      public virtual String FirstName { get; set; }
      [MaxLength(50)]
      public virtual String MiddleName { get; set; }
      [Required, MaxLength(50)]
      public virtual String LastName { get; set; }
      [MaxLength(10)]
      public virtual String Suffix { get; set; }
      [MaxLength(25)]
      public virtual String PhoneNumber { get; set; }
      [MaxLength(50)]
      public virtual String PhoneNumberType { get; set; }
      [MaxLength(50)]
      public virtual String EmailAddress { get; set; }
      [Required]
      public virtual Int32? EmailPromotion { get; set; }
   }
   [Table("Sales.vStoreWithDemographics")]
   public partial class DbvStoreWithDemographics : DbTable
   {
      [Key, Required]
      public virtual Int32? BusinessEntityID { get; set; }
      [Required, MaxLength(50)]
      public virtual String Name { get; set; }
      public virtual Decimal? AnnualSales { get; set; }
      public virtual Decimal? AnnualRevenue { get; set; }
      [MaxLength(50)]
      public virtual String BankName { get; set; }
      [MaxLength(5)]
      public virtual String BusinessType { get; set; }
      public virtual Int32? YearOpened { get; set; }
      [MaxLength(50)]
      public virtual String Specialty { get; set; }
      public virtual Int32? SquareFeet { get; set; }
      [MaxLength(30)]
      public virtual String Brands { get; set; }
      [MaxLength(30)]
      public virtual String Internet { get; set; }
      public virtual Int32? NumberEmployees { get; set; }
   }
}


