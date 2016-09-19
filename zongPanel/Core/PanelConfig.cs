using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using zongPanel.Library;

namespace zongPanel {

	public enum EfficacyDock {
		HIDDEN,
		UP,
		DOWN
	}

	public enum Shortcut {
		NOTE = 1,
		RADIO = 2,
		CALCULATOR = 4
	}

	[Serializable]
	public class PanelConfig : ISerializable {

		private RectangleF mWindRect;
		private PointF mDatePos;
		private PointF mWeekPos;
		private PointF mTimePos;
		private Color mWindBg;
		private Color mNoteBg;
		private Color mDateFg;
		private Color mWeekFg;
		private Color mTimeFg;
		private Color mNoteTitFg;
		private Color mNoteCntFg;
		private Font mDateFont;
		private Font mWeekFont;
		private Font mTimeFont;
		private Font mNoteTitFont;
		private Font mNoteCntFont;
		private Font mEffcFont;
		private SizeF mNoteSize;
		private EfficacyDock mEffcDock;
		private Shortcut mShortcut;

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
			mEffcDock = EfficacyDock.UP;
			mShortcut = Shortcut.CALCULATOR | Shortcut.NOTE | Shortcut.RADIO;
		}

		public PanelConfig(SerializationInfo info, StreamingContext context) {
			mWindRect = (RectangleF)info.GetValue("PanelRectangle", typeof(RectangleF));
			mDatePos = (PointF)info.GetValue("DatePosition", typeof(PointF));
			mWeekPos = (PointF)info.GetValue("WeekPosition", typeof(PointF));
			mTimePos = (PointF)info.GetValue("TimePosition", typeof(PointF));
			mWindBg = (Color)info.GetValue("PanelBackground", typeof(Color));
			mNoteBg = (Color)info.GetValue("NoteBackground", typeof(Color));
			mDateFg = (Color)info.GetValue("DateForeground", typeof(Color));
			mWeekFg = (Color)info.GetValue("WeekForeground", typeof(Color));
			mTimeFg = (Color)info.GetValue("TimeForeground", typeof(Color));
			mNoteTitFg = (Color)info.GetValue("NoteTitleForeground", typeof(Color));
			mNoteCntFg = (Color)info.GetValue("NoteContentForeground", typeof(Color));
			mDateFont = (Font)info.GetValue("DateFont", typeof(Font));
			mWeekFont = (Font)info.GetValue("WeekFont", typeof(Font));
			mTimeFont = (Font)info.GetValue("TimeFont", typeof(Font));
			mNoteTitFont = (Font)info.GetValue("NoteTitleFont", typeof(Font));
			mNoteCntFont = (Font)info.GetValue("NoteContentFont", typeof(Font));
			mEffcFont = (Font)info.GetValue("UsageFont", typeof(Font));
			mNoteSize = (SizeF)info.GetValue("NoteSize", typeof(SizeF));
			mEffcDock = (EfficacyDock)info.GetValue("UsageDock", typeof(EfficacyDock));
			mShortcut = (Shortcut)info.GetValue("Shortcut", typeof(Shortcut));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("PanelRectangle", mWindRect);
			info.AddValue("DatePosition", mDatePos);
			info.AddValue("WeekPosition", mWeekPos);
			info.AddValue("TimePosition", mTimePos);
			info.AddValue("PanelBackground", mWindBg);
			info.AddValue("NoteBackground", mNoteBg);
			info.AddValue("DateForeground", mDateFg);
			info.AddValue("WeekForeground", mWeekFg);
			info.AddValue("TimeForeground", mTimeFg);
			info.AddValue("NoteTitleForeground", mNoteTitFg);
			info.AddValue("NoteContentForeground", mNoteCntFg);
			info.AddValue("DateFont", mDateFont);
			info.AddValue("WeekFont", mWeekFont);
			info.AddValue("TimeFont", mTimeFont);
			info.AddValue("NoteTitleFont", mNoteTitFont);
			info.AddValue("NoteContentFont", mNoteCntFont);
			info.AddValue("UsageFont", mEffcFont);
			info.AddValue("NoteSize", mNoteSize);
			info.AddValue("UsageDock", mEffcDock);
			info.AddValue("Shortcut", mShortcut);
		}

		public PanelConfig Clone() {
			PanelConfig copied = null;
			using (MemoryStream ms = new MemoryStream()) {
				SoapFormatter sf = new SoapFormatter();
				sf.Serialize(ms, this);

				ms.Position = 0;
				copied = sf.Deserialize(ms) as PanelConfig;
			}
			return copied;
		}
	}
}
