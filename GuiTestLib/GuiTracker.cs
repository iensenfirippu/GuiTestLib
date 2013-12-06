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

		/*public GuiTracker(string fromstring)
		{
			Regex.
		}*/

		public GuiTracker(string application, Framework framework, Toolkit toolkit)
		{
			_random = new GuiTestLib.Random();
			_resourceusage = new ResourceUsage(framework);
			_timer = new Timer(TICKTIME);
			_timer.Elapsed += new ElapsedEventHandler(Tick);

			_application = application;
			_framework = framework;
			_toolkit = toolkit;

			_starttime = DateTime.Now;
			//_endtime = null;
			_executiontime = TimeSpan.Zero;
			
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
			_resourceusage.Stop();
			_random.Disable();
			_endtime = DateTime.Now;
			_executiontime = _endtime - _starttime;

			WriteToFile();
			//WriteToXml();
		}

		public void WriteToFile()
		{
			string savedirectory = GetSaveDirectory();

			// Create the file to write
			string content = this.ToString();

			// Save the file
			for (int i = 1; i < 50; i++)
			{
				string filepath = savedirectory + "run" + i.ToString("00") + ".dat";
				if (!File.Exists(filepath))
				{
					File.WriteAllText(filepath, content);
					i = 50;
				}
			}
		}

		/*public void WriteToXml()
		{
			string savedirectory = GetSaveDirectory();
			XmlSerializer serializer = new XmlSerializer(typeof(GuiTracker));
			//XmlWriter writer = XmlWriter.Create

			// Save the file
			for (int i = 1; i < 50; i++)
			{
				string filepath = savedirectory + "values" + i.ToString("00") + ".xml";
				if (!File.Exists(filepath))
				{
					using (Stream stream = new FileStream(filepath, FileMode.OpenOrCreate))
					{
						serializer.Serialize(stream, this);
					}
					i = 50;
				}
			}
		}

		public static List<GuiTracker> ReadFromXml(string loaddirectory)
		{
			List<GuiTracker> trackers = new List<GuiTracker>();
			XmlSerializer serializer = new XmlSerializer(typeof(GuiTracker));

			// Save the file
			foreach (string filepath in Directory.GetFiles(loaddirectory))
			{
				using (Stream stream = new FileStream(filepath, FileMode.Open))
				{
					// Call the Deserialize method to restore the object's state.
					trackers.Add((GuiTracker)serializer.Deserialize(stream));
				}
			}

			return trackers;
		}*/

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

		/*public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("application=").Append(_application).Append("\n");
			sb.Append("framework=").Append(_framework).Append("\n");
			sb.Append("toolkit=").Append(_toolkit).Append("\n");
			sb.Append("starttime=").Append(_starttime.ToString()).Append("\n");
			sb.Append("endtime=").Append(_endtime.ToString()).Append("\n");
			sb.Append("executiontime=").Append(_executiontime.ToString()).Append("\n");

			foreach (ResourceSnapshot rs in _resourceusage.Snapshots)
			{
				sb.Append((rs.TimeStamp - _starttime)).Append("\t");
				sb.Append(Format.Cpu(rs.Cpu, true)).Append("\t").Append(rs.Ram).Append("\t\n");
			}

			return sb.ToString();
		}

		public static GuiTracker FromString(string guitrackerstring)
		{
			GuiTracker tracker = new GuiTracker ();

			System.String.re

			tracker._application = "";
			tracker._framework = "";
			tracker._toolkit = "";
			tracker._starttime = "";
			tracker._endtime = "";
			tracker._executiontime = "";

			tracker._ = "";
			tracker._application = "";

			return tracker;
		}*/

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
			sb.Append("# Minimum RAM usage: ").Append(Format.Ram(_resourceusage.RamMin)).Append("\n");
			sb.Append("# Average RAM usage: ").Append(Format.Ram(_resourceusage.RamAvg)).Append("\n");
			sb.Append("# Maximum RAM usage: ").Append(Format.Ram(_resourceusage.RamMax)).Append("\n");
			sb.Append("######################\n\n");

			foreach (ResourceSnapshot rs in _resourceusage.Snapshots)
			{
				sb.Append(Format.Duration(rs.TimeStamp - _starttime)).Append("\t");
				sb.Append(Format.Cpu(rs.Cpu, true)).Append("\t").Append(rs.Ram).Append("\t\n");
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
