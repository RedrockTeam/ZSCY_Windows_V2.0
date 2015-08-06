using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZSCY.Util;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace ZSCY
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ApplicationDataContainer appSetting;

        private bool isExit = false;
        private int page = 1;
        private string hubSectionChange = "KBHubSection";


        public MainPage()
        {
            appSetting = ApplicationData.Current.LocalSettings; //本地存储
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            initKB(appSetting.Values["stuNum"].ToString());
            initJW();
            SetKebiaoGridBorder();
        }

        private void SetKebiaoGridBorder()
        {
            for (int i = 0; i < kebiaoGrid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < kebiaoGrid.ColumnDefinitions.Count; j++)
                {
                    var border = new Border() { BorderBrush = new SolidColorBrush(Colors.LightGray), BorderThickness = new Thickness(0.5) };
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    kebiaoGrid.Children.Add(border);
                }
            }

            Grid BackGrid = new Grid();
            BackGrid.Background = new SolidColorBrush(Color.FromArgb(255, 132, 191, 19));
            BackGrid.SetValue(Grid.RowProperty, 2);
            BackGrid.SetValue(Grid.ColumnProperty, 3);
            BackGrid.SetValue(Grid.RowSpanProperty, 2);
            kebiaoGrid.Children.Add(BackGrid);
        }

        /// <summary>
        /// 课表网络请求
        /// </summary>
        /// <param name="stuNum"></param>
        private async void initKB(string stuNum)
        {
            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("stuNum", stuNum));
            string kb = await NetWork.getHttpWebRequest("redapi2/api/kebiao", paramList);
            Debug.WriteLine("kb->" + kb);
        }

        /// <summary>
        /// 教务信息网络请求
        /// </summary>
        /// <param name="page"></param>
        private async void initJW(int page = 1)
        {
            JWListFailedStackPanel.Visibility = Visibility.Collapsed;
            JWListProgressStackPanel.Visibility = Visibility.Visible;

            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("page", page.ToString()));
            string jw = await NetWork.getHttpWebRequest("api/jwNewsList", paramList);
            Debug.WriteLine("jw->" + jw);
            JWListProgressStackPanel.Visibility = Visibility.Collapsed;
            if (jw != "")
            {
                JObject obj = JObject.Parse(jw);
                if (Int32.Parse(obj["status"].ToString()) == 200)
                {
                }
                else
                {
                    JWListFailedStackPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                JWListFailedStackPanel.Visibility = Visibility.Visible;
            }
        }

        private void JWListFailedStackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            initJW();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;//注册重写后退按钮事件
        }

        //离开页面时，取消事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;//注册重写后退按钮事件
        }

        private async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)//重写后退按钮，如果要对所有页面使用，可以放在App.Xaml.cs的APP初始化函数中重写。
        {
            e.Handled = true;
            Color.FromArgb(255, 2, 140, 253);
            if (!isExit)
            {
                await Utils.ShowSystemTrayAsync(Color.FromArgb(255, 2, 140, 253), Colors.White, text: "再次点击返回键退出...");
                isExit = true;
                await Task.Delay(2000);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                isExit = false;
                await statusBar.ProgressIndicator.HideAsync();
            }
            else
                Application.Current.Exit();
        }

        private void JiaowuListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void MainHub_SectionsInViewChanged(object sender, SectionsInViewChangedEventArgs e)
        {
            var hubSection = MainHub.SectionsInView[0];
            Debug.WriteLine(hubSection.Name);
            CommandBar commandbar = ((CommandBar)this.BottomAppBar);
            if (hubSection.Name != hubSectionChange)
            {
                switch (hubSection.Name)
                {
                    case "KBHubSection":
                        KBRefreshAppBarButton.Visibility = Visibility.Visible;
                        KBZoomAppBarButton.Visibility = Visibility.Visible;
                        KBCalendarAppBarButton.Visibility = Visibility.Visible;
                        JWRefreshAppBarButton.Visibility = Visibility.Collapsed;
                        MoreSwitchAppBarButton.Visibility = Visibility.Collapsed;
                        break;
                    case "JWHubSection":
                        KBRefreshAppBarButton.Visibility = Visibility.Collapsed;
                        KBZoomAppBarButton.Visibility = Visibility.Collapsed;
                        KBCalendarAppBarButton.Visibility = Visibility.Collapsed;
                        JWRefreshAppBarButton.Visibility = Visibility.Visible;
                        MoreSwitchAppBarButton.Visibility = Visibility.Collapsed;
                        break;
                    case "MoreHubSection":
                        KBRefreshAppBarButton.Visibility = Visibility.Collapsed;
                        KBZoomAppBarButton.Visibility = Visibility.Collapsed;
                        KBCalendarAppBarButton.Visibility = Visibility.Collapsed;
                        JWRefreshAppBarButton.Visibility = Visibility.Collapsed;
                        MoreSwitchAppBarButton.Visibility = Visibility.Visible;
                        break;
                }
            }
            hubSectionChange = hubSection.Name;
        }

        /// <summary>
        /// 课表刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KBRefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 查询他人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KBZoomAppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 切换课表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KBCalendarAppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 教务刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JWRefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            initJW();
        }


        /// <summary>
        /// 切换账号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoreSwitchAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            appSetting.Values.Remove("idNum");
            Frame.Navigate(typeof(LoginPage));
        }

        
    }
}
