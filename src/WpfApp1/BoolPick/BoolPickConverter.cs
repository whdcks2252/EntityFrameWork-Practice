using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfApp1
{
    public class BoolPickConverter : IMultiValueConverter
    {
        private bool _lastBool;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            _lastBool = values.Length > 0 && values[0] is bool b && b;

            object candidate = _lastBool
               ? (values.Length > 2 ? values[2] ?? Binding.DoNothing : Binding.DoNothing)
               : (values.Length > 1 ? values[1] ?? Binding.DoNothing : Binding.DoNothing);

            // 값이 없으면 Binding.DoNothing
            if (candidate == Binding.DoNothing ) return Binding.DoNothing;

            // 이미 타입이 맞으면 그대로
          if (candidate != null && (targetType == typeof(object) || targetType.IsInstanceOfType(candidate)))
                return candidate;

            // 문자열 타겟이면 ToString()
            if (targetType == typeof(string))
                return candidate?.ToString() ?? string.Empty;

            // 나머진 TypeConverter/ChangeType로 시도
            try
            {
                var tc = TypeDescriptor.GetConverter(targetType);
                if (tc != null && candidate != null && tc.CanConvertFrom(candidate.GetType()))
                    return tc.ConvertFrom(null, culture, candidate) ?? Binding.DoNothing;

                if (candidate is IConvertible)
                    return System.Convert.ChangeType(candidate, Nullable.GetUnderlyingType(targetType) ?? targetType, culture);
            }
            catch 
            {
                // fallthrough
            }


            return Binding.DoNothing;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // targetTypes: [ typeof(bool), typeof(TLeft), typeof(TRight) ]
            var result = new object[Math.Max(targetTypes?.Length ?? 0, 3)];
            if (targetTypes == null || targetTypes.Length < 3)
            {
                // 방어적: 길이 모자라면 아무것도 업데이트하지 않음
                for (int i = 0; i < result.Length; i++) result[i] = Binding.DoNothing;
                return result;
            }

            // [0]=_lastBool은 절대 건드리지 않음
            result[0] = Binding.DoNothing;

            if (_lastBool)
            {
                result[1] = Binding.DoNothing;
                result[2] = TryChangeType(value, targetTypes[2], culture, out var converted)
                            ? converted
                            : Binding.DoNothing;
            }
            else
            {
                result[2] = Binding.DoNothing;
                result[1] = TryChangeType(value, targetTypes[1], culture, out var converted)
                            ? converted
                            : Binding.DoNothing;
            }

            return result;
        }

        private bool TryChangeType(object input, Type targetType, CultureInfo culture, out object converted)
        {
            // null 처리 + Nullable<T> 분해
            if (input == null)
            {
                // Nullable이면 null 허용
                if (IsNullable(targetType)) { converted = null; return true; }
                converted = Binding.DoNothing;
                return false;
            }

            var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            //  이미 Nullable이 아님
            if (nonNullableType.IsInstanceOfType(input))
            {
                converted = input;
                return true;
            }

            if (input is string str && string.IsNullOrWhiteSpace(str))
            {
                //  타겟이 문자열이면 그대로 빈 문자열 허용
                if (targetType == typeof(string) || nonNullableType == typeof(string))
                {
                    converted = string.Empty;
                    return true;
                }

                // Nullable이면 null 허용
                if (IsNullable(targetType)){converted = null; return true;}

                converted = DependencyProperty.UnsetValue; 
                return false;
            }


            try
            {
                // Enum
                if (nonNullableType.IsEnum)
                {
                    if (input is string s)
                    {
                        converted = Enum.Parse(nonNullableType, s, ignoreCase: true);
                        return true;
                    }
                    converted = Enum.ToObject(nonNullableType, System.Convert.ChangeType(input, Enum.GetUnderlyingType(nonNullableType), culture)!);
                    return true;
                }

                // TypeConverter 우선
                var tc = TypeDescriptor.GetConverter(nonNullableType);//문자열이나 다른 타입 => 목표 타입 으로 바꾸는 범용 변환기
                if (tc != null && tc.CanConvertFrom(input.GetType()))//타입변환 가능한지
                {
                    if (!(input is string s))
                    {
                        converted = DependencyProperty.UnsetValue;
                        return false;

                    }

                    converted = tc.ConvertFrom(null, culture, input)!; //타입변환 실행
                    return true;
                }

                // IConvertible 경로
                if (input is IConvertible)
                {
                    converted = System.Convert.ChangeType(input, nonNullableType, culture)!;
                    return true;
                }
            }
            catch
            {
                // fallthrough
            }

            converted = Binding.DoNothing;
            return false;
        }

        private bool IsNullable(Type t) =>
            !t.IsValueType || Nullable.GetUnderlyingType(t) != null;

    }
}
