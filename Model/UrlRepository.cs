using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QuipuTest.Model
{
    public static class UrlRepository
    {
        /// <summary>
        /// Аvailable tags
        /// </summary>
        public static readonly List<string> Tags = new List<string>() { "a" };

        /// <summary>
        /// Spliter character array
        /// </summary>
        private static char[] _arraySplit = { ';', ',', ' ' };

        #region Collection URLs property

        private static ObservableCollection<Url> _urls;
        public static ObservableCollection<Url> AllUrls
        {
            get
            {
                if (_urls == null)
                    _urls = new ObservableCollection<Url>();
                return _urls;
            }
            set
            {
                _urls = value;
            }
        }

        #endregion

        /// <summary>
        /// Сhecking a string against a mask
        /// </summary>
        /// <param item="value"></param>
        /// <returns>bool</returns>
        public static bool ValidateUrl(string value)
        {
            value = value.Trim();
            Regex pattern = new Regex(@"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$");
            Match match = pattern.Match(value);
            if (match.Success == false) return false;
            return true;
        }

        /// <summary>
        /// Сalculates the number of tags on the page
        /// </summary>
        /// <param Page="url"></param>
        /// <param Search tag="tag"></param>
        /// <returns></returns>
        private static int GetCount(string url, string tag)
        {
            int _count = 0;
            HtmlDocument docHTML = new HtmlDocument();
            try
            {
                WebRequest reqGET = WebRequest.Create(url);
                WebResponse resp = reqGET.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                docHTML.LoadHtml(sr.ReadToEnd());
                _count = docHTML.DocumentNode.SelectNodes($"//{tag}").Count;
            }
            catch (WebException webExcp)
            {
                MessageBox.Show($"Ошибка: получено WebException [{webExcp.Message}]");
                WebExceptionStatus status = webExcp.Status;
                if (status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
                    MessageBox.Show($"Ошибка: сервер вернул ошибку протокола. [{httpResponse.StatusCode}]");
                }
            }
            catch (UriFormatException)
            {
                MessageBox.Show($"Ошибка: неверный формат URL ({url})");
            }
            return _count;
        }

        /// <summary>
        /// Get collection of URL-addresses
        /// </summary>
        /// <param Path to file="_pathToFile"></param>
        /// <returns></returns>
        public static List<string> GetCollectionUrl(string _pathToFile)
        {
            var tempCollection = new List<string>();
            try
            {
                string text = File.ReadAllText(_pathToFile);
                if (string.IsNullOrEmpty(text))
                {
                    throw new ArgumentException("Входный файл пуст.");
                }
                else
                {
                    var arrayUrls = text.Split(_arraySplit);
                    foreach (var item in arrayUrls)
                    {
                        if (ValidateUrl(item))
                            tempCollection.Add(item);
                    }
                    if (tempCollection.Count == 0)
                        throw new ArgumentException("Входный файл не содержит Url страниц."); ;
                }
            }
            catch (ArgumentException argsEx)
            {
                MessageBox.Show(argsEx.Message);
            }
            return tempCollection;
        }

        /// <summary>
        /// Сalculates and returns a collection of instances
        /// </summary>
        /// <param cancellation token="cancelToken"></param>
        /// <param selection tag="_selectTag"></param>
        /// <param path to file="_pathToFile"></param>
        /// <returns></returns>
        public static ObservableCollection<Url> CalculationTags(CancellationToken cancelToken, string _selectTag, string _pathToFile)
        {
            List<string> urlCollection = GetCollectionUrl(_pathToFile);
            BlockingCollection<Url> blockCollection = new BlockingCollection<Url>();
            try
            {
                Parallel.ForEach(urlCollection, (item, state) =>
                {
                    if (cancelToken.IsCancellationRequested == false)
                    {
                        var count = GetCount(item, _selectTag);
                        blockCollection.Add(new Url(item, count));
                    }
                    else
                    {
                        state.Break();
                        cancelToken.ThrowIfCancellationRequested();
                    }
                });
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Процесс остановлен.");
            }
            blockCollection.CompleteAdding();
            AllUrls = new ObservableCollection<Url>(blockCollection.GetConsumingEnumerable());
            return AllUrls;
        }
    }
}
