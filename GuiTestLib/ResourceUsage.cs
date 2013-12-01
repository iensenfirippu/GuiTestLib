using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GuiTestLib
{
	public class ResourceUsage
	{
		private const ByteDenomination _denomination = ByteDenomination.MiB;
		
		private PerformanceCounter _cpuCounter;
		
		private int _pointer = 0;
		private int _count = 0;
		private ResourceSnapshot _baseline;
		private List<ResourceSnapshot> _snapshots;
		
		private ResourceSnapshot _latest_snapshot;
		private ResourceSnapshot _mincpu_snapshot;
		private ResourceSnapshot _minram_snapshot;
		private ResourceSnapshot _maxcpu_snapshot;
		private ResourceSnapshot _maxram_snapshot;
		
		public ResourceUsage()
		{
			// Performance counter returns the overall cpu percentage load
			_cpuCounter = new PerformanceCounter();
			_cpuCounter.CategoryName = "Processor";
			//_cpuCounter.CategoryName = "Process";
			_cpuCounter.CounterName = "% Processor Time";
			_cpuCounter.InstanceName = "_Total";
			//_cpuCounter.InstanceName = Process.GetCurrentProcess().ProcessName;
			// first value returned is always 0, so we run NextValue once to ready the performancecounter
			_cpuCounter.NextValue();
			Thread.Sleep(20);
			
			_baseline = new ResourceSnapshot(DateTime.Now, getCurrentCpuUsage(), getAllocatedRAM());
			_snapshots = new List<ResourceSnapshot>();
		}

		private float getCurrentCpuUsage() { return _cpuCounter.NextValue(); }
		private float getAllocatedRAM() { return (float)GC.GetTotalMemory(true); }
		 
		public void TakeSnapshot()
		{
			_latest_snapshot = new ResourceSnapshot(_baseline, _count, DateTime.Now, getCurrentCpuUsage(), getAllocatedRAM());
			_snapshots.Add(_latest_snapshot);
			_count++;
			
			if (_latest_snapshot.Cpu < _mincpu_snapshot.Cpu) { _mincpu_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Ram < _minram_snapshot.Ram) { _minram_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Cpu > _maxcpu_snapshot.Cpu) { _maxcpu_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Ram > _maxram_snapshot.Ram) { _maxram_snapshot = _latest_snapshot; }
		}
		
		public float Cpu { get { if (_latest_snapshot != null) { return _latest_snapshot.Cpu; } else { return 0; } } }
		public float Ram { get { if (_latest_snapshot != null) { return _latest_snapshot.Ram; } else { return 0; } } }

		public float CpuMin { get { if (_mincpu_snapshot != null) { return _mincpu_snapshot.Cpu; } else { return 0; } } }
		public float RamMin { get { if (_minram_snapshot != null) { return _minram_snapshot.Ram; } else { return 0; } } }

		public float CpuMax { get { if (_mincpu_snapshot != null) { return _mincpu_snapshot.Cpu; } else { return 0; } } }
		public float RamMax { get { if (_minram_snapshot != null) { return _minram_snapshot.Ram; } else { return 0; } } }

		public float CpuAvg
		{
			get
			{
				float total = 0;
				foreach (ResourceSnapshot s in _snapshots)
				{
					total += s.Cpu;
				}
				return (total / _snapshots.Count);
			}
		}
		public float RamAvg
		{
			get
			{
				float total = 0;
				foreach (ResourceSnapshot s in _snapshots)
				{
					total += s.Ram;
				}
				return (total / _snapshots.Count);
			}
		}

		public ResourceSnapshot GetNext()
		{
			_pointer++;
			if (_pointer < (_snapshots.Count - 1))
			{
				return _snapshots[_pointer];
			}
			else
			{
				return null;
			}
		}

		public string CpuToString(float cpu)
		{
			Console.WriteLine(Process.GetCurrentProcess().ProcessName);
			return "base=" + _baseline.Cpu + " " + (cpu.ToString("##0.##") + "%");
		}

		public string RamToString(float ram)
		{
			return (ram / (int)_denomination).ToString("##0.0##") + _denomination;
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

