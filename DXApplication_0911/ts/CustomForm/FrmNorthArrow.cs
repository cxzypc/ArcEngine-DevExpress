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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace ts.CustomForm
{
    public partial class FrmNorthArrow : DevExpress.XtraEditors.XtraForm
    {
        // 定义事件
        public event Action<INorthArrow> OnQueryNorthArrow;
        PublicFunction PF = new PublicFunction();

        // 样式变量
        private ISymbologyStyleClass m_SymbologyStyleClass;
        private IStyleGalleryItem m_StyleGalleryItem;
        private INorthArrow m_NorthArrow;
        private bool returnbool;
        public FrmNorthArrow()
        {
            InitializeComponent();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.YesCancel(false);
            this.Close();
            this.Dispose();
        }

        private void FrmNorthArrow_Load(object sender, EventArgs e)
        {
            //////*** 这一行需要根据具体的安装进行修改
            axSymbologyControl1.LoadStyleFile(@"E:\Program Files (x86)\ArcGIS\Engine10.4\Styles\ESRI.ServerStyle");

            axSymbologyControl1.StyleClass = esriSymbologyStyleClass.esriStyleClassNorthArrows;

            // 选择符号
            m_SymbologyStyleClass = axSymbologyControl1.GetStyleClass(axSymbologyControl1.StyleClass);
            m_SymbologyStyleClass.SelectItem(0);

            // 预览符号
            PriviewSymbol();
            spinEdit1.Value = Convert.ToDecimal(m_NorthArrow.Size);
            this.Text = "指北针设置项";
        }
        private void PriviewSymbol()
        {
            IPictureDisp pPictureDisp = m_SymbologyStyleClass.PreviewItem(m_StyleGalleryItem, pictureEdit1.Width, pictureEdit1.Height);
            Image priviewImage = Image.FromHbitmap(new IntPtr(pPictureDisp.Handle));
            pictureEdit1.Image = priviewImage;
        }

        private void axSymbologyControl1_OnItemSelected(object sender, ISymbologyControlEvents_OnItemSelectedEvent e)
        {
            m_StyleGalleryItem = e.styleGalleryItem as IStyleGalleryItem;
            m_NorthArrow = m_StyleGalleryItem.Item as INorthArrow;

            // 
            PriviewSymbol();
            spinEdit1.Value = Convert.ToDecimal(m_NorthArrow.Size);
            simpleButton3.Appearance.BackColor = PF.ConvertToColor(m_NorthArrow.Color);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (OnQueryNorthArrow != null)
            {
                OnQueryNorthArrow(m_NorthArrow);
                this.YesCancel(true);
                this.Close();
                this.Dispose();
            }
        }

        private void spinEdit1_EditValueChanged(object sender, EventArgs e)
        {
            m_NorthArrow.Size = Convert.ToDouble(spinEdit1.Value);
            PriviewSymbol();
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
                m_NorthArrow.Color = pColor;
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

    }
}