using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuiTestLib
{
	public static class Format
	{
		private const ByteDenomination DENOMINATION = ByteDenomination.B;
		private const string CPUSTRINGFORMAT = "{0}%";
		private const string RAMSTRINGFORMAT = "{0}B";
		private const string CPUFLOATFORMAT = "##0.##";
		private const string CPUFLOATFORMAT_LONG = "00.00000";
		private const string RAMFLOATFORMAT = "##0.0##";

		private const string DATEFORMAT = "yy-MM-dd HH:mm:ss";
		private const string DURATIONFORMAT = "#0.0#";
		private const string DURATIONFORMAT_LONG = "00.00000";

		public static string Cpu(float cpu) { return Cpu(cpu, false);}
		public static string Cpu(float cpu, bool longformat)
		{
			if (longformat) { return cpu.ToString(CPUFLOATFORMAT_LONG); }
			else { return String.Format(CPUSTRINGFORMAT, cpu.ToString(CPUFLOATFORMAT)); }

		}

		public static string Ram(float ram)
		{
			return String.Format(RAMSTRINGFORMAT, (ram / (int)DENOMINATION).ToString(RAMFLOATFORMAT));
		}

		public static string DateAndTime(DateTime time)
		{
			return time.ToString(DATEFORMAT);
		}
		
		public static string Duration(TimeSpan duration) { return Duration (duration, false); }
		public static string Duration(TimeSpan duration, bool shortformat)
		{
			if (shortformat) { return duration.TotalSeconds.ToString(DURATIONFORMAT); }
			else { return duration.TotalSeconds.ToString(DURATIONFORMAT_LONG); }
		}

		public enum ByteDenomination
		{
			B = 1,
			KB = 1000,
			KiB = 1024,
			MB = 1000000,
			MiB = 1048576,
			GB = 1000000000,
			GiB = 1073741824,
		}
	}
}
