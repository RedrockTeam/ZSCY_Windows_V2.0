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
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZSCY.Data;
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
        private int wOa = 1;
        private string hubSectionChange = "KBHubSection";
        private string kb = "";

        IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;

        public MainPage()
        {
            appSetting = ApplicationData.Current.LocalSettings; //本地存储
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            MoreGRNameTextBlock.Text = appSetting.Values["name"].ToString();
            MoreGRClassTextBlock.Text = appSetting.Values["classNum"].ToString();
            MoreGRNumTextBlock.Text = appSetting.Values["stuNum"].ToString();

            initKB(appSetting.Values["stuNum"].ToString());
            initJW();
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
        }

        /// <summary>
        /// 课表网络请求
        /// </summary>
        /// <param name="stuNum"></param>
        private async void initKB(string stuNum,bool isRefresh = false)
        {
            if (stuNum == appSetting.Values["stuNum"].ToString() && !isRefresh)
            {
                try
                {
                    IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
                    IStorageFile storageFileRE = await applicationFolder.GetFileAsync("kb");
                    IRandomAccessStream accessStream = await storageFileRE.OpenReadAsync();
                    using (StreamReader streamReader = new StreamReader(accessStream.AsStreamForRead((int)accessStream.Size)))
                    {
                        kb = streamReader.ReadToEnd();
                    }
                }
                catch (Exception) { Debug.WriteLine("主页->课表数据缓存异常"); }
                showKB(2);
            }

            await Utils.ShowSystemTrayAsync(Color.FromArgb(255, 2, 140, 253), Colors.White, text: "课表刷新中...",isIndeterminate:true);


            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("stuNum", stuNum));
            kb = await NetWork.getHttpWebRequest("redapi2/api/kebiao", paramList);
            Debug.WriteLine("kb->" + kb);
            if (kb != "")
            {
                JObject obj = JObject.Parse(kb);
                if (Int32.Parse(obj["status"].ToString()) == 200)
                {
                    IStorageFile storageFileWR = await applicationFolder.CreateFileAsync("kb", CreationCollisionOption.OpenIfExists);
                    await FileIO.WriteTextAsync(storageFileWR, kb);

                    //保存当前星期
                    appSetting.Values["nowWeek"] = obj["nowWeek"].ToString();
                    //showKB(2, Int32.Parse(appSetting.Values["nowWeek"].ToString()));
                    showKB(2, 5);
                }
            }
            StatusBar statusBar = StatusBar.GetForCurrentView();
            await statusBar.ProgressIndicator.HideAsync();
        }

        /// <summary>
        /// 显示课表
        /// </summary>
        /// <param name="weekOrAll">1学期课表;2周课表</param>
        /// <param name="week">指定课表周次，默认0为本周</param>
        private void showKB(int weekOrAll = 1, int week = 0)
        {
            kebiaoGrid.Children.Clear();
            SetKebiaoGridBorder();
            JArray dateListArray = Utils.ReadJso(kb);
            int ColorI = 0;
            for (int i = 0; i < dateListArray.Count; i++)
            {
                ClassList classitem = new ClassList();
                classitem.GetAttribute((JObject)dateListArray[i]);
                int ClassColor = 0;
                if (!appSetting.Values.ContainsKey(classitem.Course))
                {
                    appSetting.Values[classitem.Course] = ColorI;
                    ClassColor = ColorI;
                    ColorI++;
                    if (ColorI > 10)
                        ColorI = 0;
                }
                else
                {
                    ClassColor = System.Int32.Parse(appSetting.Values[classitem.Course].ToString());
                }
                if (weekOrAll == 1)
                    SetClass(classitem, ClassColor);
                else
                {
                    if (week == 0)
                    {
                        if (Array.IndexOf(classitem.week, Int32.Parse(appSetting.Values["nowWeek"].ToString())) != -1)
                        {
                            SetClass(classitem, ClassColor);
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(classitem.week, week) != -1)
                        {
                            SetClass(classitem, ClassColor);
                        }
                    }
                }
            }
        }


        //课程格子的填充
        private void SetClass(ClassList item, int ClassColor)
        {
            Color[] colors = new Color[]{
                   Color.FromArgb(255,132, 191, 19),
                   Color.FromArgb(255,67, 182, 229),
                   Color.FromArgb(255,253, 137, 1),
                   Color.FromArgb(255,128, 79, 242),
                   Color.FromArgb(255,240, 68, 189),
                   Color.FromArgb(255,229, 28, 35),
                   Color.FromArgb(255,156, 39, 176),
                   Color.FromArgb(255,3, 169, 244),
                   Color.FromArgb(255,255, 193, 7),
                   Color.FromArgb(255,255, 152, 0),
                   Color.FromArgb(255,96, 125, 139),
                };

            TextBlock ClassTextBlock = new TextBlock();

            ClassTextBlock.Text = item.Course + "\n" + item.Classroom + "\n" + item.Teacher;
            ClassTextBlock.Foreground = this.Foreground;
            ClassTextBlock.FontSize = 12;
            ClassTextBlock.TextWrapping = TextWrapping.WrapWholeWords;
            ClassTextBlock.VerticalAlignment = VerticalAlignment.Center;
            ClassTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            ClassTextBlock.Margin = new Thickness(3);
            ClassTextBlock.MaxLines = 6;


            Grid BackGrid = new Grid();
            BackGrid.Background = new SolidColorBrush(colors[ClassColor]);
            BackGrid.SetValue(Grid.RowProperty, System.Int32.Parse(item.Hash_lesson * 2 + ""));
            BackGrid.SetValue(Grid.ColumnProperty, System.Int32.Parse(item.Hash_day + ""));
            BackGrid.SetValue(Grid.RowSpanProperty, System.Int32.Parse(item.Period + ""));

            BackGrid.Children.Add(ClassTextBlock);

            kebiaoGrid.Children.Add(BackGrid);
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
            initKB(appSetting.Values["stuNum"].ToString(),true);
        }

        /// <summary>
        /// 查询他人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KBZoomAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            KBZoomFlyout.ShowAt(MainHub);
        }

        /// <summary>
        /// 切换课表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KBCalendarAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            showKB(wOa);
            if (wOa == 1)
                wOa = 2;
            else
                wOa = 1;
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

        /// <summary>
        /// Flyout关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KBZoomFlyout_Closed(object sender, object e)
        {
            if (KBZoomFlyoutTextBox.Text != "" && KBZoomFlyoutTextBox.Text.Length ==10)
                initKB(KBZoomFlyoutTextBox.Text);
        }
    }
}
