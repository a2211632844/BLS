using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.JSON;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.Excel;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BLS.Entity;

namespace BLS
{
    [Description("导入Excel文件")]
    [HotUpdate]

    public class InSalOrder : AbstractDynamicFormPlugIn
    {

        DataTable dtexcel;
        string sql;
        private string _filePath = string.Empty;


        //自定义方法,文件上传完毕触发的事件
        public override void CustomEvents(CustomEventsArgs e)
        {
            string result = "获取数据完成";
            if (e.Key.EqualsIgnoreCase("F_ExcelFileUpdate"))
            {
                this.View.GetControl("F_ExcelFileUpdate").SetCustomPropertyValue("NeedCallback", true);
                this.View.GetControl("F_ExcelFileUpdate").SetCustomPropertyValue("IsRequesting", false);
                if (e.EventName.EqualsIgnoreCase("FileChanged"))
                {// 文件上传完毕
                 // 取文件上传参数，文件名
                    DynamicFormShowParameter processForm = this.View.ShowProcessForm(delegate (Kingdee.BOS.Core.DynamicForm.FormResult t)
                    {
                    }, true, "正在获取数据");
                    MainWorker.QuequeTask(Context, delegate
                    {
                        try
                        {
                            JSONObject uploadInfo = KDObjectConverter.DeserializeObject<JSONObject>(e.EventArgs);
                            if (uploadInfo != null)
                            {

                                JSONArray json = new JSONArray(uploadInfo["NewValue"].ToString());

                                //this.View.ShowMessage(uploadInfo.ToString());
                                if (json.Count > 0)
                                {
                                    // 取上传的文件名
                                    string fileName = (json[0] as Dictionary<string, object>)["ServerFileName"].ToString();

                                    this._filePath = this.GetFilePath(fileName);
                                    // 解锁确定按钮
                                    this.View.GetControl("FEnter").Enabled = true;
                                }
                                else
                                {
                                    // 锁定确定按钮
                                    this.View.GetControl("FEnter").Enabled = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result += Environment.NewLine + ex.ToString();
                        }
                        finally
                        {
                            this.View.Session["ProcessRateValue"] = 100;
                            IDynamicFormView view = this.View.GetView(processForm.PageId);
                            if (view != null)
                            {
                                view.Close();
                                this.View.SendDynamicFormAction(view);
                            }
                            this.View.ShowMessage(result);
                        }
                    }, delegate (AsynResult t)
                    {
                    });
                }
            }
        }

        //点击导入Excel按钮
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            string a;
            List<string> KH = new List<string>();
            //当点击事件不为空的时候
            //不管大小写，全部转化为大写
            if ((a = e.Key.ToUpperInvariant()) != null)
            {
                //当点击的按钮名字为FImportData
                if (a.ToUpperInvariant() == "FENTER")
                {
                    string result = "";
                    DynamicFormShowParameter processForm = this.View.ShowProcessForm(delegate (FormResult t)
                    {
                    }, true, "正在生成单据");
                    MainWorker.QuequeTask(Context, delegate
                    {
                        try
                     {

                            string ss = this._filePath;
                            ExcelOperation excelOperation = new ExcelOperation();
                            DataSet dataSet = null;
                            dataSet = excelOperation.ReadFromFile(this._filePath, 1, 0);

                            //在dataSet中获取一个Table的表中的数据
                            dtexcel = dataSet.Tables["订单信息"];
                            JObject jo = new JObject();

                            for (int i = 0; i < dtexcel.Rows.Count; i++)
                            {
                                string HTHsql = string.Format("select * from T_SAL_ORDER where F_ADMI_CONTRACTNO='{0}'", dtexcel.Rows[i]["合同号"]);
                                DataTable HTHDT = DBServiceHelper.ExecuteDataSet(Context, HTHsql).Tables[0];
                                if (HTHDT.Rows.Count == 0)
                                {
                                    List<JObject> SaveJoList = new List<JObject>();

                                    string KHName = dtexcel.Rows[i]["客户"].ToString();//客户名称
                                    string SJName = dtexcel.Rows[i]["省级"].ToString();//省级名称
                                    string CityName = dtexcel.Rows[i]["市级"].ToString();//市级名称
                                    string XxdzName = dtexcel.Rows[i]["县乡地址"].ToString();//县乡地址名称
                                    string PhoneNumber = dtexcel.Rows[i]["电话"].ToString();//电话
                                    string JXSNumber = "";
                                    if (dtexcel.Rows[i]["经销商类型"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        string JXSLXName = dtexcel.Rows[i]["经销商类型"].ToString();//经销商类型
                                        string JXSSQL = string.Format(@"select * from T_BAS_ASSISTANTDATAENTRY ASS
                                    JOIN T_BAS_ASSISTANTDATAENTRY_L ASSL ON ASS.FENTRYID=ASSL.FENTRYID
                                    WHERE FDATAVALUE = '{0}'", JXSLXName);
                                        DataTable JXSDT = DBServiceHelper.ExecuteDataSet(Context, JXSSQL).Tables[0];
                                        if (JXSDT.Rows.Count > 0)
                                        {
                                            JXSNumber = JXSDT.Rows[0]["FNumber"].ToString();
                                            if (jo["JXSLXName"].IsNullOrEmptyOrWhiteSpace() == true)
                                            {
                                                jo.Add("JXSLXName", JXSNumber);
                                            }

                                        }
                                    }
                                    string YWYNumber = "";
                                    if (dtexcel.Rows[i]["业务员"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        string YWY = dtexcel.Rows[i]["业务员"].ToString();//业务员
                                        string SALSql = string.Format(@"select * from V_BD_SALESMAN SAL
                                            JOIN V_BD_SALESMAN_L SALL ON SAL.fid= SALL.fid
                                            WHERE FNAME='{0}'", YWY);
                                        DataTable YWYDT = DBServiceHelper.ExecuteDataSet(Context, SALSql).Tables[0];
                                        if (YWYDT.Rows.Count > 0)
                                        {
                                            YWYNumber = YWYDT.Rows[0]["FNumber"].ToString();
                                            if (jo["YWY"].IsNullOrEmptyOrWhiteSpace() == true)
                                            {
                                                jo.Add("YWY", YWYNumber);
                                            }
                                        }
                                    }
                                    if (jo["FName"].IsNullOrEmptyOrWhiteSpace() == true)
                                    {
                                        jo.Add("FName", KHName);
                                    }
                                    if (jo["SJName"].IsNullOrEmptyOrWhiteSpace() == true)
                                    {
                                        jo.Add("SJName", SJName);
                                    }
                                    if (jo["CityName"].IsNullOrEmptyOrWhiteSpace() == true)
                                    {
                                        jo.Add("CityName", CityName);
                                    }
                                    if (jo["XxdzName"].IsNullOrEmptyOrWhiteSpace() == true)
                                    {
                                        jo.Add("XxdzName", XxdzName);
                                    }
                                    if (jo["PhoneNumber"].IsNullOrEmptyOrWhiteSpace() == true)
                                    {
                                        jo.Add("PhoneNumber", PhoneNumber);
                                    }
                                    string WLName = dtexcel.Rows[i]["产品"].ToString();//产品
                                    string GGXHName = "";
                                    string GGXH = "";
                                    if (dtexcel.Rows[i]["型号"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        GGXHName = dtexcel.Rows[i]["型号"].ToString();//规格型号
                                        if (jo["FSpecification"].IsNullOrEmptyOrWhiteSpace() == true)
                                        {
                                            jo.Add("FSpecification", GGXHName);
                                        }

                                        GGXH = string.Format(" AND FSPECIFICATION='{0}'", GGXHName);
                                    }
                                    else
                                    {
                                        GGXHName = " ";
                                        GGXH = "";
                                    }
                                    string ColorName = "";
                                    string COLOR = "";
                                    if (dtexcel.Rows[i]["颜色"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        ColorName = dtexcel.Rows[i]["颜色"].ToString();//颜色
                                        if (jo["FColor"].IsNullOrEmptyOrWhiteSpace() == true)
                                        {
                                            jo.Add("FColor", ColorName);
                                        }

                                        COLOR = string.Format(" AND FColor='{0}'", ColorName);
                                    }
                                    else
                                    {
                                        ColorName = " ";
                                        COLOR = "";
                                    }
                                    string SWName = "";
                            string SW = "";
                                    if (dtexcel.Rows[i]["色温"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        SWName = dtexcel.Rows[i]["色温"].ToString();//色温
                                        if (jo["F_admi_CoTemperature"].IsNullOrEmptyOrWhiteSpace() == true)
                                        {
                                            jo.Add("F_admi_CoTemperature", SWName);
                                        }
                                SW = string.Format(" AND F_admi_CoTemperature = '{0}' ",SWName);
                                    }
                                    else
                                    {
                                        SWName = " ";
                                    }
                                    string LengthName = "";
                                    string LENGTH = "";
                                    if (dtexcel.Rows[i]["长度"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        LengthName = dtexcel.Rows[i]["长度"].ToString();//长度
                                        if (jo["FLENGTH"].IsNullOrEmptyOrWhiteSpace() == true)
                                        {
                                            jo.Add("FLENGTH", LengthName);
                                        }
                                        LENGTH = string.Format(" AND FLENGTH1='{0}'", LengthName);

                                    }
                                    else
                                    {
                                        LengthName = " ";
                                        LENGTH = "";
                                    }
                                    if (jo["WLFName"].IsNullOrEmptyOrWhiteSpace() == true)
                                    {
                                        jo.Add("WLFName", WLName);
                                    }
                                    //宽
                                    string WidthName = "";
                                    string Width = "";
                                    if (dtexcel.Rows[i]["宽度"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        WidthName = dtexcel.Rows[i]["宽度"].ToString();
                                        if (jo["F_admi_Wide"].IsNullOrEmptyOrWhiteSpace() == true)
                                        {
                                            jo.Add("F_admi_Wide", WidthName);
                                            Width = string.Format(" AND F_admi_Wide='{0}'", WidthName);
                                        }
                                    }
                                    else
                                    {
                                        Width = "";
                                        WidthName = "";
                                    }
                                    string Depth = "";
                                    string DepthName = "";
                                    if (dtexcel.Rows[i]["深度"].IsNullOrEmptyOrWhiteSpace() == false)
                                    {
                                        DepthName = dtexcel.Rows[i]["深度"].ToString();
                                        if (jo["F_admi_Depth"].IsNullOrEmptyOrWhiteSpace() == true)
                                        {
                                            jo.Add("F_admi_Depth", DepthName);
                                            Depth = String.Format(" AND F_admi_Depth = '{0}'", DepthName);
                                        }
                                    }
                                    else
                                    {
                                        Depth = "";
                                        DepthName = "";
                                    }
                                    SaveJoList.Add(jo);
                                    string sql = string.Format(@"select * from T_BD_CUSTOMER_l where FNAME='{0}'", KHName);
                                    DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sql);
                                    DataTable dt = ds.Tables[0];
                                    string WLsql = $@"select FSPECIFICATION,FNAME,FColor,FLENGTH1,F_admi_Wide,F_admi_Depth,* from T_BD_MATERIAL MA
                                JOIN T_BD_MATERIAL_L MAL ON MA.FMATERIALID=MAL.FMATERIALID
                                JOIN T_BD_MATERIALBASE MAB ON MA.FMATERIALID=MAB.FMATERIALID
                                WHERE FNAME='{WLName}'{GGXH} {COLOR} {LENGTH} {Width} {Depth} {SW}";
                                    DataSet WLds = DBServiceHelper.ExecuteDataSet(Context, WLsql);
                                    DataTable WLdt = WLds.Tables[0];
                                    if (dt.Rows.Count == 0)//当客户存在的时候
                                    {
                                        //调用API生成客户
                                        API.KHJsonAdd(SaveJoList, Context.DBId, new K3CloudWebApiInfo());

                                    }
                                    else { jo = new JObject(); }
                                    if (WLdt.Rows.Count == 0)//当物料存在的时候
                                    {
                                        //调用API生成物料
                                        API.WLJsonAdd(SaveJoList, Context.DBId, new K3CloudWebApiInfo());
                                    }
                                    else { jo = new JObject(); }
                                    jo = new JObject();
                                    this.View.Session["ProcessRateValue"] = 10;
                                }
                                else
                                {
                                    throw new Exception("请勿重复导入");
                                }
                            }
                            List<Data> ExcelInfo = DataTableToList<Data>(dtexcel);
                            var sf = ExcelInfo.GroupBy(p => p.合同号).ToList();
                            List<JObject> BatchList = new List<JObject>();
                            if (sf.Count > 0)
                            {
                                int C = 200;
                                int AllCount = sf.Count / C;
                                for (int i = 0; i <= AllCount; i++)
                                { 
                                    for (int j = 0; j < C; j++)
                                    {
                                {
                                    if (i * C + j >= sf.Count)
                                    {
                                        break;
                                    }
                                    string sql = string.Format(@"select FNUMBER,* from T_BD_CUSTOMER CUS
                                            JOIN T_BD_CUSTOMER_L CUSL ON CUS.FCUSTID = CUSL.FCUSTID
                                            WHERE FNAME = '{0}' ", sf[i * C + j].Cast<Data>().First().客户);
                                    DataTable dt = DBServiceHelper.ExecuteDataSet(Context, sql).Tables[0];
                                    string SALSql = string.Format(@"select * from V_BD_SALESMAN SAL
                                                JOIN V_BD_SALESMAN_L SALL ON SAL.fid= SALL.fid
                                                WHERE FNAME='{0}'", sf[i * C + j].Cast<Data>().First().业务员);
                                    DataTable Saldt = DBServiceHelper.ExecuteDataSet(Context, SALSql).Tables[0];
                                        jo.Add("FCustId", dt.Rows[0]["FNumber"].ToString());//客户编码
                                    
                                            
                                            jo.Add("F_admi_ContractNo", sf[i * C + j].Cast<Data>().First().合同号);
                                            jo.Add("F_admi_Lieferadresse", sf[i * C + j].Cast<Data>().First().县乡地址);
                                            jo.Add("FSalerId", Saldt.Rows[0]["FNumber"].ToString());//销售员
                                            
                                            jo.Add("FDate", sf[i * C + j].Cast<Data>().First().审核日期.Substring(0, 10));


                                            jo.Add("F_admi_EAddress", sf[i * C + j].Cast<Data>().First().终端地址);
                                            jo.Add("F_admi_FactReceive", sf[i * C + j].Cast<Data>().First().实收金额);
                                            jo.Add("F_admi_Arrearage", sf[i * C + j].Cast<Data>().First().未收款金额);
                                            jo.Add("FNOTE", sf[i * C + j].Cast<Data>().First().组单备注);
                                            jo.Add("F_admi_Tphone", sf[i * C + j].Cast<Data>().First().终端电话);
                                            jo.Add("F_admi_Ecustomer", sf[i * C + j].Cast<Data>().First().终端客户);

                                            IEnumerable<Data> FEntity = sf[i * C + j].Cast<Data>().ToList();
                                            JArray entryRows = new JArray();
                                            JArray entityFranceRows = new JArray();
                                            JObject joe = new JObject();
                                            JObject jos = new JObject();
                                            string entityKey = "FSaleOrderEntry";
                                            string entityfrancekey = "FSaleOrderFinance";
                                            foreach (var it in FEntity)
                                            {
                                                string xh = "";
                                                string ys = ""; 
                                                string cd = "";
                                                string k = "";//宽
                                                string sd = "";//深度
                                        string sw = "";//色温
                                                if (it.型号 != null)
                                                {
                                                    xh = $@"AND FSPECIFICATION='{it.型号}'";
                                                }
                                                if (it.颜色 != null)
                                                {
                                                    ys = $@"AND FColor='{it.颜色}'";
                                                }
                                                if (it.长度 != null)
                                                {
                                                    cd = $@"AND FLENGTH1='{it.长度}'";
                                                }
                                                if (it.宽度 != null)
                                                {
                                                    k = $@" AND F_admi_Wide = '{it.宽度}'";
                                                }
                                                if (it.深度 != null)
                                                {
                                                    sd = $@" AND F_admi_Depth = '{it.深度}'";
                                                }
                                        if (it.色温!=null)
                                        {
                                            sw = $@" AND F_admi_CoTemperature='{it.色温}'";
                                        }
                                                string WLsql = string.Format($@"select FSPECIFICATION,FNAME,FColor,FLENGTH1 ,F_admi_Wide,F_admi_Depth,UN.FNUMBER AS FUnitNumber,* from T_BD_MATERIAL MA
                                JOIN T_BD_MATERIAL_L MAL ON MA.FMATERIALID=MAL.FMATERIALID
                                JOIN T_BD_MATERIALBASE MAB ON MA.FMATERIALID=MAB.FMATERIALID
                                JOIN T_BD_UNIT UN ON UN.FUNITID = FBASEUNITID
                                WHERE FNAME='{ it.产品}' {xh} {ys} {cd}  {k} {sd} {sw}");
                                                DataSet WLds = DBServiceHelper.ExecuteDataSet(Context, WLsql);
                                                DataTable WLdt = WLds.Tables[0];
                                                joe.Add("FMaterialId", WLdt.Rows[0]["FNumber"].ToString());//物料编码
                                                joe.Add("FUnitID",WLdt.Rows[0]["FUnitNumber"].ToString());
                                                joe.Add("F_OrderNo", it.订单号);
                                                joe.Add("FQty", it.数量);
                                                joe.Add("F_admi_SpRequest", it.特殊要求);
                                                string sssss = it.颜色;
                                                joe.Add("F_admi_Colour", it.颜色);
                                                joe.Add("F_admi_CoTemperature", it.色温);
                                                joe.Add("F_Sensor", it.内置感应);
                                                joe.Add("FEntryNote", it.订单标记);
                                                //joe.Add("FUnitID", "01");
                                                //joe.Add("FPriceUnitId", "01");
                                                string[] strArray;
                                                if (it.金额详情.IsNullOrEmptyOrWhiteSpace() == false)
                                                {
                                                    strArray = it.金额详情.Split(new string[] { ":", "*" }, StringSplitOptions.RemoveEmptyEntries);
                                                }
                                                
                                                //joe.Add("FTaxPrice", strArray[1]);//含税单价
                                                joe.Add("FDeliveryDate", it.交货日期);
                                                if (it.金额 != "0")
                                                {
                                                    joe.Add("FAllAmount", it.金额);//价税合计
                                                }
                                                else
                                                {
                                                    if (joe["FIsFree"].IsNullOrEmptyOrWhiteSpace() == true)
                                                    {
                                                        joe.Add("FIsFree", "true");//价税合计
                                                    }

                                                }

                                                joe.Add("sd", it.税点);
                                                joe.Add("FDiscountRate", (Convert.ToDecimal(10) - Convert.ToDecimal(it.订单折扣)) * 10);//折扣率

                                                if (it.税点 == "不含税价")
                                                {
                                                    joe.Add("FAmount", it.金额);//金额字段
                                                    if (it.金额详情.IsNullOrEmptyOrWhiteSpace() == false)
                                                    {
                                                        strArray = it.金额详情.Split(new string[] { ":", "*" }, StringSplitOptions.RemoveEmptyEntries);
                                                        //joe.Add("FTaxPrice", strArray[1]);//含税单价
                                                        joe.Add("FTaxPrice", 0);//含税单价
                                                        joe.Add("FPrice", strArray[1]);//单价
                                                        if (jos["FIsIncludedTax"].IsNullOrEmptyOrWhiteSpace() == true)
                                                        {
                                                            jos.Add("FIsIncludedTax", "False");
                                                        }
                                                       
                                                    }
                                                    else
                                                    {
                                                        joe.Add("FTaxPrice", 0);//含税单价
                                                        joe.Add("FPrice", 0);//单价
                                                        if (jos["FIsIncludedTax"].IsNullOrEmptyOrWhiteSpace()==true)
                                                        {
                                                            jos.Add("FIsIncludedTax", "True");
                                                        }
                                                        
                                                    }
                                                }
                                                else if (it.税点 == "含税价")
                                                {
                                                    joe.Add("FEntryTaxRate", 8);
                                                    joe.Add("FAmount", Convert.ToDecimal(it.金额) / Convert.ToDecimal(1.08));//金额
                                                    if (it.金额详情.IsNullOrEmptyOrWhiteSpace() == false)
                                                    {
                                                        strArray = it.金额详情.Split(new string[] { ":", "*" }, StringSplitOptions.RemoveEmptyEntries);
                                                        joe.Add("FTaxPrice", strArray[1]);//含税单价
                                                        joe.Add("FPrice", Convert.ToDecimal(strArray[1]) / Convert.ToDecimal(1.08));//单价
                                                    }
                                                    else
                                                    {
                                                        joe.Add("FTaxPrice", 0);//含税单价
                                                        joe.Add("FPrice", Convert.ToDecimal(0) / Convert.ToDecimal(1.08));//单价

                                                    }
                                                }
                                                entryRows.Add(joe);
                                        if (entityFranceRows.Count==0) 
                                        {
                                            entityFranceRows.Add(jos);
                                            jos = new JObject();
                                        }
                                                joe = new JObject();
                                                

                                            }
                                            jo.Add(entityKey, entryRows);
                                            jo.Add(entityfrancekey,entityFranceRows);
                                            BatchList.Add(jo);
                                            jo = new JObject();
                                        }
                                    }
                                    if (BatchList.Count > 0)
                                    {
                                        result = API.XSDDJsonAdd(BatchList, Context.DBId, new K3CloudWebApiInfo());
                                        throw new Exception(result);
                                    }
                                }
                            }
                }
                        catch (Exception ex)
                {
                    result += Environment.NewLine + ex.ToString();
                }
                finally
                {
                    this.View.Session["ProcessRateValue"] = 100;
                    IDynamicFormView view = this.View.GetView(processForm.PageId);
                    if (view != null)
                    {
                        view.Close();
                        this.View.SendDynamicFormAction(view);
                    }
                    this.View.ShowMessage(result);
                }
            }, delegate (AsynResult t)
   {
   });
        }
    }
        }


        /// <summary>
        /// 利用反射将Datatable转换为List<T>对象
        /// </summary>
        /// <typeparam name="T">集合</typeparam>
        /// <param name="dt"> datatable对象</param>
        /// <returns></returns>
        public static List<T> DataTableToList<T>(DataTable dt) where T : new()
        {
            //定义集合
            List<T> ts = new List<T>();

            //遍历dataTable中的数据行
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                //获得此模型的公共属性
                PropertyInfo[] propertys = t.GetType().GetProperties();

                //遍历该对象的所有属性
                foreach (PropertyInfo pi in propertys)
                {

                    string tempName = pi.Name;

                    if (!dt.Columns.Contains(tempName)) continue;   //检查datatable是否包含此列(列名==对象的属性名)    
                    object value = dr[tempName];      //取值
                    if (value == DBNull.Value) continue;  //如果非空，则赋给对象的属性
                    pi.SetValue(t, value, null);
                }
                //对象添加到泛型集合中
                ts.Add(t);
            }
            return ts;

        }



        //自定义方法,判断是否是上传的是Excel文件
        private bool CheckFile(string fileName)
        {
            bool flag = false;
            string[] array = fileName.Split(new char[]
            {
                '.'
            });

            //通过后缀名,判断是否是Excel
            if (array.Length == 2 && (array[1].EqualsIgnoreCase("xls") || array[1].EqualsIgnoreCase("xlsx")))
            {
                flag = true;
            }
            if (!flag)
            {
                this.View.ShowWarnningMessage("请选择正确的文件进行引入");
            }
            return flag;
        }


        //自定义方法,没有上传完成Excel,那么上传按钮是灰色的
        private void EnableButton(string key, bool bEnable)
        {
            this.View.GetControl<Button>(key).Enabled = bEnable;
        }

        //自定义方法,获取上传路径
        private string GetFilePath(string serverFileName)
        {
            string directory = "FileUpLoadServices\\UploadFiles";
            return PathUtils.GetPhysicalPath(directory, serverFileName);
        }
    }
}
