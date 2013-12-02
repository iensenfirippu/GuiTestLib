using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuiTestLib
{
	public static class Format
	{
		private const ByteDenomination DENOMINATION = ByteDenomination.KB;
		private const string CPUSTRINGFORMAT = "{0}%";
		private const string RAMSTRINGFORMAT = "{0}KB";
		private const string CPUFLOATFORMAT = "##0.##";
		private const string RAMFLOATFORMAT = "##0.0##";

		public static string Cpu(float cpu) //{ return Cpu(cpu, true); }
		//public static string Cpu(float cpu, bool dividebycpu)
		{
			float value = cpu;
			//if (!dividebycpu) { value = cpu; }
			//else { value = (cpu / Environment.ProcessorCount); }
			return String.Format(CPUSTRINGFORMAT, value.ToString(CPUFLOATFORMAT));
		}

		public static string Ram(float ram)
		{
			return String.Format(RAMSTRINGFORMAT, (ram / (int)DENOMINATION).ToString(RAMFLOATFORMAT));
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
