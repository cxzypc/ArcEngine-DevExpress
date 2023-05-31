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
using DevExpress.XtraEditors.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace ts.CustomForm
{
    public partial class FrmTitle : DevExpress.XtraEditors.XtraForm
    {
        PublicFunction PF = new PublicFunction();
        public event Action<ITextSymbol> OnQueryTitle;
        private ITextSymbol m_Title = new TextSymbolClass();
        private bool returnbool;
        
        public FrmTitle()
        {
            InitializeComponent();
        }

        private void FrmTitle_Load(object sender, EventArgs e)
        {
            simpleButton3.ButtonStyle = BorderStyles.UltraFlat;
            simpleButton3.Appearance.Options.UseBackColor = true;
            

            m_Title.Color = PF.GetRgbColor(0, 0, 0);
            simpleButton3.Appearance.BackColor = PF.ConvertToColor(m_Title.Color);
            m_Title.Size = 30;//Convert.ToDouble(spinEdit1.Value)
            spinEdit1.Value = 30;
            this.Text = "图名设置项";
        }

        private void spinEdit1_EditValueChanged(object sender, EventArgs e)
        {
            m_Title.Size = Convert.ToDouble(spinEdit1.Value);
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.YesCancel(false);
            this.Close();
            this.Dispose();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (OnQueryTitle != null)
            {
                if (textEdit1.Text != "")
                    m_Title.Text = textEdit1.Text;
                else m_Title.Text = "默认图名";
                OnQueryTitle(m_Title);
                this.YesCancel(true);
                this.Close();
                this.Dispose();
            }
        }
        public void YesCancel(bool select) {
            returnbool= select;
        }
        public  bool GetBool()
        {
            return returnbool;
        }

        private void simpleButton3_Click_1(object sender, EventArgs e)
        {
            Control tt = sender as Control;
            IColor pColor = new RgbColor();
            pColor.RGB = 255;
            tagRECT pTag = new tagRECT();

            //最开始SunnyUI
            //pTag.left = tt.PointToScreen(tt.Location).X; 
            //pTag.bottom = tt.PointToScreen(tt.Location).Y;

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
                m_Title.Color = pColor;
            }
        }
    }
}