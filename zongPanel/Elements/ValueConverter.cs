using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace zongPanel {

	/// <summary>反向的布林值轉顯示狀態</summary>
	public class InverseBooleanToVisibilityConverter : IValueConverter {

		/// <summary>將 <see cref="bool"/> 轉換成 <see cref="Visibility"/></summary>
		/// <param name="value">欲轉換的 <see cref="bool"/></param>
		/// <param name="targetType">欲轉換的目標類型</param>
		/// <param name="parameter">附加參數選項</param>
		/// <param name="culture">欲轉換的文化特性</param>
		/// <returns><see cref="Visibility"/></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (true.Equals(value)) {
				return Visibility.Collapsed;
			} else {
				return Visibility.Visible;
			}
		}

		/// <summary>將 <see cref="Visibility"/> 轉換成 <see cref="bool"/></summary>
		/// <param name="value">欲轉換的 <see cref="Visibility"/></param>
		/// <param name="targetType">欲轉換的目標類型</param>
		/// <param name="parameter">附加參數選項</param>
		/// <param name="culture">欲轉換的文化特性</param>
		/// <returns><see cref="bool"/></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			var vis = (Visibility)value;
			return vis == Visibility.Collapsed;
		}
	}
}
