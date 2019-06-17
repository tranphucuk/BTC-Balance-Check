using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mass_BTC_Balance_Checker.Static_Class.SSH
{
    public static class GridviewExtension
    {
        public static IEnumerable<T> GetSelectedItems<T>(this GridView gv) where T : class
        {
            var selectedRowHandles = gv.GetSelectedRows();
            for (int i = 0; i < selectedRowHandles.Length; i++)
            {
                var selectedItem = gv.GetRow(selectedRowHandles[i]) as T;
                yield return selectedItem;
            }
        }
    }
}
