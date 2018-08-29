using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lab23.Models;

namespace Lab23.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            GCWebORM ORM = new GCWebORM();
            Session userSession = new Session();
            if (Request.Cookies["SessionID"] == null)       //No Session Cookie Exists
            {
                HttpCookie sessionCookie = new HttpCookie("SessionID", userSession.ToString());
                sessionCookie.Expires = DateTime.Now.AddMinutes(30);
                Response.Cookies.Add(sessionCookie);
            }
            else                                            //Cookie Exists
            {

                try                                         //Try Querying the DB
                {
                    userSession = ORM.Sessions.Find(Request.Cookies["SessionID"].Value);
                }
                catch (NullReferenceException)              //Invalid Key, I think
                {
                    HttpCookie sessionCookie = new HttpCookie("SessionID", userSession.ToString());
                    sessionCookie.Expires = DateTime.Now.AddMinutes(30);
                }
                catch (Exception e)                         //General Exception, Reset Session
                {
                    ViewBag.Debug = e;

                    HttpCookie sessionCookie = new HttpCookie("SessionID", userSession.ToString());
                    sessionCookie.Expires = DateTime.Now.AddMinutes(30);
                }

                if (userSession.LogoffTime < DateTime.Now)  //Expired Session
                {
                    userSession = new Session();
                    HttpCookie sessionCookie = new HttpCookie("SessionID", userSession.ToString());
                    sessionCookie.Expires = DateTime.Now.AddMinutes(30);
                    Response.Cookies.Add(sessionCookie);
                }
                else                                        //Everything is Valid
                {
                    HttpCookie sessionCookie = new HttpCookie("SessionID", userSession.ToString());
                    sessionCookie.Expires = DateTime.Now.AddMinutes(30);
                    Response.Cookies.Add(sessionCookie);
                }

            }

            ORM.Sessions.Add(userSession);
            ORM.SaveChanges();

            ViewBag.UserSession = userSession.ToString();
            ViewBag.Items = ORM.Items.ToList();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            GCWebORM ORM = new GCWebORM();

            ViewBag.SessionList = ORM.Sessions.ToList();
            ViewBag.SessionList.Add(new Session());
            ViewBag.UserSession = ORM.Sessions.Find(Request.Cookies["SessionID"].Value);

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult NewItem()
        {

            return View();
        }

        public ActionResult SaveNewItem(Item newItem)
        {
            GCWebORM ORM = new GCWebORM();

            ORM.Items.Add(newItem);
            ORM.SaveChanges();

            return View();
        }

        public ActionResult DeleteItem(int itemID)
        {
            GCWebORM ORM = new GCWebORM();
            Item itemToDelete = ORM.Items.Find(itemID);
            ORM.Items.Remove(itemToDelete);
            try
            {
                ORM.SaveChanges();
            }
            catch (Exception e)
            {
                ViewBag.ServerException = e;
                return View("Error");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Error()
        {

            return View();
        }

        public ActionResult UpdateItem(int itemID)
        {
            GCWebORM ORM = new GCWebORM();
            Item itemToEdit = ORM.Items.Find(itemID);
            ViewBag.ItemToEdit = itemToEdit;

            return View();
        }

        public ActionResult SaveUpdatedItem(Item updatedItem)
        {
            GCWebORM ORM = new GCWebORM();
            Item oldItem = ORM.Items.Find(updatedItem.ItemID);

            oldItem.Name = updatedItem.Name;
            oldItem.Description = updatedItem.Description;
            oldItem.Stock = updatedItem.Stock;
            oldItem.Price = updatedItem.Price;

            ORM.Entry(oldItem).State = System.Data.Entity.EntityState.Modified;
            ORM.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Register()
        {

            return View();
        }

        public ActionResult AddNewUser(string Username, string Password, string Email, string Phone)
        {
            GCWebORM ORM = new GCWebORM();
            bool isAdmin = false;
            Guid UserID = Guid.NewGuid();
            User newUser = new User(UserID, Username, Password, Email, Phone, isAdmin);
            ORM.Users.Add(newUser);
            ORM.SaveChanges();
            Session newSession;

            try
            {
                Guid currentSession = Guid.Parse(Request.Cookies["SessionID"].Value);
                newSession = ORM.Sessions.Find(currentSession);
                newSession.LoginAs(newUser.UserID);
            }
            catch (NullReferenceException)
            {
                newSession = new Session();
                newSession.LoginAs(newUser.UserID);
            }
            catch (Exception e)
            {
                ViewBag.FormException = e;
                return View("Error");
            }

            HttpCookie SessionCookie = new HttpCookie("SessionID", newSession.SessionID.ToString("D"));
            Response.Cookies.Add(SessionCookie);

            return View("Index");
        }
    }
}