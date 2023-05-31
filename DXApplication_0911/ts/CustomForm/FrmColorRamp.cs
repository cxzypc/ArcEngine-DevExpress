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
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;

namespace ts.CustomForm
{
    public partial class FrmColorRamp : DevExpress.XtraEditors.XtraForm
    {
        private ISymbologyStyleClass pSymbologyStyleClass;
        private Dictionary<int, IColorRamp> colorRampDictionary;
        IRasterStretchColorRampRenderer m_RasterStretchColorRampRenderer;
        ILayer rasterRenderer;

        public ILayer RasterRenderer
        {
            get { return rasterRenderer; }
            set { rasterRenderer = value; }
        }
        AxMapControl pAxMapControl;

        public AxMapControl PAxMapControl
        {
            get { return pAxMapControl; }
            set { pAxMapControl = value; }
        }
        public FrmColorRamp()
        {
            InitializeComponent();
            
            InitSymbologyControl();
            InitColorRampCombobox();
            InitDictionary();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
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
            for (int i = 0; i < pSymbologyStyleClass.ItemCount; i++)
            {
                IStyleGalleryItem pStyleGalleryItem = pSymbologyStyleClass.GetItem(i);

                IPictureDisp pPictureDisp = pSymbologyStyleClass.PreviewItem(pStyleGalleryItem, comboBox1.Width, comboBox1.Height);
                Image image = Image.FromHbitmap(new IntPtr(pPictureDisp.Handle));
                comboBox1.Items.Add(image);
            }
            comboBox1.SelectedIndex = 0;
        }
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

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();//绘制背景
            e.DrawFocusRectangle();//绘制焦点框
            e.Graphics.DrawImage(comboBox1.Items[e.Index] as Image, e.Bounds);
        }

        private void FrmColorRamp_Load(object sender, EventArgs e)
        {
            //颜色选择还有问题，不能根据ColorRamp的值进行选择，而是根据的IRasterStretchColorRampRenderer的ColorScheme名称，进行的渲染，还有问题！！
            int indexnum;
            if (IsNumber(((RasterRenderer as IRasterLayer).Renderer as IRasterStretchColorRampRenderer).ColorScheme)) //判断是否为数字字符串
            {
                indexnum = Convert.ToInt32(((RasterRenderer as IRasterLayer).Renderer as IRasterStretchColorRampRenderer).ColorScheme);
                comboBox1.SelectedIndex = indexnum;
            }
            else
            {
                comboBox1.SelectedIndex = 21;//由黑到白的IColorRamp
            }
        }

        public static bool IsNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            const string pattern = "^[0-9]*$";
            System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(pattern);//using System.Text.RegularExpressions;
            return rx.IsMatch(s);
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            ((RasterRenderer as IRasterLayer).Renderer as IRasterStretchColorRampRenderer).ColorRamp = colorRampDictionary[comboBox1.SelectedIndex];
            ((RasterRenderer as IRasterLayer).Renderer as IRasterStretchColorRampRenderer).ColorScheme = comboBox1.SelectedIndex.ToString();
            (RasterRenderer as IRasterLayer).Renderer.Update();
            this.Close();
            //this.Dispose();
        }
        public int GetColorRampIndex(IColorRamp pColorRamp)
        {
            IRasterLayer currentLayer = PAxMapControl.Map.get_Layer(0) as IRasterLayer;
            int index = -1;
            //for (int i = 0; i < colorRampDictionary.Count; i++)
            //{
            //    ////if (pColorRamp == colorRampDictionary[i])
            //    //if (pColorRamp == colorRampDictionary[0])
            //    //{
            //    //    index = i;
            //    //    break;
            //    //}
            //    ((RasterRenderer as IRasterLayer).Renderer as IRasterStretchColorRampRenderer).ColorRamp=colorRampDictionary[i];
            //    (RasterRenderer as IRasterLayer).Renderer.Update();
            //    if (((RasterRenderer as IRasterLayer).Renderer as IRasterStretchColorRampRenderer).ColorRamp == pColorRamp)
            //    {
            //        index = i;
            //        break;
            //    }
            //}
            //if (pColorRamp != null)
            //    Console.WriteLine(pColorRamp.Name);
            //if (colorRampDictionary[0] != null)
            //{
            //    IColorRamp tt = colorRampDictionary[0];
            //    Console.WriteLine(tt.Name);
            //    MessageBox.Show("666");
            //}

            //IObjectCopy pObjectCopy = new ObjectCopyClass();
            //object copyFromLayer = currentLayer;
            //object copiedLayer = pObjectCopy.Copy(copyFromLayer);
            //IRasterLayer copyLayer=copiedLayer as IRasterLayer;
            //IRasterRenderer copyRasterRenderer = copyLayer.Renderer;

            //IRasterRenderer hh = (copiedLayer as IRasterLayer).Renderer as IRasterRenderer;

            IRasterRenderer temp = currentLayer.Renderer;
            IRasterRenderer currentRenderer = currentLayer.Renderer;
            IRasterStretchColorRampRenderer tt = currentRenderer as IRasterStretchColorRampRenderer;
            tt.ColorRamp = colorRampDictionary[8];
            currentRenderer.Update();

            Console.WriteLine(tt.ColorScheme);
            if (temp == currentRenderer) MessageBox.Show("一样");

            return index;
        }//有问题的地方，请勿删除
    }
}