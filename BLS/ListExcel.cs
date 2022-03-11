using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLS
{
    [Description("销售出库单,列表导入Excel")]
    [HotUpdate]
    public class ListExcel : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            if (e.BarItemKey == "tbOnload")
            {
                //显示动态表单
                DynamicFormShowParameter forParameter = new DynamicFormShowParameter();

                //获取动态表单的唯一标识
                forParameter.FormId = "kf2e9bbcccd394fdfbac011c5acdc85fd";
                this.View.ShowForm(forParameter);
            }
        }
    }
}
