using Nodify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Equipment.Bqjx.StandardPlatformSystem.Controls
{
    public class LinkNode:HeaderedContentControl
    {
        private Connector? _inputConnector;
        private Connector? _outputConnector;
        static LinkNode()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkNode), new FrameworkPropertyMetadata(typeof(LinkNode)));
        }
            
       /// <summary>
       /// 输入
       /// </summary>
        public object Input
        {
            get { return (object)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Input.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputProperty =
            DependencyProperty.Register("Input", typeof(object), typeof(LinkNode), new PropertyMetadata(null));



        /// <summary>
        /// 输出
        /// </summary>
        public object Output
        {
            get { return (object)GetValue(OutputProperty); }
            set { SetValue(OutputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Output.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutputProperty =
            DependencyProperty.Register("Output", typeof(object), typeof(LinkNode), new PropertyMetadata(null));


        /// <summary>
        /// 内容背景
        /// </summary>
        public Brush ContentBrush
        {
            get { return (Brush)GetValue(ContentBrushProperty); }
            set { SetValue(ContentBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentBrushProperty =
            DependencyProperty.Register("ContentBrush", typeof(Brush), typeof(LinkNode), new PropertyMetadata(Brushes.White));



        /// <summary>
        /// 节点内容容器样式
        /// </summary>
        public Style ContentContainerStyle
        {
            get { return (Style)GetValue(ContentContainerStyleProperty); }
            set { SetValue(ContentContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentContainerStyleProperty =
            DependencyProperty.Register("ContentContainerStyle", typeof(Style), typeof(LinkNode));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _inputConnector = GetTemplateChild("PART_Input") as Connector;
            _outputConnector = GetTemplateChild("PART_Output") as Connector;
        }




        public bool IsPopupOpen
        {
            get { return (bool)GetValue(IsPopupOpenProperty); }
            set { SetValue(IsPopupOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPopupOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(LinkNode), new PropertyMetadata(false));



    }
}
