using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using zongPanel.Library;

namespace zongPanel.Forms {
	/// <summary>
	/// Option.xaml 的互動邏輯
	/// </summary>
	public partial class Option : Window {

		#region Fields
		/// <summary>欲改寫的面板設定資訊</summary>
		private PanelConfig mConfig;
		/// <summary>儲存資源檔圖片與其對應的 <see cref="ImageSource"/></summary>
		private Dictionary<string, ImageSource> mResxImgSrc = new Dictionary<string, ImageSource>();
		#endregion

		#region Constructors
		public Option(PanelConfig config) {
			InitializeComponent();

			/* 暫存至全域變數 */
			mConfig = config;

			/* 讀取資源檔圖片並轉換為影像來源 */
			InitializeImageSources();
		}
		#endregion

		#region Methods
		/// <summary>載入資源檔圖片並轉換為 <see cref="ImageSource"/></summary>
		private void InitializeImageSources() {
			/* 建立 Resource 管理器，指向整個專案的 Resource */
			ResourceManager rm = new ResourceManager("zongPanel.Properties.Resources", Assembly.GetExecutingAssembly());

			var pic = rm.GetObject("save_org") as System.Drawing.Bitmap;
			mResxImgSrc.Add("SaveOrg", pic.GetImageSource());
			pic = rm.GetObject("save") as System.Drawing.Bitmap;
			mResxImgSrc.Add("SaveHover", pic.GetImageSource());
			pic = rm.GetObject("close_org") as System.Drawing.Bitmap;
			mResxImgSrc.Add("CloseOrg", pic.GetImageSource());
			pic = rm.GetObject("close") as System.Drawing.Bitmap;
			mResxImgSrc.Add("CloseHover", pic.GetImageSource());
		}
		#endregion

		#region UI Events
		private void ImageMouseEnter(object sender, MouseEventArgs e) {
			Image img = sender as Image;
			string resName = $"{img.Tag.ToString()}Hover";
			img.TryInvoke(() => img.Source = mResxImgSrc[resName]);
		}

		private void ImageMouseLeave(object sender, MouseEventArgs e) {
			Image img = sender as Image;
			string resName = $"{img.Tag.ToString()}Org";
			img.TryInvoke(() => img.Source = mResxImgSrc[resName]);
		}

		private void ShortcutChanged(object sender, RoutedEventArgs e) {
			CheckBox chkBox = sender as CheckBox;
			string tag = string.Empty;
			bool chk = false;
			chkBox.TryInvoke(
				() => {
					tag = chkBox.Tag.ToString();
					chk = chkBox.IsChecked.Value;
				}
			);
			mConfig.ChangeShortcut(tag, chk);
		}

		private void ShowSecondChanged(object sender, RoutedEventArgs e) {
			CheckBox chkBox = sender as CheckBox;
			bool chk = chkBox.TryInvoke(() => chkBox.IsChecked.Value);
			mConfig.ChangeShowSecond(chk);
		}

		private void DateWeekFormatChanged(object sender, SelectionChangedEventArgs e) {
			ComboBox comboBox = sender as ComboBox;
			string tag = string.Empty;
			int idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = comboBox.Tag.ToString();
					idx = comboBox.SelectedIndex;
				}
			);
			if ("Week".Equals(tag)) mConfig.ChangeWeekFormat(idx);
			else if ("Date".Equals(tag)) mConfig.ChangeDateFormat(idx);
		}

		private void FontClicked(object sender, RoutedEventArgs e) {
			Button btn = sender as Button;
			string tag = btn.TryInvoke(() => btn.Tag.ToString());
			mConfig.ChangeFont(tag, btn.GetFont());
		}

		private void ColorChanged(object sender, MouseButtonEventArgs e) {
			Rectangle rect = sender as Rectangle;
			rect.TryInvoke(
				() => {
					string tag = rect.Tag.ToString();
					Brush br = mConfig.ChangeColor(tag, rect.Fill.GetColor()).GetBrush();
					rect.Fill = br;
				}
			);
		}

		private void AlphaChanged(object sender, SelectionChangedEventArgs e) {
			ComboBox comboBox = sender as ComboBox;
			string tag = string.Empty;
			int idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = comboBox.Tag.ToString();
					idx = comboBox.SelectedIndex;
				}
			);
			mConfig.ChangeAlpha(tag, idx);
		}

		private void UsageDockChanged(object sender, SelectionChangedEventArgs e) {
			ComboBox comboBox = sender as ComboBox;
			int idx = comboBox.TryInvoke(() => comboBox.SelectedIndex);
			mConfig.ChangeUsageDock(idx);
		}
		#endregion
	}
}
