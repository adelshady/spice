using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spice.Utiles
{
    public class SD
    {
        public const string DefaultFoodImage = "images.jpg";
        public const string MangerUser = "Manger";
        public const string KitchenUser= "KitchenUser";
        public const string FrontDeskUser = "FrontDesk";
        public const string CustonerEndUser = "Customer";
        




        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayindex = 0;
            bool inside = false;
            for(int i =0; i<source.Length; i++)
            {
                char let = source[i];
                if(let == '<')
                {
                    inside = true;
                    continue;
                }
                if(let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayindex] = let;
                    arrayindex++;
                }
            }
            return new string(array, 0, arrayindex);
        }
    }
}
