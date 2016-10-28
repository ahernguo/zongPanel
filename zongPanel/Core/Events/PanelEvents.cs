using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zongPanel {

	/// <summary>時間事件參數</summary>
	/// <param name="timeStr">已套用格式後的時間字串</param>
	public delegate void TimeEventArgs(string timeStr);
	/// <summary>日期事件參數</summary>
	/// <param name="timeStr">已套用格式後的日期字串</param>
	/// <param name="weekStr">已套用格式後的星期字串</param>
	public delegate void DateEventArgs(string dateStr, string weekStr);
	/// <summary>已有新的樣式產生，載入引數之設定至介面</summary>
	/// <param name="config">新的面板設定資訊</param>
	public delegate void ConfigArgs(PanelConfig config);

}
