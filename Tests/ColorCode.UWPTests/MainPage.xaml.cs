// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using ColorCode.Styling;
using ColorCode.UWP.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ColorCode.UWPTests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task<Tuple<string, ILanguage>> GetCodeFileText()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");

            var file = await picker.PickSingleFileAsync();
            if (file == null) return null;
            string text = "";

            using (var reader = new StreamReader(await file.OpenStreamForReadAsync(), true))
            {
                text = await reader.ReadToEndAsync();
            }

            ILanguage Language = Languages.FindById(file.FileType.Replace(".", "")) ?? Languages.CSharp;
            return new Tuple<string, ILanguage>(text, Language);
        }

        private void RenderLight(object sender, RoutedEventArgs e)
        {
            MainGrid.RequestedTheme = ElementTheme.Light;
            Render();
        }

        private void RenderDark(object sender, RoutedEventArgs e)
        {
            MainGrid.RequestedTheme = ElementTheme.Dark;
            Render();
        }

        private async void Render()
        {
            PresentationBlock.Blocks.Clear();

            var result = await GetCodeFileText();
            if (result == null) return;

            var formatter = new RichTextBlockFormatter(MainGrid.RequestedTheme);
            var plainText = formatter.Styles[ScopeName.PlainText];
            MainGrid.Background = (plainText?.Background ?? StyleDictionary.White).GetSolidColorBrush();
            formatter.FormatRichTextBlock(result.Item1, result.Item2, PresentationBlock);
        }

        private void HTMLLight(object sender, RoutedEventArgs e)
        {
            MakeHTML(StyleDictionary.DefaultLight);
        }

        private void HTMLDark(object sender, RoutedEventArgs e)
        {
            MakeHTML(StyleDictionary.DefaultDark);
        }

        private async void MakeHTML(StyleDictionary Styles)
        {
            var result = await GetCodeFileText();
            if (result == null) return;

            var formatter = new HtmlFormatter(Styles);
            var html = formatter.GetHtmlString(result.Item1, result.Item2);

            var tempfile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("HTMLResult.html", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(tempfile, html);

            try
            {
                await Launcher.LaunchFileAsync(tempfile);
            }
            catch { }
        }
    }
}