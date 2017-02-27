using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace XAdo.Sql.Core.Parser
{
   public static class Constants
   {
      public static class SpecialChars
      {
         public const char

            // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
            SPECIAL_CHARS_STARTER = ':',

            // annotates a column that may be empty because of an outer join. The emitted binder needs to check if the
            // corresponding column is null before creating a referenced instance
            OUTER_JOIN_COLUMN = '?',

            // indicates the (a) primary key
            PRIMARY_KEY = '*',

            // indicates an autoincrement column, is considered to be a pkey as well
            AUTO_INCREMENT = '+',

            // indicates a caculated (and/or aggregated) column
            CALCULATED = '@',

            // indicates a NOT NULL column
            NOT_NULL = '!',

            // allow CREATE
            CREATE = 'C',

            // allow READ
            READ = 'R',

            // allow UPDATE
            UPDATE = 'U',

            // allow DELETE
            DELETE = 'D',

            // the database column seperator: dbo.Persons
            COLUMN_SEP = '.',

            // mapped column name sepeartor: Person.Address.Street
            NAME_SEP = '.',

            // mapping seperator: ./Address/Street/Id
            PATH_SEP = '/';

         public static readonly string
            COLUMN_SEP_STR = new string(COLUMN_SEP, 1),
            NAME_SEP_STR = new string(NAME_SEP, 1);

         // Path switches
         public const string
            CURRENT_PATH_STR = ".",
            PREV_PATH = "..",
            PREV_PREV_PATH = "...";

         // Special Chars that are allowed in columns, as column annotations
         public static readonly HashSet<char> Set = new HashSet<char>(new[]
         {
            SPECIAL_CHARS_STARTER, 
            OUTER_JOIN_COLUMN, 
            PRIMARY_KEY, 
            AUTO_INCREMENT, 
            CALCULATED, 
            NOT_NULL,
            CREATE,
            READ,
            UPDATE
         });

      }
   }
}