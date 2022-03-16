using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TNFlipView
{
    public class HorizontalSmoothScrollViewer:ScrollViewer
    {
        private int _currentIndex;


        public static readonly DependencyProperty ScrollAnimationTimeProperty = DependencyProperty.Register(
          "ScrollAnimationTime", typeof(int), typeof(VerticalSmoothScrollViewer), new PropertyMetadata(200));

        /// <summary>
        /// 毫秒
        /// </summary>
        public int ScrollAnimationTime
        {
            get { return (int)GetValue(ScrollAnimationTimeProperty); }
            set { SetValue(ScrollAnimationTimeProperty, value); }
        }

        public static readonly DependencyProperty FlipAnimationTimeProperty = DependencyProperty.Register(
            "FlipAnimationTime", typeof(int), typeof(VerticalSmoothScrollViewer), new PropertyMetadata(300));

        public int FlipAnimationTime
        {
            get { return (int)GetValue(FlipAnimationTimeProperty); }
            set { SetValue(FlipAnimationTimeProperty, value); }
        }
        public static readonly DependencyProperty ScrollRatioProperty = DependencyProperty.Register(
            "ScrollRatio", typeof(double), typeof(VerticalSmoothScrollViewer), new PropertyMetadata(1.2));

        /// <summary>
        /// 滚动比
        /// </summary>
        public double ScrollRatio
        {
            get { return (double)GetValue(ScrollRatioProperty); }
            set { SetValue(ScrollRatioProperty, value); }
        }

        public static readonly DependencyProperty ScrollerBarWidthProperty = DependencyProperty.Register(
        "ScrollerBarWidth", typeof(double), typeof(VerticalSmoothScrollViewer), new PropertyMetadata(0.0, (
            (o, args) =>
            {
                if (o is VerticalSmoothScrollViewer scrollViewer)
                {
                    scrollViewer.Padding = new Thickness(0, 0, 0, (double)args.NewValue );
                }
            })));

        public double ScrollerBarWidth
        {
            get { return (double)GetValue(ScrollerBarWidthProperty); }
            set { SetValue(ScrollerBarWidthProperty, value); }
        }


        /// <summary>
        /// flip滑动时有效，只读
        /// </summary>
        public int CurrentIndex
        {
            get { return _currentIndex; }
            private set
            {
                _currentIndex = value;
                OnIndexChange?.Invoke(value);
            }
        }
        /// <summary>
        /// 按页滑动
        /// </summary>
        public bool IsFlipScroll { get; set; }
        public Action<int> OnIndexChange { get; set; }
        //记录上一次的滚动位置
        private double LastLocation = 0;

        public HorizontalSmoothScrollViewer()
        {
            //HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
           // VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        //重写鼠标滚动事件
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
           
            double WheelChange = e.Delta;
            double newOffset = 0.0;
            if (!IsFlipScroll)
            {
                newOffset = LastLocation - WheelChange * ScrollRatio;
            }
            else
            {
               
                //一次滚动一个可视范围
                if (WheelChange > 0)
                {
                    WheelChange = 1;
                }
                else
                {
                    WheelChange = -1;
                }
                newOffset = LastLocation - ViewportWidth * WheelChange;
                //确保按页滚动的对齐
                newOffset = newOffset - newOffset % ViewportWidth;
            }


            //Animation并不会改变真正的VerticalOffset(只是它的依赖属性) 所以将VOffset设置到上一次的滚动位置 (相当于衔接上一个动画)
            ScrollToHorizontalOffset(LastLocation);
            //碰到底部和顶部时的处理
            if (newOffset < 0)
                newOffset = 0;
            if (newOffset > ScrollableWidth)
                newOffset = ScrollableWidth;

            AnimateScroll(newOffset);

            //自己处理
            e.Handled = true;
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            //计算当前滚动的index
            var index = e.HorizontalOffset / e.ViewportWidth;
            int intIndex = (int)index;
            if (index - intIndex >= 0.5)
            {
                CurrentIndex = intIndex + 1;
            }
            else
            {
                CurrentIndex = intIndex;
            }
            base.OnScrollChanged(e);
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        public void AnimateScroll(double ToValue)
        {
            //为了避免重复，先结束掉上一个动画
            BeginAnimation(ScrollViewerBehavior.HorizontalOffsetProperty, null);
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            Animation.From = HorizontalOffset;
            Animation.To = ToValue;
            //动画速度
            if (IsFlipScroll)
            {
                Animation.Duration = TimeSpan.FromMilliseconds(FlipAnimationTime);
            }
            else
            {
                Animation.Duration = TimeSpan.FromMilliseconds(ScrollAnimationTime);
            }
            //考虑到性能，可以降低动画帧数
            //Timeline.SetDesiredFrameRate(Animation, 40);
            BeginAnimation(ScrollViewerBehavior.HorizontalOffsetProperty, Animation);
            LastLocation = ToValue;
        }
    }
}
