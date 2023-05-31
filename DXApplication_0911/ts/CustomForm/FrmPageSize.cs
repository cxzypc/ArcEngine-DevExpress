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
using ESRI.ArcGIS.esriSystem;

namespace ts.CustomForm
{
    public partial class FrmPageSize : DevExpress.XtraEditors.XtraForm
    {
        IPage m_Page;
        double m_Width;
        double m_Height;
        esriUnits m_Units;

        double temp_Width;
        double temp_Height;
        esriUnits temp_Units;
        public FrmPageSize(IPage pPage)
        {
            InitializeComponent();
            m_Page = pPage;
        }

        private void FrmPageSize_Load(object sender, EventArgs e)
        {
            //
            comboBoxEdit1.Properties.Items.Add("A4");
            comboBoxEdit1.Properties.Items.Add("A3");
            comboBoxEdit1.Properties.Items.Add("A2");
            comboBoxEdit1.Properties.Items.Add("A1");
            comboBoxEdit1.Properties.Items.Add("A0");
            comboBoxEdit1.Properties.Items.Add("自定义");
            //
            comboBoxEdit2.Properties.Items.Add("磅");
            comboBoxEdit2.Properties.Items.Add("英寸");
            comboBoxEdit2.Properties.Items.Add("厘米");
            comboBoxEdit2.Properties.Items.Add("毫米");
            //
            comboBoxEdit3.Properties.Items.Add("磅");
            comboBoxEdit3.Properties.Items.Add("英寸");
            comboBoxEdit3.Properties.Items.Add("厘米");
            comboBoxEdit3.Properties.Items.Add("毫米");

            m_Width = m_Page.PrintableBounds.Width;
            m_Height = m_Page.PrintableBounds.Height;
            m_Units = m_Page.Units;

            this.textEdit1.TextChanged -= new EventHandler(textEdit1_TextChanged);
            textEdit1.Text = Math.Round(m_Width,2).ToString();
            this.textEdit1.TextChanged += new EventHandler(textEdit1_TextChanged);

            this.textEdit2.TextChanged -= new EventHandler(textEdit2_TextChanged);
            textEdit2.Text = Math.Round(m_Height, 2).ToString();
            this.textEdit2.TextChanged += new EventHandler(textEdit2_TextChanged);

            comboBoxEdit2.SelectedIndex = GetIndex(m_Page.Units);

            comboBoxEdit3.SelectedIndex = GetIndex(m_Page.Units);

            radioGroup1.SelectedIndex = RadioIndex(m_Page.Orientation);
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void comboBoxEdit2_SelectedIndexChanged(object sender, EventArgs e)
        {
            esriUnits CustomUnits = GetUnits(comboBoxEdit2.SelectedIndex);
            temp_Width = UnitsChange(m_Width, m_Units, CustomUnits);

            this.textEdit1.TextChanged -= new EventHandler(textEdit1_TextChanged);
            textEdit1.Text = Math.Round(temp_Width,2).ToString();
            this.textEdit1.TextChanged += new EventHandler(textEdit1_TextChanged);
        }

        public int GetIndex(esriUnits Units)
        {
            int inx = -1;
            switch (Units)
            {
                case esriUnits.esriPoints:
                    inx = 0;
                    break;
                case esriUnits.esriInches:
                    inx = 1;
                    break;
                case esriUnits.esriCentimeters:
                    inx = 2;
                    break;
                case esriUnits.esriMillimeters:
                    inx = 3;
                    break;
            }
            return inx;
        }
        public esriUnits GetUnits(int num) {
            esriUnits Units = new esriUnits();
            switch (num)
            {
                case 0:
                    Units = esriUnits.esriPoints;
                    break;
                case 1:
                    Units = esriUnits.esriInches;
                    break;
                case 2:
                    Units = esriUnits.esriCentimeters;
                    break;
                case 3:
                    Units = esriUnits.esriMillimeters;
                    break;
            }
            return Units;
        }

        public int RadioIndex(short num)
        {
            int returnnum = -1;
            if (num == 1) returnnum = 0;
            else if (num == 2) returnnum = 1;
            return returnnum;
        }
        public short OrientationIndex(int num)
        {
            short returnnum = -1;
            if (num == 0) returnnum = 1;
            else if (num == 1) returnnum = 2;
            return returnnum;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            temp_Units = GetUnits(comboBoxEdit2.SelectedIndex);
            m_Page.Units = temp_Units;

            esriUnits HeightUnit = GetUnits(comboBoxEdit3.SelectedIndex);
            if (temp_Units != HeightUnit)
            {
                IUnitConverter pUnitConverter = new UnitConverterClass();
                temp_Height = pUnitConverter.ConvertUnits(temp_Height, HeightUnit, temp_Units);
            }
            m_Page.PutCustomSize(temp_Width, temp_Height);
            m_Page.Orientation = OrientationIndex(radioGroup1.SelectedIndex);
            this.Close();
            this.Dispose();

        }

        private void textEdit1_TextChanged(object sender, EventArgs e)
        {
            if (textEdit1.Text == "" || Convert.ToDouble(textEdit1.Text) == 0)
            {
                this.textEdit1.TextChanged -= new EventHandler(textEdit1_TextChanged);

                esriUnits CustomUnits = GetUnits(comboBoxEdit2.SelectedIndex);
                textEdit1.Text = Math.Round(UnitsChange(m_Page.PrintableBounds.Width, m_Page.Units, CustomUnits), 2).ToString();
                //m_Width = UnitsChange(m_Page.PrintableBounds.Width, m_Page.Units, CustomUnits);
                temp_Width = m_Width = m_Page.PrintableBounds.Width;

                this.textEdit1.TextChanged += new EventHandler(textEdit1_TextChanged);
                
            }
            else
            {
                //temp_Width = m_Width = Convert.ToDouble(textEdit1.Text);
                temp_Width=m_Width = UnitsChange(Convert.ToDouble(textEdit1.Text), GetUnits(comboBoxEdit2.SelectedIndex), m_Page.Units);
            } 
        }

        private void textEdit2_TextChanged(object sender, EventArgs e)
        {
           if (textEdit2.Text == "" || Convert.ToDouble(textEdit2.Text) == 0)
            {
                this.textEdit2.TextChanged -= new EventHandler(textEdit2_TextChanged);

                esriUnits CustomUnits = GetUnits(comboBoxEdit3.SelectedIndex);
                textEdit2.Text = Math.Round(UnitsChange(m_Page.PrintableBounds.Height, m_Page.Units, CustomUnits), 2).ToString();
                temp_Height=m_Height = m_Page.PrintableBounds.Height;
                this.textEdit2.TextChanged += new EventHandler(textEdit2_TextChanged);
            }
            else
            {
                temp_Height = m_Height = Convert.ToDouble(textEdit2.Text);
                temp_Height = m_Height = UnitsChange(Convert.ToDouble(textEdit2.Text), GetUnits(comboBoxEdit3.SelectedIndex), m_Page.Units);
            }
        }

        private void comboBoxEdit3_SelectedIndexChanged(object sender, EventArgs e)
        {
            esriUnits CustomUnits = GetUnits(comboBoxEdit3.SelectedIndex);
            temp_Height = UnitsChange(m_Height, m_Units, CustomUnits);

            this.textEdit2.TextChanged -= new EventHandler(textEdit2_TextChanged);
            textEdit2.Text = Math.Round(temp_Height,2).ToString();
            this.textEdit2.TextChanged += new EventHandler(textEdit2_TextChanged);
        }
        public double UnitsChange(double data,esriUnits inUnits,esriUnits outUnits){
            IUnitConverter pUnitConverter = new UnitConverterClass();
            double convertdata = pUnitConverter.ConvertUnits(data, inUnits, outUnits);
            return convertdata;
        }

        private void textEdit1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;
            //小数点的处理。
            if ((int)e.KeyChar == 46)                           //小数点
            {
                if (textEdit1.Text.Length <= 0)
                    e.Handled = true;   //小数点不能在第一位
                else
                {
                    float f;

                    float oldf;

                    bool b1 = false, b2 = false;

                    b1 = float.TryParse(textEdit1.Text, out oldf);

                    b2 = float.TryParse(textEdit1.Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }
        }

        private void textEdit2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;
            //小数点的处理。
            if ((int)e.KeyChar == 46)                           //小数点
            {
                if (textEdit2.Text.Length <= 0)
                    e.Handled = true;   //小数点不能在第一位
                else
                {
                    float f;

                    float oldf;

                    bool b1 = false, b2 = false;

                    b1 = float.TryParse(textEdit2.Text, out oldf);

                    b2 = float.TryParse(textEdit2.Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }
        }
    }

}