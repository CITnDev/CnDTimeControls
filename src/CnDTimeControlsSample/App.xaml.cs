﻿using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace CnDTimeLineSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.GetCultureInfo("fr-FR").IetfLanguageTag)));
        }
    }
}
