﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#if WP8 || WP7 || WP8
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MyToolkit.Paging;
using MyToolkit.UI;
using Microsoft.Phone.Controls;
#endif
#if WINRT
using System.Net.Http.Headers;
using System.Net.Http;
#endif

// Developed by Rico Suter (http://rsuter.com), http://mytoolkit.codeplex.com

namespace Core.Utils.Misc {
    public class YouTubeUri {
        internal string _url;

        public int Itag { get; internal set; }
        public Uri Uri { get { return new Uri(_url, UriKind.Absolute); } }
        public string Type { get; internal set; }

        public bool HasAudio {
            get {
                return AudioQuality != YouTubeQuality.Unknown && AudioQuality != YouTubeQuality.NotAvailable;
            }
        }

        public bool HasVideo {
            get {
                return VideoQuality != YouTubeQuality.Unknown && VideoQuality != YouTubeQuality.NotAvailable;
            }
        }

        public bool Is3DVideo {
            get {
                if (VideoQuality == YouTubeQuality.Unknown)
                    return false;
                return Itag >= 82 && Itag <= 85;
            }
        }

        public YouTubeQuality VideoQuality {
            get {
                switch (Itag) {
                    // video & audio
                    case 5: return YouTubeQuality.Quality240P;
                    case 6: return YouTubeQuality.Quality270P;
                    case 17: return YouTubeQuality.Quality144P;
                    case 18: return YouTubeQuality.Quality360P;
                    case 22: return YouTubeQuality.Quality720P;
                    case 34: return YouTubeQuality.Quality360P;
                    case 35: return YouTubeQuality.Quality480P;
                    case 36: return YouTubeQuality.Quality240P;
                    case 37: return YouTubeQuality.Quality1080P;
                    case 38: return YouTubeQuality.Quality2160P;

                    // 3d video & audio
                    case 82: return YouTubeQuality.Quality360P;
                    case 83: return YouTubeQuality.Quality480P;
                    case 84: return YouTubeQuality.Quality720P;
                    case 85: return YouTubeQuality.Quality520P;

                    // video only
                    case 133: return YouTubeQuality.Quality240P;
                    case 134: return YouTubeQuality.Quality360P;
                    case 135: return YouTubeQuality.Quality480P;
                    case 136: return YouTubeQuality.Quality720P;
                    case 137: return YouTubeQuality.Quality1080P;
                    case 138: return YouTubeQuality.Quality2160P;
                    case 160: return YouTubeQuality.Quality144P;

                    // audio only
                    case 139: return YouTubeQuality.NotAvailable;
                    case 140: return YouTubeQuality.NotAvailable;
                    case 141: return YouTubeQuality.NotAvailable;
                }

                return YouTubeQuality.Unknown;
            }
        }

        public YouTubeQuality AudioQuality {
            get {
                switch (Itag) {
                    // video & audio
                    case 5: return YouTubeQuality.QualityLow;
                    case 6: return YouTubeQuality.QualityLow;
                    case 17: return YouTubeQuality.QualityLow;
                    case 18: return YouTubeQuality.QualityMedium;
                    case 22: return YouTubeQuality.QualityHigh;
                    case 34: return YouTubeQuality.QualityMedium;
                    case 35: return YouTubeQuality.QualityMedium;
                    case 36: return YouTubeQuality.QualityLow;
                    case 37: return YouTubeQuality.QualityHigh;
                    case 38: return YouTubeQuality.QualityHigh;

                    // 3d video & audio
                    case 82: return YouTubeQuality.QualityMedium;
                    case 83: return YouTubeQuality.QualityMedium;
                    case 84: return YouTubeQuality.QualityHigh;
                    case 85: return YouTubeQuality.QualityHigh;

                    // video only
                    case 133: return YouTubeQuality.NotAvailable;
                    case 134: return YouTubeQuality.NotAvailable;
                    case 135: return YouTubeQuality.NotAvailable;
                    case 136: return YouTubeQuality.NotAvailable;
                    case 137: return YouTubeQuality.NotAvailable;
                    case 138: return YouTubeQuality.NotAvailable;
                    case 160: return YouTubeQuality.NotAvailable;

                    // audio only
                    case 139: return YouTubeQuality.QualityLow;
                    case 140: return YouTubeQuality.QualityMedium;
                    case 141: return YouTubeQuality.QualityHigh;
                }
                return YouTubeQuality.Unknown;
            }
        }

        internal bool IsValid {
            get { return _url != null && _url.StartsWith("http") && Itag > 0 && Type != null; }
        }
    }

    public static class YouTube {
        public const YouTubeQuality DefaultMinQuality = YouTubeQuality.Quality144P;

        private const string BotUserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";

#if WP8 || WP7 || SL5

        /// <summary>
        /// Returns the title of the YouTube video. 
        /// </summary>
        public static Task<string> GetVideoTitleAsync(string youTubeId) // should be improved
        {
            var source = new TaskCompletionSource<string>();
            var web = new WebClient();
            web.OpenReadCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    source.SetException(args.Error);
                else if (args.Cancelled)
                    source.SetCanceled();
                else
                {
                    string result = null;
                    var bytes = args.Result.ReadToEnd();
                    var html = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    var startIndex = html.IndexOf(" title=\"");
                    if (startIndex != -1)
                    {
                        startIndex = html.IndexOf(" title=\"", startIndex + 1);
                        if (startIndex != -1)
                        {
                            startIndex += 8;
                            var endIndex = html.IndexOf("\">", startIndex);
                            if (endIndex != -1)
                                result = html.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                    source.SetResult(result);
                }
            };
            web.Headers[HttpRequestHeader.UserAgent] = BotUserAgent;
            web.OpenReadAsync(new Uri("http://www.youtube.com/watch?v=" + youTubeId + "&nomobile=1"));
            return source.Task;
        }

#elif WINRT

		/// <summary>
        /// Returns the title of the YouTube video. 
        /// </summary>
        public static Task<string> GetVideoTitleAsync(string youTubeId)
        {
            return GetVideoTitleAsync(youTubeId, CancellationToken.None);
        }

        /// <summary>
        /// Returns the title of the YouTube video. 
        /// </summary>
        public static async Task<string> GetVideoTitleAsync(string youTubeId, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", BotUserAgent);
                var response = await client.GetAsync("http://www.youtube.com/watch?v=" + youTubeId + "&nomobile=1", token);
                var html = await response.Content.ReadAsStringAsync();
                var startIndex = html.IndexOf(" title=\"");
                if (startIndex != -1)
                {
                    startIndex = html.IndexOf(" title=\"", startIndex + 1);
                    if (startIndex != -1)
                    {
                        startIndex += 8;
                        var endIndex = html.IndexOf("\">", startIndex);
                        if (endIndex != -1)
                            return html.Substring(startIndex, endIndex - startIndex);
                    }
                }
                return null;
            }
        }

#endif

        /// <summary>
        /// Returns a thumbnail for the given YouTube ID. 
        /// </summary>
        public static Uri GetThumbnailUri(string youTubeId, YouTubeThumbnailSize size = YouTubeThumbnailSize.Medium) {
            switch (size) {
                case YouTubeThumbnailSize.Small:
                    return new Uri("http://img.youtube.com/vi/" + youTubeId + "/default.jpg", UriKind.Absolute);
                case YouTubeThumbnailSize.Medium:
                    return new Uri("http://img.youtube.com/vi/" + youTubeId + "/hqdefault.jpg", UriKind.Absolute);
                case YouTubeThumbnailSize.Large:
                    return new Uri("http://img.youtube.com/vi/" + youTubeId + "/maxresdefault.jpg", UriKind.Absolute);
            }
            throw new Exception();
        }

        /// <summary>
        /// Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D.
        /// </summary>
        /// <returns>Returns null when no appropriate URI has been found. </returns>
        public static YouTubeUri GetBestVideoUri(IEnumerable<YouTubeUri> uris, YouTubeQuality minQuality, YouTubeQuality maxQuality) {
            return uris
                .Where(u => u.HasVideo && u.HasAudio && !u.Is3DVideo && u.VideoQuality >= minQuality && u.VideoQuality <= maxQuality)
                .OrderByDescending(u => u.VideoQuality)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. 
        /// </summary>
        /// <returns>Returns null when no appropriate URI has been found. </returns>
        public static Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality maxQuality) {
            return GetVideoUriAsync(youTubeId, DefaultMinQuality, maxQuality, CancellationToken.None);
        }

        /// <summary>
        /// Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. 
        /// </summary>
        /// <returns>Returns null when no appropriate URI has been found. </returns>
        public static Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality maxQuality, CancellationToken token) {
            return GetVideoUriAsync(youTubeId, DefaultMinQuality, maxQuality, token);
        }

        /// <summary>
        /// Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. 
        /// </summary>
        /// <returns>Returns null when no appropriate URI has been found. </returns>
        public static Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality minQuality, YouTubeQuality maxQuality) {
            return GetVideoUriAsync(youTubeId, minQuality, maxQuality, CancellationToken.None);
        }

        /// <summary>
        /// Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. 
        /// </summary>
        /// <returns>Returns null when no appropriate URI has been found. </returns>
        public static async Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality minQuality, YouTubeQuality maxQuality, CancellationToken token) {
            var uris = await GetUrisAsync(youTubeId, token);
            return GetBestVideoUri(uris, minQuality, maxQuality);
        }

        /// <summary>
        /// Returns all available URIs (audio-only and video) for the given YouTube ID. 
        /// </summary>
        public static Task<YouTubeUri[]> GetUrisAsync(string youTubeId) {
            return GetUrisAsync(youTubeId, CancellationToken.None);
        }

        /// <summary>
        /// Returns all available URIs (audio-only and video) for the given YouTube ID. 
        /// </summary>
        public static async Task<YouTubeUri[]> GetUrisAsync(string youTubeId, CancellationToken token) {
            var urls = new List<YouTubeUri>();
            string javaScriptCode = null;

            var response = await HttpGet("https://www.youtube.com/watch?v=" + youTubeId + "&nomobile=1");
            var match = Regex.Match(response, "url_encoded_fmt_stream_map\": \"(.*?)\"");
            var data = Uri.UnescapeDataString(match.Groups[1].Value);
            match = Regex.Match(response, "adaptive_fmts\": \"(.*?)\"");
            var data2 = Uri.UnescapeDataString(match.Groups[1].Value);

            var arr = Regex.Split(data + "," + data2, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"); // split by comma but outside quotes
            foreach (var d in arr) {
                var url = "";
                //var connection = "";
                //var stream = "";
                var signature = "";
                var tuple = new YouTubeUri();
                foreach (var p in d.Replace("\\u0026", "\t").Split('\t')) {
                    var index = p.IndexOf('=');
                    if (index != -1 && index < p.Length) {
                        try {
                            var key = p.Substring(0, index);
                            var value = Uri.UnescapeDataString(p.Substring(index + 1));
                            if (key == "url")
                                url = value;
                            //else if (key == "conn")
                            //    connection = value;
                            //else if (key == "stream")
                            //    stream = value;
                            else if (key == "itag")
                                tuple.Itag = int.Parse(value);
                            else if (key == "type" && (value.Contains("video/mp4") || value.Contains("audio/mp4")))
                                tuple.Type = value;
                            else if (key == "sig")
                                signature = value;
                            else if (key == "s") {
                                if (javaScriptCode == null) {
                                    var javaScriptUri = "http://s.ytimg.com/yts/jsbin/html5player-" +
                                                        Regex.Match(response,
                                                            "\"\\\\/\\\\/s.ytimg.com\\\\/yts\\\\/jsbin\\\\/html5player-(.+?)\\.js\"")
                                                            .Groups[1] + ".js";
                                    javaScriptCode = await HttpGet(javaScriptUri);
                                }

                                signature = GenerateSignature(value, javaScriptCode);
                            }
                        } catch (Exception exception) {
                            Debug.WriteLine("YouTube parse exception: " + exception.Message);
                        }
                    }
                }

                //if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(connection) && !string.IsNullOrEmpty(stream))
                //{
                //    url = connection + "?" + stream; 
                //}

                if (url.Contains("&signature=") || url.Contains("?signature="))
                    tuple._url = url;
                else
                    tuple._url = url + "&signature=" + signature;

                if (tuple.IsValid)
                    urls.Add(tuple);
            }

            return urls.ToArray();
        }

        private static string GenerateSignature(string signature, string javaScriptCode) {
            var functionName = Regex.Match(javaScriptCode, "signature=(.*?)\\(").Groups[1].ToString();
            var functionMath = Regex.Match(javaScriptCode, "function " + Regex.Escape(functionName) + "\\((\\w+)\\)\\{(.+?)\\}", RegexOptions.Singleline);

            var parameterName = Regex.Escape(functionMath.Groups[1].ToString());
            var functionBody = functionMath.Groups[2].ToString();

            Dictionary<string, Func<string, int, string>> methods = null;

            //var lo={wS:function(a){return a.reverse()},IC:function(a,b){return a.slice(b)},rw:function(a,b){var c=a[0];a[0]=a[b%a.length];a[b]=c;return a}};
            //function mo(a){a=a.split("");a=lo.rw(a,1);a=lo.rw(a,32);a=lo.IC(a,1);a=lo.wS(a,77);a=lo.IC(a,3);a=lo.wS(a,77);a=lo.IC(a,3);a=lo.wS(a,44);return a.join("")};

            foreach (var line in functionBody.Split(';').Select(l => l.Trim())) {
                if (Regex.IsMatch(line, parameterName + "=" + parameterName + "\\.reverse\\(\\)")) // OLD
                    signature = Reverse(signature);
                else if (Regex.IsMatch(line, parameterName + "=" + parameterName + "\\.slice\\(\\d+\\)"))
                    signature = Slice(signature, Convert.ToInt32(Regex.Match(line, parameterName + "=" + parameterName + "\\.slice\\((\\d+)\\)").Groups[1].ToString()));
                else if (Regex.IsMatch(line, parameterName + "=\\w+\\(" + parameterName + ",\\d+\\)"))
                    signature = Swap(signature, Convert.ToInt32(Regex.Match(line, parameterName + "=\\w+\\(" + parameterName + ",(\\d+)\\)").Groups[1].ToString()));
                else if (Regex.IsMatch(line, parameterName + "\\[0\\]=" + parameterName + "\\[\\d+%" + parameterName + "\\.length\\]"))
                    signature = Swap(signature, Convert.ToInt32(Regex.Match(line, parameterName + "\\[0\\]=" + parameterName + "\\[(\\d+)%" + parameterName + "\\.length\\]").Groups[1].ToString()));
                else {
                    var match = Regex.Match(line, "(" + parameterName + "=)?(.*?)\\.(.*?)\\(" + parameterName + ",(.*?)\\)");
                    if (match.Success) {
                        var root = match.Groups[2].ToString();
                        var method = match.Groups[3].ToString();
                        var parameter = int.Parse(match.Groups[4].ToString());

                        if (methods == null) {
                            // Parse methods
                            methods = new Dictionary<string, Func<string, int, string>>();

                            var code = Regex.Match(javaScriptCode, "var " + root + "=\\{(.*?)\\};function").Groups[1].ToString();
                            var methodsArray = code.Split(new[] { "}," }, StringSplitOptions.None);
                            foreach (var m in methodsArray) {
                                var arr = m.Split(':');
                                var methodName = arr[0];
                                var methodBody = arr[1];

                                if (methodBody.Contains("reverse()"))
                                    methods[methodName] = (s, i) => Reverse(s);
                                else if (methodBody.Contains(".splice(") || methodBody.Contains(".slice("))
                                    methods[methodName] = Slice;
                                else
                                    methods[methodName] = Swap;
                            }
                        }

                        signature = methods[method](signature, parameter);
                    } else {
                    }
                }
            }
            return signature;
        }

        private static string Reverse(string signature) {
            var charArray = signature.ToCharArray();
            Array.Reverse(charArray);
            signature = new string(charArray);
            return signature;
        }

        private static string Slice(string input, int length) {
            return input.Substring(length);
        }

        private static string Swap(string input, int position) {
            var str = new StringBuilder(input);
            var swapChar = str[position];
            str[position] = str[0];
            str[0] = swapChar;
            return str.ToString();
        }

#if WINRT

        private static async Task<string> HttpGet(string uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", BotUserAgent);
                var response = await client.GetAsync(uri);
                return await response.Content.ReadAsStringAsync();
            }
        }

#else

        private static async Task<string> HttpGet(string uri) {
            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("User-Agent", BotUserAgent);
                var response = await client.GetAsync(uri);
                return await response.Content.ReadAsStringAsync();
            }
        }

#endif

        #region Phone

#if WP7 || WP8


        /// <summary>
        /// Plays the YouTube video in the Windows Phone's external player. 
        /// </summary>
        /// <param name="youTubeId">The YouTube ID</param>
        /// <param name="maxQuality">The maximum allowed video quality. </param>
        /// <returns>Awaitable task. </returns>
        public static Task PlayAsync(string youTubeId, YouTubeQuality maxQuality)
        {
            return PlayAsync(youTubeId, maxQuality, CancellationToken.None);
        }

        /// <summary>
        /// Plays the YouTube video in the Windows Phone's external player. 
        /// </summary>
        /// <param name="youTubeId">The YouTube ID</param>
        /// <param name="maxQuality">The maximum allowed video quality. </param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Awaitable task. </returns>
        public static async Task PlayAsync(string youTubeId, YouTubeQuality maxQuality, CancellationToken token)
        {
            var uri = await GetVideoUriAsync(youTubeId, maxQuality, token);
            if (uri != null)
            {
                var launcher = new MediaPlayerLauncher
                {
                    Controls = MediaPlaybackControls.All,
                    Media = uri.Uri
                };
                launcher.Show();
            }
            else
                throw new Exception("no_video_urls_found");
        }

        private static CancellationTokenSource _cancellationToken;
        private static PageDeactivator _oldState;

        /// <summary>
        /// Plays the YouTube video in the Windows Phone's external player. 
        /// Disables the current page and shows a progress indicator until 
        /// the YouTube movie URI has been loaded and the video playback starts.
        /// </summary>
        /// <param name="youTubeId">The YouTube ID</param>
        /// <param name="manualActivatePage">if true add YouTube.CancelPlay() in OnNavigatedTo() of the page (better)</param>
        /// <param name="maxQuality">The maximum allowed video quality. </param>
        /// <returns>Awaitable task. </returns>
        public static async Task PlayWithPageDeactivationAsync(string youTubeId, bool manualActivatePage, YouTubeQuality maxQuality)
        {
            PhoneApplicationPage page;

            lock (typeof(YouTube))
            {
                if (_oldState != null)
                    return;

                if (SystemTray.ProgressIndicator == null)
                    SystemTray.ProgressIndicator = new ProgressIndicator();

                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.IsIndeterminate = true;

                page = Environment.PhoneApplication.CurrentPage;

                _oldState = PageDeactivator.Inactivate();
                _cancellationToken = new CancellationTokenSource();
            }

            try
            {
                await PlayAsync(youTubeId, maxQuality, _cancellationToken.Token);
                if (page == Environment.PhoneApplication.CurrentPage)
                    CancelPlay(manualActivatePage);
            }
            catch (Exception)
            {
                if (page == Environment.PhoneApplication.CurrentPage)
                    CancelPlay(false);
                throw;
            }
        }

        /// <summary>
        /// Call this method in OnBackKeyPress() or in OnNavigatedTo() when 
        /// using PlayWithDeactivationAsync() and manualActivatePage = true like this 
        /// e.Cancel = YouTube.CancelPlay();
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool CancelPlay()
        {
            return CancelPlay(false);
        }
        
        /// <summary>
        /// Should be called when using PlayWithDeactivationAsync() when the back key has been pressed. 
        /// </summary>
        /// <param name="args"></param>
        public static void HandleBackKeyPress(CancelEventArgs args)
        {
            if (CancelPlay())
                args.Cancel = true;
        }

        private static bool CancelPlay(bool manualActivatePage)
        {
            lock (typeof(YouTube))
            {
                if (_oldState == null && _cancellationToken == null)
                    return false;

                if (_cancellationToken != null)
                {
                    _cancellationToken.Cancel();
                    _cancellationToken.Dispose();
                    _cancellationToken = null;
                }

                if (!manualActivatePage && _oldState != null)
                {
                    _oldState.Revert();
                    SystemTray.ProgressIndicator.IsVisible = false;
                    _oldState = null;
                }

                return true;
            }
        }

        /// <summary>
        /// Obsolete: Use PlayAsync() instead
        /// </summary>
        [Obsolete("Use PlayAsync instead. 5/17/2014")]
        public static async void Play(string youTubeId, YouTubeQuality maxQuality, Action<Exception> completed)
        {
            try
            {
                await PlayAsync(youTubeId, maxQuality);
                if (completed != null)
                    completed(null);
            }
            catch (Exception exception)
            {
                if (completed != null)
                    completed(exception);
            }
        }

        /// <summary>
        /// Obsolete: Use PlayWithPageDeactivationAsync() instead
        /// </summary>
        [Obsolete("Use PlayWithPageDeactivationAsync instead. 5/17/2014")]
        public static async void Play(string youTubeId, bool manualActivatePage, YouTubeQuality maxQuality, Action<Exception> completed)
        {
            try
            {
                await PlayWithPageDeactivationAsync(youTubeId, manualActivatePage, maxQuality);
                if (completed != null)
                    completed(null);
            }
            catch (Exception exception)
            {
                if (completed != null)
                    completed(exception);
            }
        }

#endif
        #endregion
    }
}

