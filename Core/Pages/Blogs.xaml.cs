﻿using APIWrapper.Content;
using APIWrapper.Content.Model;
using Core.Shared.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Blogs : Page {

        private NavigationHelper navigationHelper;

        public Blogs() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            List.ItemsSource = UserUtils.UserBlogs;
        }

        #region NavigationHelper registration

        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
        }



        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void SelectBlogButton_Tapped(object sender, TappedRoutedEventArgs e) {
            UserUtils.CurrentBlog = ((Button)sender).Tag as Blog;
            MainPage.SwitchedBlog = true;
            Frame.GoBack();
        }
    }
}
