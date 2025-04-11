using System;
using System.Globalization;
using System.Windows;

namespace KannadaNudiEditor.Helpers
{
    public static class LanguageManager
    {
        public static void SwitchLanguage(string cultureCode)
        {
            var dict = new ResourceDictionary();
            switch (cultureCode)
            {
                case "kn-IN":
                    dict.Source = new Uri("Resources/Strings.kn-IN.xaml", UriKind.Relative);
                    break;
                case "en-US":
                default:
                    dict.Source = new Uri("Resources/Strings.en-US.xaml", UriKind.Relative);
                    break;
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

            CultureInfo culture = new CultureInfo(cultureCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
