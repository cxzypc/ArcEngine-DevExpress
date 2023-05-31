using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Helpers;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraEditors;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DisplayUI;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;


namespace ts
{
    public partial class Form1 : RibbonForm
    {
        private IMapControl3 m_mapControl = null;
        private IPageLayoutControl2 m_pageLayoutControl = null;

        private MapAndPage.ControlsSynchronizer m_controlsSynchronizer = null;

        PublicFunction PF = new PublicFunction();
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        TimeSpan timeSpan;  //判断是否执行图层拖动前后层关系  的时间间隔，设置为200ms
        int toIndex;
        ITOCControl mTOCControl;
        ILayer pMoveLayer;
        IFeatureLayer pTocFeatureLayer = null;
        IRasterLayer pTocRasterLayer = null;
        IMap pTocMap = null;
        IToolbarMenu m_toolbarMenu_map;
        IToolbarMenu m_toolbarMenu_page_whole;
        IToolbarMenu m_toolbarMenu_page_surround;
        IToolbarMenu m_toolbarMenu_page_mapframe;
        IElement element;
        private ITextSymbol m_Title;
        private INorthArrow m_NorthArrrow;
        private IScaleBar2 m_ScaleBar;
        private string operation;
        ITool PageOutTool = null;
        bool mybo = false;//判断TOCControl是否第一次双击


        public Form1()
        {
            //ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            IAoInitialize aoInitialize = new AoInitialize();
            esriLicenseStatus licenseStatus = esriLicenseStatus.esriLicenseUnavailable;
            licenseStatus = aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
            if (licenseStatus == esriLicenseStatus.esriLicenseNotInitialized)
            {
                MessageBox.Show("没有esriLicenseProductCodeArcInfo许可！");
                Application.Exit();
            }

            //this.Location = new Point((SystemInformation.PrimaryMonitorSize.Width - this.Width) / 2, (SystemInformation.PrimaryMonitorSize.Height - this.Height) / 2);
            InitializeComponent();

            InitSkinGallery();
            axTOCControl1.SetBuddyControl(axMapControl1);

            //string sProgID = "esriControlTools.ControlsMapFullExtentCommand";
            //axToolbarControl1.AddItem(sProgID, -1, 8, false, -1, esriCommandStyles.esriCommandStyleIconOnly);

        }
        void InitSkinGallery()
        {
            SkinHelper.InitSkinGallery(rgbiSkins, true);
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            PF.SaveMap(axMapControl1);
        }

        private void ribbonStatusBar_Click(object sender, EventArgs e)
        {

        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            bool tf = PF.LoadMxd(axMapControl1);
            if (tf)
            {
                axToolbarControl1.SetBuddyControl(axMapControl1);
                axToolbarControl2.SetBuddyControl(axPageLayoutControl1);
            }
        }

        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            PF.SaveAsMap(axMapControl1);
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            //初始化axTOCControl1_OnDoubleClick里面的窗体，必须要有，否则栅格的FrmSymbolSelector出不来
            if (!mybo)
            {
                CustomForm.FrmSymbolSelector FSS = new CustomForm.FrmSymbolSelector();
                FSS.Show();
                FSS.Close();
                FSS.Dispose();
                mybo = true;
            }

            esriTOCControlItem pItem = new esriTOCControlItem();
            IBasicMap pMap = null;
            ILayer pLayer = null;
            object unk = null;
            object data = null;

            if (e.button == 1)
            {
                watch.Reset();
                watch.Start();
                pMoveLayer = null;
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);
                if (pItem == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    if (pLayer is IAnnotationSublayer) return;//如果是注记图层则返回
                    else
                        pMoveLayer = pLayer;
                }
            }
            else if (e.button == 2)
            {
                pTocFeatureLayer = null;
                pTocRasterLayer = null;
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);
                pTocFeatureLayer = pLayer as IFeatureLayer;
                pTocRasterLayer = pLayer as IRasterLayer;
                pTocMap = pMap as IMap;
                if (pItem == esriTOCControlItem.esriTOCControlItemMap)
                {
                    popupMenu1.ShowPopup(Control.MousePosition);
                }
                else
                {
                    if (pItem == esriTOCControlItem.esriTOCControlItemLayer && (pTocFeatureLayer != null))
                    {
                        popupMenu2.ShowPopup(Control.MousePosition);
                    }
                    if (pItem == esriTOCControlItem.esriTOCControlItemLayer && (pTocRasterLayer != null))
                    {
                        popupMenu2.ShowPopup(Control.MousePosition);
                    }
                }
            }

        }


        private void barButtonItem20_ItemClick(object sender, ItemClickEventArgs e)
        {
            bool tf = PF.ShpLoad(axMapControl1);
            if (tf)
            {
                axToolbarControl1.SetBuddyControl(axMapControl1);
                axToolbarControl2.SetBuddyControl(axPageLayoutControl1);
            }
        }

        private void barButtonItem21_ItemClick(object sender, ItemClickEventArgs e)
        {
            bool tf = PF.TiffLoad(axMapControl1);
            if (tf)
            {
                axToolbarControl1.SetBuddyControl(axMapControl1);
                axToolbarControl2.SetBuddyControl(axPageLayoutControl1);
            }
        }
        
        private void barButtonItem22_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pTocFeatureLayer == null && pTocRasterLayer == null) return;
            if (pTocFeatureLayer != null)
            {
                (axMapControl1.Map as IActiveView).Extent = pTocFeatureLayer.AreaOfInterest;
                (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
            else if (pTocRasterLayer != null)
            {
                (axMapControl1.Map as IActiveView).Extent = pTocRasterLayer.AreaOfInterest;
                (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }

        private void barButtonItem23_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pTocRasterLayer == null && pTocFeatureLayer == null) return;
            if (pTocRasterLayer != null)
            {
                DialogResult result = XtraMessageBox.Show("是否删除图层“" + pTocRasterLayer.Name + "”", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    axMapControl1.Map.DeleteLayer(pTocRasterLayer);
                }
            }
            else if (pTocFeatureLayer != null)
            {
                DialogResult result = XtraMessageBox.Show("是否删除图层“" + pTocFeatureLayer.Name + "”", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    axMapControl1.Map.DeleteLayer(pTocFeatureLayer);
                }
            }
            axMapControl1.ActiveView.Refresh();
        }

        private void axTOCControl1_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        {
            esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap pBasicMap = null;
            ILayer pLayer = null;
            object unk = null;
            object data = null;

            if (e.button == 1) { watch.Stop(); timeSpan = watch.Elapsed; }

            if (e.button == 1 && timeSpan.TotalMilliseconds > 200)
            {
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pBasicMap, ref pLayer, ref unk, ref data);
                if (pMoveLayer != pLayer)//如果是原图层则不用操作
                {
                    IMap pMap = axMapControl1.Map;
                    ILayer pTempLayer;
                    for (int i = 0; i < pMap.LayerCount; i++)
                    {
                        pTempLayer = pMap.get_Layer(i);
                        if (pTempLayer == pLayer)//获取移动后的图层索引
                            toIndex = i;
                    }
                    pMap.MoveLayer(pMoveLayer, toIndex);
                    axMapControl1.ActiveView.Refresh();
                    mTOCControl.Update();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_mapControl = (IMapControl3)this.axMapControl1.Object;
            m_pageLayoutControl = (IPageLayoutControl2)this.axPageLayoutControl1.Object;

            m_controlsSynchronizer = new MapAndPage.ControlsSynchronizer(m_mapControl, m_pageLayoutControl);
            //把MapControl和PageLayoutControl绑定起来(两个都指向同一个Map),然后设置MapControl为活动的Control
            m_controlsSynchronizer.BindControls(true);
            //为了在切换MapControl和PageLayoutControl视图同步，要添加Framework Control
            m_controlsSynchronizer.AddFrameworkControl(axToolbarControl1.Object);
            m_controlsSynchronizer.AddFrameworkControl(this.axTOCControl1.Object);

            mTOCControl = axTOCControl1.Object as ITOCControl;

            //axMapContorl1右键菜单
            m_toolbarMenu_map = new ToolbarMenuClass();
            m_toolbarMenu_map.AddItem("esriControlTools.ControlsMapFullExtentCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_map.AddItem("esriControlTools.ControlsMapZoomToLastExtentBackCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_map.AddItem("esriControlTools.ControlsMapZoomToLastExtentForwardCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_map.AddItem("esriControlTools.ControlsMapZoomInFixedCommand", 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_map.AddItem("esriControlTools.ControlsMapZoomOutFixedCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_map.AddItem("esriControlTools.ControlsSelectFeaturesTool", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_map.AddItem("esriControlTools.ControlsMapIdentifyTool", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_map.AddItem("esriControlTools.ControlsZoomToSelectedCommand", 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_map.AddItem("esriControlTools.ControlsClearSelectionCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_map.AddItem(new MenuSource.ClearCurrentTool(), 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);//menusource文件夹下的EsriTool
            m_toolbarMenu_map.SetHook(axMapControl1);

            //axPageLayoutControl1右键全局菜单
            m_toolbarMenu_page_whole = new ToolbarMenuClass();
            m_toolbarMenu_page_whole.AddItem("esriControlTools.ControlsPageZoomWholePageCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_whole.AddItem("esriControlTools.ControlsPageZoomPageToLastExtentBackCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_whole.AddItem("esriControlTools.ControlsPageZoomPageToLastExtentForwardCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            ////感觉不对
            m_toolbarMenu_page_whole.AddItem("esriControlTools.ControlsEditingCutCommand", 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_whole.AddItem("esriControlTools.ControlsEditingCopyCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_whole.AddItem("esriControlTools.ControlsEditingPasteCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_whole.AddItem("esriControlTools.ControlsEditingSketchDeleteCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_page_whole.AddItem(new MenuSource.ClearCurrentPageTool(), 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);//menusource文件夹下的EsriTool

            m_toolbarMenu_page_whole.SetHook(axPageLayoutControl1);

            //axPageLayoutControl1中MapSurround的右键菜单
            m_toolbarMenu_page_surround = new ToolbarMenuClass();
            m_toolbarMenu_page_surround.AddItem("esriControlTools.ControlsPageZoomWholePageCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            ////不对
            m_toolbarMenu_page_surround.AddItem("esriControlTools.ControlsEditingCutCommand", 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_surround.AddItem("esriControlTools.ControlsEditingCopyCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_surround.AddItem("esriControlTools.ControlsEditingSketchDeleteCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_page_surround.AddItem(new MenuSource.ConvertToGraphics(), 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_surround.AddItem("esriControls.ControlsGroupCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_surround.AddItem("esriControlTools.ControlsUngroupCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            //还有Convert To Graphics、Zoom To Selected Elements等功能未实现
            m_toolbarMenu_page_surround.SetHook(axPageLayoutControl1);

            //axPageLayoutControl1中MapFrame的右键菜单
            m_toolbarMenu_page_mapframe = new ToolbarMenuClass();
            m_toolbarMenu_page_mapframe.AddItem("esriControlTools.ControlsAddDataCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_page_mapframe.AddItem("esriControlTools.ControlsMapFullExtentCommand", 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_mapframe.AddItem("esriControlTools.ControlsPageZoomWholePageCommand", 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);

            ////不对
            m_toolbarMenu_page_mapframe.AddItem("esriControlTools.ControlsEditingCutCommand", 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_mapframe.AddItem("esriControlTools.ControlsEditingCopyCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_mapframe.AddItem("esriControlTools.ControlsEditingSketchDeleteCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_page_mapframe.AddItem(new MenuSource.ConvertToGraphics(), 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_mapframe.AddItem("esriControls.ControlsGroupCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_mapframe.AddItem("esriControlTools.ControlsUngroupCommand", 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);
            m_toolbarMenu_page_mapframe.AddItem(new MenuSource.FitToMargins(), 0, -1, true, esriCommandStyles.esriCommandStyleIconAndText);
            //ICommand command = new ControlsMapFullExtentCommand();
            ////m_toolbarMenu_map.AddItem(command, 0, -1, false, esriCommandStyles.esriCommandStyleIconAndText);

            m_toolbarMenu_page_mapframe.SetHook(axPageLayoutControl1);

            //向axPageLayout中的Toolbar添加Select Elements控件
            MenuSource.SelectElement SE = new MenuSource.SelectElement(m_controlsSynchronizer);
            axToolbarControl2.AddItem(SE, -1, 10, false, -1, esriCommandStyles.esriCommandStyleIconOnly);//Text的话就是对的

            //向axMapControl中的Toolbar添加Map Pan控件
            MenuSource.MapPan MP = new MenuSource.MapPan(m_controlsSynchronizer);
            axToolbarControl1.AddItem(MP, -1, 8, false, -1, esriCommandStyles.esriCommandStyleIconOnly);
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 2)
            {
                m_toolbarMenu_map.PopupMenu(e.x, e.y, axMapControl1.hWnd);
            }
        }

        private void barButtonItem12_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            string sMapUnits = PF.GetMapUnit(axMapControl1.Map.MapUnits);
            IPoint pMovePt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y); //计算对应于设备点的地图坐标中的点
            barStaticItem4.Caption = String.Format("当前坐标为：X = {0:#.###} Y = {1:#.###} {2}", e.mapX, e.mapY, sMapUnits);
        }


        private void axPageLayoutControl1_OnMouseDown(object sender, IPageLayoutControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 1)
            {
                #region
                if (operation == "添加指北针")
                {
                    IActiveView pActiveViewv = axPageLayoutControl1.PageLayout as IActiveView;
                    IGraphicsContainer pGraphicsContainer = pActiveViewv.GraphicsContainer;

                    // 获取框架元素
                    IMapFrame pMapFrame = pGraphicsContainer.FindFrame(pActiveViewv.FocusMap) as IMapFrame;
                    IMapSurroundFrame pMapSurroundFrame = new MapSurroundFrame() as IMapSurroundFrame;
                    pMapSurroundFrame.MapFrame = pMapFrame;

                    pMapSurroundFrame.MapSurround = m_NorthArrrow as IMapSurround;
                    pMapSurroundFrame.MapSurround.Name = "图例";
                    double siz = m_NorthArrrow.Size;

                    //我自己写的
                    IEnvelope pEnvelope = new EnvelopeClass();
                    IPoint pt = new PointClass();
                    pt = pActiveViewv.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x - Convert.ToInt32(siz / 5), e.y - Convert.ToInt32(siz / 5));
                    IPoint pt2 = new PointClass();
                    pt2 = pActiveViewv.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x + Convert.ToInt32(siz / 5), e.y + Convert.ToInt32(siz / 5));
                    pEnvelope.PutCoords(pt.X, pt.Y, pt2.X, pt2.Y);

                    IEnvelope newEnvelope = new EnvelopeClass();
                    //通过IMapSurround的QueryBounds方法可以获得MapSurround对象的边界
                    pMapSurroundFrame.MapSurround.QueryBounds(pActiveViewv.ScreenDisplay as IDisplay, pEnvelope, newEnvelope);

                    //获取鼠标选取点为中心点
                    IPoint centerp = pActiveViewv.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                    newEnvelope.CenterAt(centerp);

                    IElement m_Element;
                    // 添加指北针
                    m_Element = pMapSurroundFrame as IElement;
                    //m_Element.Geometry = pEnvelope;
                    m_Element.Geometry = newEnvelope;
                    pGraphicsContainer.AddElement(m_Element, 0);
                    pActiveViewv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    operation = "";
                    axPageLayoutControl1.CurrentTool = PageOutTool;

                }
                else if (operation == "添加比例尺") //还有一些问题，不像ArcGIS里的操作
                {
                    IEnvelope pEnvelope = axPageLayoutControl1.TrackRectangle();
                    if (pEnvelope.IsEmpty || pEnvelope == null || pEnvelope.Width == 0 || pEnvelope.Height == 0)
                    {
                        XtraMessageBox.Show("请在页面中框选一个范围", "提示");
                        return;
                    }

                    // 删除已有比例尺
                    IActiveView pActiveViewv = axPageLayoutControl1.PageLayout as IActiveView;
                    IGraphicsContainer pGraphicsContainer = pActiveViewv.GraphicsContainer;

                    // 获取框架元素
                    IMapFrame pMapFrame = pGraphicsContainer.FindFrame(pActiveViewv.FocusMap) as IMapFrame;
                    IMapSurroundFrame pMapSurroundFrame = new MapSurroundFrame() as IMapSurroundFrame;
                    pMapSurroundFrame.MapFrame = pMapFrame;
                    pMapSurroundFrame.MapSurround = m_ScaleBar as IMapSurround;

                    // 添加指北针
                    IElement m_ScaleBarElement;
                    m_ScaleBarElement = pMapSurroundFrame as IElement;

                    m_ScaleBarElement.Geometry = pEnvelope;
                    pGraphicsContainer.AddElement(m_ScaleBarElement, 0);
                    pActiveViewv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    operation = "";
                    axPageLayoutControl1.CurrentTool = PageOutTool;
                }
                else if (operation == "添加图例")
                {
                    IEnvelope pEnvelope = axPageLayoutControl1.TrackRectangle();
                    if (pEnvelope.IsEmpty || pEnvelope == null || pEnvelope.Width == 0 || pEnvelope.Height == 0)
                    {
                        XtraMessageBox.Show("请在页面中框选一个范围", "提示");
                        return;
                    }

                    IActiveView pActiveViewv = axPageLayoutControl1.PageLayout as IActiveView;
                    IGraphicsContainer pGraphicsContainer = pActiveViewv.GraphicsContainer;

                    // 获取框架元素
                    UID uID = new UIDClass();//创建UID作为该图例的唯一标识符，方便创建之后进行删除、移动等操作
                    uID.Value = "esriCarto.Legend";
                    IMapFrame pMapFrame = pGraphicsContainer.FindFrame(pActiveViewv.FocusMap) as IMapFrame;
                    IMapSurroundFrame pMapSurroundFrame = pMapFrame.CreateSurroundFrame(uID, null);
                    pMapSurroundFrame.MapSurround.Name = "图例";

                    // 添加指北针
                    IElement m_LegnedElement;
                    m_LegnedElement = pMapSurroundFrame as IElement;
                    m_LegnedElement.Geometry = pEnvelope;
                    pGraphicsContainer.AddElement(m_LegnedElement, 0);

                    pActiveViewv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    operation = "";
                    axPageLayoutControl1.CurrentTool = PageOutTool;
                }
                else if (operation == "添加图名")
                {
                    IActiveView pActiveViewv = axPageLayoutControl1.PageLayout as IActiveView;
                    IGraphicsContainer pGraphicsContainer = pActiveViewv.GraphicsContainer;
                    IElement m_TitleElement;

                    IPoint point = pActiveViewv.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                    ITextElement txtEL = new TextElementClass();
                    txtEL.Text = m_Title.Text;
                    txtEL.Symbol = m_Title;
                    m_TitleElement = txtEL as IElement;
                    m_TitleElement.Geometry = point;
                    pGraphicsContainer.AddElement(m_TitleElement, 0);

                    pActiveViewv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    operation = "";
                    axPageLayoutControl1.CurrentTool=PageOutTool ;
                }
                #endregion
            }
            else if (e.button == 2)
            {
                if (axPageLayoutControl1.CurrentTool == null) m_toolbarMenu_page_whole.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                else if (!((axPageLayoutControl1.CurrentTool as ICommand).Name == "ControlToolsGraphicElement_SelectTool"))
                    m_toolbarMenu_page_whole.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                else if ((axPageLayoutControl1.CurrentTool as ICommand).Name == "ControlToolsGraphicElement_SelectTool")
                {
                    IPoint centerp = axPageLayoutControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                    IEnumElement pEnumElement = axPageLayoutControl1.ActiveView.GraphicsContainer.LocateElements(centerp, 0.01);

                    if (pEnumElement != null)
                    {
                        pEnumElement.Reset();
                        element = pEnumElement.Next();

                        while (element != null)
                        {
                            if (element is IMapSurroundFrame)
                            {

                                IMapSurround mapSurround = ((IMapSurroundFrame)element).MapSurround;
                                if (mapSurround is ILegend)//图例
                                {
                                    m_toolbarMenu_page_surround.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                                    break;
                                }
                                else if (mapSurround is IScaleBar)//比例尺
                                {
                                    m_toolbarMenu_page_surround.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                                    break;
                                }
                                else if (mapSurround is INorthArrow)//指南针
                                {
                                    m_toolbarMenu_page_surround.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                                    break;
                                }
                            }
                            else if (element is ITextElement)//文本
                            {
                                m_toolbarMenu_page_surround.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                                break;
                            }
                            else if (element is IMapFrame)//最外层框架矩形
                            {
                                m_toolbarMenu_page_mapframe.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                                break;
                            }
                            element = pEnumElement.Next();
                        }
                    }
                    else m_toolbarMenu_page_whole.PopupMenu(e.x, e.y, axPageLayoutControl1.hWnd);
                }
            }
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (this.xtraTabControl1.SelectedTabPageIndex == 0)
            {
                m_controlsSynchronizer.ActivateMap();//激活MapControl

                barButtonItem24.Enabled = false;
                barButtonItem25.Enabled = false;
                barButtonItem26.Enabled = false;
                barButtonItem27.Enabled = false;
                barButtonItem28.Enabled = false;
                barButtonItem29.Enabled = false;
            }
            else
            {
                m_controlsSynchronizer.ActivatePageLayout();//激活PageLayoutControl

                barStaticItem4.Caption = "";
                barButtonItem24.Enabled = true;
                barButtonItem25.Enabled = true;
                barButtonItem26.Enabled = true;
                barButtonItem27.Enabled = true;
                barButtonItem28.Enabled = true;
                barButtonItem29.Enabled = true;
            }
        }

        private void barButtonItem28_ItemClick(object sender, ItemClickEventArgs e)
        {
            //PF.ExportImage(axPageLayoutControl1);
            PF.ExportMapToImage(axPageLayoutControl1);
        }

        private void barButtonItem24_ItemClick(object sender, ItemClickEventArgs e)
        {
            CustomForm.FrmTitle FT = new CustomForm.FrmTitle();
            //FT.StartPosition = FormStartPosition.CenterScreen;
            FT.StartPosition = FormStartPosition.CenterParent;//在父类下居中显示
            FT.OnQueryTitle += pTitle => m_Title = pTitle;
            FT.ShowDialog();
            if (FT.GetBool())
            {
                if (axPageLayoutControl1.CurrentTool != null)
                    PageOutTool = axPageLayoutControl1.CurrentTool;//缓存当前PageLayOut的CurrentTool
                axPageLayoutControl1.CurrentTool = null;
                axPageLayoutControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                operation = "添加图名";
            }
        }

        private void axPageLayoutControl1_OnDoubleClick(object sender, IPageLayoutControlEvents_OnDoubleClickEvent e)
        {
            if (!((axPageLayoutControl1.CurrentTool as ICommand).Name == "ControlToolsGraphicElement_SelectTool"))
                return;
            else
            {
                IPoint centerp = axPageLayoutControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                IEnumElement pEnumElement = axPageLayoutControl1.ActiveView.GraphicsContainer.LocateElements(centerp, 0.01);

                if (pEnumElement != null)
                {
                    pEnumElement.Reset();
                    element = pEnumElement.Next();

                    while (element != null)
                    {
                        if (element is IMapSurroundFrame)
                        {

                            IMapSurround mapSurround = ((IMapSurroundFrame)element).MapSurround;
                            if (mapSurround is ILegend)//图例
                            {
                                break;
                            }
                            else if (mapSurround is IScaleBar)//比例尺
                            {
                                CustomForm.ScaleBarTXT SBT = new CustomForm.ScaleBarTXT(axPageLayoutControl1, element as IElement);
                                SBT.StartPosition = FormStartPosition.CenterParent;//在父类下居中显示，需要通过ShowDialog来实现
                                SBT.ShowDialog();
                                break;
                            }
                            else if (mapSurround is INorthArrow)//指南针
                            {
                                break;
                            }
                        }
                        else if (element is ITextElement)//文本
                        {
                            CustomForm.TitleTXT TT = new CustomForm.TitleTXT(axPageLayoutControl1, element as ITextElement);
                            TT.StartPosition = FormStartPosition.CenterParent;//在父类下居中显示，需要通过ShowDialog来实现
                            TT.ShowDialog();
                            break;
                        }
                        else if (element is IMapFrame)//最外层框架矩形
                        {
                            break;
                        }
                        element = pEnumElement.Next();
                    }
                }
                else return;
            }
        }

        private void barButtonItem25_ItemClick(object sender, ItemClickEventArgs e)
        {
            CustomForm.FrmNorthArrow FNA = new CustomForm.FrmNorthArrow();
            FNA.StartPosition = FormStartPosition.CenterParent;
            FNA.OnQueryNorthArrow += pNorthArrow => m_NorthArrrow = pNorthArrow;
            FNA.ShowDialog();
            if (FNA.GetBool())
            {
                if (axPageLayoutControl1.CurrentTool != null)
                    PageOutTool = axPageLayoutControl1.CurrentTool;
                axPageLayoutControl1.CurrentTool = null;
                axPageLayoutControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                operation = "添加指北针";
            }
        }

        private void barButtonItem26_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (axPageLayoutControl1.CurrentTool != null)
                PageOutTool = axPageLayoutControl1.CurrentTool;

            axPageLayoutControl1.CurrentTool = null;
            axPageLayoutControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            operation = "添加图例";
        }

        private void barButtonItem27_ItemClick(object sender, ItemClickEventArgs e)
        {
            CustomForm.FrmScaleBar FSB = new CustomForm.FrmScaleBar();
            FSB.StartPosition = FormStartPosition.CenterParent;
            FSB.OnQueryScaleBar += pScaleBar => m_ScaleBar = pScaleBar;
            FSB.ShowDialog();
            if (FSB.GetBool())
            {
                if (axPageLayoutControl1.CurrentTool != null)
                    PageOutTool = axPageLayoutControl1.CurrentTool;
                axPageLayoutControl1.CurrentTool = null;
                axPageLayoutControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                operation = "添加比例尺";
            }
        }

        private void barButtonItem29_ItemClick(object sender, ItemClickEventArgs e)
        {
            CustomForm.FrmPageSize FBS = new CustomForm.FrmPageSize(axPageLayoutControl1.PageLayout.Page);
            FBS.StartPosition = FormStartPosition.CenterParent;
            FBS.ShowDialog();
        }

        private void barButtonItem30_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pTocRasterLayer == null && pTocFeatureLayer == null) return;
            if (pTocFeatureLayer != null)
            {
                CustomForm.AttributeTable pAT = new CustomForm.AttributeTable(pTocFeatureLayer, ref pTocMap);
                pAT.StartPosition = FormStartPosition.CenterParent;
                pAT.PTocMap = pTocMap;
                pAT.ShowDialog();


            }
            if (pTocRasterLayer != null)
            {
                CustomForm.AttributeTable pAT = new CustomForm.AttributeTable(pTocRasterLayer, ref pTocMap);
                pAT.StartPosition = FormStartPosition.CenterParent;
                //pAT.PTocRasterLayer = pTocRasterLayer;
                //pAT.InitRaster();
                pAT.ShowDialog();
            }
        }

        private void barButtonItem31_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pTocFeatureLayer != null)
                PF.SetupFeaturePropertySheet(pTocFeatureLayer as ILayer, axMapControl1.Map as IActiveView, axTOCControl1);
            if (pTocRasterLayer != null)
            {
                CustomForm.FrmRasterRenderer FRR = new CustomForm.FrmRasterRenderer();
                FRR.StartPosition = FormStartPosition.CenterParent;
                FRR.PTocRasterLayer = pTocRasterLayer;
                FRR.PTocControl = axTOCControl1;
                FRR.PMapControl = axMapControl1;
                FRR.ShowDialog();
                FRR.Dispose();
                axMapControl1.Refresh();
                axTOCControl1.Refresh();
                
            }
        }
        
        private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            //还需要学习不需要安装ArcGIS的方法
            #region 修改前的代码
            //初始化代码写在了axTOCControl1_OnMouseDown里面，必须要有，否则栅格的FrmSymbolSelector出不来

            //CustomForm.FrmSymbolSelector FSS = new CustomForm.FrmSymbolSelector();
            //////需要在栅格渲染完成后，自动执行以下三行代码,如何实现自动的双击事件
            ////if (!mybo)
            ////{
            ////    FSS.Show();
            ////    FSS.Close();
            ////    FSS.Dispose();
            ////    mybo = true;
            ////    return;
            ////}

            esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap basicMap = null;
            ILayer layer = null;
            object unk = null;
            object data = null;
            axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);
            if (e.button == 1)
            {
                if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    if (layer is IFeatureLayer)
                    {
                        //取得图例
                        ILegendClass pLegendClass = ((ILegendGroup)unk).get_Class((int)data);
                        CustomForm.FrmSymbolSelector FSS1 = new CustomForm.FrmSymbolSelector(pLegendClass, layer);
                        FSS1.StartPosition = FormStartPosition.CenterParent;
                        if (FSS1.ShowDialog() == DialogResult.OK)
                        {
                            //局部更新主Map控件
                            m_mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                            //设置新的符号
                            pLegendClass.Symbol = FSS1.pSymbol;
                            //更新主Map控件和图层控件
                            this.axMapControl1.ActiveView.Refresh();
                            this.axTOCControl1.Refresh();
                        }
                    }
                    else if (layer is IRasterLayer)
                    {

                        IRasterRenderer rasterRenderer = (layer as IRasterLayer).Renderer;
                        if (rasterRenderer is IRasterUniqueValueRenderer)//唯一值
                        {
                            ILegendClass LegendClass = ((ILegendGroup)unk).get_Class((int)data);
                            CustomForm.FrmSymbolSelector FSS1 = new CustomForm.FrmSymbolSelector(LegendClass, layer, (int)data);
                            FSS1.StartPosition = FormStartPosition.CenterParent;
                            if (FSS1.ShowDialog() == DialogResult.OK)
                            {
                                axTOCControl1.Update();
                                axTOCControl1.Refresh();
                                axMapControl1.Refresh();
                            }
                        }
                        else if (rasterRenderer is IRasterStretchColorRampRenderer)//拉伸
                        {
                            //最先修改这个
                            CustomForm.FrmColorRamp FCR = new CustomForm.FrmColorRamp();
                            FCR.RasterRenderer = layer;
                            FCR.PAxMapControl = axMapControl1;
                            Control tt = sender as Control;
                            System.Drawing.Point p = new System.Drawing.Point();
                            p.X = e.x;
                            p.Y = e.y;
                            FCR.Location = tt.PointToScreen(p);
                            FCR.ShowDialog();

                            axTOCControl1.Update();
                            axTOCControl1.Refresh();
                            axMapControl1.Refresh();
                        }
                        else if (rasterRenderer is IRasterClassifyColorRampRenderer)//分类
                        {
                            ILegendClass LegendClass = ((ILegendGroup)unk).get_Class((int)data);
                            if (LegendClass == null || layer == null) MessageBox.Show("为空");
                            CustomForm.FrmSymbolSelector FSS1 = new CustomForm.FrmSymbolSelector(LegendClass, layer, (int)data);
                            FSS1.StartPosition = FormStartPosition.CenterParent;
                            if (FSS1.ShowDialog() == DialogResult.OK)
                            {
                                axTOCControl1.Update();
                                axTOCControl1.Refresh();
                                axMapControl1.Refresh();
                            }
                        }
                        else
                        {
                            MessageBox.Show("其他或无渲染");
                        }
                    }
                }
            }
            //FSS.Close();
            //FSS.Dispose();
            #endregion 修改前的代码

            #region 修改前的代码
            //CustomForm.FrmSymbolSelector FSS = new CustomForm.FrmSymbolSelector();
            ////需要在栅格渲染完成后，自动执行以下三行代码,如何实现自动的双击事件
            //if (!mybo)
            //{
            //    FSS.Show();
            //    FSS.Close();
            //    FSS.Dispose();
            //    mybo = true;
            //    return;
            //}
            //esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
            //IBasicMap basicMap = null;
            //ILayer layer = null;
            //object unk = null;
            //object data = null;
            //axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);
            //if (e.button == 1)
            //{
            //    if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
            //    {
            //        if (layer is IFeatureLayer)
            //        {
            //            //取得图例
            //            ILegendClass pLegendClass = ((ILegendGroup)unk).get_Class((int)data);

            //            ////创建符号选择器SymbolSelector实例
            //            //CustomForm.FrmSymbolSelector SymbolSelectorFrm = new CustomForm.FrmSymbolSelector(pLegendClass, layer);
            //            //SymbolSelectorFrm.StartPosition = FormStartPosition.CenterParent;
            //            //if (SymbolSelectorFrm.ShowDialog() == DialogResult.OK)
            //            //{
            //            //    //局部更新主Map控件
            //            //    m_mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            //            //    //设置新的符号
            //            //    pLegendClass.Symbol = SymbolSelectorFrm.pSymbol;
            //            //    //更新主Map控件和图层控件
            //            //    this.axMapControl1.ActiveView.Refresh();
            //            //    this.axTOCControl1.Refresh();
            //            //}
            //            CustomForm.FrmSymbolSelector FSS1 = new CustomForm.FrmSymbolSelector(pLegendClass, layer);
            //            FSS1.StartPosition = FormStartPosition.CenterParent;
            //            if (FSS1.ShowDialog() == DialogResult.OK)
            //            {
            //                //局部更新主Map控件
            //                m_mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            //                //设置新的符号
            //                pLegendClass.Symbol = FSS1.pSymbol;
            //                //更新主Map控件和图层控件
            //                this.axMapControl1.ActiveView.Refresh();
            //                this.axTOCControl1.Refresh();

            //                axTOCControl1.Update();
            //                axTOCControl1.Refresh();
            //                axMapControl1.Refresh();
            //            }
            //        }
            //        else if (layer is IRasterLayer)
            //        {

            //            IRasterRenderer rasterRenderer = (layer as IRasterLayer).Renderer;
            //            if (rasterRenderer is IRasterUniqueValueRenderer)//唯一值
            //            {
            //                ILegendClass LegendClass = ((ILegendGroup)unk).get_Class((int)data);

            //                //CustomForm.FrmSymbolSelector FSS = new CustomForm.FrmSymbolSelector(LegendClass, layer, (int)data);
            //                //FSS.StartPosition = FormStartPosition.CenterParent;
            //                //if (FSS.ShowDialog() == DialogResult.OK)
            //                //{

            //                //    axTOCControl1.Update();
            //                //    axTOCControl1.Refresh();
            //                //    axMapControl1.Refresh();
            //                //}
            //                CustomForm.FrmSymbolSelector FSS1 = new CustomForm.FrmSymbolSelector(LegendClass, layer, (int)data);
            //                FSS1.StartPosition = FormStartPosition.CenterParent;
            //                if (FSS1.ShowDialog() == DialogResult.OK)
            //                {

            //                    //axTOCControl1.Update();
            //                    //axTOCControl1.Refresh();
            //                    //axMapControl1.Refresh();
            //                }
            //            }
            //            else if (rasterRenderer is IRasterStretchColorRampRenderer)//拉伸
            //            {
            //                //最先修改这个
            //                CustomForm.FrmColorRamp FCR = new CustomForm.FrmColorRamp();
            //                FCR.RasterRenderer = layer;
            //                FCR.PAxMapControl = axMapControl1;
            //                Control tt = sender as Control;
            //                System.Drawing.Point p = new System.Drawing.Point();
            //                p.X = e.x;
            //                p.Y = e.y;
            //                FCR.Location = tt.PointToScreen(p);
            //                FCR.ShowDialog();

            //                axTOCControl1.Update();
            //                axTOCControl1.Refresh();
            //                axMapControl1.Refresh();
            //            }
            //            else if (rasterRenderer is IRasterClassifyColorRampRenderer)//分类
            //            {
            //                ILegendClass LegendClass = ((ILegendGroup)unk).get_Class((int)data);
            //                if (LegendClass == null || layer == null) MessageBox.Show("为空");
            //                CustomForm.FrmSymbolSelector FSS1 = new CustomForm.FrmSymbolSelector(LegendClass, layer, (int)data);
            //                FSS1.StartPosition = FormStartPosition.CenterParent;
            //                if (FSS1.ShowDialog() == DialogResult.OK)
            //                {
            //                    axTOCControl1.Update();
            //                    axTOCControl1.Refresh();
            //                    axMapControl1.Refresh();
            //                }
            //            }
            //            else
            //            {
            //                MessageBox.Show("其他或无渲染");
            //            }
            //        }
            //    }
            //}
            //FSS.Close();
            //FSS.Dispose();
            #endregion 修改前的代码

            #region 需要安装ArcGIS
            //esriTOCControlItem toccItem = esriTOCControlItem.esriTOCControlItemNone;
            //ILayer layer = null; IBasicMap basicMap = null; object unk = null; object data = null;//定义HitTest函数所需的参数
            //if (e.button == 1)
            //{
            //    axTOCControl1.HitTest(e.x, e.y, ref toccItem, ref basicMap, ref layer, ref unk, ref data);
            //    {
            //        if (toccItem == esriTOCControlItem.esriTOCControlItemLegendClass)
            //        {
            //            ESRI.ArcGIS.Carto.ILegendClass pLC = new LegendClassClass();//用户点击的图例
            //            ESRI.ArcGIS.Carto.ILegendGroup PLG = new LegendGroupClass();//用户点击的图例组
            //            if (unk is ILegendGroup)
            //            {
            //                PLG = (ILegendGroup)unk;
            //            }//获取图例组
            //            pLC = PLG.Class[(int)data];//获取图例组中点击的具体图例
            //            ESRI.ArcGIS.Display.ISymbol pSym;
            //            pSym = pLC.Symbol;
            //            ISymbolSelector pSS = new SymbolSelectorClass();//实例化符号选择器
            //            bool bOK = false;
            //            pSS.AddSymbol(pSym);//添加符号的目录
            //            bOK = pSS.SelectSymbol(0);
            //            if (bOK)
            //            {
            //                pLC.Symbol = pSS.GetSymbolAt(0);//利用0索引检索选中的符号
            //            }
            //            this.axMapControl1.ActiveView.Refresh();
            //            this.axTOCControl1.Refresh();
            //        }
            //    }
            //}
            #endregion 需要安装ArcGIS
        }

        private void ribbonControl_Click(object sender, EventArgs e)
        {

        }
    }
}