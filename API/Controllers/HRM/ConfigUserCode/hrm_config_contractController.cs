using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers.HRM.ConfigUserCode
{
    public class hrm_config_contractController : ApiController
    {
        [Authorize(Roles = "login")]

        public class modelC
        {
            public int year { get; set; }
            public int organization_id { get; set; }

        }

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPut]
        public async Task<HttpResponseMessage> update_data(modelC model)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_certificate = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var dv = int.Parse(dvid);
                    var Superior = db.hrm_config_contract.Where(p => p.year == model.year && p.organization_id == dv).FirstOrDefault();
                    if (Superior == null)
                    {
                        var hrm_CogfinUAD = new hrm_config_contract();
                        hrm_CogfinUAD.year = model.year;
                        hrm_CogfinUAD.initialization_number = 1;

                        hrm_CogfinUAD.organization_id = dv;
                        hrm_CogfinUAD.auto_increment = true;
                        hrm_CogfinUAD.is_order = 0;

                        hrm_CogfinUAD.created_by = uid;
                        hrm_CogfinUAD.created_date = DateTime.Now;
                        hrm_CogfinUAD.created_ip = ip;

                        db.hrm_config_contract.Add(hrm_CogfinUAD);
                        db.SaveChanges();
                        //THÊM BẢNG THIẾT LẬP

                        var hrm_config_order = new hrm_config_contract_order();
                        hrm_config_order.config_contract_id = hrm_CogfinUAD.config_contract_id;
                        hrm_config_order.is_order = 1;
                        hrm_config_order.info_col = "Số hợp đồng";
                        hrm_config_order.separator = "/";
                        hrm_config_order.order_type =1;
                        hrm_config_order.is_used = true;
                        db.hrm_config_contract_order.Add(hrm_config_order);
                        db.SaveChanges();
                        hrm_config_order.is_order = 2;
                        hrm_config_order.info_col = "Năm";
                        hrm_config_order.separator = "/";
                        hrm_config_order.order_type = 2;
                        hrm_config_order.is_used = true;
                        db.hrm_config_contract_order.Add(hrm_config_order);
                        db.SaveChanges();
                        hrm_config_order.is_order = 3;
                        hrm_config_order.info_col = "Ký hiệu";
                        hrm_config_order.order_type = 3;
                        hrm_config_order.separator = "";
                        hrm_config_order.is_used = true;
                        db.hrm_config_contract_order.Add(hrm_config_order);
                        db.SaveChanges();
                    }
                    var ListSymbol = db.hrm_ca_type_contract.Where(p => p.organization_id == dv || p.organization_id == 0).ToList();
                    var SuperiorSW = db.hrm_config_contract.Where(p => p.year == model.year && p.organization_id == dv).FirstOrDefault();
                    foreach (var item in ListSymbol)
                    {
                        var SuperiorCheck = db.hrm_config_contract_symbol.AsNoTracking().Where(p => p.type_contract_id == item.type_contract_id
                        && p.config_contract_id == SuperiorSW.config_contract_id).FirstOrDefault();

                        if (SuperiorCheck == null)
                        {
                            var hrm_Cogfin = new hrm_config_contract_symbol();
                            hrm_Cogfin.config_contract_id = SuperiorSW.config_contract_id;
                            hrm_Cogfin.type_contract_id = item.type_contract_id;
                            hrm_Cogfin.symbol = "";
                            hrm_Cogfin.created_by = uid;
                            hrm_Cogfin.created_date = DateTime.Now;
                            hrm_Cogfin.created_ip = ip;
                            db.hrm_config_contract_symbol.Add(hrm_Cogfin);
                            db.SaveChanges();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "hrm_config_contract/update_data", ip, tid, "Lỗi khi cập nhật mã số hợp đồng", 0, "mã số hợp đồng");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "hrm_config_contract/update_data", ip, tid, "Lỗi khi cập nhật mã số hợp đồng", 0, "mã số hợp đồng");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpPut]
        public async Task<HttpResponseMessage> update_config_contract()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        string fdca_certificate = "";
                        var dv = int.Parse(dvid);
                        fdca_certificate = provider.FormData.GetValues("hrm_config_contract").SingleOrDefault();
                        hrm_config_contract hrm_Config_Usercode = JsonConvert.DeserializeObject<hrm_config_contract>(fdca_certificate);
                        var hrm_Config_Check = db.hrm_config_contract.AsNoTracking().Where(p => p.year == hrm_Config_Usercode.year && p.organization_id == dv).FirstOrDefault();
                        if (hrm_Config_Check != null)
                        {
                            db.Entry(hrm_Config_Usercode).State = EntityState.Modified;
                            db.SaveChanges();
                            string fdca_1 = "";
                            fdca_1 = provider.FormData.GetValues("hrm_config_contract_order").SingleOrDefault();
                            List<hrm_config_contract_order> hrm_Contract_Order = JsonConvert.DeserializeObject<List<hrm_config_contract_order>>(fdca_1);
                            foreach (var item in hrm_Contract_Order)
                            {
                                db.Entry(item).State = EntityState.Modified;
                                
                            }
                            db.SaveChanges();
                            string fdca_2 = "";
                            fdca_2 = provider.FormData.GetValues("hrm_config_contract_symbol").SingleOrDefault();
                            List<hrm_config_contract_symbol> hrm_Contract_Symbol = JsonConvert.DeserializeObject<List<hrm_config_contract_symbol>>(fdca_2);
                            foreach (var item in hrm_Contract_Symbol)
                            {

                                db.Entry(item).State = EntityState.Modified;
                            }

                            db.SaveChanges();

                        }
                        else
                        {
                            var hrm_CogfinUAD = new hrm_config_contract();
                            hrm_CogfinUAD.year = hrm_Config_Usercode.year;
                            hrm_CogfinUAD.initialization_number = hrm_Config_Usercode.initialization_number;
                            hrm_CogfinUAD.organization_id = dv;
                            hrm_CogfinUAD.auto_increment = hrm_Config_Usercode.auto_increment;
                            hrm_CogfinUAD.is_order = hrm_Config_Usercode.is_order;
                            hrm_CogfinUAD.created_by = uid;
                            hrm_CogfinUAD.created_date = DateTime.Now;
                            hrm_CogfinUAD.created_ip = ip;
                            db.hrm_config_contract.Add(hrm_CogfinUAD);
                            db.SaveChanges();

                            string fdca_1 = "";
                            fdca_1 = provider.FormData.GetValues("hrm_config_contract_order").SingleOrDefault();
                            List<hrm_config_contract_order> hrm_Contract_Order = JsonConvert.DeserializeObject<List<hrm_config_contract_order>>(fdca_1);
                            foreach (var item in hrm_Contract_Order)
                            {
                                db.hrm_config_contract_order.Add(item);
                                db.SaveChanges();
                            }
                            string fdca_2 = "";
                            fdca_2 = provider.FormData.GetValues("hrm_config_contract_symbol").SingleOrDefault();
                            List<hrm_config_contract_symbol> hrm_Contract_Symbol = JsonConvert.DeserializeObject<List<hrm_config_contract_symbol>>(fdca_2);
                            foreach (var item in hrm_Contract_Symbol)
                            {

                                db.hrm_config_contract_symbol.Add(item);
                                db.SaveChanges();
                            }
                     

                        }
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "config_contract/update_config_contract", ip, tid, "Lỗi khi cập nhật Số hợp đồng", 0, "Số hợp đồng");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "config_contract/update_config_contract", ip, tid, "Lỗi khi cập nhật Số hợp đồng", 0, "Số hợp đồng");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

    }

}