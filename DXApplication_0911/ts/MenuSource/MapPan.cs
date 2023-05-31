using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;

namespace ts.MenuSource
{
    /// <summary>
    /// Summary description for MapPan.
    /// </summary>
    [Guid("424ca507-c513-47b5-867c-cca5194ecfbf")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ts.menusource.MapPan")]
    public sealed class MapPan : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper = null;
        private MapAndPage.ControlsSynchronizer m_controlsSynchronizer = null;

        public MapPan(MapAndPage.ControlsSynchronizer controlsSynchronizer)
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Map Pan"; //localizable text 
            base.m_caption = "Map Pan";  //localizable text 
            base.m_message = "Map Pan";  //localizable text
            base.m_toolTip = "Map Pan";  //localizable text
            base.m_name = "Map Pan";   //unique id, non-localizable (e.g. "MyCategory_MyTool")

            m_controlsSynchronizer = controlsSynchronizer;
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                {
                    m_hookHelper = null;
                }
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add MapPan.OnClick implementation
            ICommand command = new ControlsMapPanTool();
            command.OnCreate(m_controlsSynchronizer.MapControl.Object);
            m_controlsSynchronizer.MapControl.CurrentTool = command as ITool;
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add MapPan.OnMouseDown implementation
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add MapPan.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add MapPan.OnMouseUp implementation
        }
        public override bool Checked
        {
            get
            {
                bool check = false;
                if (m_controlsSynchronizer.MapControl.CurrentTool == null) check = false;
                else
                {
                    if ((m_controlsSynchronizer.MapControl.CurrentTool as ICommand).Name == "ControlToolsMapNavigation_Pan") check = true;
                    else check = false;
                }
                return check;
            }
        }
        #endregion
    }
}
