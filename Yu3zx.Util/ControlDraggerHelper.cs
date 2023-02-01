using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Yu3zx.Util
{
    /// <summary>
    /// 可以通过拖动来改变控件的位置和大小。【来自博客园的LoveJenny】
    /// </summary>
    public static class ControlDraggerHelper
    {
        private static DraggerHelperCore helper = new DraggerHelperCore();

        public static void EnableDrag(Control control)
        {
            helper.EnableDrag(control);
        }

        public static bool IsControlCanDrag(Control control)
        {
            return helper.IsControlCanDrag(control);
        }

        public static void DisableDrag(Control control)
        {
            helper.DisableDrag(control);
        }
    }

    internal sealed class DraggerHelperCore
    {
        /// <summary>
        /// 光标状态
        /// </summary>
        private enum EnumMousePointPosition
        {
            MouseSizeNone = 0, //'无   
            MouseSizeRight = 1, //'拉伸右边框   
            MouseSizeLeft = 2, //'拉伸左边框   
            MouseSizeBottom = 3, //'拉伸下边框   
            MouseSizeTop = 4, //'拉伸上边框   
            MouseSizeTopLeft = 5, //'拉伸左上角   
            MouseSizeTopRight = 6, //'拉伸右上角   
            MouseSizeBottomLeft = 7, //'拉伸左下角   
            MouseSizeBottomRight = 8, //'拉伸右下角   
            MouseDrag = 9   // '鼠标拖动
        }

        private const int Band = 5;
        private const int MinWidth = 10;
        private const int MinHeight = 10;
        private EnumMousePointPosition m_MousePointPosition;

        internal class DragControlProperty
        {
            public Point PositionMovePoint { get; set; }

            public Point SizeChangeMovePoint { get; set; }
        }

        Dictionary<Control, DragControlProperty> controlPropertyDic = new Dictionary<Control, DragControlProperty>();

        public bool IsControlCanDrag(Control control)
        {
            return controlPropertyDic.ContainsKey(control);
        }

        public void EnableDrag(Control control)
        {
            if (!controlPropertyDic.ContainsKey(control))
            {
                controlPropertyDic.Add(control, new DragControlProperty() { });

                RegisterControlEvents(control);
            }
        }

        public void DisableDrag(Control control)
        {
            if (controlPropertyDic.ContainsKey(control))
            {
                control.MouseDown -= control_MouseDown;
                control.MouseMove -= control_MouseMove;
                control.MouseLeave -= control_MouseLeave;

                controlPropertyDic.Remove(control);
            }
        }

        private void RegisterControlEvents(Control control)
        {
            control.MouseDown += control_MouseDown;
            control.MouseMove += control_MouseMove;
            control.MouseLeave += control_MouseLeave;
        }

        void control_MouseLeave(object sender, EventArgs e)
        {
            m_MousePointPosition = EnumMousePointPosition.MouseSizeNone;
            Cursor.Current = Cursors.Arrow;
        }

        void control_MouseMove(object sender, MouseEventArgs e)
        {
            Control lCtrl = (sender as Control);
            DragControlProperty property = GetControlProperty(lCtrl);

            if (e.Button == MouseButtons.Left)
            {
                switch (m_MousePointPosition)
                {
                    case EnumMousePointPosition.MouseDrag:
                        lCtrl.Left = lCtrl.Left + e.X - property.PositionMovePoint.X;
                        lCtrl.Top = lCtrl.Top + e.Y - property.PositionMovePoint.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeBottom:
                        lCtrl.Height = lCtrl.Height + e.Y - property.SizeChangeMovePoint.Y;
                        property.SizeChangeMovePoint = e.Location; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeBottomRight:
                        lCtrl.Width = lCtrl.Width + e.X - property.SizeChangeMovePoint.X;
                        lCtrl.Height = lCtrl.Height + e.Y - property.SizeChangeMovePoint.Y;
                        property.SizeChangeMovePoint = e.Location; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeRight:
                        lCtrl.Width = lCtrl.Width + e.X - property.SizeChangeMovePoint.X;
                        //       lCtrl.Height = lCtrl.Height + e.Y - property.p1.Y;   
                        property.SizeChangeMovePoint = e.Location; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeTop:
                        lCtrl.Top = lCtrl.Top + (e.Y - property.PositionMovePoint.Y);
                        lCtrl.Height = lCtrl.Height - (e.Y - property.PositionMovePoint.Y);
                        break;
                    case EnumMousePointPosition.MouseSizeLeft:
                        lCtrl.Left = lCtrl.Left + e.X - property.PositionMovePoint.X;
                        lCtrl.Width = lCtrl.Width - (e.X - property.PositionMovePoint.X);
                        break;
                    case EnumMousePointPosition.MouseSizeBottomLeft:
                        lCtrl.Left = lCtrl.Left + e.X - property.PositionMovePoint.X;
                        lCtrl.Width = lCtrl.Width - (e.X - property.PositionMovePoint.X);
                        lCtrl.Height = lCtrl.Height + e.Y - property.SizeChangeMovePoint.Y;
                        property.SizeChangeMovePoint = e.Location; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeTopRight:
                        lCtrl.Top = lCtrl.Top + (e.Y - property.PositionMovePoint.Y);
                        lCtrl.Width = lCtrl.Width + (e.X - property.SizeChangeMovePoint.X);
                        lCtrl.Height = lCtrl.Height - (e.Y - property.PositionMovePoint.Y);
                        property.SizeChangeMovePoint = e.Location; //'记录光标拖动的当前点   
                        break;
                    case EnumMousePointPosition.MouseSizeTopLeft:
                        lCtrl.Left = lCtrl.Left + e.X - property.PositionMovePoint.X;
                        lCtrl.Top = lCtrl.Top + (e.Y - property.PositionMovePoint.Y);
                        lCtrl.Width = lCtrl.Width - (e.X - property.PositionMovePoint.X);
                        lCtrl.Height = lCtrl.Height - (e.Y - property.PositionMovePoint.Y);
                        break;
                    default:
                        break;
                }
                if (lCtrl.Width < MinWidth) lCtrl.Width = MinWidth;
                if (lCtrl.Height < MinHeight) lCtrl.Height = MinHeight;
            }
            else
            {
                m_MousePointPosition = MousePointPosition(lCtrl.Size, e);   //'判断光标的位置状态   
                switch (m_MousePointPosition)   //'改变光标   
                {
                    case EnumMousePointPosition.MouseSizeNone:
                        Cursor.Current = Cursors.Arrow;        //'箭头   
                        break;
                    case EnumMousePointPosition.MouseDrag:
                        Cursor.Current = Cursors.SizeAll;      //'四方向   
                        break;
                    case EnumMousePointPosition.MouseSizeBottom:
                        Cursor.Current = Cursors.SizeNS;       //'南北   
                        break;
                    case EnumMousePointPosition.MouseSizeTop:
                        Cursor.Current = Cursors.SizeNS;       //'南北   
                        break;
                    case EnumMousePointPosition.MouseSizeLeft:
                        Cursor.Current = Cursors.SizeWE;       //'东西   
                        break;
                    case EnumMousePointPosition.MouseSizeRight:
                        Cursor.Current = Cursors.SizeWE;       //'东西   
                        break;
                    case EnumMousePointPosition.MouseSizeBottomLeft:
                        Cursor.Current = Cursors.SizeNESW;     //'东北到南西   
                        break;
                    case EnumMousePointPosition.MouseSizeBottomRight:
                        Cursor.Current = Cursors.SizeNWSE;     //'东南到西北   
                        break;
                    case EnumMousePointPosition.MouseSizeTopLeft:
                        Cursor.Current = Cursors.SizeNWSE;     //'东南到西北   
                        break;
                    case EnumMousePointPosition.MouseSizeTopRight:
                        Cursor.Current = Cursors.SizeNESW;     //'东北到南西   
                        break;
                    default:
                        break;
                }
            }

        }

        void control_MouseDown(object sender, MouseEventArgs e)
        {
            var property = GetControlProperty(sender as Control);

            property.PositionMovePoint = e.Location;
            property.SizeChangeMovePoint = e.Location;
        }

        private DragControlProperty GetControlProperty(Control control)
        {
            return controlPropertyDic[control];
        }

        private EnumMousePointPosition MousePointPosition(Size size, System.Windows.Forms.MouseEventArgs e)
        {

            if ((e.X >= -1 * Band) | (e.X <= size.Width) | (e.Y >= -1 * Band) | (e.Y <= size.Height))
            {
                if (e.X < Band)
                {
                    if (e.Y < Band) { return EnumMousePointPosition.MouseSizeTopLeft; }
                    else
                    {
                        if (e.Y > -1 * Band + size.Height)
                        { return EnumMousePointPosition.MouseSizeBottomLeft; }
                        else
                        { return EnumMousePointPosition.MouseSizeLeft; }
                    }
                }
                else
                {
                    if (e.X > -1 * Band + size.Width)
                    {
                        if (e.Y < Band)
                        { return EnumMousePointPosition.MouseSizeTopRight; }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return EnumMousePointPosition.MouseSizeBottomRight; }
                            else
                            { return EnumMousePointPosition.MouseSizeRight; }
                        }
                    }
                    else
                    {
                        if (e.Y < Band)
                        {
                            return EnumMousePointPosition.MouseSizeTop;
                        }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            {
                                return EnumMousePointPosition.MouseSizeBottom;
                            }
                            else
                            {
                                return EnumMousePointPosition.MouseDrag;
                            }
                        }
                    }
                }
            }
            else
            {
                return EnumMousePointPosition.MouseSizeNone;
            }
        }

    }
}
