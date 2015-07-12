using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;

namespace RestaurantBookingSystem.Models
{
    public class RestaurantMenuViewModel
    {
        /// <summary>
        /// Gets The Item Id
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public int ItemId { get; private set; }
        
        /// <summary>
        /// Gets or Sets a Name of the Menu Item
        /// </summary>
        [Required]
        [Display(Name = "Item Name")]
        [StringLength(100, ErrorMessage = "Maximum length for an Item's Name is 50")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets  A Description of the Menu Item
        /// </summary>
        [Required]
        [Display(Description = "A short description of the Menu Item")]
        [DataType(DataType.MultilineText)]
        [StringLength(100, ErrorMessage = "Maximum length for an Item Description is 100")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets the Price of the Menu Item
        /// </summary>
        [Display(Name = "Price", Description = "The price of the menu item")]
        [Required(ErrorMessage = "Price for a Menu Item is required")]
        [Range(0.1D, Double.MaxValue, ErrorMessage = "Price of a valid menu item cannot be less than 0.1")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or Sets the Picture File of the Menu Item
        /// </summary>
        [Display(Name = "Item Image", Description = "A 250x200 image for the menu Item")]
        [Required(ErrorMessage = "An Image for the Menu Item is required")]
        public string PictureFileName { get; set; }

        public RestaurantMenuViewModel()
        {
        }

        public RestaurantMenuViewModel(RestaurantMenuItem model)
        {
            Name = model.Name;
            PictureFileName = model.PictureFile;
            Description = model.Description;
            Price = model.Price;
        }
    }
}