using Kingdee.BOS;
using Kingdee.BOS.Log;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLS
{
    public class API
    {
        /// <summary>
        /// 创建客户基础资料
        /// </summary>
        /// <param name="saveJoList"></param>
        /// <param name="dbid"></param>
        /// <param name="apiInfo"></param>
        /// <returns></returns>
        public static string KHJsonAdd(List<JObject> saveJoList , string dbid, K3CloudWebApiInfo apiInfo) 
        {
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);
            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";
            if (loginResult == true)
            {
                // 开始构建Web API参数对象
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();
                jsonRoot.Add("IsDeleteEntry", "false");
                jsonRoot.Add("IsAutoSubmitAndAudit", "true");//自动提交审核
                // Model: 单据详细数据参数
                JObject model = new JObject();
                //jsonRoot.Add("Model", model);


                // Model: 单据详细数据参数
                JArray models = new JArray();
                foreach (var saveJo in saveJoList)
                {
                    models.Add(getKHModel(saveJo));
                }
                jsonRoot.Add("Model", models);
                result = client.BatchSave("BD_CUSTOMER", jsonRoot.ToString());
            }
            return result;
        }
        /// <summary>
        /// 将客户的信息整理
        /// </summary>
        /// <param name="saveJo"></param>
        /// <returns></returns>
        private static JObject getKHModel(JObject saveJo)
        {
            JObject model = new JObject();
            // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
            model.Add("FID", 0);

            // 开始设置单据字段值
            // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
            // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来

            // 普通字段

            model.Add("FName", saveJo["FName"]);//客户名称
            model.Add("F_admi_Province", saveJo["SJName"]);//省级名称
            model.Add("F_admi_City", saveJo["CityName"]);//市级名称
            model.Add("FADDRESS", saveJo["XxdzName"]);//县乡地址
            model.Add("FTEL", saveJo["PhoneNumber"]);//电话
            model.Add("FSELLER", new JObject() { { "FNumber",saveJo["YWY"] } });//业务员Number
            if (saveJo["JXSLXName"]!=null) 
            {
                model.Add("FCustTypeId", new JObject() { { "FNumber", saveJo["JXSLXName"] } });//经销商类型（客户类别）
            }
            

            return model;
        }

        /// <summary>
        /// 创建物料基础资料
        /// </summary>
        /// <param name="saveJoList"></param>
        /// <param name="dbid"></param>
        /// <param name="apiInfo"></param>
        /// <returns></returns>
        public static string WLJsonAdd(List<JObject> saveJoList, string dbid, K3CloudWebApiInfo apiInfo)
        {
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);
            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";
            if (loginResult == true)
            {
                // 开始构建Web API参数对象
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();
                jsonRoot.Add("IsDeleteEntry", "false");
                jsonRoot.Add("IsAutoSubmitAndAudit", "true");//自动提交审核
                // Model: 单据详细数据参数
                JObject model = new JObject();
                //jsonRoot.Add("Model", model);


                // Model: 单据详细数据参数
                JArray models = new JArray();
                foreach (var saveJo in saveJoList)
                {
                    models.Add(getWLModel(saveJo));
                }
                jsonRoot.Add("Model", models);
                result = client.BatchSave("BD_MATERIAL", jsonRoot.ToString());
            }
            return result;
        }
        /// <summary>
        /// 将物料的信息整理
        /// </summary>
        /// <param name="saveJo"></param>
        /// <returns></returns>
        private static JObject getWLModel(JObject saveJo)
        {
            JObject model = new JObject();
            // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
            model.Add("FID", 0);

            // 开始设置单据字段值
            // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
            // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来

            // 普通字段

            model.Add("FName",saveJo["WLFName"]);//产品
            model.Add("FSpecification",saveJo["FSpecification"]);//规格型号
            model.Add("F_admi_CoTemperature", saveJo["F_admi_CoTemperature"]);//色温
            model.Add("FMaterialGroup", new JObject() { { "FNumber", "C" } });//物料分组C 默认成品
            model.Add("FLENGTH1", saveJo["FLENGTH"]);
            model.Add("F_admi_Wide", saveJo["F_admi_Wide"]);//宽
            model.Add("F_admi_Depth", saveJo["F_admi_Depth"]);//深度
            //JArray entryRows = new JArray();
            string entityKey = "SubHeadEntity";
            JObject entitymodel = new JObject();

            entitymodel.Add("FColor",saveJo["FColor"]);//颜色
            //entitymodel.Add("F_admi_CoTemperature",saveJo["F_admi_CoTemperature"]);//色温
            entitymodel.Add("FBaseUnitId", new JObject() { { "FNumber", "01" } });//单位 默认pcs
            entitymodel.Add("FIsPurchase", "True");//允许采购 true
            entitymodel.Add("FIsSale", "True");//允许销售 true
            entitymodel.Add("FIsInventory", "True");//允许库存 true
            entitymodel.Add("FIsProduce", "True");//允许生产 true
            entitymodel.Add("FErpClsID", "1");//物料属性 默认外购
            entitymodel.Add("FFeatureItem", "1");//物料属性 默认外购
            entitymodel.Add("FCategoryID", new JObject() { { "FNumber", "CHLB05_SYS" } });//存货类别   产成品
            
            
            //entryRows.Add(entitymodel);
            model.Add(entityKey, entitymodel);
            return model;
        }



        /// <summary>
        /// 创建销售订单API
        /// </summary>
        /// <param name="saveJoList"></param>
        /// <param name="dbid"></param>
        /// <param name="apiInfo"></param>
        /// <returns></returns>
        public static string XSDDJsonAdd(List<JObject> saveJoList, string dbid, K3CloudWebApiInfo apiInfo)
        {
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);
            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";
            if (loginResult == true)
            {
                // 开始构建Web API参数对象
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();
                jsonRoot.Add("IsDeleteEntry", "false");
                // Model: 单据详细数据参数
                JObject model = new JObject();
                //jsonRoot.Add("Model", model);

                string Sucess = "";
                // Model: 单据详细数据参数
                JArray models = new JArray();
                foreach (var saveJo in saveJoList)
                {
                    models.Add(getXSDDModel(saveJo));
                }
                jsonRoot.Add("Model", models);
                result = client.BatchSave("SAL_SaleOrder", jsonRoot.ToString());
                JObject resultobj = (JObject)JsonConvert.DeserializeObject(result);
                if (resultobj["Result"]["ResponseStatus"]["IsSuccess"].ToString() != "True")
                {
                    throw new Exception(resultobj["Result"]["ResponseStatus"]["Errors"].ToString());
                    Logger.Info("BOS", "我是一条测试日志数据:)");
                    Logger.Error("BOS", "我是一条测试日志数据:)", new KDException("?", result.ToString()));
                    var logFilePath = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\Cloud.log";
                }
                else
                {
                    Sucess += "单据新增成功" + "\n";
                    return Sucess;
                }
            }
            return result;
        }


        /// <summary>
        /// 将销售订单的信息整理
        /// </summary>
        /// <param name="saveJo"></param>
        /// <returns></returns>
        private static JObject getXSDDModel(JObject saveJo)
        {
            JObject model = new JObject();
            model.Add("FBillTypeID", new JObject() { {"FNumber", "XSDD01_SYS" } });//单据类型
            model.Add("FCustId", new JObject() { {"FNumber" ,saveJo["FCustId"] } });//客户编码
            model.Add("FSalerId",new JObject() { {"FNumber",saveJo["FSalerId"] } });//销售员
            model.Add("F_admi_ContractNo", saveJo["F_admi_ContractNo"]);//销售订单号
            model.Add("FSettleCurrId", new JObject() { {"FNumber", "PRE001" } });//币别 默认人民币
            model.Add("FDATE", saveJo["FDate"] );//审核日期
            model.Add("F_admi_Lieferadresse",saveJo["F_admi_Lieferadresse"]);//送货地址

            model.Add("F_admi_EAddress", saveJo["F_admi_EAddress"]);//终端地址
            model.Add("F_admi_Tphone", saveJo["F_admi_Tphone"]);//终端电话
            model.Add("F_admi_Ecustomer", saveJo["F_admi_Ecustomer"]);//终端客户
            model.Add("FNote", saveJo["FNOTE"]);//备注
            model.Add("F_admi_FactReceive", saveJo["F_admi_FactReceive"]);//实收金额
            model.Add("F_admi_Arrearage", saveJo["F_admi_Arrearage"]);//未收款金额
           

            JArray francejarray = new JArray();
            string franceKey = "FSaleOrderFinance";
            JObject francemodel = new JObject();
            JToken to = saveJo["FSaleOrderFinance"];

            foreach (var item in to)
            {
                francemodel.Add("FIsIncludedTax", item["FIsIncludedTax"]);//是否含税  复选框
            }
            
            model.Add(franceKey, francemodel);
            JArray entryRows = new JArray();
            string entityKey = "FSaleOrderEntry";
            JObject entitymodel = new JObject();
            JToken price = saveJo["FSaleOrderEntry"];
            foreach (var it in price)
            {
                entitymodel.Add("FMaterialId", new JObject() { { "FNumber", it["FMaterialId"] } });//物料
                entitymodel.Add("FUnitID", new JObject() { { "FNumber", it["FUnitID"] } });//单位
                entitymodel.Add("FPriceUnitId", new JObject() { { "FNumber", it["FUnitID"] } });//单位

               
                entitymodel.Add("FQty", it["FQty"]);//计价数量

                entitymodel.Add("FPrice",it["FPrice"]);//单价
                entitymodel.Add("FTaxPrice", it["FTaxPrice"]);//含税单价
                if (it["FAllAmount"] != null)
                {
                    entitymodel.Add("FAllAmount", it["FAllAmount"]);//价税合计
                }
                else 
                {
                    entitymodel.Add("FIsFree", "true");//赠品
                }

                entitymodel.Add("FEntryTaxRate",it["FEntryTaxRate"]);
                entitymodel.Add("FEntryNote", it["FEntryNote"]);
                entitymodel.Add("FDiscountRate", it["FDiscountRate"]);//折扣率
                entitymodel.Add("F_OrderNo", it["F_OrderNo"]);//订单号
                entitymodel.Add("F_admi_SpRequest", it["F_admi_SpRequest"]);//特殊要求
                //entitymodel.Add("F_admi_Colour", it["F_admi_Colour"]);//颜色
                //entitymodel.Add("F_admi_CoTemperature", it["F_admi_CoTemperature"]);//色温
                entitymodel.Add("F_Sensor", it["F_Sensor"]);//内置感应
                entitymodel.Add("FDeliveryDate", it["FDeliveryDate"]);//交货日期
                entryRows.Add(entitymodel);
                entitymodel = new JObject();
            }
            model.Add(entityKey, entryRows);
            //throw new Exception(model.ToString()) ;
            return model;
        }
    }
}
