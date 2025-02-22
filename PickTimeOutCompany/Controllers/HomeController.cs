using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace PickTimeOutCompany.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                Session.Abandon();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string userid, string password)
        {
            string pwd = EC(password);
            string dept = "";
            using (SqlConnection con1 = new SqlConnection(@"Data Source=10.5.1.126;Initial Catalog=ACCOUNT; User ID=sa; Password=gmportalsa"))
            {
                con1.Open();
                string SelSQL = "SELECT * FROM UserAccount WHERE username = @userid AND password = @password AND FLAG = 'Y'";

                using (SqlCommand MScommand = new SqlCommand(SelSQL, con1))
                {
                    // Sử dụng tham số để tránh SQL Injection
                    MScommand.Parameters.AddWithValue("@userid", userid);
                    MScommand.Parameters.AddWithValue("@password", pwd);

                    using (SqlDataReader MSReader = MScommand.ExecuteReader())
                    {
                        if (MSReader.Read())
                        {
                            string connectionString = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.5.1.53)(PORT = 1621))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME =ERPDW)));Password= vnhr$2819Hp;User ID=hcp;";
                            string sqlinfo = string.Format(@"select emp_no,emp_name,dept_name from VN_EAS_EMP_V where emp_no='{0}'", userid);
                            using (OracleConnection conn = new OracleConnection(connectionString))
                            {
                                conn.Open();
                                using (OracleCommand cmd = new OracleCommand(sqlinfo, conn))
                                {
                                    OracleDataReader reader = cmd.ExecuteReader();
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            Session["UserId"] = userid;
                                            Session["UserName"] = reader["emp_name"].ToString();
                                            Session["DepName"] = reader["dept_name"].ToString();
                                            dept = reader["dept_name"].ToString();
                                        }
                                    }
                                }
                            }

                            switch (dept) {
                                case "Phong Tong vu 總務課":
                                case "Bo Phan Nhan su 人力資源部":
                                case "Phong Nhan su 人力資源課":
                                    return RedirectToAction("DataForHR", "DataForHR");
                                default:
                                    return RedirectToAction("VehicleDispatch", "VehicleDispatch"); ; 
                            }

                            //return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.error = "Password is wrong!!!";
                            return View();
                        }
                    }
                }
            }
        }
        public ActionResult Logout()
        {
            var username = Session["UserId"] as string;
            if (!string.IsNullOrEmpty(username))
            {
                //_dbHelper.LogLogout(username);
            }

            Session.Abandon();
            return RedirectToAction("Login");
        }
        public ActionResult Index2()
        {

            return View();
        }
        public ActionResult Index3()
        {

            return View();
        }


        public string EC(string str)
        {
            string Result = "";
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] dBytes = encoding.GetBytes(str);
            for (int i = 0; i < dBytes.Length; i++)
            {
                int MyInterger = Convert.ToInt16(dBytes[i]);
                MyInterger = MyInterger + 8 + i;
                string strSEQ = string.Format("{0:X3}", MyInterger);
                Result = Result + strSEQ;
            }
            return Result;
        }
        #endregion

    }
}