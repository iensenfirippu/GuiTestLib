using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace GraphMakerLite
{
	public class MainClass
	{
		private const int RAMINTERVAL = 1024;
		private const int RAMTICKINTERVAL = 128;

		public static void Main (string[] args)
		{
			string savedirectory = (Environment.OSVersion.Platform == PlatformID.Unix || 
									Environment.OSVersion.Platform == PlatformID.MacOSX)
									? Environment.GetEnvironmentVariable("HOME")
									: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			savedirectory += "/GuiTest/Data/";
			string[] experiments = Directory.GetDirectories(savedirectory);
			string graphscript = "/usr/bin/gnuplot <<__EOF\n";
			string input = "";
			int intvalue = 0;

			// Continue the program until q is entered
			while (!input.Contains("q"))
			{
				// Print all input options
				Console.WriteLine("Which experiment would you like to make a graph for?");
				Console.WriteLine ("q=quit");
				int i = 0;
				foreach (string experiment in experiments)
				{
					i++;
					Console.WriteLine ("{0}: {1}", i.ToString(), Path.GetFileName(experiment));
				}
				input = Console.ReadLine();

				// If input was given, attempt to process the selected Experiment data
				if (int.TryParse(input, out intvalue))
				{
					// Turn the user input into array index
					intvalue--;

					// Process request, if valid
					if (intvalue >= 0 && intvalue < experiments.Length)
					{
						// ready the necessary variables
						int maxtime = 0;
						int maxram = RAMINTERVAL;
						int ramtick = RAMTICKINTERVAL;
						bool printmarkers;
						string defaultselection = "2,3,4,5|1,6,7";
						string[] parts;
						int x = 0;
						int markercount = 0;
						List<List<Framework>> selection = new List<List<Framework>>();
						ExperimentData exp = new ExperimentData(experiments[intvalue]);
						exp.LoadFrameworks();

						// Ask if script should print event markers
						Console.WriteLine("Do you want event markers?");
						Console.WriteLine ("y=yes n=no");
						input = Console.ReadLine();
						printmarkers = input.Contains("y");

						// Go through each framework available in the folder
						Console.WriteLine("Showing available frameworks:");
						i = 0;
						foreach (Framework f in exp.frameworks)
						{
							i++;
							Console.WriteLine("{0}: {1}", i, f.name);
						}
						Console.WriteLine("Write a selection in this format \"1,3|2,4,5\". default={0}", defaultselection);
						input = Console.ReadLine();
						if (input != string.Empty) { parts = input.Split("|".ToCharArray()); }
						else { parts = defaultselection.Split("|".ToCharArray()); }
						int outputvalue = 0;
						foreach (string part in parts)
						{
							List<Framework> innerlist = new List<Framework>();
							foreach (string s in part.Split(",".ToCharArray()))
							{
								outputvalue = -1;
								int.TryParse(s, out outputvalue);
								outputvalue--;
								if (outputvalue > -1 && outputvalue < exp.frameworks.Count) { innerlist.Add(exp.frameworks[outputvalue]); }
							}
							selection.Add(innerlist);
						}

						Console.WriteLine("Generating graph script for \"{0}\"...", exp.name);
						Console.WriteLine("Loading framework resultsets...");

						// Go through each of the selected frameworks
						foreach (List<Framework> l in selection)
						{
							x++;
							foreach (Framework f in l)
							{
								// Ready variables for determining the average run
								float total = 0;
								float average = 0;
								float diff = 0;
								float closest = 1000;
								int closestrun = 0;

								// Load all runs for the specific framework
								Console.WriteLine("\tLoading data for \"{0}\"...", f.name);
								f.LoadRuns();
								i = 0;
								foreach (Run r in f.runs)
								{
									i++;
									Console.WriteLine("\t\tLoading data for run {0}...", i);
								}
								// Compare execution times
								Console.WriteLine("\tChecking execution times...");
								Console.Write("\t\t");
								// Print the execution times
								foreach (Run r in f.runs)
								{
									total += r.time;
									Console.Write("{0}\t", r.time);
								}
								// Get the average execution time
								average = (total / f.runs.Count);
								Console.WriteLine("\n\t\tAverage: {0}", average);
								Console.Write("\t\t");
								// Print the differences
								foreach (Run r in f.runs)
								{
									diff = (r.time - average);
									Console.Write("{0}\t", diff);
									if (diff < 0) { diff -= (diff * 2); } // invert if negative
									if (diff < closest) { closest = diff; closestrun = f.runs.IndexOf(r); }
								}
								// Select the run closest to the average execution time
								f.selectedrun = closestrun;
								Console.WriteLine("\n\tRun {0}, was closest to the collective average...", (f.selectedrun +1));

								// Set scale max for time, if selected run time is bigger 
								int time = (int)Math.Ceiling(f.runs[f.selectedrun].time);
								if (time > maxtime) { maxtime = time; }

								// Set scale max for ram, if selected run maxram is bigger
								int ram = (int)Math.Ceiling(f.runs[f.selectedrun].maxram);
								while (ram > maxram) { maxram += RAMINTERVAL; ramtick += RAMTICKINTERVAL; }
							}

							// Start the gunplot script
							graphscript += "set title \"" + exp.name + " - CPU usage\"\n" +
										  "set xlabel \"Time (s)\"\n" +
										  "set ylabel \"CPU (%)\" \n" +
										  "set xrange [0:" + maxtime + "]\n" +
										  "set yrange [0:100]\n" +
										  "set term png\n" +
										  "set output \"cpu" + x + ".png\"\n" +
										  "set xtic 1\n" +
										  "set ytic 10\n";

							if (printmarkers)
							{
								// Add event data from all the selected runs
								i = 0;
								int innercount = 0;
								foreach (Framework f in l)
								{
									foreach (DataPoint d in f.SelectedRun.datapoints)
									{
										i++; innercount++;
										graphscript += "set object " + i + " circle at " + d.time + "," + d.cpu + " size 0.05 fs solid " + d.state + " fc rgb \"" + d.color + "\"\n";
									}
								}
								if (innercount > markercount) { markercount = innercount; }
								while (i < markercount)
								{
									i++;
									graphscript += "set object " + i + " at -100,-100\n";
								}
							}


							// Add plot data for all the selected runs
							graphscript += "plot ";
							i = 0;
							foreach (Framework f in l)
							{
								i++;
								graphscript += "\"" + f.name + "/run" + (f.selectedrun +1).ToString("00") + ".dat\" u 1:2 w lines title '" + Values.Title(f.name) + "' lc rgb '" + Values.Colors(f.name) + "' lw 2";
								if (i < l.Count) { graphscript += ", \\\n"; }
								else { graphscript += "\n";}
							}
							// (cpu.png will be created)

							// Setup script for ram.png
							graphscript += "set title \"" + exp.name + " - RAM usage\"\n" +
										   "set ylabel \"RAM (KB)\" \n" +
										   "set yrange [0:" + maxram + "]\n" +
										   "set output \"ram" + x + ".png\"\n" +
										   "set ytic " + ramtick + "\n";
							
							if (printmarkers)
							{
								// Re-place the event markers to fit the RAM data
								i = 0;
								int innercount = 0;
								foreach (Framework f in l)
								{
									foreach (DataPoint d in f.SelectedRun.datapoints)
									{
										i++; innercount++;
										graphscript += "set object " + i + " at " + d.time + "," + d.ram + "\n";
									}
								}
								if (innercount > markercount) { markercount = innercount; }
								while (i < markercount)
								{
									i++;
									graphscript += "set object " + i + " at -100,-100\n";
								}
							}
							
							// Add plot data for all the selected runs
							graphscript += "plot ";
							i = 0;
							foreach (Framework f in l)
							{
								i++;
								graphscript += "\"" + f.name + "/run" + (f.selectedrun +1).ToString("00") + ".dat\" u 1:3 w lines title '" + Values.Title(f.name) + "' lc rgb '" + Values.Colors(f.name) + "' lw 2";
								if (i < l.Count) { graphscript += ", \\\n"; }
								else { graphscript += "\n";}
							}
						}

						// End the gnuplot script
						graphscript += "pause -1\n" +
									   "__EOF\n";

						// Write the script to file
						File.WriteAllText(exp.location + "/mkgraph.sh", graphscript);
					}
				}
			}

			//Destroy?
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

		public Run SelectedRun { get { return runs [selectedrun]; } }
	}

	public class Run
	{
		public float time;
		public float maxcpu;
		public float maxram;
		public List<DataPoint> datapoints;

		public Run(string filelocation)
		{
			string content = File.ReadAllText(filelocation);
			string floatstring = string.Empty;
			float outvariable = 0;

			floatstring = Regex.Match(content, @"(?<=Execution time: )[0-9]+.[0-9]+").Groups[0].ToString();
			float.TryParse(floatstring, out outvariable);
			time = outvariable;

			floatstring = Regex.Match(content, @"(?<=Maximum CPU usage: )[0-9]+.[0-9]+").Groups[0].ToString();
			float.TryParse(floatstring, out outvariable);
			maxcpu = outvariable;

			floatstring = Regex.Match(content, @"(?<=Maximum RAM usage: )[0-9]+.[0-9]+").Groups[0].ToString();
			float.TryParse(floatstring, out outvariable);
			maxram = outvariable;

			datapoints = new List<DataPoint>();
			MatchCollection matches = Regex.Matches(content, @"[0-9]+.[0-9]+\t[0-9]+.[0-9]+\t[0-9]+.[0-9]+\t[a-zA-Z]+ [a-zA-Z]+");
			for (int i = 0; i < matches.Count; i++) { datapoints.Add(new DataPoint(matches[i].ToString())); }
		}
	}

	public class DataPoint
	{
		public float time;
		public float cpu;
		public float ram;
		public string title;
		public string color;
		public string state;

		public DataPoint(String match)
		{
			string[] parts = match.Split("\t".ToCharArray());
			
			float outvariable = 0;

			float.TryParse(parts[0], out outvariable);
			time = outvariable;

			float.TryParse(parts[1], out outvariable);
			cpu = outvariable;

			float.TryParse(parts[2], out outvariable);
			ram = outvariable;

			title = parts[3];

			parts = title.Split(" ".ToCharArray());
			color = Values.DataPointColor(parts[0]);
			state = Values.DataPointState(parts[1]);
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
			case "Mono-Gtk2":
				retval = "Mono - Gtk (compiled in .net)";
				break;
			case "Mono-Win":
				retval = "Mono - Winforms";
				break;
			case "Mono-Win2":
				retval = "Mono - Winforms (compiled in .net)";
				break;
			case "Mono-Wpf":
				retval = "Mono - WPF";
				break;
			case "Dnet-Gtk":
				retval = ".net - Gtk";
				break;
			case "Dnet-Gtk2":
				retval = ".net - Gtk (compiled in Mono)";
				break;
			case "Dnet-Win":
				retval = ".net - Winforms";
				break;
			case "Dnet-Win2":
				retval = ".net - Winforms (compiled in Mono)";
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
			case "Mono-Gtk2":
				retval = "#5555ff";
				break;
			case "Mono-Win":
				retval = "#ff00ff";
				break;
			case "Mono-Win2":
				retval = "#ff88ff";
				break;
			case "Mono-Wpf":
				retval = "#0000aa";
				break;
			case "Dnet-Gtk":
				retval = "#00ff00";
				break;
			case "Dnet-Gtk2":
				retval = "#aaffaa";
				break;
			case "Dnet-Win":
				retval = "#ff0000";
				break;
			case "Dnet-Win2":
				retval = "#ff5555";
				break;
			case "Dnet-Wpf":
				retval = "#aa0000";
				break;
			}

			return retval;
		}

		public static string DataPointColor(string eventname)
		{
			string retval = string.Empty;

			switch (eventname)
			{
				case "sleep":
				retval = "#cccccc";
				break;
				case "test":
				retval = "#000000";
				break;
				case "init":
				retval = "#555555";
				break;
				case "draw":
				retval = "#aaaaaa";
				break;
			}

			return retval;
		}

		public static string DataPointState(string state)
		{
			string retval = string.Empty;

			switch (state)
			{
			case "start":
				//retval = "border rgb \"#888888\"";
				break;
			case "end":
				retval = "border rgb \"#000000\"";
				break;
			}

			return retval;
		}
	}
}
