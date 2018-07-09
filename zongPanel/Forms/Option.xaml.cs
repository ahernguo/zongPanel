using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using zongPanel.Library;

namespace zongPanel.Forms {
	/// <summary>
	/// Option.xaml 的互動邏輯
	/// </summary>
	public partial class Option : Window, INotifyPropertyChanged {

		#region Definitions
		/// <summary>顯示字體切換按鈕之 <see cref="Control.FontSize"/></summary>
		private static readonly float BTN_FONT_SIZE = 14;
		#endregion

		#region Fields
		/// <summary>主核心所帶入的可更改設定檔</summary>
		private PanelConfig mConfig;
		/// <summary>儲存資源檔圖片與其對應的 <see cref="ImageSource"/></summary>
		private Dictionary<string, ImageSource> mResxImgSrc = new Dictionary<string, ImageSource>();
		/// <summary>當前是否鎖定 UI</summary>
		private bool mLock = true;
		#endregion

		#region Event Declarations
		/// <summary>捷徑選項改變事件</summary>
		public event EventHandler<ShortcutEventArgs> OnShortcutChanged;
		/// <summary>數值改變事件</summary>
		public event EventHandler<FormatEventArgs> OnFormatChanged;
		/// <summary>字型改變事件</summary>
		public event EventHandler<FontEventArgs> OnFontChanging;
		/// <summary>顏色改變事件</summary>
		public event EventHandler<ColorEventArgs> OnColorChanging;
		/// <summary>效能面板停靠改變事件</summary>
		public event EventHandler<DockEventArgs> OnDockChanged;
		/// <summary>元件解鎖改變事件</summary>
		public event EventHandler<BoolEventArgs> OnLockChanged;
		/// <summary>視窗關閉事件</summary>
		public event EventHandler<PropertyChangedEventArgs> OnWindowClosing;
		#endregion

		#region INotifyPropertyChanged Event Handles
		/// <summary>屬性變更事件</summary>
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>發布屬性變更事件</summary>
		/// <param name="name">屬性名稱，保持空白則以觸發者為名</param>
		private void RaisePropChg([CallerMemberName]string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		#endregion

		#region Properties
		/// <summary>取得當前是否鎖定 UI 狀況</summary>
		public bool IsLocked {
			get => mLock;
			set {
				mLock = value;
				RaisePropChg();
			}
		}
		#endregion

		#region Constructors
		/// <summary>建立選項視窗，並帶入當前的面板設定資訊</summary>
		/// <param name="config">目前的面板設定資訊，請帶入複製或可更改的 instance</param>
		public Option(PanelConfig config) {
			InitializeComponent();

			/* 指定資料來源 */
			this.DataContext = this;
			/* 設定檔並還原當前的樣式 */
			mConfig = config;
			LoadConfig(config);
		}
		#endregion

		#region Methods
		/// <summary>將 <see cref="System.Drawing.Color.A"/> 轉換為 10 級距的 Index，供 <see cref="ComboBox.SelectedIndex"/> 使用</summary>
		/// <param name="color">欲判斷的顏色</param>
		/// <returns>10 級距的 Index</returns>
		private int CalculateAlphaToSelectedIndex(System.Drawing.Color color) {
			var alpha = (color.A / 255) * 100;
			var quotient = alpha / 10;
			if ((alpha % 10) > 5) quotient++;

			return quotient - 1;
		}

		/// <summary>將 <see cref="PanelConfig"/> 之設定套用到當前的控制項</summary>
		/// <param name="config">欲套用的設定檔</param>
		private void LoadConfig(PanelConfig config) {
			chkShowSec.IsChecked = config.ShowSecond;

			chkCalc.IsChecked = config.Shortcuts.HasFlag(Shortcut.Calculator);
			chkNote.IsChecked = config.Shortcuts.HasFlag(Shortcut.Note);

			config.GetAlpha(PanelComponent.Date, out var alpha);
			cbDateAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Note, out alpha);
			cbNoteBgAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.NoteContent, out alpha);
			cbNoteCntAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.NoteTitle, out alpha);
			cbNoteTitAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Background, out alpha);
			cbPnBgAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Time, out alpha);
			cbTimeAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.StatusBar, out alpha);
			cbUsgFontAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Week, out alpha);
			cbWeekAlpha.SelectedIndex = alpha;

			config.GetFormat(PanelComponent.Date, out int index);
			cbDateFmt.SelectedIndex = index;
			config.GetFormat(PanelComponent.Time, out index);
			cbTimeFmt.SelectedIndex = index;
			config.GetFormat(PanelComponent.Week, out index);
			cbWeekFmt.SelectedIndex = index;

			config.GetBrush(PanelComponent.Date, out var brush);
			rectDateColor.Fill = brush;
			config.GetBrush(PanelComponent.Note, out brush);
			rectNoteBg.Fill = brush;
			config.GetBrush(PanelComponent.NoteContent, out brush);
			rectNoteCntColor.Fill = brush;
			config.GetBrush(PanelComponent.NoteTitle, out brush);
			rectNoteTitColor.Fill = brush;
			config.GetBrush(PanelComponent.Background, out brush);
			rectPnBg.Fill = brush;
			config.GetBrush(PanelComponent.Time, out brush);
			rectTimeColor.Fill = brush;
			config.GetBrush(PanelComponent.StatusBar, out brush);
			rectUsgFont.Fill = brush;
			config.GetBrush(PanelComponent.Week, out brush);
			rectWeekColor.Fill = brush;

			config.GetFont(PanelComponent.Date, out var font);
			btnDateFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.NoteContent, out font);
			btnNoteCntFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.NoteTitle, out font);
			btnNoteTitFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.Time, out font);
			btnTimeFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.StatusBar, out font);
			btnUsgFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.Week, out font);
			btnWeekFont.SetFont(font, BTN_FONT_SIZE);
		}
		#endregion

		#region UI Events
		private void ImageMouseEnter(object sender, MouseEventArgs e) {
			var img = sender as Image;
			var resName = $"{img.Tag.ToString()}Hover";
			img.TryInvoke(() => img.Source = mResxImgSrc[resName]);
		}

		private void ImageMouseLeave(object sender, MouseEventArgs e) {
			var img = sender as Image;
			var resName = $"{img.Tag.ToString()}Org";
			img.TryInvoke(() => img.Source = mResxImgSrc[resName]);
		}

		private void ShortcutChanged(object sender, RoutedEventArgs e) {
			var chkBox = sender as CheckBox;
			var tag = Shortcut.Calculator;
			var chk = false;
			chkBox.TryInvoke(
				() => {
					tag = (Shortcut)chkBox.Tag;
					chk = chkBox.IsChecked.Value;
				}
			);
			var scut = mConfig.ChangeShortcut(tag, chk);
			OnShortcutChanged?.BeginInvoke(
				this,
				new ShortcutEventArgs(scut),
				null, null
			);
		}

		private void ShowSecondChanged(object sender, RoutedEventArgs e) {
			var chkBox = sender as CheckBox;
			var chk = chkBox.TryInvoke(() => chkBox.IsChecked.Value);
			var format = mConfig.ChangeShowSecond(chk);
			OnFormatChanged?.BeginInvoke(
				this,
				new FormatEventArgs(PanelComponent.Time, format),
				null, null
			);
		}

		private void DateWeekFormatChanged(object sender, SelectionChangedEventArgs e) {
			Format format = null;
			var comboBox = sender as ComboBox;
			var tag = PanelComponent.Week;
			var idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = (PanelComponent)comboBox.Tag;
					idx = comboBox.SelectedIndex;
				}
			);

			switch (tag) {
				case PanelComponent.Time:
					format = mConfig.ChangeTimeFormat(idx);
					break;
				case PanelComponent.Date:
					format = mConfig.ChangeDateFormat(idx);
					break;
				case PanelComponent.Week:
					format = mConfig.ChangeWeekFormat(idx);
					break;
				default:
					throw new InvalidEnumArgumentException("tag", (int) tag, typeof(PanelComponent));
			}

			OnFormatChanged?.BeginInvoke(
				this,
				new FormatEventArgs(tag, format),
				null, null
			);
		}

		private void FontClicked(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			var tag = btn.TryInvoke(() => (PanelComponent)btn.Tag);

			using (var diag = new System.Windows.Forms.FontDialog()) {
				diag.ShowColor = false;
				diag.ShowEffects = true;
				diag.Font = btn.GetFont();
				diag.FixedPitchOnly = false;
				if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
					mConfig.ChangeFont(tag, diag.Font);
					OnFontChanging?.Invoke( //跑同步，避免 Font 被刪掉
						this,
						new FontEventArgs(tag, diag.Font)
					);
					btn.SetFont(diag.Font, BTN_FONT_SIZE);
				}
			}
		}

		private void ColorChanged(object sender, MouseButtonEventArgs e) {
			var rect = sender as Rectangle;
			var tag = rect.TryInvoke(() => (PanelComponent)rect.Tag);

			using (var diag = new System.Windows.Forms.ColorDialog()) {
				diag.AllowFullOpen = true;
				diag.AnyColor = true;
				diag.FullOpen = true;
				diag.SolidColorOnly = true;
				if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
					mConfig.ChangeColor(tag, diag.Color);
					OnColorChanging?.Invoke(
						this,
						new ColorEventArgs(tag, diag.Color)
					);
				}
				rect.TryInvoke(() => rect.Fill = diag.Color.GetBrush());
			}
		}

		private void AlphaChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var tag = PanelComponent.Background;
			var idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = (PanelComponent)comboBox.Tag;
					idx = comboBox.SelectedIndex;
				}
			);
			var color = mConfig.ChangeAlpha(tag, idx);
			OnColorChanging?.Invoke(
				this,
				new ColorEventArgs(tag, color)
			);
		}

		private void UsageDockChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var idx = (UsageDock)comboBox.TryInvoke(() => comboBox.SelectedIndex);
			mConfig.ChangeUsageDock(idx);
			OnDockChanged?.BeginInvoke(
				this,
				new DockEventArgs(idx),
				null, null
			);
		}

		private void SaveClicked(object sender, RoutedEventArgs e) {
			mConfig.SaveToFile();
		}

		private void ExitClicked(object sender, RoutedEventArgs e) {
			/* 清除記憶體 */
			if (mConfig != null) {
				mConfig.Dispose();
			}
			/* 發布事件 */
			OnWindowClosing?.Invoke(this, new PropertyChangedEventArgs("Option"));
			/* 關閉視窗 */
			this.TryInvoke(() => this.Close());
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			chkNote.Checked += ShortcutChanged;
			chkNote.Unchecked += ShortcutChanged;
			chkCalc.Checked += ShortcutChanged;
			chkCalc.Unchecked += ShortcutChanged;

			cbUsgDock.SelectionChanged += UsageDockChanged;

			cbDateAlpha.SelectionChanged += AlphaChanged;
			cbNoteBgAlpha.SelectionChanged += AlphaChanged;
			cbNoteCntAlpha.SelectionChanged += AlphaChanged;
			cbNoteTitAlpha.SelectionChanged += AlphaChanged;
			cbPnBgAlpha.SelectionChanged += AlphaChanged;
			cbTimeAlpha.SelectionChanged += AlphaChanged;
			cbUsgFontAlpha.SelectionChanged += AlphaChanged;
			cbWeekAlpha.SelectionChanged += AlphaChanged;

			rectDateColor.MouseLeftButtonUp += ColorChanged;
			rectNoteBg.MouseLeftButtonUp += ColorChanged;
			rectNoteCntColor.MouseLeftButtonUp += ColorChanged;
			rectNoteTitColor.MouseLeftButtonUp += ColorChanged;
			rectPnBg.MouseLeftButtonUp += ColorChanged;
			rectTimeColor.MouseLeftButtonUp += ColorChanged;
			rectUsgFont.MouseLeftButtonUp += ColorChanged;
			rectWeekColor.MouseLeftButtonUp += ColorChanged;

			btnDateFont.Click += FontClicked;
			btnNoteCntFont.Click += FontClicked;
			btnNoteTitFont.Click += FontClicked;
			btnTimeFont.Click += FontClicked;
			btnUsgFont.Click += FontClicked;
			btnWeekFont.Click += FontClicked;

			cbTimeFmt.SelectionChanged += DateWeekFormatChanged;
			cbDateFmt.SelectionChanged += DateWeekFormatChanged;
			cbWeekFmt.SelectionChanged += DateWeekFormatChanged;

			chkShowSec.Checked += ShowSecondChanged;
			chkShowSec.Unchecked += ShowSecondChanged;
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			var pos = e.GetPosition(this);
			var posAllow = (pos.X > 0) && (pos.Y > 0) && (pos.X < imgSave.Margin.Left) && (pos.Y < tabControl.Margin.Top);
			if ((e.ChangedButton == MouseButton.Left) && posAllow)
				this.DragMove();
		}

		private void LockClicked(object sender, RoutedEventArgs e) {
			IsLocked = !mLock;
			OnLockChanged?.BeginInvoke(
				this,
				new BoolEventArgs(PanelComponent.Background, mLock),
				null,
				null
			);
		}
		#endregion
	}
}
