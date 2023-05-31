using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;

namespace ts.MapAndPage
{
    public class ControlsSynchronizer
    {
        private IMapControl3 m_mapControl = null;
        private IPageLayoutControl2 m_pageLayoutControl = null;
        private ITool m_mapActiveTool = null;
        private ITool m_pageLayoutActiveTool = null;
        private bool m_IsMapCtrlactive = true;

        private ArrayList m_frameworkControls = null;

        public ControlsSynchronizer()
        {
            //初始化ArrayList
            m_frameworkControls = new ArrayList();
        }
        public ControlsSynchronizer(IMapControl3 mapControl, IPageLayoutControl2 pageLayoutControl)
            : this()
        {
            //为类成员赋值
            m_mapControl = mapControl;
            m_pageLayoutControl = pageLayoutControl;
        }

        public IMapControl3 MapControl/// 取得或设置MapControl
        {
            get { return m_mapControl; }
            set { m_mapControl = value; }
        }
        public IPageLayoutControl2 PageLayoutControl/// 取得或设置PageLayoutControl
        {
            get { return m_pageLayoutControl; }
            set { m_pageLayoutControl = value; }
        }
        public string ActiveViewType /// 取得当前ActiveView的类型
        {
            get
            {
                if (m_IsMapCtrlactive)
                    return "MapControl";
                else
                    return "PageLayoutControl";
            }
        }
        public object ActiveControl /// 取得当前活动的Control
        {
            get
            {
                if (m_mapControl == null || m_pageLayoutControl == null)
                    throw new Exception("ControlsSynchronizer::ActiveControl:\r\nEitherMapControl or PageLayoutControl are not initialized!");

                if (m_IsMapCtrlactive)
                    return m_mapControl.Object;
                else
                    return m_pageLayoutControl.Object;
            }
        }
        public void ActivateMap() /// 激活MapControl并解除the PageLayoutControl
        {
            try
            {
                if (m_pageLayoutControl == null || m_mapControl == null)
                    throw new Exception("ControlsSynchronizer::ActivateMap:\r\nEitherMapControl or PageLayoutControl are not initialized!");

                ////缓存当前PageLayout的CurrentTool
                //if (m_pageLayoutControl.CurrentTool != null) m_pageLayoutActiveTool = m_pageLayoutControl.CurrentTool;

                //解除PagleLayout
                m_pageLayoutControl.ActiveView.Deactivate();

                //激活MapControl
                m_mapControl.ActiveView.Activate(m_mapControl.hWnd);

                ////将之前MapControl最后使用的tool，作为活动的tool，赋给MapControl的CurrentTool
                //if (m_mapActiveTool != null) m_mapControl.CurrentTool = m_mapActiveTool;

                //MapControl激活后的Tool为地图移动MapPan
                MenuSource.MapPan MP = new MenuSource.MapPan(new ControlsSynchronizer(m_mapControl, m_pageLayoutControl));
                MP.OnClick();

                m_IsMapCtrlactive = true;

                //为每一个的framework controls,设置Buddy control为MapControl
                this.SetBuddies(m_mapControl.Object);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("ControlsSynchronizer::ActivateMap:\r\n{0}", ex.Message));
            }
        }
        public void ActivatePageLayout() /// 激活PageLayoutControl并解除MapCotrol
        {
            try
            {
                if (m_pageLayoutControl == null || m_mapControl == null)
                    throw new Exception("ControlsSynchronizer::ActivatePageLayout:\r\nEitherMapControl or PageLayoutControl are not initialized!");

                ////缓存当前MapControl的CurrentTool
                //if (m_mapControl.CurrentTool != null) m_mapActiveTool = m_mapControl.CurrentTool;

                //解除MapControl
                m_mapControl.ActiveView.Deactivate();

                //激活PageLayoutControl
                m_pageLayoutControl.ActiveView.Activate(m_pageLayoutControl.hWnd);

                ////将之前PageLayoutControl最后使用的tool，作为活动的tool，赋给PageLayoutControl的CurrentTool
                //if (m_pageLayoutActiveTool != null) m_pageLayoutControl.CurrentTool = m_pageLayoutActiveTool;

                MenuSource.SelectElement SE = new MenuSource.SelectElement(new ControlsSynchronizer(m_mapControl, m_pageLayoutControl));
                SE.OnClick();

                m_IsMapCtrlactive = false;

                //为每一个的framework controls,设置Buddy control为PageLayoutControl
                this.SetBuddies(m_pageLayoutControl.Object);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("ControlsSynchronizer::ActivatePageLayout:\r\n{0}", ex.Message));
            }
        }
        public void ReplaceMap(IMap newMap)/// 给予一个地图, 置换PageLayoutControl和MapControl的focus map
        {
            if (newMap == null)
                throw new Exception("ControlsSynchronizer::ReplaceMap:\r\nNew map for replacement is not initialized!");

            if (m_pageLayoutControl == null || m_mapControl == null)
                throw new Exception("ControlsSynchronizer::ReplaceMap:\r\nEitherMapControl or PageLayoutControl are not initialized!");

            //create a new instance of IMaps collection which is needed by the PageLayout
            //创建一个PageLayout需要用到的,新的IMaps collection的实例
            IMaps maps = new Maps();
            //add the new map to the Maps collection
            //把新的地图加到Maps collection里头去
            maps.Add(newMap);

            bool bIsMapActive = m_IsMapCtrlactive;

            //call replace map on the PageLayout in order to replace the focus map
            //we must call ActivatePageLayout, since it is the control we call 'ReplaceMaps'
            //调用PageLayout的replace map来置换focus map
            //我们必须调用ActivatePageLayout,因为它是那个我们可以调用"ReplaceMaps"的Control
            this.ActivatePageLayout();
            m_pageLayoutControl.PageLayout.ReplaceMaps(maps);

            //assign the new map to the MapControl
            //把新的地图赋给MapControl
            m_mapControl.Map = newMap;

            //reset the active tools
            //重设active tools
            m_pageLayoutActiveTool = null;
            m_mapActiveTool = null;

            //make sure that the last active control is activated
            //确认之前活动的control被激活
            if (bIsMapActive)
            {
                this.ActivateMap();
                m_mapControl.ActiveView.Refresh();
            }
            else
            {
                this.ActivatePageLayout();
                m_pageLayoutControl.ActiveView.Refresh();
            }
        }
        public void BindControls(IMapControl3 mapControl, IPageLayoutControl2 pageLayoutControl, bool activateMapFirst) /// 指定共同的Map来把MapControl和PageLayoutControl绑在一起
        {
            if (mapControl == null || pageLayoutControl == null)
                throw new Exception("ControlsSynchronizer::BindControls:\r\nEitherMapControl or PageLayoutControl are not initialized!");

            m_mapControl = MapControl;
            m_pageLayoutControl = pageLayoutControl;

            this.BindControls(activateMapFirst);
        }
        public void BindControls(bool activateMapFirst)/// 指定共同的Map来把MapControl和PageLayoutControl绑在一起
        {
            if (m_pageLayoutControl == null || m_mapControl == null)
                throw new Exception("ControlsSynchronizer::BindControls:\r\nEitherMapControl or PageLayoutControl are not initialized!");

            //create a new instance of IMap
            //创造IMap的一个实例
            IMap newMap = new MapClass();
            newMap.Name = "Map";

            //create a new instance of IMaps collection which is needed by the PageLayout
            //创造一个新的IMaps collection的实例,这是PageLayout所需要的
            IMaps maps = new Maps();
            //add the new Map instance to the Maps collection
            //把新的Map实例赋给Maps collection
            maps.Add(newMap);

            //call replace map on the PageLayout in order to replace the focus map
            //调用PageLayout的replace map来置换focus map
            m_pageLayoutControl.PageLayout.ReplaceMaps(maps);
            //assign the new map to the MapControl
            //把新的map赋给MapControl
            m_mapControl.Map = newMap;

            //reset the active tools
            //重设active tools
            m_pageLayoutActiveTool = null;
            m_mapActiveTool = null;

            //make sure that the last active control is activated
            //确定最后活动的control被激活
            if (activateMapFirst)
                this.ActivateMap();
            else
                this.ActivatePageLayout();
        }
        public void AddFrameworkControl(object control)
        {
            if (control == null)
                throw new Exception("ControlsSynchronizer::AddFrameworkControl:\r\nAdded control is not initialized!");
            m_frameworkControls.Add(control);
        }
        public void RemoveFrameworkControl(object control)
        {
            if (control == null)
                throw new Exception("ControlsSynchronizer::RemoveFrameworkControl:\r\nControl tobe removed is not initialized!");

            m_frameworkControls.Remove(control);
        }
        public void RemoveFrameworkControlAt(int index)
        {
            if (m_frameworkControls.Count < index)
                throw new Exception("ControlsSynchronizer::RemoveFrameworkControlAt:\r\nIndex is out of range!");

            m_frameworkControls.RemoveAt(index);
        }
        private void SetBuddies(object buddy)
        {
            try
            {
                if (buddy == null)
                    throw new Exception("ControlsSynchronizer::SetBuddies:\r\nTarget Buddy Control is not initialized!");

                foreach (object obj in m_frameworkControls)
                {
                    if (obj is IToolbarControl)
                    {
                        ((IToolbarControl)obj).SetBuddyControl(buddy);
                    }
                    else if (obj is ITOCControl)
                    {
                        ((ITOCControl)obj).SetBuddyControl(buddy);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("ControlsSynchronizer::SetBuddies:\r\n{0}", ex.Message));
            }
        }
    }
}
