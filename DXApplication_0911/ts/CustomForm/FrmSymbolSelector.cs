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

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Framework;

namespace ts.CustomForm
{
    public partial class FrmSymbolSelector : DevExpress.XtraEditors.XtraForm
    {
        private IStyleGalleryItem pStyleGalleryItem;
        private ILegendClass pLegendClass;
        private ILayer pLayer;
        public ISymbol pSymbol;
        public Image pSymbolImage;
        PublicFunction PF = new PublicFunction();
        int pnum = -1;

        //菜单是否已经初始化标志
        bool contextMenuMoreSymbolInitiated = false;

        public FrmSymbolSelector(ILegendClass tempLegendClass, ILayer tempLayer)
        {
            InitializeComponent();

            this.pLegendClass = tempLegendClass;
            this.pLayer = tempLayer;
        }
        public FrmSymbolSelector()
        {
            InitializeComponent();
            axSymbologyControl1.LoadStyleFile(@"E:\Program Files (x86)\ArcGIS\Engine10.4\Styles\ESRI.ServerStyle");
        }
        public FrmSymbolSelector(ILegendClass tempLegendClass, ILayer tempLayer,int num)
        {
            InitializeComponent();

            this.pnum = num;
            this.pLegendClass = tempLegendClass;
            this.pLayer = tempLayer;
        }
        private void SetFeatureClassStyle(esriSymbologyStyleClass symbologyStyleClass)/// 初始化SymbologyControl的StyleClass,图层如果已有符号,则把符号添加到SymbologyControl中的第一个符号,并选中
        {
            axSymbologyControl1.StyleClass = symbologyStyleClass;
            ISymbologyStyleClass pSymbologyStyleClass = axSymbologyControl1.GetStyleClass(symbologyStyleClass);
            if (this.pLegendClass != null)
            {
                IStyleGalleryItem currentStyleGalleryItem = new ServerStyleGalleryItem();
                currentStyleGalleryItem.Name = "当前符号";
                currentStyleGalleryItem.Item = pLegendClass.Symbol;
                pSymbologyStyleClass.AddItem(currentStyleGalleryItem, 0);
                pStyleGalleryItem = currentStyleGalleryItem;
            }
            pSymbologyStyleClass.SelectItem(0);
        }

        private void FrmSymbolSelector_Load(object sender, EventArgs e)
        {
            //载入ESRI.ServerStyle文件到SymbologyControl
            //axSymbologyControl1.LoadStyleFile(sInstall + "\\Styles\\ESRI.ServerStyle");//@"E:\Program Files (x86)\ArcGIS\Engine10.4\Styles\ESRI.ServerStyle"
            axSymbologyControl1.LoadStyleFile(@"E:\Program Files (x86)\ArcGIS\Engine10.4\Styles\ESRI.ServerStyle");

            //确定图层的类型(点线面),设置好SymbologyControl的StyleClass,设置好各控件的可见性(visible)
            if (pLayer is IFeatureLayer)
            {
                IGeoFeatureLayer pGeoFeatureLayer = (IGeoFeatureLayer)pLayer;
                switch (((IFeatureLayer)pLayer).FeatureClass.ShapeType)
                {
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                        SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassMarkerSymbols);
                        lblAngle.Visible = true;
                        nudAngle.Visible = true;
                        lblSize.Visible = true;
                        nudSize.Visible = true;
                        lblWidth.Visible = false;
                        nudWidth.Visible = false;
                        lblOutlineColor.Visible = false;
                        groupControl2.Visible = false;
                        btnOutlineColor.Visible = false;
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                        SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassLineSymbols);
                        lblAngle.Visible = false;
                        nudAngle.Visible = false;
                        lblSize.Visible = false;
                        nudSize.Visible = false;
                        lblWidth.Visible = true;
                        nudWidth.Visible = true;
                        lblOutlineColor.Visible = false;
                        groupControl2.Visible = false;
                        btnOutlineColor.Visible = false;
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                        SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols);
                        lblAngle.Visible = false;
                        nudAngle.Visible = false;
                        lblSize.Visible = false;
                        nudSize.Visible = false;
                        lblWidth.Visible = true;
                        nudWidth.Visible = true;
                        lblOutlineColor.Visible = true;
                        groupControl2.Visible = true;
                        btnOutlineColor.Visible = true;
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultiPatch:
                        SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols);
                        lblAngle.Visible = false;
                        nudAngle.Visible = false;
                        lblSize.Visible = false;
                        nudSize.Visible = false;
                        lblWidth.Visible = true;
                        nudWidth.Visible = true;
                        lblOutlineColor.Visible = true;
                        groupControl2.Visible = true;
                        btnOutlineColor.Visible = true;
                        break;
                    default:
                        this.Close();
                        break;
                }
            }
            else if (pLayer is IRasterLayer)
            {
                SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols);
                lblAngle.Visible = false;
                nudAngle.Visible = false;
                lblSize.Visible = false;
                nudSize.Visible = false;
                lblWidth.Visible = true;
                nudWidth.Visible = true;
                lblOutlineColor.Visible = true;
                groupControl2.Visible = true;
                btnOutlineColor.Visible = true;
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if (pLayer is IFeatureLayer)
            {
                //取得选定的符号
                this.pSymbol = (ISymbol)pStyleGalleryItem.Item;
                //更新预览图像
                this.pSymbolImage = pictureEdit1.Image;
            }
            else if (pLayer is IRasterLayer)
            {
                IRasterRenderer rasterRenderer = (pLayer as IRasterLayer).Renderer;
                if (rasterRenderer is IRasterClassifyColorRampRenderer)
                {
                    this.pSymbol = (ISymbol)pStyleGalleryItem.Item;
                    this.pSymbolImage = pictureEdit1.Image;

                    IRasterClassifyColorRampRenderer classifyRenderer = rasterRenderer as IRasterClassifyColorRampRenderer;
                    classifyRenderer.set_Symbol(pnum, (ISymbol)pStyleGalleryItem.Item);
                    rasterRenderer.Update();
                }
                else if (rasterRenderer is IRasterUniqueValueRenderer)
                {
                    this.pSymbol = (ISymbol)pStyleGalleryItem.Item;
                    this.pSymbolImage = pictureEdit1.Image;

                    IRasterUniqueValueRenderer uvRenderer = rasterRenderer as IRasterUniqueValueRenderer;
                    uvRenderer.set_Symbol(0,pnum, (ISymbol)pStyleGalleryItem.Item);
                    rasterRenderer.Update();
                }
            }
            
            this.Close();
            this.Dispose();
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void axSymbologyControl1_OnDoubleClick(object sender, ISymbologyControlEvents_OnDoubleClickEvent e)
        {
            simpleButton4.PerformClick();
        }
        private void PreviewImage()///把选中并设置好的符号在picturebox控件中预览
        {
            stdole.IPictureDisp picture = axSymbologyControl1.GetStyleClass(axSymbologyControl1.StyleClass).PreviewItem(pStyleGalleryItem, pictureEdit1.Width, pictureEdit1.Height);
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(new System.IntPtr(picture.Handle));
            pictureEdit1.Image = image;
        }

        private void axSymbologyControl1_OnStyleClassChanged(object sender, ISymbologyControlEvents_OnStyleClassChangedEvent e)
        {
            if (pLayer is IFeatureLayer)
            {
                switch (((ISymbologyStyleClass)e.symbologyStyleClass).StyleClass)
                {
                    case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                        lblAngle.Visible = true;
                        nudAngle.Visible = true;
                        lblSize.Visible = true;
                        nudSize.Visible = true;
                        lblWidth.Visible = false;
                        nudWidth.Visible = false;
                        lblOutlineColor.Visible = false;
                        groupControl2.Visible = false;
                        btnOutlineColor.Visible = false;
                        break;
                    case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                        lblAngle.Visible = false;
                        nudAngle.Visible = false;
                        lblSize.Visible = false;
                        nudSize.Visible = false;
                        lblWidth.Visible = true;
                        nudWidth.Visible = true;
                        lblOutlineColor.Visible = false;
                        groupControl2.Visible = false;
                        btnOutlineColor.Visible = false;
                        break;
                    case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                        lblAngle.Visible = false;
                        nudAngle.Visible = false;
                        lblSize.Visible = false;
                        nudSize.Visible = false;
                        lblWidth.Visible = true;
                        nudWidth.Visible = true;
                        lblOutlineColor.Visible = true;
                        groupControl2.Visible = true;
                        btnOutlineColor.Visible = true;
                        break;
                }
            }
            else if (pLayer is IRasterLayer)
            {
                lblAngle.Visible = false;
                nudAngle.Visible = false;
                lblSize.Visible = false;
                nudSize.Visible = false;
                lblWidth.Visible = true;
                nudWidth.Visible = true;
                lblOutlineColor.Visible = true;
                groupControl2.Visible = true;
                btnOutlineColor.Visible = true;
            }
        }

        private void axSymbologyControl1_OnItemSelected(object sender, ISymbologyControlEvents_OnItemSelectedEvent e)
        {
            pStyleGalleryItem = (IStyleGalleryItem)e.styleGalleryItem;
            Color color;
            switch (axSymbologyControl1.StyleClass)
            {
                //点符号
                case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                    color = PF.ConvertIRgbColorToColor(((IMarkerSymbol)pStyleGalleryItem.Item).Color as IRgbColor);
                    //设置点符号角度和大小初始值
                    this.nudAngle.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Angle;
                    this.nudSize.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Size;
                    break;
                //线符号
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    color = PF.ConvertIRgbColorToColor(((ILineSymbol)pStyleGalleryItem.Item).Color as IRgbColor);
                    //设置线宽初始值
                    this.nudWidth.Value = (decimal)((ILineSymbol)this.pStyleGalleryItem.Item).Width;
                    break;
                //面符号
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                    color = PF.ConvertToColor((pStyleGalleryItem.Item as IFillSymbol).Color);
                    //color = PF.ConvertIRgbColorToColor(((IFillSymbol)pStyleGalleryItem.Item).Color as IRgbColor);
                    btnOutlineColor.Appearance.BackColor = PF.ConvertIRgbColorToColor(((IFillSymbol)pStyleGalleryItem.Item).Outline.Color as IRgbColor);
                    //设置外框线宽度初始值
                    this.nudWidth.Value = (decimal)((IFillSymbol)this.pStyleGalleryItem.Item).Outline.Width;
                    break;
                default:
                    color = Color.Black;
                    break;
            }
            //设置按钮背景色
            simpleButton3.Appearance.BackColor = color;
            //预览符号
            this.PreviewImage();
        }

        private void nudSize_ValueChanged(object sender, EventArgs e)
        {
            ((IMarkerSymbol)this.pStyleGalleryItem.Item).Size = (double)this.nudSize.Value;
            this.PreviewImage();
        }

        private void nudAngle_ValueChanged(object sender, EventArgs e)
        {
            ((IMarkerSymbol)this.pStyleGalleryItem.Item).Angle = (double)this.nudAngle.Value;
            this.PreviewImage();
        }

        private void nudWidth_ValueChanged(object sender, EventArgs e)
        {
            switch (this.axSymbologyControl1.StyleClass)
            {
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    ((ILineSymbol)this.pStyleGalleryItem.Item).Width = Convert.ToDouble(this.nudWidth.Value);
                    break;
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                    //取得面符号的轮廓线符号
                    ILineSymbol pLineSymbol = ((IFillSymbol)this.pStyleGalleryItem.Item).Outline;
                    pLineSymbol.Width = Convert.ToDouble(this.nudWidth.Value);
                    ((IFillSymbol)this.pStyleGalleryItem.Item).Outline = pLineSymbol;
                    break;
            }
            this.PreviewImage();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            Control tt = sender as Control;
            IColor pColor = new RgbColor();
            pColor.RGB = 255;
            tagRECT pTag = new tagRECT();

            Point yPoint = new Point();
            yPoint.X = tt.Location.X;
            yPoint.Y = tt.Location.Y + tt.Size.Height;
            pTag.left = tt.Parent.PointToScreen(tt.Location).X;
            pTag.bottom = tt.Parent.PointToScreen(yPoint).Y;

            IColorPalette pColorPalette = new ColorPalette();
            bool b = pColorPalette.TrackPopupMenu(ref pTag, pColor, false, 0);
            if (b)
            {
                pColor = pColorPalette.Color;
                simpleButton3.Appearance.BackColor = PF.ConvertToColor(pColor);
                //m_NorthArrow.Color = pColor;
                switch (this.axSymbologyControl1.StyleClass)
                {
                    //点符号
                    case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                        ((IMarkerSymbol)this.pStyleGalleryItem.Item).Color = pColor;
                        break;
                    //线符号
                    case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                        ((ILineSymbol)this.pStyleGalleryItem.Item).Color = pColor;
                        break;
                    //面符号
                    case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                        ((IFillSymbol)this.pStyleGalleryItem.Item).Color = pColor;
                        break;
                }
                //更新符号预览
                this.PreviewImage();
            }
        }

        private void btnOutlineColor_Click(object sender, EventArgs e)
        {
            Control tt = sender as Control;
            IColor pColor = new RgbColor();
            pColor.RGB = 255;
            tagRECT pTag = new tagRECT();

            Point yPoint = new Point();
            yPoint.X = tt.Location.X;
            yPoint.Y = tt.Location.Y + tt.Size.Height;
            pTag.left = tt.Parent.PointToScreen(tt.Location).X;
            pTag.bottom = tt.Parent.PointToScreen(yPoint).Y;

            IColorPalette pColorPalette = new ColorPalette();
            bool b = pColorPalette.TrackPopupMenu(ref pTag, pColor, false, 0);
            if (b)
            {
                pColor = pColorPalette.Color;
                btnOutlineColor.Appearance.BackColor = PF.ConvertToColor(pColor);

                //取得面符号中的外框线符号
                ILineSymbol pLineSymbol = ((IFillSymbol)this.pStyleGalleryItem.Item).Outline;
                //设置外框线颜色
                pLineSymbol.Color = pColor;
                //重新设置面符号中的外框线符号
                ((IFillSymbol)this.pStyleGalleryItem.Item).Outline = pLineSymbol;

                //更新符号预览
                this.PreviewImage();
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (this.contextMenuMoreSymbolInitiated == false)
            {
                //取得菜单项数量
                string[] styleNames = System.IO.Directory.GetFiles(@"E:\Program Files (x86)\ArcGIS\Engine10.4\Styles", "*.ServerStyle");

                #region StripMenu
                //ToolStripMenuItem[] symbolContextMenuItem = new ToolStripMenuItem[styleNames.Length + 1];
                ////循环添加其它符号菜单项到菜单
                //for (int i = 0; i < styleNames.Length; i++)
                //{
                //    symbolContextMenuItem[i] = new ToolStripMenuItem();
                //    symbolContextMenuItem[i].CheckOnClick = true;
                //    symbolContextMenuItem[i].Text = System.IO.Path.GetFileNameWithoutExtension(styleNames[i]);
                //    if (symbolContextMenuItem[i].Text == "ESRI")
                //    {
                //        symbolContextMenuItem[i].Checked = true;
                //    }
                //    symbolContextMenuItem[i].Name = styleNames[i];
                //}
                ////添加“更多符号”菜单项到菜单最后一项
                //symbolContextMenuItem[styleNames.Length] = new ToolStripMenuItem();
                //symbolContextMenuItem[styleNames.Length].Text = "添加符号";
                //symbolContextMenuItem[styleNames.Length].Name = "AddMoreSymbol";

                ////添加所有的菜单项到菜单
                //this.contextMenuStrip1.Items.AddRange(symbolContextMenuItem);

                //this.contextMenuMoreSymbolInitiated = true;
                #endregion StripMenu

                //自己修改的popupMenu
                DevExpress.XtraBars.BarCheckItem[] symbolContextBarItem = new DevExpress.XtraBars.BarCheckItem[styleNames.Length + 1];//BarItem
                for (int i = 0; i < styleNames.Length; i++)
                {
                    symbolContextBarItem[i] = new DevExpress.XtraBars.BarCheckItem();
                    symbolContextBarItem[i].Caption = System.IO.Path.GetFileNameWithoutExtension(styleNames[i]);
                    if (symbolContextBarItem[i].Caption == "ESRI")
                    {
                        symbolContextBarItem[i].Checked = true;
                    }
                    symbolContextBarItem[i].Name = styleNames[i];
                };
                //添加“更多符号”菜单项到菜单最后一项
                symbolContextBarItem[styleNames.Length] = new DevExpress.XtraBars.BarCheckItem();
                symbolContextBarItem[styleNames.Length].Caption = "添加符号";
                symbolContextBarItem[styleNames.Length].Name = "AddMoreSymbol";
                //添加所有的菜单项到菜单
                popupMenu1.AddItems(symbolContextBarItem);

                this.contextMenuMoreSymbolInitiated = true;
            }
            Control tt = sender as Control;
            //显示菜单
            //this.contextMenuStrip1.Show(tt.Parent.PointToScreen(simpleButton2.Location));//contextMenuStrip
            popupMenu1.ShowPopup(barManager1, tt.Parent.PointToScreen(simpleButton2.Location));//popupMenu
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem pToolStripMenuItem = (ToolStripMenuItem)e.ClickedItem;
            //如果单击的是“添加符号”
            if (pToolStripMenuItem.Name == "AddMoreSymbol")
            {
                OpenFileDialog pOpenFileDialog = new OpenFileDialog();
                pOpenFileDialog.Title = "添加符号";
                pOpenFileDialog.Filter = "Styles 文件(*.ServerStyle)|*.ServerStyle;";//筛选器语句
                //弹出打开文件对话框
                if (pOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //导入style file到SymbologyControl
                    this.axSymbologyControl1.LoadStyleFile(pOpenFileDialog.FileName);
                    //刷新axSymbologyControl控件
                    this.axSymbologyControl1.Refresh();
                }
            }
            else//如果是其它选项
            {
                if (pToolStripMenuItem.Checked == false)
                {
                    axSymbologyControl1.LoadStyleFile(pToolStripMenuItem.Name);
                    axSymbologyControl1.Refresh();
                }
                else
                {
                    axSymbologyControl1.RemoveFile(pToolStripMenuItem.Name);
                    axSymbologyControl1.Refresh();
                }
            }          
        }

        private void barManager1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DevExpress.XtraBars.BarCheckItem pBarCheckItem = e.Item as DevExpress.XtraBars.BarCheckItem;

            //如果单击的是“添加符号”
            if (pBarCheckItem.Name == "AddMoreSymbol")
            {
                OpenFileDialog pOpenFileDialog = new OpenFileDialog();
                pOpenFileDialog.Title = "添加符号";
                pOpenFileDialog.Filter = "Styles 文件(*.ServerStyle)|*.ServerStyle;";//筛选器语句
                //弹出打开文件对话框
                if (pOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //导入style file到SymbologyControl
                    this.axSymbologyControl1.LoadStyleFile(pOpenFileDialog.FileName);
                    //刷新axSymbologyControl控件
                    this.axSymbologyControl1.Refresh();
                }
            }
            else//如果是其它选项
            {
                if (pBarCheckItem.Checked != false)//因为DevExpress是ItemClick，而WinForm是ItemClicked。此处不一样
                {
                    axSymbologyControl1.LoadStyleFile(pBarCheckItem.Name);
                    axSymbologyControl1.Refresh();
                }
                else
                {
                    axSymbologyControl1.RemoveFile(pBarCheckItem.Name);
                    axSymbologyControl1.Refresh();
                }
            }
        } 
    }
}