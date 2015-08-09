using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using ZSCY.Common;
using ZSCY.Data;
using ZSCY.Pages;
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
        private string stuNum = "";
        //private  ObservableCollection<Group>  morepageclass=new ObservableCollection<Group>();
        private ObservableDictionary morepageclass = new ObservableDictionary();
        //private  ObservableCollection<Morepageclass> morepageclass= new ObservableCollection<Morepageclass>();
        private readonly NavigationHelper navigationHelper;

        //private string[,,] classtime = new string[7, 6,*];
        string[,][] classtime = new string[7, 6][];

        List<ClassList> classList = new List<ClassList>();
        ObservableCollection<JWList> JWList = new ObservableCollection<JWList>();

        Grid backweekgrid = new Grid();

        IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;

        public ObservableDictionary Morepageclass
        {
            get
            {
                return morepageclass;
            }
        }

        public MainPage()
        {
            appSetting = ApplicationData.Current.LocalSettings; //本地存储
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            MoreGRNameTextBlock.Text = appSetting.Values["name"].ToString();
            MoreGRClassTextBlock.Text = appSetting.Values["classNum"].ToString();
            MoreGRNumTextBlock.Text = appSetting.Values["stuNum"].ToString();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;

            stuNum = appSetting.Values["stuNum"].ToString();
            initKB();
            initJW();
        }

        private void SetKebiaoGridBorder()
        {
            //边框
            //for (int i = 0; i < kebiaoGrid.RowDefinitions.Count; i++)
            //{
            //    for (int j = 0; j < kebiaoGrid.ColumnDefinitions.Count; j++)
            //    {
            //        var border = new Border() { BorderBrush = new SolidColorBrush(Colors.LightGray), BorderThickness = new Thickness(0.5) };
            //        Grid.SetRow(border, i);
            //        Grid.SetColumn(border, j);
            //        kebiaoGrid.Children.Add(border);
            //    }
            //}

            //星期背景色
            Grid backgrid = new Grid();
            backgrid.Background = new SolidColorBrush(Color.FromArgb(255, 255, 251, 235));
            backgrid.SetValue(Grid.RowProperty, 0);
            backgrid.SetValue(Grid.ColumnProperty, (Int16.Parse(Utils.GetWeek()) + 6) % 7);
            backgrid.SetValue(Grid.RowSpanProperty, 12);
            kebiaoGrid.Children.Add(backgrid);

            backweekgrid.Background = new SolidColorBrush(Color.FromArgb(255, 255, 251, 235));
            backweekgrid.SetValue(Grid.ColumnProperty, (Int16.Parse(Utils.GetWeek()) == 0 ? 7 : Int16.Parse(Utils.GetWeek())));
            KebiaoWeekGrid.Children.Remove(backweekgrid);
            KebiaoWeekGrid.Children.Add(backweekgrid);

            TextBlock KebiaoWeek = new TextBlock();
            KebiaoWeek.Text = Utils.GetWeek(2);
            KebiaoWeek.FontSize = 20;
            KebiaoWeek.Foreground = new SolidColorBrush(Colors.Black);
            KebiaoWeek.FontWeight = FontWeights.Light;
            KebiaoWeek.VerticalAlignment = VerticalAlignment.Center;
            KebiaoWeek.HorizontalAlignment = HorizontalAlignment.Center;
            KebiaoWeek.SetValue(Grid.ColumnProperty, (Int16.Parse(Utils.GetWeek()) == 0 ? 7 : Int16.Parse(Utils.GetWeek())));
            KebiaoWeekGrid.Children.Add(KebiaoWeek);
        }

        /// <summary>
        /// 课表网络请求
        /// </summary>
        /// <param name="isRefresh"> 是否为刷新</param>
        private async void initKB(bool isRefresh = false)
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
                    HubSectionKBNum.Text = "第" + appSetting.Values["nowWeek"].ToString() + "周";
                    showKB(2);
                }
                catch (Exception) { Debug.WriteLine("主页->课表数据缓存异常"); }
            }
            if (stuNum == appSetting.Values["stuNum"].ToString())
            {
                HubSectionKBTitle.Text = "我的课表";
                HubSectionKBTitle.FontSize = 35;
            }

            await Utils.ShowSystemTrayAsync(Color.FromArgb(255, 2, 140, 253), Colors.White, text: "课表刷新中...", isIndeterminate: true);


            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("stuNum", stuNum));
            string kbtemp = await NetWork.getHttpWebRequest("redapi2/api/kebiao", paramList);
            if (kb != "" && kbtemp != "")
                kb = kbtemp;
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
                    HubSectionKBNum.Text = "第" + obj["nowWeek"].ToString() + "周";
                    //showKB(2, Int32.Parse(appSetting.Values["nowWeek"].ToString()));
#if DEBUG
                    showKB(2, 5);
#else
                    showKB(2);
#endif
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
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 6; j++)
                    classtime[i, j] = null;


            kebiaoGrid.Children.Clear();
            SetKebiaoGridBorder();
            classList.Clear();
            JArray ClassListArray = Utils.ReadJso(kb);
            int ColorI = 0;
            for (int i = 0; i < ClassListArray.Count; i++)
            {
                ClassList classitem = new ClassList();
                classitem.GetAttribute((JObject)ClassListArray[i]);
                classList.Add(classitem);
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
                        if (Array.IndexOf(classitem.Week, Int32.Parse(appSetting.Values["nowWeek"].ToString())) != -1)
                        {
                            SetClass(classitem, ClassColor);
                        }
                    }
                    else
                    {
                        if (Array.IndexOf(classitem.Week, week) != -1)
                        {
                            SetClass(classitem, ClassColor);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 课程格子的填充
        /// </summary>
        /// <param name="item">ClassList类型的item</param>
        /// <param name="ClassColor">颜色数组，0~9</param>
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

            if (classtime[item.Hash_day, item.Hash_lesson] != null)
            {
                Image img = new Image();
                img.Source = new BitmapImage(new Uri("ms-appx:///Assets/shape.png", UriKind.Absolute));
                img.VerticalAlignment = VerticalAlignment.Bottom;
                img.HorizontalAlignment = HorizontalAlignment.Right;
                img.Width = 10;
                BackGrid.Children.Add(img);

                string[] temp = classtime[item.Hash_day, item.Hash_lesson];
                string[] tempnew = new string[temp.Length + 1];
                for (int i = 0; i < temp.Length; i++)
                    tempnew[i] = temp[i];
                tempnew[temp.Length] = item._Id;
                classtime[item.Hash_day, item.Hash_lesson] = tempnew;
            }
            else
            {
                string[] tempnew = new string[1];
                tempnew[0] = item._Id;
                classtime[item.Hash_day, item.Hash_lesson] = tempnew;
            }

            BackGrid.Tapped += BackGrid_Tapped;
            kebiaoGrid.Children.Add(BackGrid);
        }

        private void BackGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("前" + KBCLassFlyoutPivot.Items.Count.ToString());
            do
            {
                KBCLassFlyoutPivot.Items.RemoveAt(0);
            }
            while (KBCLassFlyoutPivot.Items.Count.ToString() != "0");
            Debug.WriteLine("删除" + KBCLassFlyoutPivot.Items.Count.ToString());
            Grid g = sender as Grid;
            Debug.WriteLine(g.GetValue(Grid.ColumnProperty));
            Debug.WriteLine(g.GetValue(Grid.RowProperty));
            string[] temp = classtime[Int32.Parse(g.GetValue(Grid.ColumnProperty).ToString()), Int32.Parse(g.GetValue(Grid.RowProperty).ToString()) / 2];
            for (int i = 0; i < temp.Length; i++)
            {
                ClassList c = classList.Find(p => p._Id.Equals(temp[i]));

                PivotItem pi = new PivotItem();
                TextBlock HeaderTextBlock = new TextBlock();
                HeaderTextBlock.Text = c.Course;
                HeaderTextBlock.FontSize = 25;
                pi.Header = HeaderTextBlock;
                ListView lv = new ListView();
                lv.ItemTemplate = KBCLassFlyoutListView.ItemTemplate;
                List<ClassList> cc = new List<ClassList>();
                cc.Add(c);
                lv.ItemsSource = cc;
                pi.Content = lv;
                KBCLassFlyoutPivot.Items.Add(pi);
                Debug.WriteLine("后" + KBCLassFlyoutPivot.Items.Count.ToString());
            }
            KBCLassFlyout.ShowAt(MainHub);
        }

        /// <summary>
        /// 教务信息网络请求
        /// </summary>
        /// <param name="page">页码</param>
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
                    JArray JWListArray = Utils.ReadJso(jw);
                    for (int i = 0; i < JWListArray.Count; i++)
                    {
                        JWList JWitem = new JWList();
                        JWitem.GetListAttribute((JObject)JWListArray[i]);
                        List<KeyValuePair<String, String>> contentparamList = new List<KeyValuePair<String, String>>();
                        contentparamList.Add(new KeyValuePair<string, string>("id", JWitem.ID));
                        string jwContent = await NetWork.getHttpWebRequest("api/jwNewsContent", contentparamList);
                        Debug.WriteLine("jwContent->" + jwContent);
                        if (jwContent != "")
                        {
                            string JWContentText = jwContent.Replace("(\r?\n(\\s*\r?\n)+)", "\r\n");
                            while (JWContentText.StartsWith("\r\n "))
                                JWContentText = JWContentText.Substring(3);
                            while (JWContentText.StartsWith("\r\n"))
                                JWContentText = JWContentText.Substring(2);
                            JObject jwContentobj = JObject.Parse(JWContentText);
                            if (Int32.Parse(jwContentobj["status"].ToString()) == 200)
                                JWitem.Content = jwContentobj["data"]["content"].ToString();
                            else
                                JWitem.Content = "";
                        }
                        JWList.Add(new JWList { Title = JWitem.Title, Date = "时间：" + JWitem.Date, Read = "阅读量：" + JWitem.Read, Content = JWitem.Content, ID = JWitem.ID });
                        JWListView.ItemsSource = JWList;
                        setOpacity();
                    }
                    continueJWGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    JWListFailedStackPanel.Visibility = Visibility.Visible;
                    continueJWGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                JWListFailedStackPanel.Visibility = Visibility.Visible;
                continueJWGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void setOpacity()
        {
            opacityGrid.Visibility = Visibility.Visible;
            OpacityJWGrid.Begin();
            await Task.Delay(1000);
            opacityGrid.Visibility = Visibility.Collapsed;
        }

        private void JWListFailedStackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            initJW();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;//注册重写后退按钮事件
            this.navigationHelper.OnNavigatedTo(e);

        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // var group = await DataSource.Get();
            //this.Morepageclass["Group"] = group;
            more.Margin = new Thickness(0, 0, 0, -Utils.getPhoneHeight() + 300);
            //this.morepageclass = (ObservableCollection<Group>) @group;
            //this.MoreHubSection.DataContext = Morepageclass;
            //IEnumerable<Group> g =group;
            //var a=g.ToArray();
            //for (int i = 0; i < a[0].items.Count; i++)
            //{
            //    morepageclass.Add(a[0].items[i]);
            //}
            //this.MoreHubSection.DataContext = morepageclass;
            //this.fuck.ItemsSource = morepageclass;


        }
        //离开页面时，取消事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;//注册重写后退按钮事件
            this.navigationHelper.OnNavigatedFrom(e);

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
            JWList JWItem = new JWList(((JWList)e.ClickedItem).Title, ((JWList)e.ClickedItem).Date, ((JWList)e.ClickedItem).Read, ((JWList)e.ClickedItem).Content);

            this.Frame.Navigate(typeof(JWContentPage), JWItem);
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
                        MoreBlueGRGrid.Opacity = 0;

                        KBRefreshAppBarButton.Visibility = Visibility.Visible;
                        KBZoomAppBarButton.Visibility = Visibility.Visible;
                        KBCalendarAppBarButton.Visibility = Visibility.Visible;
                        JWRefreshAppBarButton.Visibility = Visibility.Collapsed;
                        MoreSwitchAppBarButton.Visibility = Visibility.Collapsed;
                        break;
                    case "JWHubSection":
                        MoreBlueGRGrid.Opacity = 0;

                        KBRefreshAppBarButton.Visibility = Visibility.Collapsed;
                        KBZoomAppBarButton.Visibility = Visibility.Collapsed;
                        KBCalendarAppBarButton.Visibility = Visibility.Collapsed;
                        JWRefreshAppBarButton.Visibility = Visibility.Visible;
                        MoreSwitchAppBarButton.Visibility = Visibility.Collapsed;
                        break;
                    case "MoreHubSection":
                        //MoreGRGrid.Margin = new Thickness(-20,0,0,0);
                        MoveMoreBlueGRGrid.Begin();

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
            stuNum = appSetting.Values["stuNum"].ToString();
            initKB(true);
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
            {
                wOa = 2;
                HubSectionKBNum.Visibility = Visibility.Collapsed;
                HubSectionKBNum.Text = "第" + appSetting.Values["nowWeek"].ToString() + "周";
            }
            else
            {
                wOa = 1;
                HubSectionKBNum.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 教务刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JWRefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            JWList.Clear();
            continueJWGrid.Visibility = Visibility.Collapsed;
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

        private void KBSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (KBZoomFlyoutTextBox.Text != "" && KBZoomFlyoutTextBox.Text.Length == 10 && KBZoomFlyoutTextBox.Text.IndexOf(".") == -1)
            {
                stuNum = KBZoomFlyoutTextBox.Text;
                HubSectionKBTitle.Text = stuNum + "的课表";
                HubSectionKBTitle.FontSize = 30;
                initKB();
                KBZoomFlyout.Hide();
            }
            else
                Utils.Message("请输入正确的学号");
        }
        private void HubSectionKBNum_Tapped(object sender, TappedRoutedEventArgs e)
        {
            KBNumFlyout.ShowAt(MainHub);
        }
        private void KBNumSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (KBNumFlyoutTextBox.Text != "" && KBNumFlyoutTextBox.Text.IndexOf(".") == -1)
            {
                showKB(2, Int16.Parse(KBNumFlyoutTextBox.Text));
                HubSectionKBNum.Text = "第" + KBNumFlyoutTextBox.Text + "周";
                KBNumFlyout.Hide();
            }
            else
                Utils.Message("请输入正确的周次");
        }


        private void continueJWGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            page++;
            initJW(page);
        }

        private void Calendar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(CalendarPage));
        }

        private void Empty_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(EmptyRoomsPage));

        }

        private void Exam_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ExamPage));

        }

        private void Score_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ScorePage));

        }

        private void ReExam_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ExamPage));

        }

        private void Setting_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingPage));

        }

        //private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    var item = e.ClickedItem as Morepageclass;
        //    Debug.WriteLine(item.UniqueID);
        //    switch (item.UniqueID)
        //    {
        //        case "Setting":
        //            Frame.Navigate(typeof(SettingPage)); break;
        //        case "ReExam": Frame.Navigate(typeof(ExamPage), 3); break;
        //        case "Exam": Frame.Navigate(typeof(ExamPage), 2); break;
        //        case "Socre": Frame.Navigate(typeof(ScorePage)); break;
        //        case "ClassRoom":
        //            Frame.Navigate(typeof(EmptyRoomsPage));
        //            break;
        //        case "Calendar":
        //            Frame.Navigate(typeof(CalendarPage));
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}
