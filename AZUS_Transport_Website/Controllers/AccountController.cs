using AZUS_Transport_Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace AZUS_Transport_Website.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            using (ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie())
            {
                return View(db.Users.ToList());
            }
        }

        //public ActionResult GetItems(int id)
        //{
        //    ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
        //    return PartialView(db.Divisions.Where(c => c.Id == id).ToList());
        //}


        public ActionResult Register(int? statusID = null, int? divisionID = 1)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();

            IEnumerable<Users> users = db.Users;

            List<Divisions> divisions = db.Divisions.ToList();
            if (divisionID.HasValue)
            {
                var blg = divisions.FirstOrDefault(x => x.Id == divisionID);
                
                ViewBag.building = blg.Building;
            }

            //для RadioButton
            ViewBag.usrAdm = db.Users.FirstOrDefault(x => x.StatusID == 1);
            ViewBag.usrDspNIIAR = db.Users.FirstOrDefault(x => x.StatusID == 5);
            ViewBag.usrDspATA = db.Users.FirstOrDefault(x => x.StatusID == 7);
            ViewBag.usrDpr = db.Users.FirstOrDefault(x => x.StatusID == 6);
            var usrDrc = db.Users.FirstOrDefault(x => x.DivisionID == divisionID && x.StatusID == 3);
            var usrEcn = db.Users.FirstOrDefault(x => x.DivisionID == divisionID && x.StatusID == 4);
            ViewBag.usrDrc = usrDrc;
            ViewBag.usrEcn = usrEcn;
            ViewBag.applications = 0;
            if (usrDrc != null)
            {
                ViewBag.usrDrcChoice = usrDrc.SurName + " " + usrDrc.Name + " " + usrDrc.Partonymic;
            }
            else
            {
                ViewBag.usrDrcChoice = "Руководитель не зарегистрирован в системе!";
            }
            if (usrEcn != null)
            {
                ViewBag.usrEcnChoice = usrEcn.SurName + " " + usrEcn.Name + " " + usrEcn.Partonymic;
            }
            else
            {
                ViewBag.usrEcnChoice = "Экономист не зарегистрирован в системе!";
            }
            //для RadioButton

            if (statusID.HasValue)
            {
                users = users.Where(p => p.StatusID == statusID);
            }

            Users usr = new Users
            {
                Divisions_ = new SelectList(divisions, "Id", "Name")
            };

            return View(usr);
        }

        [HttpPost]
        public ActionResult Register(Users account, int? statusID = null, int? divisionID = 1)
        {
            ViewBag.applications = 0;
            if (ModelState.IsValid)
            {
                using (ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie())
                {
                    var us = db.Users.FirstOrDefault(x => x.Username == account.Username);

                    if (us == null)
                    {
                        db.Users.Add(account);
                        db.SaveChanges();

                        if (us == null)
                        {
                            var usr = db.Users.FirstOrDefault(u => u.Username == account.Username && u.Password == account.Password);

                            ViewBag.UserID = usr.Id;
                            ViewBag.statusID = usr.StatusID;
                            
                            Users.mode = usr.StatusID;
                            Users.UserID = usr.Id;
                            //Session["FirstName"] = usr.FirstName.ToString();
                            //Session["LastName"] = usr.LastName.ToString();
                            Session["Status"] = usr.StatusID;

                            return RedirectToAction("Index", "Home");
                        }
                    }

                    if (us != null)
                    {
                        ModelState.AddModelError("", "Логин занят!");

                    }

                }

            }
            ASUZ_Transport_DBEntitie bd = new ASUZ_Transport_DBEntitie();
            List<Divisions> divisions = bd.Divisions.ToList();
            IEnumerable<Users> users = bd.Users;
            //для RadioButton
            ViewBag.usrAdm = bd.Users.FirstOrDefault(x => x.StatusID == 1);
            ViewBag.usrDspNIIAR = bd.Users.FirstOrDefault(x => x.StatusID == 5);
            ViewBag.usrDspATA = bd.Users.FirstOrDefault(x => x.StatusID == 7);
            ViewBag.usrDpr = bd.Users.FirstOrDefault(x => x.StatusID == 6);
            var usrDrc = bd.Users.FirstOrDefault(x => x.DivisionID == divisionID && x.StatusID == 3);
            var usrEcn = bd.Users.FirstOrDefault(x => x.DivisionID == divisionID && x.StatusID == 4);
            ViewBag.usrDrc = usrDrc;
            ViewBag.usrEcn = usrEcn;
            if (usrDrc != null)
            {
                ViewBag.usrDrcChoice = usrDrc.SurName + " " + usrDrc.Name + " " + usrDrc.Partonymic;
            }
            else
            {
                ViewBag.usrDrcChoice = "Руководитель не зарегистрирован в системе!";
            }
            if (usrEcn != null)
            {
                ViewBag.usrEcnChoice = usrEcn.SurName + " " + usrEcn.Name + " " + usrEcn.Partonymic;
            }
            else
            {
                ViewBag.usrEcnChoice = "Экономист не зарегистрирован в системе!";
            }
            //для RadioButton

            if (statusID.HasValue)
            {
                users = users.Where(p => p.StatusID == statusID);
            }

            Users usrd = new Users
            {
                Divisions_ = new SelectList(divisions, "Id", "Name")
            };

            return View(usrd);
        }




        //Вход
        public ActionResult Login() //первоначальная загрузка
        {
            ViewBag.applications = 0;
            Users users = checkCookie();// данные из Cookie
            if (users == null)
            {
                return View();
            }
            else
            {
                ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
                var usr = db.Users.FirstOrDefault(u => u.Username == users.Username && u.Password == users.Password);
                if (usr != null)
                {
                    //Session["username"] = usr.Username;
                    ViewBag.statusID = usr.StatusID;
                    Users.mode = usr.StatusID;
                    Users.UserID = usr.Id;
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.error = "Аккаунт не существует";
                    return View();
                }

            }
        }

        //загрузка данных о пользователе из Cookie
        public Users checkCookie()
        {

            Users users = null;
            string username = string.Empty, password = string.Empty, status = string.Empty;
            if (Request.Cookies["username"] != null)
            {
                username = Request.Cookies["username"].Value;
            }
            if (Request.Cookies["password"] != null)
            {
                password = Request.Cookies["password"].Value;
            }
            if (Request.Cookies["status"] != null)
            {
                status = Request.Cookies["status"].Value;
            }

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password) && !String.IsNullOrEmpty(status))
            {
                users = new Users { Username = username, Password = password, StatusID = int.Parse(status)};
            }
            return users;
        }
        //загрузка данных о пользователе из Cookie

        [HttpPost]
        public ActionResult Login(Users user)
        {
            ViewBag.applications = 0;
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();

            var usr = db.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (usr != null)
            {
                //Session["username"] = usr.Username;
               
                ViewBag.UserID = usr.Id;
                ViewBag.statusID = usr.StatusID;
                Users.mode = usr.StatusID;
                Users.UserID = usr.Id;
                
                //Session["FirstName"] = usr.FirstName.ToString();
                //Session["LastName"] = usr.LastName.ToString();
                Session["Status"] = usr.StatusID;

                if (user.remember)// если checkBox = true, то данные сохраняются в Cookie
                {
                    HttpCookie ckUsername = new HttpCookie("username");
                    ckUsername.Expires = DateTime.Now.AddSeconds(3600);
                    ckUsername.Value = user.Username;
                    Response.Cookies.Add(ckUsername);

                    HttpCookie ckPassword = new HttpCookie("password");
                    ckPassword.Expires = DateTime.Now.AddSeconds(3600);
                    ckPassword.Value = user.Password;
                    Response.Cookies.Add(ckPassword);

                    HttpCookie ckStarus = new HttpCookie("status");
                    ckStarus.Expires = DateTime.Now.AddSeconds(3600);
                    ckStarus.Value = user.StatusID.ToString();
                    Response.Cookies.Add(ckStarus);
                }

                return RedirectToAction("Index", "Home");
            }
            if (user.Username != null && user.Password != null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль!");
                return View();
            }
            return View();
        }

        // кнопка вход
        public ActionResult LoggedIn()
        {
            if (ViewBag.UserID != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }


        }
        // кнопка вход

        //выход
        public ActionResult AccountExit()
        {
            //Session.Remove("username"); //очистка сессии
            ViewBag.statusID = null;
            ViewBag.UserID = null;
            ViewBag.building = null;
            ViewBag.usrDrcChoice = null;
            ViewBag.usrEcnChoice = null;
            Users.mode = 0;
            Users.UserID = 0;
            if (Response.Cookies["username"] != null) // очистка Cookie
            {
                HttpCookie ckUsername = new HttpCookie("username");
                ckUsername.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(ckUsername);
            }
            if (Response.Cookies["password"] != null)
            {
                HttpCookie ckPassword = new HttpCookie("password");
                ckPassword.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(ckPassword);
            }
            if (Response.Cookies["status"] != null)
            {
                HttpCookie ckstatus = new HttpCookie("status");
                ckstatus.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(ckstatus);
            }

            //return View("Login", "Account");


            //Session["UserId"] = null;
            //Session["FirstName"] = null;
            //Session["LastName"] = null;
            //Session["Status"] = null;
            return RedirectToAction("Login", "Account");
        }
        //выход
        public ActionResult Recover()
        {
            ViewBag.applications = 0;
            return View();
        }

        [HttpPost]
        public ActionResult Recover(Users users)
        {
            ViewBag.applications = 0;
            //if (ModelState.IsValid)
            //{
            using (ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie())
                {
                    var us = db.Users.FirstOrDefault(x => x.Email == users.Email);
                    var usr = db.Users.FirstOrDefault(x => x.StatusID == 5);
                    if (us != null)
                    {
                        SmtpClient SmtpClient = new SmtpClient("smtp.mail.ru", 2525);
                        SmtpClient.Credentials = new NetworkCredential(usr.Email, "DC7zZ5CvANF9GZhzK65V");
                        MailMessage Message = new MailMessage();
                        SmtpClient.EnableSsl = true;
                        Message.IsBodyHtml = true;
                        Message.From = new MailAddress(usr.Email);
                        Message.To.Add(new MailAddress(us.Email));
                        Message.Subject = "Восстановление доступа";
                        Message.Body = "Ваш логин: " + us.Username + "<br/>" + "Ваш пароль: " + us.Password + "<br/><br/>Перейти в " + "<a href=\"https://localhost:44324/Account/Login\">АСУЗ 'Транспорт'</a>"
                                 + "<style>.colorDiv {color: #d0d4ce}</style>" + "<br/><br/><hr><div class='colorDiv'>Данное письмо было сформировано автоматически подсистемой уведомления АСУЗ 'Транспорт'. Ответ не требуется</div>";

                        try
                        {
                            SmtpClient.Send(Message);
                            TempData["message"] = string.Format("Логин и пароль отправлены на электронную почту " + us.Email + "!");
                            return RedirectToAction("Login", "Account");
                        }
                        catch (Exception ex)
                        {
                            TempData["message"] = string.Format("Логин и пароль не отправлены на электронную почту! Причина:\n" + ex.Message);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Пользователь с почтой " + users.Email + " не зарегистрирован" + "!");
                        return View();
                    }
                }
            //}
  
        }

    }
}

