using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using PickTimeOutCompany.Models;

namespace PickTimeOutCompany.Controllers
{
    public class VehicleDispatchController : Controller
    {
        private static string connectionString = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.41.1.29)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = vnothdb)));User Id=GEMTEK_ATTENDANCE;Password=ATTENDANCE01;";
        // GET: VehicleDispatch
        public ActionResult VehicleDispatch()
        {
            if ((string)Session["UserId"] == null)
            {
                //ViewBag.error = TempData["Please login first!!!"];
                TempData["Error"] = "Please Login!!!";
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.username = (string)Session["UserName"];
                ViewBag.userID = (string)Session["UserId"];

                DateTime currentDate = DateTime.Now;
                string formattedDate = currentDate.ToString("yyyy/MM/dd");
                string getAddress = @"select address, time_out, supermarket from GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG where user_name = '" + (string)Session["UserId"] + "' and DATE_OUT = '" + formattedDate + "'";
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(getAddress, conn))
                    {
                        OracleDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ViewBag.selectedAddress = reader["address"].ToString();
                                ViewBag.selectedTimeout = reader["time_out"].ToString();
                                ViewBag.selectedSupermarket = reader["supermarket"].ToString();
                            }
                        }
                    }
                }
                //ViewBag.grpNo = (string)Session["GRPNO"];
                //string html_Ready_To_StockOut = GET_DATA();
                //ViewBag.HtmlCountTime = html_Ready_To_StockOut;
                return View();
            }
        }

        public JsonResult GetTableData()
        {
            List<VehicleDispatchModel> vehicleDispatchModels = new List<VehicleDispatchModel>();
            DateTime currentDate = DateTime.Now;
            string formattedDate = currentDate.ToString("yyyy/MM/dd");
            string today = currentDate.ToString("yyyy/MM/dd");
            string nextDay = currentDate.AddDays(1).ToString("yyyy/MM/dd");
            string nextTomorrowDay = currentDate.AddDays(2).ToString("yyyy/MM/dd");
            //string connectionString = @"ConnstrOther=Provider=MSDAORA.1;Password=ATTENDANCE01;User ID=GEMTEK_ATTENDANCE;Data Source=VNOTHDB;Persist Security Info=True;";
            string getData = @"SELECT * FROM GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG where user_name = '" + (string)Session["UserId"] + "' AND TO_DATE(date_out, 'yyyy/MM/dd') >= trunc(SYSDATE) order by date_out";
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand(getData, conn))
                {
                    OracleDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string date = reader["date_out"].ToString();
                            string user = reader["user_name"].ToString();
                            string des = reader["description"].ToString();
                            string time = reader["time_out"].ToString();
                            string address = reader["address"].ToString();
                            string supermarket = reader["supermarket"].ToString();
                            switch (address)
                            {
                                case "TIEN LOC":
                                    address = "近禄";
                                    break;
                                case "THANH DAT":
                                    address = "誠達";
                                    break;
                                case "BAO SON":
                                    address = "寶山";
                                    break;
                            }
                            string cdt = reader["cdt"].ToString();
                            DateTime dateTime = Convert.ToDateTime(cdt);
                            cdt = dateTime.ToString("yyyy/MM/dd HH:mm:ss");
                            string udt = reader["udt"].ToString();
                            dateTime = Convert.ToDateTime(udt);
                            udt = dateTime.ToString("yyyy/MM/dd HH:mm:ss");
                            VehicleDispatchModel temp = new VehicleDispatchModel { DATE_OUT = date, USER_NAME = user, DESCRIPTION = des, TIME_OUT = time, ADDRESS = address, SUPERMARKET = supermarket, CDT = cdt, UDT = udt };
                            vehicleDispatchModels.Add(temp);
                        }
                    }
                }
            }
            return Json(vehicleDispatchModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult InsertData(string timeout, string address, string checkbox, string selectedDate)
        {
            try
            {
                if ((string)Session["UserName"] == null)
                {
                    TempData["Error"] = "Session End, Please Login Again!!!";
                    return Json(new { redirectUrl = Url.Action("Login", "Home") }, JsonRequestBehavior.AllowGet);
                }

                string changeDate = @"MERGE INTO GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG e " +
                                                      @"USING (SELECT '" + selectedDate + "' AS date_out, '" + (string)Session["UserId"] + "' AS user_name, '" + (string)Session["UserName"] + "' AS description, '" + timeout + "' AS time_out, '" + address + "' as address, '" + checkbox + "' as checkbox,'" + (string)Session["DepName"] + "' AS depname FROM DUAL) src " +
                                                      @"ON (e.user_name = src.user_name and e.date_out = src.date_out)
                                      WHEN MATCHED THEN
                                          UPDATE SET 
                                              e.time_out = src.time_out,
                                              e.address = src.address,
                                              e.udt = sysdate,
                                              e.supermarket = src.checkbox
                                      WHEN NOT MATCHED THEN
                                          INSERT (date_out, user_name, description, time_out, address, cdt, udt, supermarket,DEPARTMENT)
                                          VALUES (src.date_out, src.user_name, src.description, src.time_out, src.address, sysdate, sysdate, src.checkbox,src.depname)";

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    OracleTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        using (OracleCommand cmdOracle = new OracleCommand(changeDate, connection))
                        {
                            int t = cmdOracle.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getSelectedbyDate(string date_out)
        {
            string getData = @"select time_out, address, supermarket from GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG where user_name = '" + (string)Session["UserId"] + "' and date_out = '" + date_out + "'";
            List<object> jsonResult = new List<object>();
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand(getData, conn))
                {
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        jsonResult.Add(new { Time = reader["time_out"], Address = reader["address"], Supermarket = reader["supermarket"] });
                    }
                }
            }
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}