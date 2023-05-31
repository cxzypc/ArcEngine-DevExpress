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
    [Guid("8cce6f86-4097-43fb-a871-3bda9cc63a97")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ts.MenuSource.ConvertToGraphics")]
    public sealed class ConvertToGraphics : BaseCommand
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
        private bool graphics = false;  //判断是否存在Convert To Graphics 的转化过程
        public ConvertToGraphics()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Convert To Graphics"; //localizable text
            base.m_caption = "Convert To Graphics";  //localizable text 
            base.m_message = "Convert To Graphics";  //localizable text
            base.m_toolTip = "Convert To Graphics";  //localizable text
            base.m_name = "Convert To Graphics";   //unique id, non-localizable (e.g. "MyCategory_MyCommand")

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
            // TODO: Add ConvertToGraphics.OnClick implementation
            IGraphicsContainer graphicsContainer = m_hookHelper.ActiveView.GraphicsContainer;
            IGraphicsContainerSelect pGraphicsContainerSelect = graphicsContainer as IGraphicsContainerSelect;

            // 判断是否有元素被选中
            IEnumElement pEnumElement = pGraphicsContainerSelect.SelectedElements;
            pEnumElement.Reset();
            element = pEnumElement.Next();
            graphics = false;
            //// Convert legend to graphics 

            IPageLayout pageLayout = m_hookHelper.PageLayout;
            IActiveView activeView = (IActiveView)pageLayout;
            while (element != null)
            {
                if (element is IMapSurroundFrame)
                {

                    IMapSurround mapSurround = ((IMapSurroundFrame)element).MapSurround;
                    if (mapSurround is ILegend)
                    {
                        graphics = true;
                        ILegend legend = (ILegend)mapSurround;
                        IGraphicsComposite graphComp = (IGraphicsComposite)legend;
                        IEnumElement enumElem = graphComp.get_Graphics(activeView.ScreenDisplay, element.Geometry.Envelope);

                        IElement pElement = enumElem.Next();
                        while (pElement != null)
                        {
                            graphicsContainer.AddElement(pElement, 0);
                            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                            pElement = enumElem.Next();
                        }
                    }
                    //else if (mapSurround is IScaleBar)
                    //{
                    //    graphics = false;
                    //    //graphics = true;
                    //    //IScaleBar scalebar = (IScaleBar)mapSurround;
                    //    //IGraphicsComposite graphComp = (IGraphicsComposite)scalebar;
                    //    //IEnumElement enumElem = graphComp.get_Graphics(activeView.ScreenDisplay, element.Geometry.Envelope);

                    //    //IElement pElement = enumElem.Next();
                    //    //while (pElement != null)
                    //    //{
                    //    //    graphicsContainer.AddElement(pElement, 0);
                    //    //    activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    //    //    pElement = enumElem.Next();
                    //    //}
                    //}
                }
                element = pEnumElement.Next();
            }

            //移除原始的Legend
            pEnumElement.Reset();
            element = pEnumElement.Next();

            if (!graphics) return;
            while (element != null)
            {
                graphicsContainer.DeleteElement(element);
                element = pEnumElement.Next();
            }

            activeView.Refresh();
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            
        }
        public override bool Enabled
        {
            get
            {
                bool enabled = false;

                IGraphicsContainer graphicsContainer = m_hookHelper.ActiveView.GraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect = graphicsContainer as IGraphicsContainerSelect;

                // 判断是否有元素被选中
                IEnumElement pEnumElement = pGraphicsContainerSelect.SelectedElements;
                pEnumElement.Reset();
                element = pEnumElement.Next();
                //// Convert legend to graphics 

                IPageLayout pageLayout = m_hookHelper.PageLayout;
                IActiveView activeView = (IActiveView)pageLayout;
                while (element != null)
                {
                    if (element is IMapSurroundFrame)
                    {

                        IMapSurround mapSurround = ((IMapSurroundFrame)element).MapSurround;
                        if (mapSurround is ILegend)
                        {
                            enabled = true;
                        }
                        //else if (mapSurround is IScaleBar)
                        //{
                        //    enabled = true;
                        //}
                    }
                    element = pEnumElement.Next();
                }
                return enabled;
            }
        }
        #endregion
    }
}
