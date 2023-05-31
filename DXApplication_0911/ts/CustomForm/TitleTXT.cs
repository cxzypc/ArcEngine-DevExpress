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

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace ts.CustomForm
{
    public partial class TitleTXT : DevExpress.XtraEditors.XtraForm
    {
        AxPageLayoutControl pPageLayout = null;
        ITextElement pTextElement = null;
        ITextSymbol pTextSymbol = null;
        ICharacterOrientation pCharacterOrientation = null;
        IGraphicsContainer pGraphicsContainer = null;
        PublicFunction PF = new PublicFunction();
        public TitleTXT(AxPageLayoutControl pageLayout, ITextElement textElement)
        {
            InitializeComponent();
            pPageLayout = pageLayout;
            pTextElement = textElement;
            pTextSymbol = pTextElement.Symbol;
            pGraphicsContainer = pageLayout.ActiveView.GraphicsContainer;
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void TitleTXT_Load(object sender, EventArgs e)
        {
            textBox1.Text = pTextElement.Text;
            textEdit1.Text = pTextSymbol.Font.Name + "  " + pTextSymbol.Font.Size.ToString();
            spinEdit1.Value = Convert.ToDecimal(pTextSymbol.Angle);
            checkEdit1.Checked = pTextSymbol.Font.Bold;
            checkEdit2.Checked = pTextSymbol.Font.Italic;
            checkEdit3.Checked = pTextSymbol.Font.Underline;

            pCharacterOrientation = pTextSymbol as ICharacterOrientation;
            checkEdit4.Checked = pCharacterOrientation.CJKCharactersRotation;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = PF.GetFontFromIFontDisp(pTextSymbol.Font);
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                Font selectFont = fontDialog.Font;
                textEdit1.Text = selectFont.Name + "  " + selectFont.Size.ToString();
                stdole.IFontDisp pFont = ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(selectFont) as stdole.IFontDisp;

                pTextSymbol.Font = pFont;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            pCharacterOrientation = pTextSymbol as ICharacterOrientation;
            pCharacterOrientation.CJKCharactersRotation = checkEdit4.Checked;

            stdole.IFontDisp pFont = pTextSymbol.Font;
            pFont.Bold = checkEdit1.Checked;
            pFont.Underline = checkEdit3.Checked;
            pFont.Italic = checkEdit2.Checked;
            pTextSymbol.Font = pFont;
            pTextSymbol.Angle = Convert.ToDouble(spinEdit1.Value);

            pTextElement.Text = textBox1.Text;
            pTextElement.Symbol = pTextSymbol;

            pGraphicsContainer.UpdateElement(pTextElement as IElement);
            pPageLayout.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            this.Close();
            this.Dispose();
        }
    }
}