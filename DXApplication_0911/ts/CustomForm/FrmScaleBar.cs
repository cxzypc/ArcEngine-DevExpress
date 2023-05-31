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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace ts.CustomForm
{
    public partial class FrmScaleBar : DevExpress.XtraEditors.XtraForm
    {
        // 定义事件
        public event Action<IScaleBar2> OnQueryScaleBar;
        PublicFunction PF = new PublicFunction();

        // 样式变量
        private ISymbologyStyleClass m_SymbologyStyleClass;
        private IStyleGalleryItem m_StyleGalleryItem;
        private IScaleBar2 m_ScaleBar;
        private bool returnbool;
        public FrmScaleBar()
        {
            InitializeComponent();
        }

        private void FrmScaleBar_Load(object sender, EventArgs e)
        {
            axSymbologyControl1.LoadStyleFile(@"E:\Program Files (x86)\ArcGIS\Engine10.4\Styles\ESRI.ServerStyle");

            axSymbologyControl1.StyleClass = esriSymbologyStyleClass.esriStyleClassScaleBars;

            // 选择符号
            m_SymbologyStyleClass = axSymbologyControl1.GetStyleClass(axSymbologyControl1.StyleClass);
            m_SymbologyStyleClass.SelectItem(0);

            // 预览符号
            PriviewSymbol();
            spinEdit1.Value = (int)m_ScaleBar.Divisions;
            spinEdit2.Value = (int)m_ScaleBar.Subdivisions;
            this.Text = "比例尺设置项";

            comboBoxEdit1.Properties.Items.Add("Centimeters");
            comboBoxEdit1.Properties.Items.Add("Decimal Degrees");
            comboBoxEdit1.Properties.Items.Add("Decimeters");
            comboBoxEdit1.Properties.Items.Add("Feet");
            comboBoxEdit1.Properties.Items.Add("Inches");
            comboBoxEdit1.Properties.Items.Add("Kilometers");
            comboBoxEdit1.Properties.Items.Add("Meters");
            comboBoxEdit1.Properties.Items.Add("Miles");
            comboBoxEdit1.Properties.Items.Add("Millimeters");
            comboBoxEdit1.Properties.Items.Add("Nautical Miles");
            comboBoxEdit1.Properties.Items.Add("Points");
            comboBoxEdit1.Properties.Items.Add("Unknown Units");
            comboBoxEdit1.Properties.Items.Add("Yards");
            comboBoxEdit1.SelectedIndex = GetIndex(m_ScaleBar.Units);
        }
        private void PriviewSymbol()
        {
            IPictureDisp pPictureDisp = m_SymbologyStyleClass.PreviewItem(m_StyleGalleryItem, pictureEdit1.Width, pictureEdit1.Height);
            Image priviewImage = Image.FromHbitmap(new IntPtr(pPictureDisp.Handle));
            pictureEdit1.Image = priviewImage;
        }
        public int GetIndex(esriUnits Units)
        {
            int inx = -1;
            switch (Units)
            {
                case esriUnits.esriCentimeters:
                    inx = 0;
                    break;
                case esriUnits.esriDecimalDegrees:
                    inx = 1;
                    break;
                case esriUnits.esriDecimeters:
                    inx = 2;
                    break;
                case esriUnits.esriFeet:
                    inx = 3;
                    break;
                case esriUnits.esriInches:
                    inx = 4;
                    break;
                case esriUnits.esriKilometers:
                    inx = 5;
                    break;
                case esriUnits.esriMeters:
                    inx = 6;
                    break;
                case esriUnits.esriMiles:
                    inx = 7;
                    break;
                case esriUnits.esriMillimeters:
                    inx = 8;
                    break;
                case esriUnits.esriNauticalMiles:
                    inx = 9;
                    break;
                case esriUnits.esriPoints:
                    inx = 10;
                    break;
                case esriUnits.esriUnknownUnits:
                    inx = 11;
                    break;
                case esriUnits.esriYards:
                    inx = 12;
                    break;
            }
            return inx;
        }

        private void axSymbologyControl1_OnItemSelected(object sender, ISymbologyControlEvents_OnItemSelectedEvent e)
        {
            m_StyleGalleryItem = e.styleGalleryItem as IStyleGalleryItem;
            m_ScaleBar = m_StyleGalleryItem.Item as IScaleBar2;

            // 
            PriviewSymbol();
            comboBoxEdit1.SelectedIndex = GetIndex(m_ScaleBar.Units);
            spinEdit1.Value = (int)m_ScaleBar.Divisions;
            spinEdit2.Value = (int)m_ScaleBar.Subdivisions;
            simpleButton3.Appearance.BackColor = PF.ConvertToColor(m_ScaleBar.BarColor);
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
            pTag.left = tt.Parent.PointToScreen(tt.Location).X;//Location是相对于其父类的坐标，要求点的坐标，要确定好对应关系
            pTag.bottom = tt.Parent.PointToScreen(yPoint).Y;

            IColorPalette pColorPalette = new ColorPalette();
            bool b = pColorPalette.TrackPopupMenu(ref pTag, pColor, false, 0);
            if (b)
            {
                pColor = pColorPalette.Color;
                simpleButton3.Appearance.BackColor = PF.ConvertToColor(pColor);
                m_ScaleBar.BarColor = pColor;
            }
        }

        private void spinEdit1_EditValueChanged(object sender, EventArgs e)
        {
            m_ScaleBar.Divisions = (short)spinEdit1.Value;
            PriviewSymbol();
        }

        private void spinEdit2_EditValueChanged(object sender, EventArgs e)
        {
            m_ScaleBar.Subdivisions = (short)spinEdit2.Value;
            PriviewSymbol();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.YesCancel(false);
            this.Close();
            this.Dispose();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (OnQueryScaleBar != null)
            {
                //m_ScaleBar.BarHeight *= 3;

                OnQueryScaleBar(m_ScaleBar);
                this.YesCancel(true);
                this.Close();
                this.Dispose();
            }
        }
        public void YesCancel(bool select)
        {
            returnbool = select;
        }
        public bool GetBool()
        {
            return returnbool;
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxEdit1.SelectedIndex)
            {
                case 0:
                    m_ScaleBar.Units = esriUnits.esriCentimeters;
                    break;
                case 1:
                    m_ScaleBar.Units = esriUnits.esriDecimalDegrees;
                    break;
                case 2:
                    m_ScaleBar.Units = esriUnits.esriDecimeters;
                    break;
                case 3:
                    m_ScaleBar.Units = esriUnits.esriFeet;
                    break;
                case 4:
                    m_ScaleBar.Units = esriUnits.esriInches;
                    break;
                case 5:
                    m_ScaleBar.Units = esriUnits.esriKilometers;
                    break;
                case 6:
                    m_ScaleBar.Units = esriUnits.esriMeters;
                    break;
                case 7:
                    m_ScaleBar.Units = esriUnits.esriMiles;
                    break;
                case 8:
                    m_ScaleBar.Units = esriUnits.esriMillimeters;
                    break;
                case 9:
                    m_ScaleBar.Units = esriUnits.esriNauticalMiles;
                    break;
                case 10:
                    m_ScaleBar.Units = esriUnits.esriPoints;
                    break;
                case 11:
                    m_ScaleBar.Units = esriUnits.esriUnknownUnits;
                    break;
                case 12:
                    m_ScaleBar.Units = esriUnits.esriYards;
                    break;
            }
            PriviewSymbol();
        }
    }
}