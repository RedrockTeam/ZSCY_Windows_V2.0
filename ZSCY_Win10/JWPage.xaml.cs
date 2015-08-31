using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using ZSCY_Win10.Util;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ZSCY_Win10
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class JWPage : Page
    {
        private int page = 1;
        ObservableCollection<JWList> JWList = new ObservableCollection<JWList>();
        public JWPage()
        {
            this.InitializeComponent();
            this.SizeChanged += (s, e) =>
            {
                var state = "VisualState000";
                if (e.NewSize.Width > 000)
                {
                }
                if (e.NewSize.Width > 750)
                {
                    state = "VisualState750";

                }
                VisualStateManager.GoToState(this, state, true);
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            initJWList();
        }

        //离开页面时，取消事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }
        private async void initJWList(int page = 1)
        {
            //JWListFailedStackPanel.Visibility = Visibility.Collapsed;
            //JWListProgressStackPanel.Visibility = Visibility.Visible;

            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("page", page.ToString()));
            string jw = await NetWork.getHttpWebRequest("api/jwNewsList", paramList);
            Debug.WriteLine("jw->" + jw);
            //JWListProgressStackPanel.Visibility = Visibility.Collapsed;
            if (jw != "")
            {
                JObject obj = JObject.Parse(jw);
                if (Int32.Parse(obj["status"].ToString()) == 200)
                {
                    JArray JWListArray = Utils.ReadJso(jw);
                    for (int i = 0; i < JWListArray.Count; i++)
                    {
                        int failednum = 0;
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
                            {
                                JWitem.Content = "";
                                failednum++;
                            }
                        }
                        else
                        {
                            failednum++;
                            if (failednum < 2)
                            {
                                jwContent = await NetWork.getHttpWebRequest("api/jwNewsContent", contentparamList);
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
                                    {
                                        JWitem.Content = "";
                                        failednum++;
                                    }
                                }
                            }
                        }
                        JWList.Add(new JWList { Title = JWitem.Title, Date = "时间：" + JWitem.Date, Read = "阅读量：" + JWitem.Read, Content = JWitem.Content, ID = JWitem.ID });
                        JWListView.ItemsSource = JWList;
                        //setOpacity();
                    }
                    //continueJWGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    //JWListFailedStackPanel.Visibility = Visibility.Visible;
                    //continueJWGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                //JWListFailedStackPanel.Visibility = Visibility.Visible;
                //continueJWGrid.Visibility = Visibility.Collapsed;
            }
        }

        public Frame AppFrame { get { return this.frame; } }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
        }

        private void JWRefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void JWListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
