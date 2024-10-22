using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace QQBotCodePlugin.utils
{
    class Dialog
    {
        public async void show(string title, string text, string FirstButtonText, string SecondButtonText, string CloseButtonText, XamlRoot xaml)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.TextWrapping = TextWrapping.Wrap;
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = textBlock
            };

            ContentDialog dialog = new ContentDialog
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                PrimaryButtonText = FirstButtonText,
                SecondaryButtonText = SecondButtonText,
                CloseButtonText = CloseButtonText,
                DefaultButton = ContentDialogButton.Primary,
                Content = scrollViewer,
                XamlRoot = xaml
            };

            var result = await dialog.ShowAsync();
        }
    }
}
