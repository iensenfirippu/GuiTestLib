using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using System.Text.RegularExpressions;
using System.Timers;

namespace GuiTestLib
{
	public class GuiTracker
	{
		private const int TICKTIME = 100;

		private GuiTestLib.Random _random;
		private ResourceUsage _resourceusage;
		private Timer _timer;

		private string _application;
		private Framework _framework;
		private Toolkit _toolkit;
		
		private DateTime _starttime;
		private DateTime _endtime;
		private TimeSpan _executiontime;

		public GuiTracker(string application, Framework framework, Toolkit toolkit)
		{
			_starttime = DateTime.Now;
			//_endtime = null;
			_executiontime = TimeSpan.Zero;

			_random = new GuiTestLib.Random();
			_resourceusage = new ResourceUsage(framework);
			_timer = new Timer(TICKTIME);
			_timer.Elapsed += new ElapsedEventHandler(Tick);

			_application = application;
			_framework = framework;
			_toolkit = toolkit;
			
			_timer.Start();	
		}
		
		public GuiTestLib.Random Random { get { return _random; } }
		public ResourceUsage Usage { get { return _resourceusage; } }
		public TimeSpan ExecutionTime { get { return _executiontime; } }
		
		private void Tick(object sender, ElapsedEventArgs e)
		{
			_resourceusage.TakeSnapshot();
		}
		
		public void Stop()
		{
			_timer.Stop();
			bool error = _resourceusage.Stop();
			_random.Disable();
			_endtime = DateTime.Now;
			_executiontime = _endtime - _starttime;

			WriteToFile(error);
		}

		public void WriteToFile(bool error)
		{
			string savedirectory = GetSaveDirectory();

			// Create the file to write
			string content = this.ToString();

			// Save the file
			for (int i = 1; i < 50; i++)
			{
				string filepath = savedirectory;
				if (error) { filepath += "error"; } else { filepath += "run";}
				filepath += i.ToString("00") + ".dat";
				if (!File.Exists(filepath))
				{
					File.WriteAllText(filepath, content);
					i = 50;
				}
			}
		}

		public string GetSaveDirectory()
		{
			// Get Home directory and add the subdirectory for the save location
			string savedirectory = (Environment.OSVersion.Platform == PlatformID.Unix || 
			                        Environment.OSVersion.Platform == PlatformID.MacOSX)
				? Environment.GetEnvironmentVariable("HOME")
					: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			savedirectory += "/GuiTest/Data/" + _application + "/" + _framework + "-" + _toolkit + "/";

			// Create the save directory if it doesn't exist
			if (!Directory.Exists(savedirectory)) { Directory.CreateDirectory(savedirectory); }
			return savedirectory;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append("######################\n");
			sb.Append("# Application: ").Append(_application).Append("\n");
			sb.Append("# Framework: ").Append(_framework).Append("\n");
			sb.Append("# Toolkit: ").Append(_toolkit).Append("\n");
			sb.Append("# --------------------\n");
			sb.Append("# Started: ").Append(Format.DateAndTime(_starttime)).Append("\n");
			sb.Append("# Ended: ").Append(Format.DateAndTime(_endtime)).Append("\n");
			sb.Append("# Execution time: ").Append(Format.Duration(_executiontime, true)).Append(" seconds\n");
			sb.Append("# --------------------\n");
			sb.Append("# Minimum CPU usage: ").Append(Format.Cpu(_resourceusage.CpuMin)).Append("\n");
			sb.Append("# Average CPU usage: ").Append(Format.Cpu(_resourceusage.CpuAvg)).Append("\n");
			sb.Append("# Maximum CPU usage: ").Append(Format.Cpu(_resourceusage.CpuMax)).Append("\n");
			sb.Append("# --------------------\n");
			sb.Append("# Minimum RAM usage: ").Append(Format.RamDisplay(_resourceusage.RamMin)).Append("\n");
			sb.Append("# Average RAM usage: ").Append(Format.RamDisplay(_resourceusage.RamAvg)).Append("\n");
			sb.Append("# Maximum RAM usage: ").Append(Format.RamDisplay(_resourceusage.RamMax)).Append("\n");
			sb.Append("######################\n\n");

			foreach (ResourceSnapshot rs in _resourceusage.Snapshots)
			{
				sb.Append(Format.Duration(rs.TimeStamp - _starttime)).Append("\t");
				sb.Append(Format.Cpu(rs.Cpu, true)).Append("\t").Append(Format.Ram(rs.Ram));
				if (rs.Name != string.Empty) { sb.Append("\t").Append(rs.Name); }
				sb.Append("\n");
			}

			return sb.ToString();
		}

		public enum Framework
		{
			Mono,
			Dnet
		}

		public enum Toolkit
		{
			Gtk,
			Win,
			Wpf
		}
	}
}
