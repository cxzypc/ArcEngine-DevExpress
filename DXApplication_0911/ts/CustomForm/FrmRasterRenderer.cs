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

using stdole;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;

namespace ts.CustomForm
{
    public partial class FrmRasterRenderer : DevExpress.XtraEditors.XtraForm
    {
        IRasterLayer pTocRasterLayer = null;
        PublicFunction PF = new PublicFunction();

        public IRasterLayer PTocRasterLayer
        {
            get { return pTocRasterLayer; }
            set { pTocRasterLayer = value; }
        }

        ESRI.ArcGIS.Controls.AxTOCControl pTocControl = null;

        public ESRI.ArcGIS.Controls.AxTOCControl PTocControl
        {
            get { return pTocControl; }
            set { pTocControl = value; }
        }

        ESRI.ArcGIS.Controls.AxMapControl pMapControl = null;

        public ESRI.ArcGIS.Controls.AxMapControl PMapControl
        {
            get { return pMapControl; }
            set { pMapControl = value; }
        }
        private ISymbologyStyleClass pSymbologyStyleClass;
        private Dictionary<int, IColorRamp> colorRampDictionary;
        public FrmRasterRenderer()
        {
            InitializeComponent();
            InitSymbologyControl();
            InitColorRampCombobox();
            InitDictionary();
            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBoxEdit1.SelectedIndex = 1;
            comboBoxEdit2.SelectedIndex = 3;
            xtraTabControl1.SelectedTabPageIndex=1;
        }
        private void InitSymbologyControl()
        {
            //this.axSymbologyControl1.LoadStyleFile(Application.StartupPath + "\\ESRI.ServerStyle");
            //对应Engine的安装路径
            this.axSymbologyControl1.LoadStyleFile(@"E:\Program Files (x86)\ArcGIS\Engine10.4\Styles\ESRI.ServerStyle");
            this.axSymbologyControl1.StyleClass = esriSymbologyStyleClass.esriStyleClassColorRamps;
            this.pSymbologyStyleClass = axSymbologyControl1.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
        }

        private void InitColorRampCombobox()//注意comboBoxEdit4实现了下拉色带的出现，未实现选取后显示的效果
        {
            //DevExpress.Utils.ImageCollection imageCollection = new DevExpress.Utils.ImageCollection();
            
            
            this.comboBox1.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            this.comboBox4.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;//

            this.comboBox6.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboBox6.DropDownStyle = ComboBoxStyle.DropDownList;

            //comboBoxEdit4.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

            for (int i = 0; i < pSymbologyStyleClass.ItemCount; i++)
            {
                IStyleGalleryItem pStyleGalleryItem = pSymbologyStyleClass.GetItem(i);

                IPictureDisp pPictureDisp = pSymbologyStyleClass.PreviewItem(pStyleGalleryItem, comboBox1.Width, comboBox1.Height);
                Image image = Image.FromHbitmap(new IntPtr(pPictureDisp.Handle));
                comboBox1.Items.Add(image);

                IPictureDisp pPictureDisp4 = pSymbologyStyleClass.PreviewItem(pStyleGalleryItem, comboBox4.Width, comboBox4.Height);
                Image image4 = Image.FromHbitmap(new IntPtr(pPictureDisp4.Handle));
                comboBox4.Items.Add(image4);

                IPictureDisp pPictureDisp6 = pSymbologyStyleClass.PreviewItem(pStyleGalleryItem, comboBox6.Width, comboBox6.Height);
                Image image6 = Image.FromHbitmap(new IntPtr(pPictureDisp6.Handle));
                comboBox6.Items.Add(image6);

                IPictureDisp pPictureDisp8 = pSymbologyStyleClass.PreviewItem(pStyleGalleryItem, comboBoxEdit4.Width, comboBoxEdit4.Height);
                Image image8 = Image.FromHbitmap(new IntPtr(pPictureDisp8.Handle));
                comboBoxEdit4.Properties.Items.Add(image8);      
                
            }
            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;

            //comboBoxEdit4.SelectedIndex = 0;
        }
        // 初始化字典

        private void InitDictionary()
        {
            this.colorRampDictionary = new Dictionary<int, IColorRamp>();
            for (int i = 0; i < pSymbologyStyleClass.ItemCount; i++)
            {
                IStyleGalleryItem pStyleGalleryItem = pSymbologyStyleClass.GetItem(i);
                IColorRamp pColorRamp = pStyleGalleryItem.Item as IColorRamp;
                colorRampDictionary.Add(i, pColorRamp);
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            switch (this.xtraTabControl1.SelectedTabPageIndex)
            {
                case 0:
                    IRaster raster0 = pTocRasterLayer.Raster;
                    IRasterBandCollection rasterbandcollection0 = raster0 as IRasterBandCollection;
                    IRasterBand rasterband0 = rasterbandcollection0.Item(0);
                    IRasterDataset rasterdataset0 = rasterband0 as IRasterDataset;
                    UnqueValueRenderer(rasterdataset0);
                    break;
                case 1:
                    IRaster raster1 = pTocRasterLayer.Raster;
                    IRasterBandCollection rasterbandcollection1 = raster1 as IRasterBandCollection;
                    IRasterBand rasterband1 = rasterbandcollection1.Item(0);
                    IRasterDataset rasterdataset1 = rasterband1 as IRasterDataset;
                    ClassifyRenderer(rasterdataset1);
                    break;
                case 2:
                    StretchRenderer(pTocRasterLayer);
                    break;
            }
        }

        private IColorRamp CreateColorRamp(int size) //分级渲染
        {
            bool ok = false;
            IColorRamp pColorRamp = colorRampDictionary[comboBox4.SelectedIndex];
            pColorRamp.Size = size;
            pColorRamp.CreateRamp(out ok);
            return pColorRamp;
        }
        private IColorRamp getColorRamp(int size) //唯一值渲染
        {
            bool ok = false;
            IColorRamp pColorRamp = colorRampDictionary[comboBox1.SelectedIndex];
            pColorRamp.Size = size;
            pColorRamp.CreateRamp(out ok);
            return pColorRamp;
        }

        public void ClassifyRenderer(IRasterDataset rasterDataset)  //分级渲染 没有Value
        {
            try
            {
                //Create the classify renderer.
                IRasterClassifyColorRampRenderer classifyRenderer = new
                  RasterClassifyColorRampRendererClass();
                IRasterRenderer rasterRenderer = (IRasterRenderer)classifyRenderer;

                int breaknum = Convert.ToInt32(comboBoxEdit2.Text);
                
                
                //Set up the renderer properties.
                IRaster raster = rasterDataset.CreateDefaultRaster();
                IRasterBandCollection rasterbandcollection2 = raster as IRasterBandCollection;
                IRasterBand rasterband2 = rasterbandcollection2.Item(0);
                if (rasterband2.Histogram == null)
                {
                    rasterband2.ComputeStatsAndHist();
                }

                rasterRenderer.Raster = raster;
                rasterRenderer.Update();

                //分类方法
                IClassify classify = null;

                switch (comboBoxEdit1.Text)
                {
                    case "自然断点分级":
                        classify = new NaturalBreaksClass();
                        break;
                    case "等间距分级":
                        classify = new EqualIntervalClass();
                        break;
                    case "分位数":
                        classify = new QuantileClass();
                        break;
                    case "几何间断":
                        classify = new GeometricalIntervalClass();
                        break;
                }

                classify.Classify(breaknum);
                double[] Classes = classify.ClassBreaks as double[];
                UID pUid = classify.ClassID;
                IRasterClassifyUIProperties rasClassifyUI = classifyRenderer as IRasterClassifyUIProperties;
                rasClassifyUI.ClassificationMethod = pUid;
                classifyRenderer.ClassCount = breaknum;
                
                //Set the color ramp for the symbology.

                IColorRamp pColorsRamp = CreateColorRamp(breaknum);
                //Create the symbol for the classes.
                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                ////for (int i = 0; i < classifyRenderer.ClassCount; i++)
                ////{
                ////    fillSymbol.Color = pColorsRamp.get_Color(i);
                ////    classifyRenderer.set_Symbol(i, (ISymbol)fillSymbol);
                ////    //classify后的label
                ////    //classifyRenderer.set_Label(i, Convert.ToString(i));
                ////    //classifyRenderer.set_Label(i, Classes[i].ToString("0.000") + "-" + Classes[i + 1].ToString("0.000"));
                ////}

                int gap = pColorsRamp.Size / (breaknum - 1);
                for (int i = 0; i < classifyRenderer.ClassCount; i++)
                {
                    int index;
                    if (i < classifyRenderer.ClassCount - 1)
                    {
                        index = i * gap;
                    }
                    else
                    {
                        index = pColorsRamp.Size - 1;
                    }
                    fillSymbol.Color = pColorsRamp.get_Color(index);
                    classifyRenderer.set_Symbol(i, fillSymbol as ISymbol);
                    classifyRenderer.set_Label(i, classifyRenderer.get_Break(i).ToString("0.00") + "-" + classifyRenderer.get_Break(i + 1).ToString("0.00"));
                    
                }
                classifyRenderer.set_Description(0, comboBox4.SelectedIndex.ToString());//用于获取自制色带的Index
                //
                
                pTocRasterLayer.Renderer = rasterRenderer;
                pTocControl.Update();
                pMapControl.Refresh();
                ////this.Close();
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        IColorRamp changeColorRamp;
        public void StretchRenderer(IRasterLayer pTocRasterLayer)   //拉伸着色
        {
            try
            {
                IRaster raster2 = pTocRasterLayer.Raster;
                IRasterBandCollection rasterbandcollection2 = raster2 as IRasterBandCollection;
                IRasterBand rasterband2 = rasterbandcollection2.Item(0);
                if (rasterband2.Histogram == null)
                {
                    rasterband2.ComputeStatsAndHist();
                }
                IRasterDataset rasterDataset = rasterband2 as IRasterDataset;

                //Create a stretch renderer.
                IRasterStretchColorRampRenderer stretchRenderer = new
                  RasterStretchColorRampRendererClass();
                IRasterRenderer rasterRenderer = (IRasterRenderer)stretchRenderer;
                //Set the renderer properties.
                IRaster raster = rasterDataset.CreateDefaultRaster();
                rasterRenderer.Raster = raster;
                rasterRenderer.Update();

                stretchRenderer.BandIndex = comboBox6.SelectedIndex;
                stretchRenderer.ColorRamp = colorRampDictionary[comboBox6.SelectedIndex];
                stretchRenderer.ColorScheme = comboBox6.SelectedIndex.ToString();  ///使用这个字段，进行颜色的确定

                ////Set the stretch type.
                IRasterStretch stretchType = rasterRenderer as IRasterStretch;
                switch (comboBoxEdit3.Text)
                {
                    case "无": //None
                        stretchType.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_NONE;
                        break;
                    case "自定义":
                        stretchType.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_Custom;
                        break;
                    case "标准差":
                        stretchType.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_StandardDeviations;
                        stretchType.StandardDeviationsParam = 2.5;
                        break;
                    case "直方图均衡化":
                        stretchType.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_HistogramEqualize;
                        break;
                    case "最值":
                        stretchType.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum;
                        break;
                    case "百分比截断":
                        stretchType.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_PercentMinimumMaximum;
                        break;
                }
                rasterRenderer.Update();
                pTocRasterLayer.Renderer = rasterRenderer;

                pTocControl.Update();
                pMapControl.Refresh();
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void UnqueValueRenderer(IRasterDataset rasterDataset) //唯一值
        {
            try
            {
                //Get the raster attribute table and the size of the table.
                IRaster2 raster = (IRaster2)rasterDataset.CreateDefaultRaster();
                ITable rasterTable = raster.AttributeTable;
                if (rasterTable == null)
                {
                    IRaster pRaster = rasterDataset.CreateDefaultRaster();   //定义一个栅格格式，用pRasterDataset创建一个栅格格式
                    IRasterLayer pRasterLayer = new RasterLayer();
                    pRasterLayer.CreateFromRaster(pRaster);
                    rasterTable = PF.BuildRasterTable(pRasterLayer as ILayer);
                }

                int tableRows = rasterTable.RowCount(null);

                IColorRamp colorRamp = getColorRamp(tableRows);

                //Create a unique value renderer.
                IRasterUniqueValueRenderer uvRenderer = new RasterUniqueValueRendererClass();
                IRasterRenderer rasterRenderer = (IRasterRenderer)uvRenderer;
                rasterRenderer.Raster = rasterDataset.CreateDefaultRaster();
                rasterRenderer.Update();
                uvRenderer.ColorScheme = comboBox1.SelectedIndex.ToString();

                //Set the renderer properties.
                uvRenderer.HeadingCount = 1;
                uvRenderer.set_Heading(0, "Value");
                uvRenderer.set_ClassCount(0, tableRows);
                uvRenderer.Field = "Value"; //Or any other field in the table.
                IRow row;
                ISimpleFillSymbol fillSymbol;//esriSymbologyStyleClass
                for (int i = 0; i < tableRows; i++)
                {
                    row = rasterTable.GetRow(i);
                    uvRenderer.AddValue(0, i, Convert.ToInt16(row.get_Value(1)));
                    // Assuming the raster is 8-bit.
                    uvRenderer.set_Label(0, i, Convert.ToString(row.get_Value(1)));
                    fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = colorRamp.get_Color(i);
                    uvRenderer.set_Symbol(0, i, (ISymbol)fillSymbol);
                }
                rasterRenderer.Update();
                pTocRasterLayer.Renderer = rasterRenderer;
                pTocControl.Update();

                pMapControl.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                XtraMessageBox.Show("唯一值数量已达到限制（65536）");
            }
        }

        private void FrmRasterRenderer_Load(object sender, EventArgs e)
        {
            IRasterRenderer rasterRenderer = PTocRasterLayer.Renderer;
            if (rasterRenderer is IRasterUniqueValueRenderer) //唯一值
            { 
                xtraTabControl1.SelectedTabPageIndex = 0;
                IRasterUniqueValueRenderer pRasterUniqueValueRenderer = rasterRenderer as IRasterUniqueValueRenderer;
                int indexnum;
                if (IsNumber(pRasterUniqueValueRenderer.ColorScheme))//判断是否为数字字符串
                {
                    indexnum = Convert.ToInt32(pRasterUniqueValueRenderer.ColorScheme);
                    comboBox1.SelectedIndex = indexnum;
                }
                else
                {
                    comboBox1.SelectedIndex = 0;
                } 
            }
            else if (rasterRenderer is IRasterClassifyColorRampRenderer)//分级
            {
                xtraTabControl1.SelectedTabPageIndex = 1;
                IRasterClassifyColorRampRenderer pRasterClassifyColorRampRenderer = rasterRenderer as IRasterClassifyColorRampRenderer;

                //分级方法
                IRasterClassifyUIProperties pRasterClassifyUIProperties = pRasterClassifyColorRampRenderer as IRasterClassifyUIProperties;
                UID pUID = pRasterClassifyUIProperties.ClassificationMethod;
                IClassify classify1 = new NaturalBreaksClass();     //"自然断点分级"
                if (classify1.ClassID.Value.ToString() == pUID.Value.ToString()) comboBoxEdit1.SelectedIndex = 0;
                IClassify classify0 = new EqualIntervalClass();     //"等间距分级"
                if (classify0.ClassID.Value.ToString() == pUID.Value.ToString()) comboBoxEdit1.SelectedIndex = 0;
                IClassify classify2 = new QuantileClass();          //"分位数"
                if (classify2.ClassID.Value.ToString() == pUID.Value.ToString()) comboBoxEdit1.SelectedIndex = 2;
                IClassify classify3 = new GeometricalIntervalClass(); //"几何间断"
                if (classify3.ClassID.Value.ToString() == pUID.Value.ToString()) comboBoxEdit1.SelectedIndex = 3;
                //分级个数
                int ClassCount = pRasterClassifyColorRampRenderer.ClassCount;
                comboBoxEdit2.SelectedIndex = ClassCount - 1;
                //分级色带
                Console.WriteLine(pRasterClassifyColorRampRenderer.get_Description(0));
                int indexnum;
                if (IsNumber(pRasterClassifyColorRampRenderer.get_Description(0)))//判断是否为数字字符串
                {
                    indexnum = Convert.ToInt32(pRasterClassifyColorRampRenderer.get_Description(0));
                    comboBox4.SelectedIndex = indexnum;
                }
                else
                {
                    comboBox4.SelectedIndex = 0;
                } 
            }
            else if (rasterRenderer is IRasterStretchColorRampRenderer)//拉伸
            {
                xtraTabControl1.SelectedTabPageIndex = 2;
                IRasterStretchColorRampRenderer pRasterStretchColorRampRenderer = rasterRenderer as IRasterStretchColorRampRenderer;

                int indexnum;
                if (IsNumber(pRasterStretchColorRampRenderer.ColorScheme))//判断是否为数字字符串
                {
                    indexnum = Convert.ToInt32(pRasterStretchColorRampRenderer.ColorScheme);
                    comboBox6.SelectedIndex = indexnum;
                }
                else
                {
                    comboBox6.SelectedIndex = 21;//由黑到白的IColorRamp
                }

                IRasterStretch pRasterStretch = rasterRenderer as IRasterStretch;
                switch (pRasterStretch.StretchType) 
                {
                    case esriRasterStretchTypesEnum.esriRasterStretch_NONE:
                        comboBoxEdit3.SelectedIndex = 0;
                        break;
                    case esriRasterStretchTypesEnum.esriRasterStretch_Custom:
                        comboBoxEdit3.SelectedIndex = 1;
                        break;
                    case esriRasterStretchTypesEnum.esriRasterStretch_StandardDeviations:
                        comboBoxEdit3.SelectedIndex = 2;
                        break;
                    case esriRasterStretchTypesEnum.esriRasterStretch_HistogramEqualize:
                        comboBoxEdit3.SelectedIndex = 3;
                        break;
                    case esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum:
                        comboBoxEdit3.SelectedIndex = 4;
                        break;
                    case esriRasterStretchTypesEnum.esriRasterStretch_PercentMinimumMaximum:
                        comboBoxEdit3.SelectedIndex = 5;
                        break;
                }
            }
        }
        public static bool IsNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            const string pattern = "^[0-9]*$";
            System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(pattern);//using System.Text.RegularExpressions;
            return rx.IsMatch(s);
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();//绘制背景
            e.DrawFocusRectangle();//绘制焦点框
            //绘制图例
            //Rectangle iRectangle = new Rectangle(e.Bounds.Left, e.Bounds.Top, 215, 27);
            ////Bitmap getBitmap = new Bitmap(imageList1.Images[e.Index]);
            e.Graphics.DrawImage(comboBox1.Items[e.Index] as Image, e.Bounds);
        }

        private void comboBox4_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();//绘制背景
            e.DrawFocusRectangle();//绘制焦点框
            //绘制图例
            //Rectangle iRectangle = new Rectangle(e.Bounds.Left, e.Bounds.Top, 215, 27);
            ////Bitmap getBitmap = new Bitmap(imageList1.Images[e.Index]);
            e.Graphics.DrawImage(comboBox4.Items[e.Index] as Image, e.Bounds);
        }

        private void comboBox6_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();//绘制背景
            e.DrawFocusRectangle();//绘制焦点框
            //绘制图例
            //Rectangle iRectangle = new Rectangle(e.Bounds.Left, e.Bounds.Top, 215, 27);
            ////Bitmap getBitmap = new Bitmap(imageList1.Images[e.Index]);
            e.Graphics.DrawImage(comboBox6.Items[e.Index] as Image, e.Bounds);
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }


        private void comboBoxEdit4_DrawItem(object sender, ListBoxDrawItemEventArgs e)
        {
            e.DefaultDraw();//绘制背景

            //e.DrawFocusRectangle();//绘制焦点框
            //绘制图例
            //Rectangle iRectangle = new Rectangle(e.Bounds.Left, e.Bounds.Top, 215, 27);
            ////Bitmap getBitmap = new Bitmap(imageList1.Images[e.Index]);

            e.Graphics.DrawImage(comboBoxEdit4.Properties.Items[e.Index] as Image, e.Bounds);
        }
    }
}