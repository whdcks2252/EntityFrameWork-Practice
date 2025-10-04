// NumericBehavior.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Common
{
    public static class Numeric
    {
        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(Numeric),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetAllowDecimal(DependencyObject obj) => (bool)obj.GetValue(AllowDecimalProperty);
        public static void SetAllowDecimal(DependencyObject obj, bool value) => obj.SetValue(AllowDecimalProperty, value);
        public static readonly DependencyProperty AllowDecimalProperty =
            DependencyProperty.RegisterAttached("AllowDecimal", typeof(bool), typeof(Numeric), new PropertyMetadata(false));

        public static bool GetAllowNegative(DependencyObject obj) => (bool)obj.GetValue(AllowNegativeProperty);
        public static void SetAllowNegative(DependencyObject obj, bool value) => obj.SetValue(AllowNegativeProperty, value);
        public static readonly DependencyProperty AllowNegativeProperty =
            DependencyProperty.RegisterAttached("AllowNegative", typeof(bool), typeof(Numeric), new PropertyMetadata(false));

        // -1: 제한 없음, 그 외: 소수점 이하 자릿수 제한
        public static int GetMaxDecimalPlaces(DependencyObject obj) => (int)obj.GetValue(MaxDecimalPlacesProperty);
        public static void SetMaxDecimalPlaces(DependencyObject obj, int value) => obj.SetValue(MaxDecimalPlacesProperty, value);
        public static readonly DependencyProperty MaxDecimalPlacesProperty =
            DependencyProperty.RegisterAttached("MaxDecimalPlaces", typeof(int), typeof(Numeric), new PropertyMetadata(-1));

        // 마지막 유효 텍스트(IME/붙여넣기 대비)
        private static readonly DependencyProperty LastValidTextProperty =
            DependencyProperty.RegisterAttached("LastValidText", typeof(string), typeof(Numeric), new PropertyMetadata(string.Empty));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb) return;

            if ((bool)e.NewValue)
            {
                tb.PreviewTextInput += OnPreviewTextInput;
                tb.PreviewKeyDown += OnPreviewKeyDown;
                DataObject.AddPastingHandler(tb, OnPaste);
                tb.TextChanged += OnTextChanged; // IME/드래그드롭 대비
                tb.SetValue(LastValidTextProperty, tb.Text);
            }
            else
            {
                tb.PreviewTextInput -= OnPreviewTextInput;
                tb.PreviewKeyDown -= OnPreviewKeyDown;
                DataObject.RemovePastingHandler(tb, OnPaste);
                tb.TextChanged -= OnTextChanged;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 허용: 편집/제어키
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Home || e.Key == Key.End)
                return;

            // Ctrl/Alt 조합(복붙 등) 허용
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                return;
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var candidate = TextAfterInput(tb, e.Text);
            if (!IsValid(candidate, tb))
                e.Handled = true;
        }

        private static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.UnicodeText))
            {
                var tb = (TextBox)sender;
                var pasteText = e.DataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
                var candidate = TextAfterInput(tb, pasteText);
                if (!IsValid(candidate, tb))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        // IME 입력/드래그드롭 등으로 Text가 직접 바뀐 경우
        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            var last = (string?)tb.GetValue(LastValidTextProperty) ?? string.Empty;
            if (IsValid(tb.Text, tb))
            {
                tb.SetValue(LastValidTextProperty, tb.Text);
            }
            else
            {
                // 되돌림
                var caret = tb.CaretIndex - 1;
                tb.Text = last;
                tb.CaretIndex = Math.Max(0, Math.Min(tb.Text.Length, caret));
            }
        }

        private static string TextAfterInput(TextBox tb, string input)
        {
            var start = tb.SelectionStart;
            var length = tb.SelectionLength;
            var text = tb.Text ?? string.Empty;
            if (length > 0)
                text = text.Remove(start, length);
            return text.Insert(start, input);
        }

        private static bool IsValid(string text, TextBox tb)
        {
            var allowDecimal = GetAllowDecimal(tb);
            var allowNegative = GetAllowNegative(tb);
            var maxPlaces = GetMaxDecimalPlaces(tb);
            var nfi = CultureInfo.CurrentCulture.NumberFormat;
            var dec = nfi.NumberDecimalSeparator;
            var neg = nfi.NegativeSign;

            if (string.IsNullOrEmpty(text)) return true; // 빈값 허용(필요시 false로)

            // 음수 기호
            if (!allowNegative && text.Contains(neg)) return false;
            if (allowNegative && text.Contains(neg))
            {
                // 음수 기호는 맨 앞에 1개만
                if (text.IndexOf(neg, StringComparison.Ordinal) != 0) return false;
                if (text.Split(neg).Length > 2) return false;
            }

            // 소수점
            if (!allowDecimal && text.Contains(dec)) return false;
            if (allowDecimal && text.Contains(dec))
            {
                if (text.Split(new[] { dec }, StringSplitOptions.None).Length > 2) return false;
                if (maxPlaces >= 0)
                {
                    var idx = text.IndexOf(dec, StringComparison.Ordinal);
                    var places = text.Length - idx - dec.Length;
                    if (idx >= 0 && places > maxPlaces) return false;
                }
            }

            // 숫자/허용기호만 존재하는지 확인
            // (문화권 소수점/음수기호 제외 모두 숫자여야 함)
            foreach (var ch in text)
            {
                var s = ch.ToString();
                if (char.IsDigit(ch)) continue;
                if (allowDecimal && s == dec) continue;
                if (allowNegative && s == neg) continue;
                return false;
            }

            // 단독 기호만 입력된 상태("-", ".", "-." 등)도 임시 허용하려면 여기서 true
            // 필요 시 아래처럼 숫자 최소 1개 요구로 변경 가능:
            // if (!text.Any(char.IsDigit)) return false;

            return true;
        }
    }
}
