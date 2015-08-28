using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZSCY_Win10.Util;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ZSCY_Win10
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class KBPage : Page
    {
        private string stuNum = "";
        private string kb = "";
        ApplicationDataContainer appSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
        IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
        public KBPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("OnNavigatedTo");
        }

        //离开页面时，取消事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Debug.WriteLine("OnNavigatedFrom");
        }

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
                    //HubSectionKBNum.Text = "第" + appSetting.Values["nowWeek"].ToString() + "周";
                    showKB(2);
                }
                catch (Exception) { Debug.WriteLine("主页->课表数据缓存异常"); }
            }
            if (stuNum == appSetting.Values["stuNum"].ToString())
            {
                //HubSectionKBTitle.Text = "我的课表";
                //HubSectionKBTitle.FontSize = 35;
            }

            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("stuNum", stuNum));

            string kbtemp = await NetWork.getHttpWebRequest("redapi2/api/kebiao", paramList); //新
            //string kbtemp = await NetWork.getHttpWebRequest("api/kebiao", paramList); //旧

            if (kbtemp != "")
                kb = kbtemp;
            Debug.WriteLine("kb->" + kb);
            if (kb != "")
            {
                JObject obj = JObject.Parse(kb);
                if (Int32.Parse(obj["status"].ToString()) == 200)
                {
                    IStorageFile storageFileWR = await applicationFolder.CreateFileAsync("kb", CreationCollisionOption.OpenIfExists);
                    try
                    {
                        await FileIO.WriteTextAsync(storageFileWR, kb);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("主页 -> 课表缓存，读取异常");
                    }
                    //保存当前星期
                    appSetting.Values["nowWeek"] = obj["nowWeek"].ToString();
                    //HubSectionKBNum.Text = "第" + obj["nowWeek"].ToString() + "周";
                    //showKB(2, Int32.Parse(appSetting.Values["nowWeek"].ToString()));
#if DEBUG
                    showKB(2);
#else
                    showKB(2);
#endif
                }
            }
        }

        private void showKB(int v)
        {
            throw new NotImplementedException();
        }
    }
}
