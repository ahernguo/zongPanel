using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace zongPanel {

	/// <summary>反向的布林值轉顯示狀態</summary>
	public class InverseBooleanToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (true.Equals(value)) {
				return Visibility.Collapsed;
			} else {
				return Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			var vis = (Visibility)value;
			return vis == Visibility.Collapsed;
		}
	}
}
