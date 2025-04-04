using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TraceabilityV3.Models
{
    public static class Global
    {
        //public static string CentralURL { get; set; } = "https://rclmldev01.tsb.co.za:3448/";     //DEV HOSTED
        //public static string CentralURL { get; set; } = "https://localhost:44314/";               //DEV LOCAL
        public static string CentralURL { get; set; } = "https://rclstrace01.tsb.co.za:4443/";      //LIVE CENTRAL

    }
}