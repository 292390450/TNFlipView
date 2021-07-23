using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TNFlipView
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TNFlipView"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TNFlipView;assembly=TNFlipView"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:HorizontalFlipView/>
    ///
    /// </summary>
    public class HorizontalFlipView : Control
    {
        private HorizontalSmoothScrollViewer _scrollViewer;
        static HorizontalFlipView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HorizontalFlipView), new FrameworkPropertyMetadata(typeof(HorizontalFlipView)));
        }
        #region 依赖属性

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(HorizontalFlipView), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
            "ItemSource", typeof(IEnumerable), typeof(HorizontalFlipView), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public static readonly DependencyProperty CurrentIndexProperty = DependencyProperty.Register(
            "CurrentIndex", typeof(int), typeof(HorizontalFlipView), new PropertyMetadata(default(int)));

        public int CurrentIndex
        {
            get { return (int)GetValue(CurrentIndexProperty); }
            private set { SetValue(CurrentIndexProperty, value); }
        }
        #endregion
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //
            var items = Template.FindName("Items", this) as ItemsControl;
            _scrollViewer = Template.FindName("ScrollViewer", this) as HorizontalSmoothScrollViewer;
            _scrollViewer.OnIndexChange = (index) => { CurrentIndex = index; };
            if (items != null)
            {
                items.IsManipulationEnabled = true;
                items.ManipulationStarting += UIElement_OnManipulationStarting;
                items.ManipulationDelta += UIElement_OnManipulationDelta;
                items.ManipulationCompleted += UIElement_OnManipulationCompleted;
            }
        }
        private void UIElement_OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = _scrollViewer;
            e.Mode = ManipulationModes.TranslateX;
        }

        private void UIElement_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset - e.DeltaManipulation.Translation.X);
        }

        private void UIElement_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            double offset = (_scrollViewer.CurrentIndex) * _scrollViewer.ViewportWidth;
            if (e.FinalVelocities.LinearVelocity.X > 1)
            {
                offset = (_scrollViewer.CurrentIndex - 1) * _scrollViewer.ViewportWidth;
            }

            if (e.FinalVelocities.LinearVelocity.X < -1)
            {
                offset = (_scrollViewer.CurrentIndex + 1) * _scrollViewer.ViewportWidth;
            }

            //看看当前坐标
            _scrollViewer.AnimateScroll(offset);
        }
    }
}
