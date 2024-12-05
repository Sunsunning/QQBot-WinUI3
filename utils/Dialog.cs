using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace QQBotCodePlugin.utils
{
    class Dialog
    {
        public async void show(string title, string text, string FirstButtonText, string SecondButtonText, string CloseButtonText, XamlRoot xaml)
        {
            try
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
                App.GetAppLogger().Log($"显示Dialog - {title}: {text}");
                var result = await dialog.ShowAsync();
            }catch (Exception ex)
            {
                StackTrace stackTrace = new StackTrace(ex, true);
                App.GetAppLogger().Log($"Error - {ex.Message}",true,stackTrace);
            }
        }
    }
}
