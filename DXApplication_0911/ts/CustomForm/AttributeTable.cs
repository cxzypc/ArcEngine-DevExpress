using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Data;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;

using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SpatialAnalystUI;

namespace ts.CustomForm
{
    public partial class AttributeTable : DevExpress.XtraEditors.XtraForm
    {
        IFeatureLayer pTocFeatureLayer = null;
        List<int> tempOIDList = new List<int>();
        PublicFunction PF = new PublicFunction();
        bool isFirstLoad = false;

        public IFeatureLayer PTocFeatureLayer
        {
            get { return pTocFeatureLayer; }
            set { pTocFeatureLayer = value; }
        }
        IRasterLayer pTocRasterLayer = null;

        public IRasterLayer PTocRasterLayer
        {
            get { return pTocRasterLayer; }
            set { pTocRasterLayer = value; }
        }
        private IMap pTocMap = null;
        IActiveView _curActive = null;
        ISelection selection = null;

        public IMap PTocMap
        {
            get { return pTocMap; }
            set { pTocMap = value; }
        }
        public AttributeTable(IFeatureLayer pTocFeatureLayer, ref IMap pTocMap)
        {
            _curActive = pTocMap as IActiveView;
            selection = pTocMap.FeatureSelection;
            PTocMap = pTocMap;
            InitializeComponent();
            PTocFeatureLayer = pTocFeatureLayer;
        }
        public AttributeTable(IRasterLayer pTocRasterLayer, ref IMap pTocMap)
        {
            _curActive = pTocMap as IActiveView;
            InitializeComponent();
            PTocRasterLayer = pTocRasterLayer;
        }

        private void AttributeTable_Load(object sender, EventArgs e)
        {
            this.gridView1.OptionsView.ColumnAutoWidth = false;
            if (PTocFeatureLayer != null)
                InitUI(PTocFeatureLayer);
            else if (PTocRasterLayer != null)
                InitRaster(PTocRasterLayer);
        }

        public void InitUI(IFeatureLayer pTocFeatureLayer) ///Feature属性表
        {
            int inx = 0;
            PTocFeatureLayer = pTocFeatureLayer;
            if (PTocFeatureLayer == null)
                return;
            IFeature pFeature = null;

            DataTable pFeatDT = new DataTable(); //创建数据表
            DataRow pDataRow = null; //数据表行变量
            DataColumn pDataCol = null; //数据表列变量
            IField pField = null;
            for (int i = 0; i < PTocFeatureLayer.FeatureClass.Fields.FieldCount; i++)
            {
                pDataCol = new DataColumn();
                pField = PTocFeatureLayer.FeatureClass.Fields.get_Field(i);//获得字段对象get_Field
                pDataCol.ColumnName = pField.AliasName; //获取字段名作为列标题
                pDataCol.DataType = Type.GetType("System.Object");//定义列字段类型
                pFeatDT.Columns.Add(pDataCol); //在数据表中添加字段信息
            }
            //
            IFeatureCursor pFeatureCursor = PTocFeatureLayer.Search(null, true);
            pFeature = pFeatureCursor.NextFeature();
            while (pFeature != null)
            {
                inx++;
                pDataRow = pFeatDT.NewRow();
                //获取字段属性get_Value
                for (int k = 0; k < pFeatDT.Columns.Count; k++)
                {
                    pDataRow[k] = pFeature.get_Value(k);
                }
                pFeatDT.Rows.Add(pDataRow); //在数据表中添加字段属性信息
                pFeature = pFeatureCursor.NextFeature();
            }
            labelControl1.Text = "字段数：" + PTocFeatureLayer.FeatureClass.Fields.FieldCount.ToString() + "， 行数：" + inx.ToString();
            //释放指针
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

            //ISelection selection = pTocMap.FeatureSelection;
            IEnumFeatureSetup iEnumFeatureSetup = (IEnumFeatureSetup)selection;
            iEnumFeatureSetup.AllFields = true;
            IEnumFeature enumFeature = (IEnumFeature)iEnumFeatureSetup;

            enumFeature.Reset();
            IFeature feature = enumFeature.Next();

            while (feature != null)
            {
                //string hehe = feature.get_Value(0).ToString();//这边get_Value(5)里面的数字代表你shapefile文件里面dbf表中字段的位置，0代表第一个，我这里面5代表的是第6个字段哈
                int ind = Convert.ToInt16(feature.get_Value(0));
                tempOIDList.Add(ind);
                //MessageBox.Show(hehe);
                feature = enumFeature.Next();
            }

            ////释放指针
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeature);

            gridControl1.DataSource = pFeatDT;

            MapSelect();

        }

        public void InitRaster(IRasterLayer PTocRasterLayer) ///Raster属性表
        {
            int inx = 0;
            if (PTocRasterLayer == null)
                return;
            IRaster2 raster = PTocRasterLayer.Raster as IRaster2;
            ITable iTable = raster.AttributeTable;
            if (iTable == null)
            {
                iTable = PF.BuildRasterTable(PTocRasterLayer as ILayer);
            }

            DataTable pFeatDT = new DataTable(); //创建数据表

            DataRow pDataRow = null; //数据表行变量
            DataColumn pDataCol = null; //数据表列变量

            IFields fields = iTable.Fields;

            IField pField = null;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                pDataCol = new DataColumn();
                pField = fields.get_Field(i);//获得字段对象get_Field
                pDataCol.ColumnName = pField.AliasName; //获取字段名作为列标题
                pDataCol.DataType = Type.GetType("System.Object");//定义列字段类型
                pFeatDT.Columns.Add(pDataCol); //在数据表中添加字段信息
            }
            ////

            ICursor pCursor = iTable.Search(null, false);
            IRow pRrow = pCursor.NextRow();
            while (pRrow != null)
            {
                inx++;
                pDataRow = pFeatDT.NewRow();
                //获取字段属性get_Value
                for (int k = 0; k < pRrow.Fields.FieldCount; k++)
                {
                    pDataRow[k] = pRrow.Value[k].ToString();
                }
                pFeatDT.Rows.Add(pDataRow);//在数据表中添加字段属性信息
                pRrow = pCursor.NextRow();
            }
            labelControl1.Text = "字段数：" + fields.FieldCount.ToString() + "， 行数：" + inx.ToString();
            //释放指针
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);

            gridControl1.DataSource = pFeatDT;
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            if (PTocFeatureLayer != null)
            {
                if (isFirstLoad || tempOIDList.Count == 0)
                {
                    PTocMap.ClearSelection();  //清除是主要问题
                    IActiveView _curActive = PTocMap as IActiveView;
                    int[] selectedRows = gridView1.GetSelectedRows();
                    if (selectedRows.Length == 0 || selectedRows == null)
                    {
                        return;
                    }
                    string strOID = string.Empty;
                    List<string> OIDList = new List<string>();

                    for (int i = 0; i < selectedRows.Length; i++)
                    {
                        DataRow row = gridView1.GetDataRow(selectedRows[i]);
                        strOID = row.ItemArray[0].ToString();
                        OIDList.Add(strOID);
                    }
                    SelectFeatures(OIDList, _curActive);
                }
            }
        }
        public void SelectFeatures(List<string> oidList, IActiveView _curActive)
        {
            IFeatureClass featureClass = PTocFeatureLayer.FeatureClass;
            string strID = string.Empty;
            string[] IDs = oidList.ToArray();
            for (int i = 0; i < IDs.Length; i++)
            {
                strID = IDs[i];
                IFeature selectedFeature = featureClass.GetFeature(Convert.ToInt32(strID));
                PTocMap.SelectFeature(PTocFeatureLayer, selectedFeature);
            }
            _curActive.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, _curActive.Extent);
        }

        public void MapSelect()
        {
            //
            this.gridView1.SelectionChanged -= new SelectionChangedEventHandler(gridView1_SelectionChanged);
            //
            if (tempOIDList.Count == 0) return;
            List<int> OIDList = new List<int>();
            OIDList = tempOIDList;
            if (!(OIDList.Count > 0))
            {
                return;
            }
            for (int i = 0; i < OIDList.Count; i++)
            {
                for (int j = 0; j < gridView1.RowCount; j++)
                {
                    if (gridView1.GetDataRow(j).ItemArray[0].ToString() == OIDList[i].ToString())
                    {
                        gridView1.SelectRow(j);
                        break;
                    }
                }
                if (i == OIDList.Count - 1)
                    isFirstLoad = true;
            }
            this.gridView1.SelectionChanged += new SelectionChangedEventHandler(gridView1_SelectionChanged);
        }
    }
}