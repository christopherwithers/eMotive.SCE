using System;
using System.Collections.Generic;

namespace eMotive.Managers
{
    public static class ManagerExtensions
    {
     public static int FindIndex<T>(this IEnumerable<T> list, Predicate<T> finder)
       {
           int index = 0;
           foreach (var item in list)
           {
               
               if (finder(item))
               {
                   return index;
               }
               index++;
             //  index++;
           }
    
           return -1;
       }
    }
}
