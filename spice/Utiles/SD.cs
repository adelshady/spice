using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spice.Utiles
{
    public class SD
    {
        public const string SessionFrontDiskAndManger = "SessionFrontDiskAndManger";
        public const string DefaultFoodImage = "images.jpg";
        public const string MangerUser = "Manger";
        public const string KitchenUser= "KitchenUser";
        public const string FrontDeskUser = "FrontDesk";
        public const string CustonerEndUser = "Customer";
        public const string StatusInProcess = "Being Prepared";

        public const string StatusSubmitted = "Submitted";

        public const string StatusReady = "Ready For Pickup";

        public const string StatusCompleted = "Completed";

        public const string StatusCancelled = "Cancelled";

        public const string PaymentStatusPending = "Pending";

        public const string PaymentStatusApproved = "Aprroved";

        public const string PaymentStatusRejected = "Rejected";

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
