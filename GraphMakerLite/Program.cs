using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace GraphMakerLite
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			string savedirectory = (Environment.OSVersion.Platform == PlatformID.Unix || 
									Environment.OSVersion.Platform == PlatformID.MacOSX)
									? Environment.GetEnvironmentVariable("HOME")
									: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			savedirectory += "/GuiTest/Data/";
			string[] experiments = Directory.GetDirectories(savedirectory);
			string graphscript = "";
			string input = "";
			int intvalue = 0;

			while (!input.Contains("q"))
			{
				Console.WriteLine("Which experiment would you like to make a graph for?");
				Console.WriteLine ("q: quit");
				int i = 0;
				foreach (string experiment in experiments)
				{
					i++;
					Console.WriteLine ("{0}: {1}", i.ToString(), Path.GetFileName(experiment));
				}
				input = Console.ReadLine();

				if (int.TryParse(input, out intvalue))
				{
					intvalue--;
					if (intvalue >= 0 && intvalue < experiments.Length)
					{
						int maxtime = 0;
						ExperimentData exp = new ExperimentData(experiments[intvalue]);
						Console.WriteLine("Generating graph script for \"{0}\"...", exp.name);
						Console.WriteLine("Loading framework resultsets...");
						exp.LoadFrameworks();
						foreach (Framework f in exp.frameworks)
						{
							Console.WriteLine("\tLoading data for \"{0}\"...", f.name);
							f.LoadRuns();
							i = 0;
							foreach (Run r in f.runs)
							{
								i++;
								Console.WriteLine("\t\tLoading data for run {0}...", i);
							}
							
							float total = 0;
							float average = 0;
							float diff = 0;
							float closest = 1000;
							int closestrun = 0;
							Console.WriteLine("\tChecking execution times...");
							Console.Write("\t\t");
							foreach (Run r in f.runs)
							{
								total += r.time;
								Console.Write("{0}\t", r.time);
							}
							average = (total / f.runs.Count);
							Console.WriteLine("\n\t\tAverage: {0}", average);
							Console.Write("\t\t");
							foreach (Run r in f.runs)
							{
								diff = (r.time - average);
								Console.Write("{0}\t", diff);
								if (diff < 0) { diff -= (diff * 2); } // invert if negative
								if (diff < closest) { closest = diff; closestrun = f.runs.IndexOf(r); }
							}
							f.selectedrun = closestrun;
							Console.WriteLine("\n\tRun {0}, was closest to the collective average...", (f.selectedrun +1));

							int time = (int)Math.Ceiling(f.runs[f.selectedrun].time);
							if (time > maxtime) { maxtime = time; }
						}
						graphscript = "/usr/bin/gnuplot <<__EOF\n" +
									  "set title \"" + exp.name + " - CPU usage\"\n" +
									  "set xlabel \"Time (s)\"\n" +
									  "set ylabel \"CPU (%)\" \n" +
									  "set xrange [0:" + maxtime + "]\n" +
									  "set yrange [0:100]\n" +
									  "set term png\n" +
									  "set output \"cpu.png\"\n" +
									  "set xtic 1\n" +
									  "set ytic 10\n" +
									  "plot ";
						i = 0;
						foreach (Framework f in exp.frameworks)
						{
							i++;
							graphscript += "\"" + f.name + "/run" + (f.selectedrun +1).ToString("00") + ".dat\" u 1:2 w lines title '" + Values.Title(f.name) + "' linecolor rgb '" + Values.Colors(f.name) + "'";
							if (i < exp.frameworks.Count) { graphscript += ", \\\n"; }
							else { graphscript += "\n";}
						}
						graphscript += "set title \"" + exp.name + " - RAM usage\"\n" +
									   "set ylabel \"RAM (KB)\" \n" +
									   "set yrange [0:512]\n" +
									   "set output \"ram.png\"\n" +
									   "set ytic 64\n" +
									   "plot ";
						i = 0;
						foreach (Framework f in exp.frameworks)
						{
							i++;
							graphscript += "\"" + f.name + "/run" + (f.selectedrun +1).ToString("00") + ".dat\" u 1:3 w lines title '" + Values.Title(f.name) + "' linecolor rgb '" + Values.Colors(f.name) + "'\t";
							if (i < exp.frameworks.Count) { graphscript += ", \\\n"; }
							else { graphscript += "\n";}
						}
						graphscript += "pause -1\n" +
									   "__EOF\n";
							
						File.WriteAllText(exp.location + "/mkgraph.sh", graphscript);
					}
				}
			}
		}
	}

	public class ExperimentData
	{
		public string name;
		public string location;
		public List<Framework> frameworks;

		public ExperimentData(string folderlocation)
		{
			name = Path.GetFileName(folderlocation);
			location = folderlocation;
			frameworks = new List<Framework>();
		}

		public void LoadFrameworks()
		{
			string[] frameworkfolders = Directory.GetDirectories(location);

			foreach (string folder in frameworkfolders)
			{
				frameworks.Add(new Framework(folder));
			}
		}
	}

	public class Framework
	{
		public string name;
		public string location;
		public List<Run> runs;
		public int selectedrun;

		public Framework(string folderlocation)
		{
			name = Path.GetFileName(folderlocation);
			location = folderlocation;
			runs = new List<Run>();
		}

		public void LoadRuns()
		{
			string[] runfiles = Directory.GetFiles(location);

			foreach (string run in runfiles)
			{
				runs.Add(new Run(run));
			}
		}
	}

	public class Run
	{
		public float time;
		public float cpu;
		public float ram;
		public List<DataPoint> datapoints;

		public Run(string filelocation)
		{
			string content = File.ReadAllText(filelocation);
			string floatstring = Regex.Match(content, @"Execution time: ([0-9]+.[0-9]+)").Groups[0].ToString();
			floatstring = floatstring.Remove(0, 16);
			float outvariable = 0;
			float.TryParse(floatstring, out outvariable);
			time = outvariable;

			datapoints = new List<DataPoint>();
		}
	}

	public class DataPoint
	{
		public float time;
		public float cpu;
		public float ram;

		public DataPoint()
		{

		}
	}

	public static class Values
	{
		public static string Title(string framework)
		{
			string retval = string.Empty;

			switch (framework)
			{
			case "Mono-Gtk":
				retval = "Mono - Gtk";
				break;
			case "Mono-Win":
				retval = "Mono - Winforms";
				break;
			case "Mono-Wpf":
				retval = "Mono - WPF";
				break;
			case "Dnet-Gtk":
				retval = ".net - Gtk";
				break;
			case "Dnet-Win":
				retval = ".net - Winforms";
				break;
			case "Dnet-Wpf":
				retval = ".net - WPF";
				break;
			}

			return retval;
		}

		public static string Colors(string framework)
		{
			string retval = string.Empty;

			switch (framework)
			{
			case "Mono-Gtk":
				retval = "#0000ff";
				break;
			case "Mono-Win":
				retval = "#000088";
				break;
			case "Mono-Wpf":
				retval = "#8888ff";
				break;
			case "Dnet-Gtk":
				retval = "#880000";
				break;
			case "Dnet-Win":
				retval = "#ff0000";
				break;
			case "Dnet-Wpf":
				retval = "#ff8888";
				break;
			}

			return retval;
		}
	}
}
