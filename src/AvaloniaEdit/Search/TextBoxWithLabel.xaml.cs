using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Reflection;

namespace AvaloniaEdit.Search
{
    public class TextBoxWithLabel : TextBox
    {
        public static readonly AvaloniaProperty<string> LabelProperty =
            AvaloniaProperty.Register<Spinner, string>(nameof(Label));

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        TextPresenter presenterLabel;

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            presenterLabel = e.NameScope.Get<TextPresenter>("PART_TextPresenterLabel");
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            presenterLabel.IsVisible = false;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (Text.Length == 0) presenterLabel.IsVisible = true;
        }
    }
}
