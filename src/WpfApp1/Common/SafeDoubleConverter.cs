using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfApp1.Common
{
    public class SafeDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value == null ? "" : System.Convert.ToString(value, c);

        public object ConvertBack(object value, Type t, object p, CultureInfo c)
        {
            var s = (value as string)?.Trim();

            // ① 이전 값 유지(업데이트 스킵)
            if (string.IsNullOrEmpty(s)) return DependencyProperty.UnsetValue;

            // ② 빈값을 0으로 저장하고 싶다면 위 줄 대신 ↓
            // if (string.IsNullOrEmpty(s)) return 0d;

            if (double.TryParse(s, NumberStyles.Float, c, out var d)) return d;

            // 잘못된 입력은 갱신 시도 자체를 건너뜀
            return Binding.DoNothing;
        }
    }
}
