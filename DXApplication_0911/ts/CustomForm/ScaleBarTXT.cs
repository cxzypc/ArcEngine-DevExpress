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
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;

namespace ts.CustomForm
{
    public partial class ScaleBarTXT : DevExpress.XtraEditors.XtraForm
    {
        AxPageLayoutControl pPageLayout = null;
        ITextSymbol pTextSymbol = null;
        IGraphicsContainer pGraphicsContainer = null;
        IScaleBar2 pScaleBar = null;
        IElement pElement = null;
        PublicFunction PF = new PublicFunction();
        public ScaleBarTXT(AxPageLayoutControl pageLayout, IElement selectElement)
        {
            InitializeComponent();
            pPageLayout = pageLayout;
            pGraphicsContainer = pageLayout.ActiveView.GraphicsContainer;
            IMapSurround mapSurround = ((IMapSurroundFrame)selectElement).MapSurround;
            pScaleBar = mapSurround as IScaleBar2;
            pElement = selectElement;
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxEdit1.SelectedIndex)
            {
                case 0:
                    textEdit1.Text = "Centimeters";
                    break;
                case 1:
                    textEdit1.Text = "Decimal Degrees";
                    break;
                case 2:
                    textEdit1.Text = "Decimeters";
                    break;
                case 3:
                    textEdit1.Text = "Feet";
                    break;
                case 4:
                    textEdit1.Text = "Inches";
                    break;
                case 5:
                    textEdit1.Text = "Kilometers";
                    break;
                case 6:
                    textEdit1.Text = "Meters";
                    break;
                case 7:
                    textEdit1.Text = "Miles";
                    break;
                case 8:
                    textEdit1.Text = "Millimeters";
                    break;
                case 9:
                    textEdit1.Text = "Nautical Miles";
                    break;
                case 10:
                    textEdit1.Text = "Points";
                    break;
                case 11:
                    textEdit1.Text = "Unknown Units";
                    break;
                case 12:
                    textEdit1.Text = "Yards";
                    break;
            }
            CustomChange(pScaleBar, comboBoxEdit1.SelectedIndex);
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = PF.GetFontFromIFontDisp(pTextSymbol.Font);
            
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                Font selectFont = fontDialog.Font;
                stdole.IFontDisp pFont = ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(selectFont) as stdole.IFontDisp;

                pTextSymbol.Font = pFont;//pTextSymbol.Font.Size
            }
        }

        private void ScaleBarTXT_Load(object sender, EventArgs e)
        {
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
            comboBoxEdit1.SelectedIndex = GetIndex(pScaleBar.Units);
            spinEdit1.Value = (int)pScaleBar.Divisions;
            spinEdit2.Value = (int)pScaleBar.Subdivisions;
            pTextSymbol = pScaleBar.UnitLabelSymbol;
            textEdit1.Text = pScaleBar.UnitLabel;
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
        public esriUnits GetUnits(int index)
        {
            esriUnits Units = new esriUnits();
            switch (index)
            {
                case 0:
                    Units = esriUnits.esriCentimeters;
                    break;
                case 1:
                    Units = esriUnits.esriDecimalDegrees;
                    break;
                case 2:
                    Units = esriUnits.esriDecimeters;
                    break;
                case 3:
                    Units = esriUnits.esriFeet;
                    break;
                case 4:
                    Units = esriUnits.esriInches;
                    break;
                case 5:
                    Units = esriUnits.esriKilometers;
                    break;
                case 6:
                    Units = esriUnits.esriMeters;
                    break;
                case 7:
                    Units = esriUnits.esriMiles;
                    break;
                case 8:
                    Units = esriUnits.esriMillimeters;
                    break;
                case 9:
                    Units = esriUnits.esriNauticalMiles;
                    break;
                case 10:
                    Units = esriUnits.esriPoints;
                    break;
                case 11:
                    Units = esriUnits.esriUnknownUnits;
                    break;
                case 12:
                    Units = esriUnits.esriYards;
                    break;
            }
            return Units;
        }
        public void CustomChange(IScaleBar2 pScaleBar, int num) //用户自定义的比例尺单位名称，只要修改后选取就是之后修改的名称
        {
            if (num == GetIndex(pScaleBar.Units)) textEdit1.Text = pScaleBar.UnitLabel;
        }
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            pScaleBar.Divisions = (short)spinEdit1.Value;
            pScaleBar.Subdivisions = (short)spinEdit2.Value;
            pScaleBar.Units = GetUnits(comboBoxEdit1.SelectedIndex);

            stdole.IFontDisp pFont = pTextSymbol.Font;
            pTextSymbol.Font = pFont;
            pScaleBar.UnitLabelSymbol = pTextSymbol;
            pScaleBar.UnitLabel = textEdit1.Text;

            pGraphicsContainer.UpdateElement(pElement as IElement);
            pPageLayout.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            this.Close();
            this.Dispose();
        }

        
    }
}