using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace XAdo.Quobs.Core.Parser
{
   public static class Constants
   {
      public static class Syntax
      {
         public static class Chars
         {
            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               SPECIAL_CHARS_STARTER = ':';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               OUTER_JOIN_COLUMN = '?';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               PRIMARY_KEY = '*';

            public const char
               // indicates JSON start char
               JSON_START = '{';

            public const char
               // indicates JSON end char
               JSON_END = '}';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               AUTO_INCREMENT = '+';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               CALCULATED = '@';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               NOT_NULL = '!';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               CREATE = 'C';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               READ = 'R';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               UPDATE = 'U';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               DELETE = 'D';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               COLUMN_SEP = '.';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               NAME_SEP = '.';

            public const char

               // this optional character mark the starting point from which annotation characters follow: FirstName:*CRU
               PATH_SEP = '/';

            public const char 
               // marks unique column
               UNIQUE = '#';


            public static readonly string
               COLUMN_SEP_STR = new string(COLUMN_SEP, 1);

            public static readonly string
               NAME_SEP_STR = new string(NAME_SEP, 1);

            // Special Chars that are allowed in columns, as column annotations

            public static readonly HashSet<char> TagCharsSet = new HashSet<char>(new[]
            {
               SPECIAL_CHARS_STARTER, 
               OUTER_JOIN_COLUMN, 
               PRIMARY_KEY, 
               AUTO_INCREMENT, 
               CALCULATED, 
               NOT_NULL,
               UNIQUE
            });

            public static readonly HashSet<char> TagCharsSplitSet = new HashSet<char>(new[]
            {
               SPECIAL_CHARS_STARTER, 
               OUTER_JOIN_COLUMN, 
               PRIMARY_KEY, 
               AUTO_INCREMENT, 
               CALCULATED, 
               NOT_NULL,
               UNIQUE,
               JSON_START
            });

         }

         // Path switches
         public const string
            CURRENT_PATH_STR = ".",
            PREV_PATH = "..",
            PREV_PREV_PATH = "...",

            TAG_COMMENT = "//",
            TAG_AS_GENERIC_LITERAL = "-- >";

      }
   }
}