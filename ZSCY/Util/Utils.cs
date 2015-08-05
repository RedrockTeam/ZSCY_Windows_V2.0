﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ZSCY.Util
{
    internal class Utils
    {

        /// <summary>
        /// Toast
        /// </summary>
        /// <param name="text"></param>
        public static async void Toast(string text)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            XmlNodeList elements = toastXml.GetElementsByTagName("text");
            elements[0].AppendChild(toastXml.CreateTextNode(text));
            ToastNotification toast = new ToastNotification(toastXml);
            //toast.Activated += toast_Activated;//点击
            //toast.Dismissed += toast_Dismissed;//消失
            //toast.Failed += toast_Failed;//消除
            ToastNotificationManager.CreateToastNotifier().Show(toast);


            //从通知中心删除
            await Task.Delay(3000);
            ToastNotificationManager.History.Clear();

        }

        /// <summary>
        ///UNICODE字符转为中文 
        /// </summary>
        /// <param name="unicodeString"></param>
        /// <returns></returns>
        public static string ConvertUnicodeStringToChinese(string unicodeString)
        {
            if (string.IsNullOrEmpty(unicodeString))
                return string.Empty;

            string outStr = unicodeString;

            Regex re = new Regex("\\\\u[0123456789abcdef]{4}", RegexOptions.IgnoreCase);
            MatchCollection mc = re.Matches(unicodeString);
            foreach (Match ma in mc)
            {
                outStr = outStr.Replace(ma.Value, ConverUnicodeStringToChar(ma.Value).ToString());
            }
            return outStr;
        }

        private static char ConverUnicodeStringToChar(string str)
        {
            char outStr = Char.MinValue;
            outStr = (char)int.Parse(str.Remove(0, 2), System.Globalization.NumberStyles.HexNumber);
            return outStr;
        }


        public static async Task ShowSystemTrayAsync(Color backgroundColor, Color foregroundColor, double opacity = 1,
            string text = "", bool isIndeterminate = false)
        {
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBar.BackgroundColor = backgroundColor;
            statusBar.ForegroundColor = foregroundColor;
            statusBar.BackgroundOpacity = opacity;

            statusBar.ProgressIndicator.Text = text;
            if (!isIndeterminate)
            {
                statusBar.ProgressIndicator.ProgressValue = 0;
            }
            await statusBar.ProgressIndicator.ShowAsync();
        }

        /// <summary>
        /// 弹出对话框
        /// </summary>
        /// <param name="text"></param>
        public static async void Message(string text, string title = "错误")
        {
            try
            {
                await new MessageDialog(text, title).ShowAsync();
            }
            catch (Exception) { Debug.WriteLine("Utils,MessageDialog异常"); }
        }

        /// <summary>
        /// 屏幕高度
        /// </summary>
        /// <returns></returns>
        public static double getPhoneHeight()
        {
            return Window.Current.Bounds.Height;
        }

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        /// <returns></returns>
        public static double getPhoneWidth()
        {
            return Window.Current.Bounds.Width;
        }

        /// <summary>
        /// 时间转时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetTimeStamp(DateTimeOffset date, TimeSpan time)
        {
            DateTime nowdate = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);
            TimeSpan ts = nowdate.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// 时间戳转时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = new DateTime(1970, 1, 1, 8, 0, 0);
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        public static JArray ReadJso(string jsonstring)
        {
            if (jsonstring != "")
            {
                JObject obj = JObject.Parse(jsonstring);
                if (Int32.Parse(obj["status"].ToString()) == 200)
                {
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(jsonstring);
                    string json = jObject["data"].ToString();
                    JArray jArray = (JArray)JsonConvert.DeserializeObject(json);
                    return jArray;
                }
                else
                {
                    Message("请求失败", "失败");
                    return null;
                }
            }

            else
            {
                Message("网络错误！", "错误");
                return null;
            }
        }

        /// <summary>
        /// 异步从网络下载图片
        /// </summary>
        /// <param name="outfileName">下载保存到本地的图片文件名</param>
        /// <param name="downloadUriString">图片uri</param>
        /// <param name="scaleSize">图片尺寸</param>
        /// <returns></returns>
        public static async Task DownloadAndScale(string outfileName, string downloadUriString, Size scaleSize)
        {
            try
            {
                Uri downLoadingUri = new Uri(downloadUriString);//创建uri对象
                HttpClient client = new HttpClient();//实例化httpclient对象
                using (var response = await client.GetAsync(downLoadingUri))
                {
                    var buffer = await response.Content.ReadAsBufferAsync();//从返回的数据中读取buffer
                    var memoryStream = new InMemoryRandomAccessStream();
                    await memoryStream.WriteAsync(buffer);//将buffer写入memorystream
                    await memoryStream.FlushAsync();//刷新
                    var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(memoryStream);//解密文件流
                    //确定图片大小
                    var bt = new Windows.Graphics.Imaging.BitmapTransform();
                    bt.ScaledWidth = (uint)scaleSize.Width;
                    bt.ScaledHeight = (uint)scaleSize.Height;
                    //得到像素数值
                    var pixelProvider = await decoder.GetPixelDataAsync(
                        decoder.BitmapPixelFormat, decoder.BitmapAlphaMode, bt,
                        ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.ColorManageToSRgb);


                    //下面保存图片
                    // Now that we have the pixel data, get the destination file
                    var localFolder = ApplicationData.Current.LocalFolder;
                    //var resultsFolder = await localFolder.CreateFolderAsync("Results", CreationCollisionOption.OpenIfExists);
                    var scaledFile = await localFolder.CreateFileAsync(outfileName, CreationCollisionOption.ReplaceExisting);
                    using (var scaledFileStream = await scaledFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(
                            BitmapEncoder.JpegEncoderId, scaledFileStream);
                        var pixels = pixelProvider.DetachPixelData();
                        encoder.SetPixelData(
                            decoder.BitmapPixelFormat,
                            decoder.BitmapAlphaMode,
                            (uint)scaleSize.Width,
                            (uint)scaleSize.Height,
                            decoder.DpiX,
                            decoder.DpiY,
                            pixels
                            );
                        await encoder.FlushAsync();
                    }
                }
            }
            catch (Exception) { Debug.WriteLine("工具，图片异常"); }
        }
    }
}
