using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;

namespace ts.MenuSource
{
    /// <summary>
    /// Command that works in ArcMap/Map/PageLayout
    /// </summary>
    [Guid("831da256-0218-461a-bf1c-091c073e38b4")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ts.MenuSource.FitToMargins")]
    public sealed class FitToMargins : BaseCommand
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
        private IElement element;
        IPageLayoutControl2 m_PageLayoutControl;
        public FitToMargins()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Fit To Margins"; //localizable text
            base.m_caption = "Fit To Margins";  //localizable text 
            base.m_message = "Fit To Margins";  //localizable text
            base.m_toolTip = "Fit To Margins";  //localizable text
            base.m_name = "Fit To Margins";   //unique id, non-localizable (e.g. "MyCategory_MyCommand")

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                m_PageLayoutControl = m_hookHelper.Hook as IPageLayoutControl2;
                if (m_hookHelper.ActiveView == null)
                    m_hookHelper = null;
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
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            IPageLayout pageLayout = m_PageLayoutControl.PageLayout;
            IActiveView activeView = (IActiveView)pageLayout;

            IGraphicsContainer graphicsContainer = activeView.GraphicsContainer;
            IGraphicsContainerSelect pGraphicsContainerSelect = graphicsContainer as IGraphicsContainerSelect;

            IElement element = null;
            IEnumElement pEnumElement = pGraphicsContainerSelect.SelectedElements;
            pEnumElement.Reset();
            element = pEnumElement.Next();
            IElement nn = null;

            while (element != null)
            {
                nn = element;
                element = pEnumElement.Next();
            }
            nn.Geometry = m_PageLayoutControl.PageLayout.Page.PrintableBounds;

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        //public override bool Enabled
        //{
        //    get
        //    {
        //        bool enabled = false;

        //        IGraphicsContainer graphicsContainer = m_hookHelper.ActiveView.GraphicsContainer;
        //        IGraphicsContainerSelect pGraphicsContainerSelect = graphicsContainer as IGraphicsContainerSelect;

        //        // 判断是否有元素被选中
        //        IEnumElement pEnumElement = pGraphicsContainerSelect.SelectedElements;
        //        pEnumElement.Reset();
        //        element = pEnumElement.Next();

        //        IPageLayout pageLayout = m_hookHelper.PageLayout;
        //        IActiveView activeView = (IActiveView)pageLayout;
        //        while (element != null)
        //        {
        //            enabled = true;
        //            element = pEnumElement.Next();
        //        }
        //        return enabled;
        //    }
        //}

        #endregion
    }
}
