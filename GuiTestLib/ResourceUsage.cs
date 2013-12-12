using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GuiTestLib
{
	[Serializable]
	public class ResourceUsage
	{		
		private PerformanceCounter _cpuCounter;

		private int _count = 0;

		private List<ResourceSnapshot> _snapshots;

		private ResourceSnapshot _latest_snapshot;
		private ResourceSnapshot _mincpu_snapshot;
		private ResourceSnapshot _minram_snapshot;
		private ResourceSnapshot _maxcpu_snapshot;
		private ResourceSnapshot _maxram_snapshot;
		
		public ResourceUsage(GuiTracker.Framework framework)
		{
			_cpuCounter = new PerformanceCounter();
			_snapshots = new List<ResourceSnapshot>();

			// Checking for process CPU usage
			_cpuCounter.CategoryName = "Process";
			_cpuCounter.CounterName = "% Processor Time";
			// Get process by id in mono, or name in .net
			if (Environment.OSVersion.Platform == PlatformID.Unix || 
			    Environment.OSVersion.Platform == PlatformID.MacOSX)
			{
				_cpuCounter.InstanceName = Process.GetCurrentProcess().Id.ToString();
			}
			else
			{
				_cpuCounter.InstanceName = Process.GetCurrentProcess().ProcessName;
			}
			TakeSnapshot("sleep start", true);
			Thread.Sleep(1000);
			TakeSnapshot("sleep end", true);

			// first value returned is always 0, so we run NextValue once and sleep for a second to ready the performancecounter.				
			_cpuCounter.NextValue();
			TakeSnapshot("test start");
		}

		private float getCurrentCpuUsage() { return _cpuCounter.NextValue(); }
		private float getAllocatedRAM() { return (float)GC.GetTotalMemory(true); }
		
		public void TakeSnapshot() { TakeSnapshot(string.Empty, false); }
		public void TakeSnapshot(string name) { TakeSnapshot(name, false); }
		public void TakeSnapshot(string name, bool ignorecpu)
		{
			if (ignorecpu) { _latest_snapshot = new ResourceSnapshot(null, _count, name, DateTime.Now, 0, getAllocatedRAM()); }
			else { _latest_snapshot = new ResourceSnapshot(null, _count, name, DateTime.Now, getCurrentCpuUsage(), getAllocatedRAM()); }

			_snapshots.Add(_latest_snapshot);
			_count++;

			if (_latest_snapshot.Cpu < CpuMin) { _mincpu_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Ram < RamMin) { _minram_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Cpu > CpuMax) { _maxcpu_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Ram > RamMax) { _maxram_snapshot = _latest_snapshot; }
		}

		public List<ResourceSnapshot> Snapshots { get { return _snapshots; } }
		public float Cpu { get { if (_latest_snapshot != null) { return _latest_snapshot.Cpu; } else { return 0; } } }
		public float Ram { get { if (_latest_snapshot != null) { return _latest_snapshot.Ram; } else { return 0; } } }
		public float CpuMin { get { if (_mincpu_snapshot != null) { return _mincpu_snapshot.Cpu; } else { return float.MaxValue; } } }
		public float RamMin { get { if (_minram_snapshot != null) { return _minram_snapshot.Ram; } else { return float.MaxValue; } } }
		public float CpuMax { get { if (_maxcpu_snapshot != null) { return _maxcpu_snapshot.Cpu; } else { return float.MinValue; } } }
		public float RamMax { get { if (_maxram_snapshot != null) { return _maxram_snapshot.Ram; } else { return float.MinValue; } } }
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

		public bool Stop()
		{
			bool error = false;
			TakeSnapshot("test end");
			if (Environment.ProcessorCount > 1)
			{
				foreach (ResourceSnapshot rs in _snapshots)
				{
					if (rs.RecalculateCpu()) { error = true; }
				}
			}
			return error;
		}
	}
}

