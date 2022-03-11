using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLS
{
    public class Entity
    {
       public class Data
        {
            /// <summary>
            /// 合同号（销售合同号）
            /// </summary>
            public string 合同号 { get; set; }
            /// <summary>
            /// 客户（客户）
            /// </summary>
            public string 客户 { get; set; }
            /// <summary>
            /// 业务员（销售员）
            /// </summary>
            public string 业务员 { get; set; }
            /// <summary>
            /// 县乡地址（送货地址）
            /// </summary>
            public string 县乡地址 { get; set; }
            /// <summary>
            /// 产品（物料名称）
            /// </summary>
            public string 产品 { get; set; }
            /// <summary>
            /// 型号（规格型号）
            /// </summary>
            public string 型号 { get; set; }
            /// <summary>
            /// 订单号（订单号）
            /// </summary>
            public string 订单号 { get; set; }
            /// <summary>
            /// 数量（销售数量）
            /// </summary>
            public string 数量 { get; set; }
            /// <summary>
            /// 特殊要求（特殊要求）
            /// </summary>
            public string 特殊要求 { get; set; }
            /// <summary>
            /// 颜色（颜色）
            /// </summary>
            public string 颜色 { get; set; }
            /// <summary>
            /// 色温（色温）
            /// </summary>
            public string 色温 { get; set; }
            /// <summary>
            /// 内置感应（内置感应）
            /// </summary>
            public string 内置感应 { get; set; }
            /// <summary>
            /// 金额详情（价税合计）
            /// </summary>
            public string 金额详情 { get; set; }
            /// <summary>
            /// 交货日期（要货日期）
            /// </summary>
            public string 交货日期 { get; set; }
            /// <summary>
            /// 订单标记（备注）
            /// </summary>
            public string 订单标记 { get; set; }
            /// <summary>
            /// 长度（长）
            /// </summary>
            public string 长度 { get; set; }
            /// <summary>
            /// 税点（不含税价 含税价  或为空）
            /// </summary>
            public string 税点 { get; set; }
            /// <summary>
            /// 金额（价税合计）
            /// </summary>
            public string 金额 { get; set; }
            /// <summary>
            /// 订单折扣（折扣率）
            /// </summary>
            public string 订单折扣 { get; set; }
            /// <summary>
            /// 审核日期
            /// </summary>
            public string  审核日期 { get; set; }
            /// <summary>
            /// 宽
            /// </summary>
            public string 宽度 { get; set; }
            /// <summary>
            /// 深度
            /// </summary>
            public string 深度 { get; set; }

            /// <summary>
            /// 终端地址
            /// </summary>
            public string 终端地址 { get; set; }

            /// <summary>
            /// 实收金额
            /// </summary>
            public string 实收金额 { get; set; }
            /// <summary>
            /// 未收款金额
            /// </summary>
            public string 未收款金额 { get; set; }
            /// <summary>
            /// 组单备注
            /// </summary>
            public string 组单备注 { get; set; }
            /// <summary>
            /// 终端电话
            /// </summary>
            public string 终端电话 { get; set; }
            /// <summary>
            /// 终端客户
            /// </summary>
            public string 终端客户 { get; set; }
        }
    }
}
