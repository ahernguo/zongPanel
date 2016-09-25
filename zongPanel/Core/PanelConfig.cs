using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using zongPanel.Library;

namespace zongPanel {

	/// <summary>效能監視面板停靠點</summary>
	public enum UsageDock {
		/// <summary>不顯示效能監視面板，隱藏</summary>
		HIDDEN,
		/// <summary>停靠於主面板上方</summary>
		UP,
		/// <summary>停靠於主面板下方</summary>
		DOWN
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

	/// <summary>面板設定資訊</summary>
	[DataContract(Name = "Configurations")]
	[KnownType(typeof(Color))]
	[KnownType(typeof(DayOfWeek))]
	[KnownType(typeof(Font))]
	[KnownType(typeof(FontStyle))]
	[KnownType(typeof(GraphicsUnit))]
	[KnownType(typeof(PointF))]
	[KnownType(typeof(RectangleF))]
	[KnownType(typeof(Shortcut))]
	[KnownType(typeof(SizeF))]
	public class PanelConfig  {

		[DataMember(Name = "WindowRectangle")]
		private RectangleF mWindRect;
		[DataMember(Name = "DatePosition")]
		private PointF mDatePos;
		[DataMember(Name = "WeekPosition")]
		private PointF mWeekPos;
		[DataMember(Name = "TimePosition")]
		private PointF mTimePos;
		[DataMember(Name = "WindowBackground")]
		private Color mWindBg;
		[DataMember(Name = "NoteBackground")]
		private Color mNoteBg;
		[DataMember(Name = "DateForeground")]
		private Color mDateFg;
		[DataMember(Name = "WeekForeground")]
		private Color mWeekFg;
		[DataMember(Name = "TimeForeground")]
		private Color mTimeFg;
		[DataMember(Name = "NoteTitleForeground")]
		private Color mNoteTitFg;
		[DataMember(Name = "NoteContentForeground")]
		private Color mNoteCntFg;
		[DataMember(Name = "DateFont")]
		private Font mDateFont;
		[DataMember(Name = "WeekFont")]
		private Font mWeekFont;
		[DataMember(Name = "TimeFont")]
		private Font mTimeFont;
		[DataMember(Name = "NoteTitleFont")]
		private Font mNoteTitFont;
		[DataMember(Name = "NoteContentFont")]
		private Font mNoteCntFont;
		[DataMember(Name = "UsageFont")]
		private Font mEffcFont;
		[DataMember(Name = "NoteSize")]
		private SizeF mNoteSize;
		[DataMember(Name = "UsageDock")]
		private UsageDock mEffcDock;
		[DataMember(Name = "Shortcut")]
		private Shortcut mShortcut;
		[DataMember(Name = "DateFormat")]
		private string mDateFmt;
		[DataMember(Name = "WeekFormat")]
		private Dictionary<DayOfWeek, string> mWeekFmt;
		[DataMember(Name = "ShowSecond")]
		private bool mShowSec;

		public PanelConfig() {
			var workRect = Screen.PrimaryScreen.WorkingArea;
			float x = workRect.X + workRect.Right - 385;
			float y = workRect.Y + workRect.Bottom - 245;
			mWindRect = new RectangleF(x, y, 385, 245);
			mDatePos = new PointF(65, 65);
			mWeekPos = new PointF(280, 65);
			mTimePos = new PointF(20, 120);
			mWindBg = Color.FromArgb(154, 200, 200, 200);
			mNoteBg = Color.FromArgb(154, 200, 200, 200);
			mDateFg = Color.FromArgb(230, 0, 0, 0);
			mWeekFg = Color.FromArgb(230, 0, 0, 0);
			mTimeFg = Color.FromArgb(230, 0, 0, 0);
			mNoteTitFg = Color.Black;
			mNoteCntFg = Color.Black;
			mDateFont = new Font("Consolas", 24);
			mWeekFont = new Font("Consolas", 24);
			mTimeFont = new Font("Consolas", 72);
			mNoteTitFont = new Font("微軟正黑體", 12);
			mNoteCntFont = new Font("微軟正黑體", 12);
			mEffcFont = new Font("Agency FB", 12, FontStyle.Bold);
			mNoteSize = new SizeF(300, 250);
			mEffcDock = UsageDock.UP;
			mShortcut = Shortcut.CALCULATOR | Shortcut.NOTE | Shortcut.RADIO;
			mShowSec = false;
			mDateFmt = @"dd/MMM/yyyy";
			mWeekFmt = new Dictionary<DayOfWeek, string> {
				{ DayOfWeek.Sunday, "Sun" }, { DayOfWeek.Monday, "Mon" },
				{ DayOfWeek.Tuesday, "Tue" }, { DayOfWeek.Wednesday, "Wed" },
				{ DayOfWeek.Thursday, "Thu" }, { DayOfWeek.Friday, "Fri" },
				{ DayOfWeek.Saturday, "Sat" }
			};
		}

		/// <summary>取得此設定檔之複製品，深層複製</summary>
		public PanelConfig Clone() {
			PanelConfig copied = null;
			using (MemoryStream ms = new MemoryStream()) {
				/* 將目前的資訊序列化儲存至 MemoryStream 裡 */
				DataContractSerializer contSer = new DataContractSerializer(typeof(PanelConfig));
				contSer.WriteObject(ms, this);
				/* 從 MemoryStream 做反序列化，此即複製品 */
				ms.Position = 0;
				copied = contSer.ReadObject(ms) as PanelConfig;
			}
			return copied;
		}
	}
}
