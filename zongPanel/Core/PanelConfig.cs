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
		HIDDEN = 0,
		/// <summary>停靠於主面板上方</summary>
		[EnumMember]
		UP = 1,
		/// <summary>停靠於主面板下方</summary>
		[EnumMember]
		DOWN = 2
	}

	/// <summary>主面板之功能</summary>
	[DataContract]
	[Flags]
	public enum Shortcut {
		/// <summary>便利貼</summary>
		[EnumMember]
		NOTE = 1,
		/// <summary>網路收音機</summary>
		[EnumMember]
		RADIO = 2,
		/// <summary>小算盤捷徑</summary>
		[EnumMember]
		CALCULATOR = 4
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
	[KnownType(typeof(PointF))]
	[KnownType(typeof(RectangleF))]
	[KnownType(typeof(Shortcut))]
	[KnownType(typeof(SizeF))]
	public class PanelConfig {

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
		private Dictionary<string, PointF> mPositions;
		/// <summary>各 <see cref="Label"/> 所使用的 <see cref="Font"/></summary>
		[DataMember(Name = "FontStyles")]
		private Dictionary<string, Font> mFonts;
		/// <summary>主面板、便利貼與其他控制項所顯示的前景或背景顏色</summary>
		[DataMember(Name = "ColorStyles")]
		private Dictionary<string, Color> mColors;
		/// <summary>日期與星期格式</summary>
		[DataMember(Name = "DateWeekFormat")]
		private Dictionary<string, Format> mFormats;
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
		/// <summary>取得日期標籤位置</summary>
		public PointF DatePosition { get { return mPositions.ContainsKey("Date") ? mPositions["Date"].Clone() : PointF.Empty; } }
		/// <summary>取得星期標籤位置</summary>
		public PointF WeekPosition { get { return mPositions.ContainsKey("Week") ? mPositions["Week"].Clone() : PointF.Empty; } }
		/// <summary>取得時間標籤位置</summary>
		public PointF TimePosition { get { return mPositions.ContainsKey("Time") ? mPositions["Time"].Clone() : PointF.Empty; } }
		/// <summary>取得日期標籤字體</summary>
		public Font DateFont { get { return mFonts.ContainsKey("Date") ? mFonts["Date"].Copy() : null; } }
		/// <summary>取得星期標籤字體</summary>
		public Font WeekFont { get { return mFonts.ContainsKey("Week") ? mFonts["Week"].Copy() : null; } }
		/// <summary>取得時間標籤字體</summary>
		public Font TimeFont { get { return mFonts.ContainsKey("Time") ? mFonts["Time"].Copy() : null; } }
		/// <summary>取得便利貼標題字體</summary>
		public Font NoteTitleFont { get { return mFonts.ContainsKey("Title") ? mFonts["Title"].Copy() : null; } }
		/// <summary>取得便利貼內容字體</summary>
		public Font NoteContentFont { get { return mFonts.ContainsKey("Content") ? mFonts["Content"].Copy() : null; } }
		/// <summary>取得效能監視字體</summary>
		public Font UsageFont { get { return mFonts.ContainsKey("Usage") ? mFonts["Usage"].Copy() : null; } }
		/// <summary>取得主面板背景顏色</summary>
		public Color PanelBackground { get { return mColors.ContainsKey("PanelBg") ? mColors["PanelBg"].Clone() : Color.Empty; } }
		/// <summary>取得便利貼背景顏色</summary>
		public Color NoteBackground { get { return mColors.ContainsKey("NoteBg") ? mColors["NoteBg"].Clone() : Color.Empty; } }
		/// <summary>取得日期標籤文字顏色</summary>
		public Color DateForeground { get { return mColors.ContainsKey("DateFg") ? mColors["DateFg"].Clone() : Color.Empty; } }
		/// <summary>取得星期標籤文字顏色</summary>
		public Color WeekForeground { get { return mColors.ContainsKey("WeekFg") ? mColors["WeekFg"].Clone() : Color.Empty; } }
		/// <summary>取得時間標籤文字顏色</summary>
		public Color TimeForeground { get { return mColors.ContainsKey("TimeFg") ? mColors["TimeFg"].Clone() : Color.Empty; } }
		/// <summary>取得便利貼標題文字顏色</summary>
		public Color NoteTitleForeground { get { return mColors.ContainsKey("NoteTitFg") ? mColors["NoteTitFg"].Clone() : Color.Empty; } }
		/// <summary>取得便利貼內容文字顏色</summary>
		public Color NoteContentForeground { get { return mColors.ContainsKey("NoteCntFg") ? mColors["NoteCntFg"].Clone() : Color.Empty; } }
		/// <summary>取得效能監視文字顏色</summary>
		public Color UsageForeground { get { return mColors.ContainsKey("UsageFg") ? mColors["UsageFg"].Clone() : Color.Empty; } }
		/// <summary>取得日期於 <see cref="DateTime.ToString"/> 之相關格式</summary>
		public Format DateFormat { get { return mFormats.ContainsKey("Date") ? mFormats["Date"].Clone() : null; } }
		/// <summary>取得星期於 <see cref="DateTime.ToString"/> 之相關格式</summary>
		public Format WeekFormat { get { return mFormats.ContainsKey("Week") ? mFormats["Week"].Clone() : null; } }
		#endregion

		#region Constructors
		/// <summary>建立帶有預設值的設定資訊</summary>
		public PanelConfig() {
			var workRect = Screen.PrimaryScreen.WorkingArea;
			float x = workRect.X + workRect.Right - 385;
			float y = workRect.Y + workRect.Bottom - 245;
			mWindRect = new RectangleF(x, y, 385, 245);

			mNoteSize = new SizeF(300, 250);
			mEffcDock = UsageDock.UP;
			mShortcut = Shortcut.CALCULATOR | Shortcut.NOTE | Shortcut.RADIO;
			mShowSec = false;

			mPositions = new Dictionary<string, PointF> {
				{ "Date",  new PointF(65, 65) }, { "Week",  new PointF(280, 65) }, { "Time",  new PointF(20, 120) }
			};

			mFonts = new Dictionary<string, Font> {
				{ "Date", new Font("Consolas", 24) }, { "Week", new Font("Consolas", 24) },
				{ "Time", new Font("Consolas", 72) }, { "Title", new Font("微軟正黑體", 12) },
				{ "Content", new Font("微軟正黑體", 12) }, { "Usage", new Font("Agency FB", 12, FontStyle.Bold) }
			};

			mColors = new Dictionary<string, Color> {
				{ "PanelBg", Color.FromArgb(154, 200, 200, 200) }, { "NoteBg", Color.FromArgb(154, 200, 200, 200) },
				{ "DateFg", Color.FromArgb(230, 0, 0, 0) }, { "WeekFg", Color.FromArgb(230, 0, 0, 0) },
				{ "TimeFg", Color.FromArgb(230, 0, 0, 0) }, { "NoteTitFg", Color.Black },
				{ "NoteCntFg", Color.Black }, { "UsageFg",  Color.FromArgb(230, 0, 0, 0) }
			};

			mFormats = new Dictionary<string, Format> {
				{ "Date", new Format(@"yyyy 年 MM 月 dd 日", "zh-TW") },
				{ "Week", new Format(@"dddd", "zh-TW") }
			};

		}
		#endregion

		#region Public Operations
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
			}
			return copied;
		}

		/// <summary>設定小功能捷徑啟用狀態</summary>
		/// <param name="tag">功能名稱，對應 <see cref="Shortcut"/></param>
		/// <param name="chk">(<see cref="true"/>)啟用  (<see cref="false"/>)禁用</param>
		public void ChangeShortcut(string tag, bool chk) {
			var shortcut = (Shortcut)Enum.Parse(typeof(Shortcut), tag);

			if (chk) mShortcut |= shortcut;
			else mShortcut &= ~shortcut;
		}

		/// <summary>設定顯示秒數功能</summary>
		/// <param name="show">(<see cref="true"/>)顯示秒數  (<see cref="false"/>)僅顯示時、分</param>
		public void ChangeShowSecond(bool show) {
			mShowSec = show;
		}

		/// <summary>設定星期顯示格式</summary>
		/// <param name="idx">樣式代碼</param>
		/// <remarks>目前是直接寫死，請注意參照!</remarks>
		public void ChangeWeekFormat(int idx) {
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

			if (mFormats.ContainsKey("Week"))
				mFormats["Week"] = new Format(fmt, cult);
		}

		/// <summary>設定日期顯示格式</summary>
		/// <param name="idx">樣式代碼</param>
		/// <remarks>目前是直接寫死，請注意參照!</remarks>
		public void ChangeDateFormat(int idx) {
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

			if (mFormats.ContainsKey("Date"))
				mFormats["Date"] = new Format(fmt, cult);
		}

		/// <summary>設定字體樣式，調用 <see cref="FontDialog"/> 供使用者選取</summary>
		/// <param name="tag">欲設定的控制項名稱</param>
		/// <remarks>目前採用 Tag 方式，請注意參照建構元的預設名稱</remarks>
		public Font ChangeFont(string tag) {
			Font font = null;
			using (var dialog = new FontDialog()) {
				if (mFonts[tag] != null) dialog.Font = mFonts[tag].Clone() as Font;
				if (dialog.ShowDialog() == DialogResult.OK) {
					font = dialog.Font.Clone() as Font;
				}
			}

			if (font != null && mFonts.ContainsKey(tag))
				mFonts[tag] = font;

			return font;
		}

		/// <summary>設定相關顏色，調用 <see cref="ColorDialog"/> 供使用者選取</summary>
		/// <param name="tag">欲設定的控制項名稱</param>
		/// <param name="oriColor">當前顯示的顏色</param>
		/// <remarks>目前採用 Tag 方式，請注意參照建構元的預設名稱</remarks>
		public Color ChangeColor(string tag, Color oriColor) {
			var clr = Color.Empty;
			using (var dialog = new ColorDialog()) {
				dialog.AllowFullOpen = true;
				dialog.AnyColor = true;
				dialog.FullOpen = true;
				dialog.Color = oriColor;
				if (dialog.ShowDialog() == DialogResult.OK) {
					clr = Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
				}
			}

			if (!clr.Equals(Color.Empty) && mColors.ContainsKey(tag)) {
				mColors[tag] = clr;
			} else if (clr.Equals(Color.Empty)) {
				clr = oriColor.Clone();
			}

			return clr;
		}

		/// <summary>設定相關顏色透明度</summary>
		/// <param name="tag">欲設定的控制項名稱</param>
		/// <param name="idx">0 ~ 9 等級，分別對應 10% ~ 100%</param>
		/// <remarks>目前採用 Tag 方式，請注意參照建構元的預設名稱</remarks>
		public void ChangeAlpha(string tag, int idx) {
			if (mColors.ContainsKey(tag)) {
				int alpha = ((idx + 1) / 10) * 255;
				Color oriClr = mColors[tag];
				mColors[tag] = Color.FromArgb(alpha, oriClr);
			}
		}

		/// <summary>設定效能監視停靠點</summary>
		/// <param name="idx">對應 <see cref="UsageDock"/> 的數值</param>
		public void ChangeUsageDock(int idx) {
			mEffcDock = (UsageDock)idx;
		}

		/// <summary>將目前的設定資訊匯出至文件，文件為 DataContract 序列化檔案</summary>
		/// <param name="path">欲儲存的路徑，如 @"D:\config.xml"</param>
		public void SaveToFile(string path) {
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
				}
			}
			return config;
		}
		#endregion
	}
}
