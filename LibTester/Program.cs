using System;
using GuiTestLib;
using System.Xml.Serialization;

namespace LibTester
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Program p = new Program(new GuiTracker("LibTester", GuiTracker.Framework.Dnet, GuiTracker.Toolkit.Gtk));
			p.AutomaticTest(args);
		}
	}

	class Program
	{
		private const string SYSTEMMESSAGE = "Please enter a command (1=randomString 2=randomNumber 3=randomDecimal 4=resourceUsage q=quit)";
		private const string STRINGMESSAGE = "Please enter a size (1=normal 2=short 3=long)";
				
		private GuiTracker _tracker;

		public Program(GuiTracker tracker)
		{
			_tracker = tracker;
		}

		public void AutomaticTest(string[] args)
		{
			Console.WriteLine("Welcome to LibTester...");
			Console.WriteLine(SYSTEMMESSAGE);
			
			for (int i = 0; i < 1000; i++)
			{
				Console.WriteLine(i.ToString());
				
				ConsoleWriteLineString(_tracker.Random.String);
				ConsoleWriteLineString(_tracker.Random.String);
				ConsoleWriteLineString(_tracker.Random.String);
				
				ConsoleWriteLineString(_tracker.Random.ShortString);
				ConsoleWriteLineString(_tracker.Random.ShortString);
				ConsoleWriteLineString(_tracker.Random.ShortString);
				
				ConsoleWriteLineString(_tracker.Random.LongString);
				ConsoleWriteLineString(_tracker.Random.LongString);
				ConsoleWriteLineString(_tracker.Random.LongString);
				
				PrintInt();
				PrintInt();
				PrintInt();
				
				PrintDouble();
				PrintDouble();
				PrintDouble();
				
				PrintResourceUsage();
				
				if (i % 3 == 1) { Console.Clear(); }
			}
			
			Console.WriteLine("Waiting for tracker to make a final tick...");
			
			_tracker.Stop();
			Console.WriteLine("Thanks for using LibTester");
			Console.WriteLine("  Execution time was: {0})", _tracker.ExecutionTime);
			Console.WriteLine("  CPU usage was: {0} (min={1} max={2})", _tracker.Usage.CpuAvg, _tracker.Usage.CpuMin, _tracker.Usage.CpuMax);
			Console.WriteLine("  RAM usage was: {0} (min={1} max={2})", _tracker.Usage.RamAvg, _tracker.Usage.RamMin, _tracker.Usage.RamMax);
			
			
			Console.WriteLine("-- value dump --");

			//Console.WriteLine("BASE {0}:{1}", Format.Cpu(_tracker.Usage.Base.CpuUsage), Format.Ram(_tracker.Usage.Base.RamUsage));
			foreach (ResourceSnapshot rs in _tracker.Usage.Snapshots)
			{
				Console.WriteLine("{0}:{1}", Format.Cpu(rs.Cpu), Format.Ram(rs.Ram));
			}

			Console.ReadLine();
		}

		public void ManuelTest(string[] args)
		{
			Console.WriteLine("Welcome to LibTester...");
			Console.WriteLine(SYSTEMMESSAGE);

			string input = Console.ReadLine();
			while (input != "q")
			{
				switch (input)
				{
					case "1":
						PrintString();
						break;
					case "2":
						PrintInt();
						break;
					case "3":
						PrintDouble();
						break;
					case "4":
						PrintResourceUsage();
						break;
					default:
						Console.WriteLine("input error");
						break;
				}

				Console.WriteLine(SYSTEMMESSAGE);
				input = Console.ReadLine();
			}
			_tracker.Stop();
			Console.WriteLine("Thanks for using LibTester");
			Console.WriteLine("  Execution time was: {0})", _tracker.ExecutionTime);
			Console.WriteLine("  CPU usage was: {0} (min={1} max={2})", _tracker.Usage.CpuAvg, _tracker.Usage.CpuMin, _tracker.Usage.CpuMax);
			Console.WriteLine("  RAM usage was: {0} (min={1} max={2})", _tracker.Usage.RamAvg, _tracker.Usage.RamMin, _tracker.Usage.RamMax);
		}
	
		private void PrintString()
		{
			Console.WriteLine(STRINGMESSAGE);

			string input = Console.ReadLine();
			switch (input)
			{
				case "1":
					ConsoleWriteLineString(_tracker.Random.String);
					break;
				case "2":
					ConsoleWriteLineString(_tracker.Random.ShortString);
					break;
				case "3":
					ConsoleWriteLineString(_tracker.Random.LongString);
					break;
				default:
					Console.WriteLine("input error");
					break;
			}
		}
	
		private void ConsoleWriteLineString(string s)
		{
			Console.WriteLine("Printing random string: {0}", s);
		}

		private void PrintInt()
		{
			Console.WriteLine("Printing random int: {0}", _tracker.Random.Int);
		}

		private void PrintDouble()
		{
			Console.WriteLine("Printing random double: {0}", _tracker.Random.Double);
		}

		private void PrintResourceUsage()
		{
			Console.WriteLine("Printing current CPU usage: {0}", _tracker.Usage.Cpu);
			Console.WriteLine("Printing current RAM usage: {0}", _tracker.Usage.Ram);
		}
	}
}
