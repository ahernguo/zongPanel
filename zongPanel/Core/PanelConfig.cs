using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;

using zongPanel.Library;

namespace zongPanel {

	#region Global Enumerations
	/// <summary>效能監視面板停靠點</summary>
	[DataContract]
	public enum UsageDock {
		/// <summary>不顯示效能監視面板，隱藏</summary>
		[EnumMember]
		Hidden = 0x00,
		/// <summary>停靠於主面板上方</summary>
		[EnumMember]
		Up = 0x01,
		/// <summary>停靠於主面板下方</summary>
		[EnumMember]
		Down = 0x02
	}

	/// <summary>主面板之功能</summary>
	[DataContract]
	[Flags]
	public enum Shortcut {
		/// <summary>便利貼</summary>
		[EnumMember]
		Note = 0x01,
		/// <summary>網路收音機</summary>
		[EnumMember]
		Radio = 0x02,
		/// <summary>小算盤捷徑</summary>
		[EnumMember]
		Calculator = 0x04
	}

	/// <summary>面板元件</summary>
	[DataContract]
	[Flags]
	public enum PanelComponent {
		/// <summary>時間</summary>
		[EnumMember]
		Time = 0x0001,
		/// <summary>日期</summary>
		[EnumMember]
		Date = 0x0002,
		/// <summary>星期</summary>
		[EnumMember]
		Week = 0x0004,
		/// <summary>主面板</summary>
		[EnumMember]
		Background = 0x0008,
		/// <summary>便利貼</summary>
		[EnumMember]
		Note = 0x0010,
		/// <summary>收音機</summary>
		[EnumMember]
		Radio = 0x0020,
		/// <summary>計算機</summary>
		[EnumMember]
		Calculator = 0x0040,
		/// <summary>狀態列</summary>
		[EnumMember]
		StatusBar = 0x0080,
		/// <summary>便利貼之標題</summary>
		[EnumMember]
		NoteTitle = 0x0100,
		/// <summary>便利貼之內容</summary>
		[EnumMember]
		NoteContent = 0x0200
	}

	/// <summary>路徑清單</summary>
	public enum Paths {
		/// <summary>主目錄</summary>
		Main = 0x01,
		/// <summary>設定檔</summary>
		Config = 0x02,
		/// <summary>便利貼來源</summary>
		Note = 0x04,
		/// <summary>記錄檔</summary>
		Log = 0x08
	}
	#endregion

	#region Global Support Classes
	/// <summary>可套用 <see cref="DateTime.ToString"/> 相關物件</summary>
	[DataContract(Name = "DateFormat")]
	public class Format {

		#region Fields
		/// <summary><see cref="DateTime.ToString"/> 格式</summary>
		[DataMember(Name = "FormattedString")]
		private string mFmt = string.Empty;
		/// <summary>套用於文化樣式之文化名稱，如 "en-US"、"zh-TW" 等</summary>
		[DataMember(Name = "CultureInfo")]
		private string mCult = string.Empty;
		#endregion

		#region Properties
		/// <summary>取得格式字串</summary>
		public string FormattedString { get { return mFmt; } }
		/// <summary>取得對應的文化特性</summary>
		public CultureInfo Culture { get { return new CultureInfo(mCult); } }
		#endregion

		#region Constructors
		/// <summary>建構可套用於 <see cref="DateTime.ToString"/> 之物件</summary>
		/// <param name="fmt">格式字串</param>
		/// <param name="cult">文化名稱</param>
		public Format(string fmt, string cult) {
			mFmt = fmt;
			mCult = cult;
		}
		#endregion

		#region Public Operations
		/// <summary>取得此物件的複製品</summary>
		/// <returns>複製品</returns>
		public Format Clone() {
			return new Format(mFmt, mCult);
		}

		/// <summary>將 <see cref="DateTime"/> 轉換為對應的字串描述</summary>
		/// <param name="time">欲轉換的 <see cref="DateTime"/></param>
		/// <returns>使用儲存的格式所產生的對應時間描述</returns>
		public string ToString(DateTime time) {
			CultureInfo info = new CultureInfo(mCult);
			return time.ToString(mFmt, info);
		}

		/// <summary>取得此物件的描述文字</summary>
		/// <returns>描述文字</returns>
		public override string ToString() {
			return $"{mCult}, {mFmt}";
		}
		#endregion
	}
	#endregion

	/// <summary>面板設定資訊</summary>
	[DataContract(Name = "Configurations")]
	[KnownType(typeof(Color))]
	[KnownType(typeof(DayOfWeek))]
	[KnownType(typeof(Font))]
	[KnownType(typeof(FontStyle))]
	[KnownType(typeof(Format))]
	[KnownType(typeof(GraphicsUnit))]
	[KnownType(typeof(PanelComponent))]
	[KnownType(typeof(PointF))]
	[KnownType(typeof(RectangleF))]
	[KnownType(typeof(Shortcut))]
	[KnownType(typeof(SizeF))]
	public class PanelConfig : IDisposable {

		#region Fields
		/// <summary>主面板大小與其位置</summary>
		[DataMember(Name = "WindowRectangle")]
		private RectangleF mWindRect;
		/// <summary>便利貼視窗大小</summary>
		[DataMember(Name = "NoteSize")]
		private SizeF mNoteSize;

		/// <summary>效能監視停靠點</summary>
		[DataMember(Name = "UsageDock")]
		private UsageDock mEffcDock;
		/// <summary>小功能捷徑</summary>
		[DataMember(Name = "Shortcut")]
		private Shortcut mShortcut;
		/// <summary>是否顯示秒數</summary>
		[DataMember(Name = "ShowSecond")]
		private bool mShowSec;

		/// <summary>各個 <see cref="Label"/> 位置</summary>
		[DataMember(Name = "Positions")]
		private Dictionary<PanelComponent, PointF> mPositions;
		/// <summary>各 <see cref="Label"/> 所使用的 <see cref="Font"/></summary>
		[DataMember(Name = "FontStyles")]
		private Dictionary<PanelComponent, Font> mFonts;
		/// <summary>主面板、便利貼與其他控制項所顯示的前景或背景顏色</summary>
		[DataMember(Name = "ColorStyles")]
		private Dictionary<PanelComponent, Color> mColors;
		/// <summary>日期與星期格式</summary>
		[DataMember(Name = "DateWeekFormat")]
		private Dictionary<PanelComponent, Format> mFormats;

		/// <summary>載入的設定檔路徑</summary>
		private string mConfigFile = null;
		#endregion

		#region Properties
		/// <summary>取得主面板位置與大小</summary>
		public RectangleF WindowRectangle { get { return new RectangleF(mWindRect.X, mWindRect.Y, mWindRect.Width, mWindRect.Height); } }
		/// <summary>取得便利貼預設視窗大小</summary>
		public SizeF NoteSize { get { return new SizeF(mNoteSize); } }
		/// <summary>取得效能監視停靠點</summary>
		public UsageDock UsageDocking { get { return mEffcDock; } }
		/// <summary>取得啟用的小功能捷徑</summary>
		public Shortcut Shortcuts { get { return mShortcut; } }
		/// <summary>取得是否顯示秒數</summary>
		public bool ShowSecond { get { return mShowSec; } }
		#endregion

		#region Constructors
		/// <summary>建立帶有預設值的設定資訊</summary>
		public PanelConfig() {
			var workRect = Screen.PrimaryScreen.WorkingArea;
			float x = workRect.X + workRect.Right - 385;
			float y = workRect.Y + workRect.Bottom - 245;
			mWindRect = new RectangleF(x, y, 385, 245);

			mNoteSize = new SizeF(300, 250);
			mEffcDock = UsageDock.Up;
			mShortcut = Shortcut.Calculator | Shortcut.Note | Shortcut.Radio;
			mShowSec = false;

			mPositions = new Dictionary<PanelComponent, PointF> {
				{ PanelComponent.Date, new PointF(65, 65) },
				{ PanelComponent.Week, new PointF(280, 65) },
				{ PanelComponent.Time, new PointF(20, 120) }
			};

			mFonts = new Dictionary<PanelComponent, Font> {
				{ PanelComponent.Date, new Font("Consolas", 24) },
				{ PanelComponent.Week, new Font("Consolas", 24) },
				{ PanelComponent.Time, new Font("Consolas", 72) },
				{ PanelComponent.NoteTitle, new Font("微軟正黑體", 12) },
				{ PanelComponent.NoteContent, new Font("微軟正黑體", 12) },
				{ PanelComponent.StatusBar, new Font("Agency FB", 12, FontStyle.Bold) }
			};

			mColors = new Dictionary<PanelComponent, Color> {
				{ PanelComponent.Background, Color.FromArgb(154, 200, 200, 200) },
				{ PanelComponent.Note, Color.FromArgb(154, 200, 200, 200) },
				{ PanelComponent.Date, Color.FromArgb(230, 0, 0, 0) },
				{ PanelComponent.Week, Color.FromArgb(230, 0, 0, 0) },
				{ PanelComponent.Time, Color.FromArgb(230, 0, 0, 0) },
				{ PanelComponent.NoteTitle, Color.Black },
				{ PanelComponent.NoteContent, Color.Black },
				{ PanelComponent.StatusBar, Color.FromArgb(230, 0, 0, 0) }
			};

			mFormats = new Dictionary<PanelComponent, Format> {
				{ PanelComponent.Date, new Format(@"yyyy 年 MM 月 dd 日", "zh-TW") },
				{ PanelComponent.Week, new Format(@"dddd", "zh-TW") }
			};

		}
		#endregion

		#region Other Operations
		/// <summary>設定設定檔路徑</summary>
		/// <param name="path">檔案路徑</param>
		private void SetConfigFile(string path) {
			mConfigFile = path;
		}

		/// <summary>取得此設定檔之複製品，深層複製</summary>
		public PanelConfig Clone() {
			PanelConfig copied = null;
			using (var ms = new MemoryStream()) {
				/* 將目前的資訊序列化儲存至 MemoryStream 裡 */
				var contSer = new DataContractSerializer(typeof(PanelConfig));
				contSer.WriteObject(ms, this);
				/* 從 MemoryStream 做反序列化，此即複製品 */
				ms.Position = 0;
				copied = contSer.ReadObject(ms) as PanelConfig;
				if (!string.IsNullOrEmpty(mConfigFile) && copied != null) copied.SetConfigFile(mConfigFile);
			}
			return copied;
		}

		/// <summary>複寫當前的設定檔至原始載入的文件，為 DataContract 序列化檔案</summary>
		public void SaveToFile() {
			if (!string.IsNullOrEmpty(mConfigFile)) {
				var setting = new XmlWriterSettings() { Indent = true, IndentChars = "\t" };
				using (var xw = XmlWriter.Create(mConfigFile, setting)) {
					var contSer = new DataContractSerializer(typeof(PanelConfig));
					contSer.WriteObject(xw, this);
				} 
			}
		}

		/// <summary>將目前的設定資訊匯出至文件，文件為 DataContract 序列化檔案</summary>
		/// <param name="path">欲儲存的路徑，如 @"D:\config.xml"</param>
		public void SaveToFile(string path) {
			mConfigFile = path;
			var setting = new XmlWriterSettings() { Indent = true, IndentChars = "\t" };
			using (var xw = XmlWriter.Create(path, setting)) {
				var contSer = new DataContractSerializer(typeof(PanelConfig));
				contSer.WriteObject(xw, this);
			}
		}

		/// <summary>載入已儲存的 DataContract 序列化檔案</summary>
		/// <param name="path">儲存的檔案路徑，如 @"D:\config.xml"</param>
		/// <returns>已載入的 <see cref="PanelConfig"/>，若檔案不存在則回傳 <see langword="null"/></returns>
		public static PanelConfig LoadFromFile(string path) {
			PanelConfig config = null;
			if (File.Exists(path)) {
				using (var xr = XmlReader.Create(path)) {
					var contSer = new DataContractSerializer(typeof(PanelConfig));
					config = contSer.ReadObject(xr) as PanelConfig;
					if (config != null) config.SetConfigFile(path);
				}
			}
			return config;
		}

		#endregion

		#region Changes
		/// <summary>設定小功能捷徑啟用狀態</summary>
		/// <param name="shortcut">欲更改的 <see cref="Shortcut"/></param>
		/// <param name="chk">(<see cref="true"/>)啟用  (<see cref="false"/>)禁用</param>
		public Shortcut ChangeShortcut(Shortcut shortcut, bool chk) {
			if (chk) mShortcut |= shortcut;
			else mShortcut &= ~shortcut;
			return mShortcut;
		}

		/// <summary>設定顯示秒數功能</summary>
		/// <param name="show">(<see cref="true"/>)顯示秒數  (<see cref="false"/>)僅顯示時、分</param>
		public void ChangeShowSecond(bool show) {
			mShowSec = show;
		}

		/// <summary>設定星期顯示格式</summary>
		/// <param name="idx">樣式代碼</param>
		/// <returns>新的格式物件執行個體，可直接 Assign</returns>
		/// <remarks>目前是直接寫死，請注意參照!</remarks>
		public Format ChangeWeekFormat(int idx) {
			var fmt = string.Empty;
			var cult = string.Empty;
			switch (idx) {
				case 0:
					fmt = "ddd"; cult = "en-US";
					break;
				case 1:
					fmt = "dddd"; cult = "en-US";
					break;
				case 2:
					fmt = "ddd"; cult = "zh-TW";
					break;
				case 3:
				default:
					fmt = "dddd"; cult = "zh-TW";
					break;
			}

			mFormats[PanelComponent.Week] = new Format(fmt, cult);
			return new Format(fmt, cult);
		}

		/// <summary>設定日期顯示格式</summary>
		/// <param name="idx">樣式代碼</param>
		/// <returns>新的格式物件執行個體，可直接 Assign</returns>
		/// <remarks>目前是直接寫死，請注意參照!</remarks>
		public Format ChangeDateFormat(int idx) {
			var fmt = string.Empty;
			var cult = string.Empty;
			switch (idx) {
				case 0:
					fmt = @"MM/dd"; cult = "en-US";
					break;
				case 1:
					fmt = @"MMM/dd"; cult = "en-US";
					break;
				case 2:
					fmt = @"yyyy/MM/dd"; cult = "en-US";
					break;
				case 3:
					fmt = @"yyyy-MM-dd"; cult = "en-US";
					break;
				case 4:
					fmt = @"yyyy/MMM/dd"; cult = "en-US";
					break;
				case 5:
					fmt = @"yyyy-MMM-dd"; cult = "en-US";
					break;
				case 6:
					fmt = @"MM/dd/yyyy"; cult = "en-US";
					break;
				case 7:
					fmt = @"MM/dd/yy"; cult = "en-US";
					break;
				case 8:
					fmt = @"MM 月 dd 日"; cult = "zh-TW";
					break;
				case 9:
				default:
					fmt = @"yyyy 年 MM 月 dd 日"; cult = "zh-TW";
					break;
			}

			mFormats[PanelComponent.Date] = new Format(fmt, cult);
			return new Format(fmt, cult);
		}

		/// <summary>設定字體樣式</summary>
		/// <param name="component">欲更改的元件</param>
		/// <param name="font">欲設定的新字體</param>
		public void ChangeFont(PanelComponent component, Font font) {
			if (mFonts.ContainsKey(component)) {
				var oldFont = mFonts[component];
				mFonts[component] = font.Copy();
				oldFont?.Dispose();
			}
		}

		/// <summary>設定相關顏色</summary>
		/// <param name="component">欲更改的元件</param>
		/// <param name="color">欲設定的新顏色</param>
		public void ChangeColor(PanelComponent component, Color color) {
			if (mColors.ContainsKey(component)) {
				mColors[component] = color.Clone();
			}
		}

		/// <summary>設定相關顏色透明度</summary>
		/// <param name="component">欲更改的元件名稱</param>
		/// <param name="idx">0 ~ 9 等級，分別對應 10% ~ 100%</param>
		/// <returns>最後產生的新顏色</returns>
		public Color ChangeAlpha(PanelComponent component, int idx) {
			var color = Color.Empty;
			if (mColors.ContainsKey(component)) {
				var alpha = (int)Math.Ceiling(((idx + 1) / 10F) * 255);
				var oriClr = mColors[component];
				color = Color.FromArgb(alpha, oriClr);
				mColors[component] = color;
			}
			return color;
		}

		/// <summary>設定效能監視停靠點</summary>
		/// <param name="idx">對應 <see cref="UsageDock"/> 的數值</param>
		public void ChangeUsageDock(UsageDock idx) {
			mEffcDock = idx;
		}
		#endregion

		#region Gets
		/// <summary>取得元件之座標</summary>
		/// <param name="component">欲取得座標的面板元件</param>
		/// <param name="point">目前設定檔所儲存的座標</param>
		/// <returns>(True)成功取得 (False)取得失敗</returns>
		public bool GetPosition(PanelComponent component, out PointF point) {
			var exist = mPositions.ContainsKey(component);
			point = exist ? mPositions[component].Clone() : PointF.Empty;
			return exist;
		}

		/// <summary>取得元件之字型</summary>
		/// <param name="component">欲取得字型的面板元件</param>
		/// <param name="font">目前設定檔所儲存的字型</param>
		/// <returns>(True)成功取得 (False)取得失敗</returns>
		public bool GetFont(PanelComponent component, out Font font) {
			var exist = mFonts.ContainsKey(component);
			font = exist ? mFonts[component].Copy() : null;
			return exist;
		}

		/// <summary>取得元件之前景或背景顏色</summary>
		/// <param name="component">欲取得顏色的面板元件</param>
		/// <param name="color">目前設定檔所儲存的顏色</param>
		/// <returns>(True)成功取得 (False)取得失敗</returns>
		public bool GetColor(PanelComponent component, out Color color) {
			var exist = mColors.ContainsKey(component);
			color = exist ? mColors[component].Clone() : Color.Empty;
			return exist;
		}

		/// <summary>取得元件之透明程度，以 0 ~ 9 代表 10% ~ 100%</summary>
		/// <param name="component">欲取得透明程度的面板元件</param>
		/// <param name="level">目前設定檔所儲存的透明程度</param>
		/// <returns>(True)成功取得 (False)取得失敗</returns>
		public bool GetAlpha(PanelComponent component, out int level) {
			var exist = mColors.ContainsKey(component);
			if (exist) {
				var color = mColors[component];
				var alpha = (color.A / 255.0) * 100.0;
				var quotient = (int)(alpha / 10);
				if ((alpha % 10) > 5) quotient++;
				level = quotient - 1;
			} else {
				level = -1;
			}
			return exist;
		}

		/// <summary>取得元件之 <see cref="System.Windows.Media.Brush"/></summary>
		/// <param name="component">欲取得 <see cref="System.Windows.Media.Brush"/> 的面板元件</param>
		/// <param name="brush">目前設定檔所儲存的 <see cref="System.Windows.Media.Brush"/></param>
		/// <returns>(True)成功取得 (False)取得失敗</returns>
		public bool GetBrush(PanelComponent component, out System.Windows.Media.Brush brush) {
			var exist = mColors.ContainsKey(component);
			brush = exist ? mColors[component].GetBrush() : null;
			return exist;
		}

		/// <summary>取得元件之格式內容</summary>
		/// <param name="component">欲取得格式的面板元件</param>
		/// <param name="format">目前設定檔所儲存的格式</param>
		/// <returns>(True)成功取得 (False)取得失敗</returns>
		public bool GetFormat(PanelComponent component, out Format format) {
			var exist = mFormats.ContainsKey(component);
			format = exist ? mFormats[component].Clone() : null;
			return exist;
		}
		#endregion

		#region IDisposable Support
		private bool disposedValue = false; // 偵測多餘的呼叫

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: 處置 Managed 狀態 (Managed 物件)。
					foreach (var kvp in mFonts) {
						kvp.Value.Dispose();
					}

					mPositions.Clear();
					mFonts.Clear();
					mColors.Clear();
					mFormats.Clear();
				}

				// TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
				// TODO: 將大型欄位設為 null。
				mPositions = null;
				mFonts = null;
				mColors = null;
				mFormats = null;
				disposedValue = true;
			}
		}

		// TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
		// ~PanelConfig() {
		//   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
		//   Dispose(false);
		// }

		// 加入這個程式碼的目的在正確實作可處置的模式。
		public void Dispose() {
			// 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
			Dispose(true);
			// TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
