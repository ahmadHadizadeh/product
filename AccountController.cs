using Core.DTO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Core.Convertors;
using Core.Genarate;
using Core.Security;
using Core.Security.Sender;
using Core.Services.Interfaces;
using DataLayer.Entity.UserModel;
using Core.Services;

namespace Product.Controllers
{
    public class AccountController : Controller
    {
        private IUserservice _userservice;
        private IViewRenderService _renderService;

        public AccountController(IUserservice userservice, IViewRenderService renderService)
        {
            _userservice = userservice;
            _renderService = renderService;
        }


        #region Register

        [Route("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register(RegisterViewModel register)
        {
            if (!ModelState.IsValid)
            {
                return View(register);
            }
            if (_userservice.IsExistUserName(register.UserName))
            {
                ModelState.AddModelError("UserName", "نام کاربری معتبر نمی باشد");
                return View(register);
            }

            if (_userservice.IsExistEmail(FixedTexd.FixedEmail(register.Email)))
            {
                ModelState.AddModelError("Email", "ایمیل معتبر نمی باشد");
                return View(register);
            }
            DataLayer.Entity.UserModel.Users user = new Users()
            {
                ActiveCode = CreateUniqCode.BuildUniqCod(),
                Email = FixedTexd.FixedEmail(register.Email),
                IsActive = false,
                Password = PasswordEncript.EncriptPassword(register.Password),
                RegisterDate = DateTime.Now,
                UserImage = "Defult.jpg",
                UserName = register.UserName
            };
            _userservice.AddUser(user);
            string body = _renderService.RenderToStringAsync("_ActiveEmail", user);
            SendEmail.Send(user.Email, "ایمیل فعال سازی", body);

            return View("SeuccessRegister", user);
        }

        #endregion Register

        #region Login

        [Route("login", Name = "LoginGet")]
        
        public ActionResult Login(bool EditProfile = false)
        {
            ViewBag.EditProfile = EditProfile;

            return View();
        }

        [HttpPost]
        [Route("login", Name = "LoginPost")]
        public ActionResult Login(LoginViewModel login, IFormCollection form, string ReturnUrl = "/")
        {
            #region LoginPost

            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var user = _userservice.LoginUser(login);

            if (user != null)
            {
                if (user.IsActive)
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                        new Claim(ClaimTypes.Name,user.UserName)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var properties = new AuthenticationProperties
                    {
                        IsPersistent = login.RememberMe
                    };
                    HttpContext.SignInAsync(principal, properties);

                    ViewBag.IsSuccess = true;
                    if (ReturnUrl != "/")
                    {
                        return Redirect(ReturnUrl);
                    }
                    return View();
                }
                else
                {
                    ModelState.AddModelError("Email", "حساب کاربری شما فعال نمی باشد");
                }
            }
            ModelState.AddModelError("Email", "کاربری با مشخصات وارد شده یافت نشد");

            return View(login);

            #endregion LoginPost
        }
        #endregion Login

        #region ActiveCode

        public IActionResult ActiveCode(string id)
        {
            ViewBag.IsActive = _userservice.ActiveCodeAccount(id);
            return View();
        }

        #endregion ActiveCode

        #region Logout

        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Login");
        }

        #endregion Logout

        #region ForgotPassword

        [Route("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [Route("ForgotPassword")]
        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel forgot)
        {
            if (!ModelState.IsValid)
            {
                return View(forgot);
            }
            string fixemail = FixedTexd.FixedEmail(forgot.Email);
            var user = _userservice.GetUserByEmail(fixemail);
            if (user == null)
            {
                ModelState.AddModelError("Email", "کاربری یافت نشد");
                return View(forgot);
            }
            String bodyEmail = _renderService.RenderToStringAsync("_ForgotPassword", user);
            SendEmail.Send(user.Email, "بازیابی کلمه عبور", bodyEmail);
            ViewBag.IsSuccess = true;
            return View();
        }

        #endregion ForgotPassword

        #region ResetPassword

        public IActionResult ResetPassword(string id)
        {
            return View(new ResetPasswordViewModel()
            {
                ActiveCode = id,
            });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel reset)
        {
            if (!ModelState.IsValid)
            {
                return View(reset);
            }

            var user = _userservice.GetUserActiveCode(reset.ActiveCode);
            if (user == null)

                return NotFound();

            string ConvertPass = PasswordEncript.EncriptPassword(reset.Password);
            user.Password = ConvertPass;
            _userservice.UpdateUser(user);
            ViewBag.IsSuccessResstPass = true;
            return View(reset);
        }

        #endregion ResetPassword

    }
}
