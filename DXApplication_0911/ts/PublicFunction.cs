using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Drawing;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;

namespace ts
{
    public class PublicFunction
    {
        public bool LoadMxd(AxMapControl axMapControl1)//加载.Mxd地图文件
        {
            OpenFileDialog pOpenFileDialog1 = new OpenFileDialog();
            pOpenFileDialog1.Title = "添加文件";
            pOpenFileDialog1.Filter = "Map Doucument(*.mxd)|*.mxd;|ArcMap模板(*.mxt)|*.mxt;|发布地图文件(*.pmf)|*.pmf;|所有地图格式(*.mxd;*.mxt;*.pmf)|*.mxd;*.mxt;*.pmf";//筛选器语句
            pOpenFileDialog1.ShowDialog();
            string strFileName = pOpenFileDialog1.FileName;
            if (strFileName == "")
                return false;
            if (axMapControl1.CheckMxFile(strFileName))
            {
                axMapControl1.LoadMxFile(strFileName);
            }
            return true;
        }
        public void SaveMap(AxMapControl axMapControl1)//保存为.Mxd或修改后的.Mxd
        {
            string sMxdFileName = axMapControl1.DocumentFilename;
            IMapDocument pMapDocument = new MapDocumentClass();
            if (sMxdFileName != null && axMapControl1.CheckMxFile(sMxdFileName))
            {
                if (pMapDocument.get_IsReadOnly(sMxdFileName))
                {
                    XtraMessageBox.Show("此地图为只读，不能保存");
                    pMapDocument.Close();
                    return;
                }
            }
            else
            {
                SaveFileDialog pSaveFileDialog = new SaveFileDialog();
                pSaveFileDialog.Title = "请选择保存路径";
                pSaveFileDialog.Filter = "ArcMap文档(*.mxd)|*.mxd;|ArcMap模板(*.mxt)|*.mxt";
                if (pSaveFileDialog.ShowDialog() == DialogResult.OK)     //控制保存的地图格式为.mxd或者.mxt，点击弹出窗口的确定
                    sMxdFileName = pSaveFileDialog.FileName;             //这个if...else一定要写，否则会报错
                else
                    return;
            }
            pMapDocument.New(sMxdFileName);
            pMapDocument.ReplaceContents(axMapControl1.Map as IMxdContents);

            IDocumentInfo2 pDocInfo = pMapDocument as IDocumentInfo2;     //修改Map Properties的接口
            pDocInfo.Comments = "hh";                                     //title-DocumentTitle;  author-Author  ;       

            pMapDocument.Save(pMapDocument.UsesRelativePaths, true);
            pMapDocument.Close();
            XtraMessageBox.Show("地图保存成功！");  //MXD地图生成成功
        }
        public void SaveAsMap(AxMapControl axMapControl1)//另存为其他路径的.Mxd
        {
            string sMxdFileName = axMapControl1.DocumentFilename;
            IMapDocument pMapDocument = new MapDocumentClass();
            if (sMxdFileName != null && axMapControl1.CheckMxFile(sMxdFileName))
            {
                if (pMapDocument.get_IsReadOnly(sMxdFileName))
                {
                    XtraMessageBox.Show("此地图为只读，不能保存");
                    pMapDocument.Close();
                    return;
                }
                else
                {
                    SaveFileDialog pSaveFileDialog = new SaveFileDialog();
                    pSaveFileDialog.Title = "请选择保存路径";
                    pSaveFileDialog.Filter = "ArcMap文档(*.mxd)|*.mxd;|ArcMap模板(*.mxt)|*.mxt";
                    if (pSaveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        sMxdFileName = pSaveFileDialog.FileName;
                        pMapDocument.New(sMxdFileName);
                        pMapDocument.ReplaceContents(axMapControl1.Map as IMxdContents);
                        pMapDocument.Save(true, true);
                        pMapDocument.Close();
                        XtraMessageBox.Show("地图保存成功！");
                    }
                    else
                        return;
                }
            }
        }
        public bool ShpLoad(AxMapControl axMapControl1)//矢量数据的添加
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.Title = "添加文件";
            pOpenFileDialog.Filter = "Shape文件(*.shp)|*.shp";
            if (pOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                string strFileName = pOpenFileDialog.FileName;
                int pIndex = strFileName.LastIndexOf("\\");
                string pFilePath = strFileName.Substring(0, pIndex); //获取文件路径
                string pFileName = strFileName.Substring(pIndex + 1); //获取文件名 

                //axToolbarControl1.SetBuddyControl(axPageLayoutControl1);

                if (strFileName == "")
                    return false;
                IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
                //IWorkspaceFactory需要引用ESRI.ArcGIS.Geodatabase      ShapefileWorkspaceFactory需要引用ArcGIS.DataSourcesFile
                IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(pFilePath, 0);
                IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(pFileName);
                IFeatureLayer pFeatureLayer = new FeatureLayer();         //注意是FeatureLayer而不是IFeatureLayer;
                pFeatureLayer.FeatureClass = pFeatureClass;
                pFeatureLayer.Name = pFeatureClass.AliasName;
                axMapControl1.Map.AddLayer(pFeatureLayer);
                axMapControl1.ActiveView.Refresh();
                return true;
            }
            return false;
        }
        public bool TiffLoad(AxMapControl axMapControl1)//栅格数据的添加
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.Title = "添加文件";
            pOpenFileDialog.Filter = "栅格文件 (*.*)|*.bmp;*.tif;*.jpg;*.img|(*.bmp)|*.bmp|(*.tif)|*.tif|(*.jpg)|*.jpg|(*.img)|*.img";
            if (pOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                string strFileName11 = pOpenFileDialog.FileName;
                string pFilePath11 = System.IO.Path.GetDirectoryName(strFileName11);
                string pFileName11 = System.IO.Path.GetFileName(strFileName11);
                //axToolbarControl1.SetBuddyControl(axPageLayoutControl1);
                if (strFileName11 == "")
                    return false;
                IWorkspaceFactory pWorkspaceFactory = new RasterWorkspaceFactory();
                IRasterWorkspace pRasterWorkspace = (IRasterWorkspace)pWorkspaceFactory.OpenFromFile(pFilePath11, 0);
                IRasterDataset pRasterDataset = pRasterWorkspace.OpenRasterDataset(pFileName11);
                IRasterPyramid pRasterPyramid = pRasterDataset as IRasterPyramid;
                if (pRasterPyramid != null)
                {
                    if (!(pRasterPyramid.Present))
                        pRasterPyramid.Create();
                }
                //因为无法使用 pRasterLayer.RasterDataset=pRasterDataset;   所以使用以下方法：
                IRaster pRaster = pRasterDataset.CreateDefaultRaster();   //定义一个栅格格式，用pRasterDataset创建一个栅格格式
                IRasterLayer pRasterLayer = new RasterLayer();
                pRasterLayer.CreateFromRaster(pRaster);      //在栅格图层中创建栅格图层，加载pRaster
                axMapControl1.Map.AddLayer(pRasterLayer);    //在主窗体内加入图层
                axMapControl1.ActiveView.Refresh();
                return true;
            }
            return false;
        }
        public string GetMapUnit(esriUnits _esriMapUnit)//获取Map的单位
        {
            string sMapUnits = string.Empty;
            switch (_esriMapUnit)
            {
                case esriUnits.esriCentimeters:
                    sMapUnits = "厘米";
                    break;
                case esriUnits.esriDecimalDegrees:
                    sMapUnits = "十进制";
                    break;
                case esriUnits.esriDecimeters:
                    sMapUnits = "分米";
                    break;
                case esriUnits.esriFeet:
                    sMapUnits = "尺";
                    break;
                case esriUnits.esriInches:
                    sMapUnits = "英寸";
                    break;
                case esriUnits.esriKilometers:
                    sMapUnits = "千米";
                    break;
                case esriUnits.esriMeters:
                    sMapUnits = "米";
                    break;
                case esriUnits.esriMiles:
                    sMapUnits = "英里";
                    break;
                case esriUnits.esriMillimeters:
                    sMapUnits = "毫米";
                    break;
                case esriUnits.esriNauticalMiles:
                    sMapUnits = "海里";
                    break;
                case esriUnits.esriPoints:
                    sMapUnits = "点";
                    break;
                case esriUnits.esriUnitsLast:
                    sMapUnits = "UnitsLast";
                    break;
                case esriUnits.esriUnknownUnits:
                    sMapUnits = "未知单位";
                    break;
                case esriUnits.esriYards:
                    sMapUnits = "码";
                    break;
                default:
                    break;
            }
            return sMapUnits;
        }
        public void ExportImage(AxPageLayoutControl axPageLayoutControl1)//地图整饰完成后导出（未实现交互，存放位置、图片类型、图片质量等）,打印边框还有一些问题
        {
            IActiveView pActiveView;
            pActiveView = axPageLayoutControl1.ActiveView;
            ESRI.ArcGIS.Output.IExport pExport = new ESRI.ArcGIS.Output.ExportBMPClass();
            pExport.ExportFileName = @"D:\cscs.png";
            pExport.Resolution = 96;
            ESRI.ArcGIS.esriSystem.tagRECT pExportRECT;
            pExportRECT.left = 0;
            pExportRECT.top = 0;
            pExportRECT.right = pActiveView.ExportFrame.right * (96 / 12);//这里应该怎么写？
            //pExportRECT.right = pActiveView.ExportFrame.right * (96 / 96);
            pExportRECT.bottom = pActiveView.ExportFrame.bottom * (96 / 12);

            ESRI.ArcGIS.Geometry.IEnvelope pEnvelope = new ESRI.ArcGIS.Geometry.EnvelopeClass();
            pEnvelope.PutCoords(pExportRECT.left, pExportRECT.top, pExportRECT.right, pExportRECT.bottom);
            pExport.PixelBounds = pEnvelope;

            System.Int32 hDC = pExport.StartExporting();

            pActiveView.Output(hDC, (System.Int16)pExport.Resolution, ref pExportRECT, null, null);
            pExport.FinishExporting();
            pExport.Cleanup();
            XtraMessageBox.Show("导出完成","提示");
        }
        public void ExportMapToImage(AxPageLayoutControl axPageLayoutControl1)
        {
            try
            {
                SaveFileDialog pSaveDialog = new SaveFileDialog();
                pSaveDialog.FileName = "";
                pSaveDialog.Filter = "jpg图片(*.jpg)|*.jpg|tif图片(*.tif)|*.tif|PDF文档(*.pdf)|*.pdf";
                //pSaveDialog.Filter = "JPG图片(*.jpg)|*.jpg|tif图片(*.tif)|*.tif|png图片(*.png)|*.png|PDF文档(*.pdf)|*.pdf";
                if (pSaveDialog.ShowDialog() == DialogResult.OK)
                {
                    double iScreenDispalyResolution = axPageLayoutControl1.ActiveView.ScreenDisplay.DisplayTransformation.Resolution;// 获取屏幕分辨率的值
                    IExporter pExporter = null;
                    if (pSaveDialog.FilterIndex == 1)
                    {
                        pExporter = new JpegExporterClass();
                    }
                    else if (pSaveDialog.FilterIndex == 2)
                    {
                        pExporter = new TiffExporterClass();
                    }
                    else if (pSaveDialog.FilterIndex == 3)
                    {
                        pExporter = new PDFExporterClass();
                    }
                    pExporter.ExportFileName = pSaveDialog.FileName;
                    pExporter.Resolution = (short)iScreenDispalyResolution; //分辨率
                    tagRECT deviceRect = axPageLayoutControl1.ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame();
                    IEnvelope pDeviceEnvelope = new EnvelopeClass();
                    pDeviceEnvelope.PutCoords(deviceRect.left, deviceRect.bottom, deviceRect.right, deviceRect.top);
                    pExporter.PixelBounds = pDeviceEnvelope; // 输出图片的范围
                    ITrackCancel pCancle = new CancelTrackerClass();//可用ESC键取消操作
                    axPageLayoutControl1.ActiveView.Output(pExporter.StartExporting(), pExporter.Resolution, ref deviceRect, axPageLayoutControl1.ActiveView.Extent, pCancle);
                    Application.DoEvents();
                    pExporter.FinishExporting();
                    XtraMessageBox.Show("导出完成", "提示");
                }
            }
            catch (Exception Err)
            {
                XtraMessageBox.Show(Err.Message, "输出图片", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public IColor ConvertToRgbColor(Color color)// Color转换为IColor
        {
            IColor pColor = new RgbColor();
            pColor.RGB = color.R + color.G * 256 + color.B * 65536;
            return pColor;
        }
        public Color ConvertToColor(IColor pColor)// IColor转换为Color
        {
            return ColorTranslator.FromOle(pColor.RGB);
        }
        public Color ConvertIRgbColorToColor(IRgbColor pRgbColor)// IRgbColor转换为Color
        {

            return ColorTranslator.FromOle(pRgbColor.RGB);

        }
        public IRgbColor GetRgbColor(int intR, int intG, int intB) //根据RGB值生成IRgbColor
        {
            IRgbColor pRgbColor = null;
            if (intR < 0 || intR > 255 || intG < 0 || intG > 255 || intB < 0 || intB > 255)
            {
                return pRgbColor;
            }
            pRgbColor = new RgbColorClass();
            pRgbColor.Red = intR;
            pRgbColor.Green = intG;
            pRgbColor.Blue = intB;
            return pRgbColor;
        }
        public Font GetFontFromIFontDisp(stdole.IFontDisp pFontDisp) //IFontDisp转为Font
        {
            FontStyle pFontStyle = new FontStyle();
            if (pFontDisp.Bold)
                pFontStyle = FontStyle.Bold;
            if (pFontDisp.Italic)
                pFontStyle = FontStyle.Italic;
            if (pFontDisp.Underline)
                pFontStyle = FontStyle.Underline;

            Font pFont = new Font(pFontDisp.Name, (float)pFontDisp.Size, pFontStyle);
            return pFont;
        }
        public ITable BuildRasterTable(ILayer pLayer) //创建栅格数据的属性表
        {
            if (!(pLayer is IRasterLayer)) return null;
            IRasterLayer rasterLayer = pLayer as IRasterLayer;
            IRaster pRaster = rasterLayer.Raster;
            IRasterProps rProp = pRaster as IRasterProps;
            if (rProp == null)
            {
                return null;
            }
            if (rProp.PixelType == rstPixelType.PT_FLOAT || rProp.PixelType == rstPixelType.PT_DOUBLE) //判断栅格像元值是否是整型
            {
                return null;
            }
            IRasterBandCollection pRasterbandCollection = (IRasterBandCollection)pRaster;
            IRasterBand rasterBand = pRasterbandCollection.Item(0);
            ITable rTable = rasterBand.AttributeTable;
            if (rTable != null) return rasterBand.AttributeTable; //直接获取属性表

            IDataLayer2 dataLayer = rasterLayer as IDataLayer2;
            IDatasetName ds = dataLayer.DataSourceName as IDatasetName;
            IWorkspaceName ws = ds.WorkspaceName;
            string strPath = ws.PathName + "\\" + rasterLayer.Name;

            //string strPath = rasterLayer.FilePath;
            string strDirName = System.IO.Path.GetDirectoryName(strPath);
            string strRasterName = System.IO.Path.GetFileName(strPath);
            //创建工作空间
            IWorkspaceFactory pWork = new RasterWorkspaceFactoryClass();
            //打开工作空间路径 ，工作空间的参数是目录，不是具体的文件名
            IRasterWorkspace pRasterWs = (IRasterWorkspace)pWork.OpenFromFile(strDirName, 0);
            //打开工作空间下的文件，
            IRasterDataset rasterDataset = pRasterWs.OpenRasterDataset(strRasterName);
            IRasterDatasetEdit2 rasterDatasetEdit = (IRasterDatasetEdit2)rasterDataset;
            if (rasterDatasetEdit == null)
            {
                return null;
            }
            //Build default raster attribute table with VALUE and COUNT
            rasterDatasetEdit.BuildAttributeTable();  //建立属性表
            //更新属性表
            pRasterbandCollection = (IRasterBandCollection)rasterDataset;
            rasterBand = pRasterbandCollection.Item(0);
            return rasterBand.AttributeTable;    //重新获取属性表
        }

        public bool SetupFeaturePropertySheet(ILayer layer, IActiveView activeview, ESRI.ArcGIS.Controls.AxTOCControl mTOCControl)//矢量数据的渲染界面调用
        {
            if (layer == null) return false;
            ESRI.ArcGIS.Framework.IComPropertySheet pComPropSheet;
            pComPropSheet = new ESRI.ArcGIS.Framework.ComPropertySheet();
            pComPropSheet.Title = layer.Name + " - 属性";

            ESRI.ArcGIS.esriSystem.UID pPPUID = new ESRI.ArcGIS.esriSystem.UIDClass();
            pComPropSheet.AddCategoryID(pPPUID);

            // General....
            ESRI.ArcGIS.Framework.IPropertyPage pGenPage = new ESRI.ArcGIS.CartoUI.GeneralLayerPropPageClass();
            pComPropSheet.AddPage(pGenPage);

            // Source
            ESRI.ArcGIS.Framework.IPropertyPage pSrcPage = new ESRI.ArcGIS.CartoUI.FeatureLayerSourcePropertyPageClass();
            pComPropSheet.AddPage(pSrcPage);

            // Selection...
            ESRI.ArcGIS.Framework.IPropertyPage pSelectPage = new ESRI.ArcGIS.CartoUI.FeatureLayerSelectionPropertyPageClass();
            pComPropSheet.AddPage(pSelectPage);

            // Display....
            ESRI.ArcGIS.Framework.IPropertyPage pDispPage = new ESRI.ArcGIS.CartoUI.FeatureLayerDisplayPropertyPageClass();
            pComPropSheet.AddPage(pDispPage);

            // Symbology....
            ESRI.ArcGIS.Framework.IPropertyPage pDrawPage = new ESRI.ArcGIS.CartoUI.LayerDrawingPropertyPageClass();
            pComPropSheet.AddPage(pDrawPage);

            // Fields... 
            ESRI.ArcGIS.Framework.IPropertyPage pFieldsPage = new ESRI.ArcGIS.CartoUI.LayerFieldsPropertyPageClass();
            pComPropSheet.AddPage(pFieldsPage);

            // Definition Query... 
            ESRI.ArcGIS.Framework.IPropertyPage pQueryPage = new ESRI.ArcGIS.CartoUI.LayerDefinitionQueryPropertyPageClass();
            pComPropSheet.AddPage(pQueryPage);

            // Labels....
            ESRI.ArcGIS.Framework.IPropertyPage pSelPage = new ESRI.ArcGIS.CartoUI.LayerLabelsPropertyPageClass();
            pComPropSheet.AddPage(pSelPage);

            // Joins & Relates....
            ESRI.ArcGIS.Framework.IPropertyPage pJoinPage = new ESRI.ArcGIS.ArcMapUI.JoinRelatePageClass();
            pComPropSheet.AddPage(pJoinPage);

            // Setup layer link
            ESRI.ArcGIS.esriSystem.ISet pMySet = new ESRI.ArcGIS.esriSystem.SetClass();
            pMySet.Add(layer);
            pMySet.Reset();

            // make the symbology tab active
            pComPropSheet.ActivePage = 4;

            // show the property sheet
            bool bOK = pComPropSheet.EditProperties(pMySet, 0);

            activeview.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, activeview.Extent);
            mTOCControl.Update();

            return (bOK);
        }

    }

}
