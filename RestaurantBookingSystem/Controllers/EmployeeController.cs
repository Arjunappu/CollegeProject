using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly RestaurantUserRepository Repository;
        private readonly int PageSize = AppConfigHelper.MediumPageSize;
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }

        //
        // GET: /Employee/

        public ActionResult Index(int? page)
        {
            var users = MembershipService.GetAllUsers()
                .Where(
                    user =>
                    user.UserRole != UserBase.RestaurantUserRole.Guest &&
                    user.UserRole != UserBase.RestaurantUserRole.Customer);

            var paginatedlist = new PaginatedList<RestaurantUser>(users, page ?? 1, page.HasValue ? PageSize : users.Count());
            return View(paginatedlist);
        }

        public ActionResult Add()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(new RegisterEmployeeModel());
        }

        [HttpPost]
        public ActionResult Add(RegisterEmployeeModel model)
        {
            try
            {
                //Explicitly Validate Model for UserRole
                if (model.UserRole != EmployeeUserRole.Admin && model.UserRole != EmployeeUserRole.Employee)
                    ModelState.AddModelError("createstatus", "An Employee Needs to be assigned one of the Roles");
                if (ModelState.IsValid)
                {
                    // Attempt to register the employee
                    MembershipCreateStatus createStatus;
                    var restaurantuser = MembershipService.CreateUser(
                         new RestaurantUser(0, model.EmployeeName, Guid.NewGuid())
                         {
                             EmailId = model.Email,
                             MobileNumber = UInt64.Parse(model.MobileNumber ?? "0"),
                             Address = model.Address,
                             Password = model.Password,
                             UserRole = (UserBase.RestaurantUserRole)((int)model.UserRole)
                         }
                         , out createStatus, model.SecretQuestion, model.SecretAnswer);

                    if (createStatus == MembershipCreateStatus.Success && restaurantuser != null)
                    {
                        TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                        {
                            Message = String.Format("{0} was successfully Added as a new {1}", model.EmployeeName, model.UserRole),
                            Result = true,
                            State = ActionResultNotification.MessageState.Information
                        };
                        return RedirectToAction("Index");
                    }
                    ModelState.AddModelError("createstatus", AccountValidation.ErrorCodeToString(createStatus));
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("createstatus") ? ModelState["createstatus"].Errors[0].ErrorMessage : "There was an Error registering you as our Guest User, please try again",
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View(model);
            }
            catch (Exception e)
            {
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = e.Message,
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                return View(model);
            }
        }

        //Only Ajax Request is processed, a normal post request is redirected to Index action method
        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var user = MembershipService.GetUser(id, false);
                    var result = false;
                    if (user != null)
                        result = MembershipService.DeleteUser(user.UserName);
                    var message = result ? String.Format("The Employee '{0}' was successfully deleted", user.Name) : String.Format("Failed to delete the Employee, please try again");
                    return
                        Json(
                            new ActionResultNotification
                                {
                                    Result = result,
                                    Message = message
                                });
                }
                catch (Exception e)
                {
                    return
                        Json(
                            new ActionResultNotification
                            {
                                Result = false,
                                Message = e.Message
                            });
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var restaurantuser = (RestaurantUser)Repository.Find(id);

            if (restaurantuser != null)
            {
                restaurantuser = MembershipService.GetUser(restaurantuser.UserGuid, false);
                if (restaurantuser != null && restaurantuser.UserRole >= UserBase.RestaurantUserRole.Employee)
                    return View(new UpdateEmployeeModel
                                    {
                                        Address = restaurantuser.Address,
                                        MobileNumber = restaurantuser.MobileNumber.ToString(),
                                        UserGuid = restaurantuser.UserGuid,
                                        UserRole = (EmployeeUserRole)(int)restaurantuser.UserRole,
                                        EmployeeName = restaurantuser.Name,
                                    });
            }
            TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Message = String.Format("The Employee {0}cannot be edited because he {1}", restaurantuser != null ? restaurantuser.Name + " " : "", restaurantuser != null ? "is a " + restaurantuser.UserRole : "does not Exist"),
                        Result = false,
                        State = ActionResultNotification.MessageState.Error
                    };
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(UpdateEmployeeModel model)
        {
            try
            {
                //Explicitly Validate Model for UserRole
                if (model.UserRole != EmployeeUserRole.Admin && model.UserRole != EmployeeUserRole.Employee)
                    ModelState.AddModelError("updatestatus", "An Employee Needs to be assigned one of the Roles");
                if (ModelState.IsValid)
                {
                    // Get previous details
                    var existinguser = MembershipService.GetUser(model.UserGuid, false);
                    if (existinguser == null) throw new InvalidOperationException("Sorry the update cannot be done as no such user exist");
                    existinguser.Address = model.Address;
                    existinguser.Name = model.EmployeeName;
                    existinguser.MobileNumber = UInt64.Parse(model.MobileNumber ?? "0");
                    existinguser.UserRole = (UserBase.RestaurantUserRole)(int)model.UserRole;

                    //And push the changed details
                    MembershipService.UpdateUser(existinguser);
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Message = String.Format("Details for {0} was successfully Updated", model.EmployeeName),
                        Result = true,
                        State = ActionResultNotification.MessageState.Information
                    };
                    return RedirectToAction("Index");
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = "Cannot Update the details of the employee with given details, please try again",
                    Result = false,
                    State = ActionResultNotification.MessageState.Warning
                };
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View(model);
            }
            catch (Exception e)
            {
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = e.Message,
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                return View(model);
            }
        }

        public EmployeeController()
        {
            Repository = new RestaurantUserRepository();
        }
    }
}
