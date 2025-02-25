using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using ActionResult = System.Web.Mvc.ActionResult;
using HttpGetAttribute = System.Web.Mvc.HttpGetAttribute;
using Controller = System.Web.Mvc.Controller;

namespace PickTimeOutCompany.Controllers
{
    public class DataForHRController : Controller
    {
        // GET: DataForHR
        private string connectionString = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.41.1.29)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = vnothdb)));User Id=GEMTEK_ATTENDANCE;Password=ATTENDANCE01;";

        public ActionResult DataForHR()
        {
            if ((string)Session["UserId"] == null)
            {
                //ViewBag.error = TempData["Please login first!!!"];
                TempData["Error"] = "Plese Login!!!";
                return RedirectToAction("Login", "Home");
            }
            else
            {
                string dept = (string)Session["DepName"];
                string[] arrdep = { "Phong Tong vu 總務課", "新技術開發課", "新技術系統部", "系統整合課", "資訊處", "製造資訊部", "SFCS一課", "SFCS二課", "Bo Phan Nhan su 人力資源部", "Phong Nhan su 人力資源課" };
                if (!arrdep.Contains(dept))
                {
                    TempData["Alert"] = "You do not have permission to access this section.(您没有权限访问此部分。)";
                    return RedirectToAction("Index", "Home");
                }

                string html_Ready_To_StockOut = GET_DATA();
                ViewBag.HtmlCountTime = html_Ready_To_StockOut;

                return View();
            }
        }

        private string GET_DATA()
        {
            string sHTML = string.Empty;
            try
            {
                string add1 = "TIEN LOC";
                string add2 = "THANH DAT";
                string add3 = "BAO SON";
                string sSql = string.Format(@"SELECT 
                                                timeout.TIME_OUT,
                                                COUNT(CASE WHEN log.ADDRESS = 'TIEN LOC' THEN log.ADDRESS END) AS ADDRESS_1_COUNT,
                                                COUNT(CASE WHEN log.ADDRESS = 'THANH DAT' THEN log.ADDRESS END) AS ADDRESS_2_COUNT,
                                                COUNT(CASE WHEN log.ADDRESS = 'BAO SON' THEN log.ADDRESS END) AS ADDRESS_3_COUNT
                                            FROM 
                                                GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG log
                                            LEFT JOIN 
                                                GEMTEK_ATTENDANCE.GM1_TIMEOUT timeout ON timeout.TIME_OUT = log.TIME_OUT
                                            WHERE 
                                                log.date_out = to_char(sysdate,'yyyy/MM/dd')
                                            GROUP BY 
                                                timeout.TIME_OUT
                                            ORDER BY 
                                                timeout.TIME_OUT");
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    OracleCommand command = new OracleCommand(sSql, conn);
                    OracleDataAdapter adapter = new OracleDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    //TempData["DataReadyToStockOut"] = dt;

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            sHTML += "<tr>" +
                                "<td>" + dr["TIME_OUT"] + "</td>" +
                                "<td><button type = button class='btn btn-primary' onclick='GetPickInfo(\"" + dr["TIME_OUT"] + "\",\"" + add1 + "\")'>" + dr["ADDRESS_1_COUNT"] + "</button></td>" +
                                "<td><button type = button class='btn btn-primary' onclick='GetPickInfo(\"" + dr["TIME_OUT"] + "\",\"" + add2 + "\")'>" + dr["ADDRESS_2_COUNT"] + "</button></td>" +
                                "<td><button type = button class='btn btn-primary' onclick='GetPickInfo(\"" + dr["TIME_OUT"] + "\",\"" + add3 + "\")'>" + dr["ADDRESS_3_COUNT"] + "</button></td>" +
                                "</tr>";
                        }
                    }
                    else
                    {
                        sHTML = "<tr><th colspan='7' class='not-data'>Data Not Found</th></tr>";
                    }
                }
            }
            catch (Exception ex)
            {
                sHTML = "<tr><th colspan='7' class='not-data'>" + ex.Message + "</th></tr>";
            }
            return sHTML;
        }

        public ActionResult GetPickInfo(string time, string add)
        {
            string html_PickInfo = GET_PICK_INFO(time, add);
            ViewBag.HtmlPickInfo = html_PickInfo;
            return Content(html_PickInfo);
        }

        private string GET_PICK_INFO(string time, string add)
        {
            string sHTML = string.Empty;
            try
            {
                string sSql = string.Format(@"select user_name,DESCRIPTION from GEMTEK_ATTENDANCE.GM1_TIMEOUT_LOG where date_out = to_char(sysdate,'yyyy/MM/dd') and time_out='{0}' and address='{1}'", time, add);
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    OracleCommand command = new OracleCommand(sSql, conn);
                    OracleDataAdapter adapter = new OracleDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                            sHTML += "<tr><td>" + dr["user_name"] + "</td><td>" + dr["DESCRIPTION"] + "</td></tr>";
                    }
                    else
                    {
                        sHTML = "<tr><th colspan='7' class='not-data'>Data Not Found</th></tr>";
                    }
                }

            }
            catch (Exception ex)
            {
                sHTML = "<tr><th colspan='7' class='not-data'>" + ex.Message + "</th></tr>";
            }
            return sHTML;
        }
    }
}