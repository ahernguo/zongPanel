using System.Windows.Controls;
using System.Windows.Media;

namespace zongPanel {

	/// <summary>含有圖片的可針對滑鼠行為進行 <see cref="Visibility"/> 更改的 <see cref="Button"/></summary>
	public class VisibilityChangeableButton : Button {

		#region Properties
		/// <summary>取得或設定按鈕內欲顯示的圖片</summary>
		public ImageSource Source { get; set; }
		#endregion

		#region Constructors
		/// <summary>建構可更改 <see cref="Visibility"/> 的 <see cref="Button"/></summary>
		public VisibilityChangeableButton() {

		}
		#endregion
	}
}
