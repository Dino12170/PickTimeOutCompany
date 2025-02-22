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
        // GET: VehicleDispatch
        public ActionResult VehicleDispatch()
        {
            if ((string)Session["UserId"] == null)
            {
                //ViewBag.error = TempData["Please login first!!!"];
                TempData["Error"] = "Vui lòng đăng nhập!!!";
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.username = (string)Session["UserName"];
                ViewBag.userID = (string)Session["UserId"];
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
            //string connectionString = @"ConnstrOther=Provider=MSDAORA.1;Password=ATTENDANCE01;User ID=GEMTEK_ATTENDANCE;Data Source=VNOTHDB;Persist Security Info=True;";
            string connectionString = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.41.1.29)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = vnothdb)));User Id=GEMTEK_ATTENDANCE;Password=ATTENDANCE01;";
            string getData = @"SELECT * FROM GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG where user_name = '" + (string)Session["UserId"] + "' and date_out = '" + formattedDate + "'";
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
                            string cdt = reader["cdt"].ToString();
                            string udt = reader["udt"].ToString();
                            VehicleDispatchModel temp = new VehicleDispatchModel { DATE_OUT = date, USER_NAME = user, DESCRIPTION = des, TIME_OUT = time, ADDRESS = address, CDT = cdt, UDT = udt };
                            vehicleDispatchModels.Add(temp);
                        }
                    }
                }
            }
            return Json(vehicleDispatchModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult InsertData(string timeout, string address)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                string formattedDate = currentDate.ToString("yyyy/MM/dd");
                string connectionString = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.41.1.29)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = vnothdb)));User Id=GEMTEK_ATTENDANCE;Password=ATTENDANCE01;";
                string insertData = @"insert into GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG (date_out, user_name, description, time_out, address, cdt, udt) values "
                                      + @"('" + formattedDate + "', '" + (string)Session["UserId"] + "', '" + (string)Session["UserName"] + "', '" + timeout + "', '" + address + "', sysdate, sysdate)";

                string changeDate = @"MERGE INTO GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG e " +
                                      @"USING (SELECT '" + formattedDate + "' AS date_out, '" + (string)Session["UserId"] + "' AS user_name, '" + (string)Session["UserName"] + "' AS description, '" + timeout + "' AS time_out, '" + address + "' as address FROM DUAL) src " +
                                      @"ON (e.user_name = src.user_name)
                                      WHEN MATCHED THEN
                                          UPDATE SET 
                                              e.date_out = src.date_out,
                                              e.time_out = src.time_out,
                                              e.address = src.address,
                                              e.udt = sysdate
                                      WHEN NOT MATCHED THEN
                                          INSERT (date_out, user_name, description, time_out, address, cdt, udt)
                                          VALUES (src.date_out, src.user_name, src.description, src.time_out, src.address, sysdate, sysdate)";

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
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}