using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestaurantBookingSystem.Models
{
    public class NavigationMenuModel
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public bool Selected { get; set; }
        public List<NavigationMenuModel> InnerMenu { get; set; }

        public NavigationMenuModel()
        {
            InnerMenu = null;
        }
    }
}