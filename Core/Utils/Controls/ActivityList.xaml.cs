﻿using API;
using API.Data;
using API.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class ActivityList : UserControl {

        private static HttpClient client = new HttpClient();
        public bool ContentLoaded;
        private static ScrollViewer sv;

        public ActivityList() {
            this.InitializeComponent();
        }

        public async void LoadPosts() {
            try {
                MainPage.sb = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                MainPage.sb.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
                MainPage.sb.ProgressIndicator.Text = "Loading activity...";
                await MainPage.sb.ProgressIndicator.ShowAsync();
                GroupData(await RequestHandler.RetrieveActivity());
                await MainPage.sb.ProgressIndicator.HideAsync();
                ContentLoaded = true;
            } catch (Exception e) {
                DebugHandler.Error("Error loading activity feed. ", e.StackTrace);
            }
        }

        public void ClearPosts()
        {
            //GroupData(new ObservableCollection<APIContent.Content.Activity.Notification>());
            Notifications.Visibility = Visibility.Collapsed;
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.BlogDetails), ((Image)sender).Tag)) {
                throw new Exception("NavFail");
            }
        }

        public void GroupData(List<API.Content.Activity.Notification> items) {
            var result = from item in items group item by item.date into itemGroup orderby itemGroup.Key select itemGroup;
            csvNotifications.Source = result.Reverse();
            Notifications.Visibility = Visibility.Visible;
        }

        private void Notifications_Loaded(object sender, RoutedEventArgs e) {
            //if (Notifications.Items.Count != 0)
                //Config.LastNotification = (Notifications.Items.First() as APIContent.Content.Activity.Notification).timestamp;
        }

        private void GoToPost(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.PostDetails), ((Image)sender).Tag)) {
                throw new Exception("NavFail");
            }
        }

        private async void FollowIcon_Tapped(object sender, TappedRoutedEventArgs e) {
            var x = ((Grid)sender);
            if (await RequestHandler.FollowUnfollow(true, x.Tag.ToString())) {
                x.Visibility = Visibility.Collapsed;
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e) {
            ((Border)sender).Width = Window.Current.Bounds.Width;
        }
    }
}
