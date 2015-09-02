using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ZSCY_Win10
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
            this.SizeChanged += (s, e) =>
            {
                Debug.WriteLine(e.NewSize.Height);
                var state = "VisualState_W0H0";
                if (e.NewSize.Height > 380)
                    state = "VisualState_W0H1";
                if (e.NewSize.Height > 500)
                    state = "VisualState_W0H2";
                VisualStateManager.GoToState(this, state, true);
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //HardwareButtons.BackPressed += HardwareButtons_BackPressed;//注册重写后退按钮事件
            //UmengSDK.UmengAnalytics.TrackPageStart("AboutPage");
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //HardwareButtons.BackPressed -= HardwareButtons_BackPressed;//注册重写后退按钮事件
            //UmengSDK.UmengAnalytics.TrackPageEnd("AboutPage");
        }
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)//重写后退按钮，如果要对所有页面使用，可以放在App.Xaml.cs的APP初始化函数中重写。
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }

        }

        private async void LikeAppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("zune:reviewapp?appid=" + CurrentApp.AppId)); //用于商店app，自动获取ID
        }

        private void BackAppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
            Frame.Navigate(typeof(SettingPage));
        }
    }
}
