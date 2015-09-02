using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZSCY.Data;
using ZSCY_Win10.Common;
using ZSCY_Win10.Data;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ZSCY_Win10
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MorePage : Page
    {
        ObservableDictionary morepageclass = new ObservableDictionary();
        public MorePage()
        {
            this.InitializeComponent();
            this.SizeChanged += (s, e) =>
            {
                var state = "VisualState000";
                if (e.NewSize.Width > 000 && e.NewSize.Width < 750)
                {
                    //if (JWListView.SelectedIndex != -1)
                    //{
                    //    JWBackAppBarButton.Visibility = Visibility.Visible;
                    //}
                    //JWListView.Width = e.NewSize.Width;
                }
                if (e.NewSize.Width > 750)
                {
                    //JWBackAppBarButton.Visibility = Visibility.Collapsed;
                    //JWRefreshAppBarButton.Visibility = Visibility.Visible;
                    //JWListView.Width = 400;
                    state = "VisualState750";
                }
                VisualStateManager.GoToState(this, state, true);
                cutoffLine.Y2 = e.NewSize.Height;
            };
        }
        public ObservableDictionary Morepageclass
        {
            get
            {
                return morepageclass;
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var group = await DataSource.Get();
            this.Morepageclass["Group"] = group;
            InitMore();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private void InitMore()
        {
        }


        public Frame MoreFrame { get { return this.frame; } }

        private void MoreBackAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //if (JWFrame == null)
            //    return;
            //if (JWFrame.CanGoBack)
            //{
            //    JWFrame.GoBack();
            //}
            MoreBackAppBarButton.Visibility = Visibility.Collapsed;
            MoreFrame.Visibility = Visibility.Collapsed;
            //JWListView.SelectedIndex = -1;
        }

        private void MoreListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
