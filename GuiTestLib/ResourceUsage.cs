using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GuiTestLib
{
	public class ResourceUsage
	{		
		private PerformanceCounter _cpuCounter;
		
		//private int _pointer = 0;
		private int _count = 0;
		//private ResourceSnapshot _base;

		private List<ResourceSnapshot> _snapshots;

		private ResourceSnapshot _latest_snapshot;
		private ResourceSnapshot _mincpu_snapshot;
		private ResourceSnapshot _minram_snapshot;
		private ResourceSnapshot _maxcpu_snapshot;
		private ResourceSnapshot _maxram_snapshot;
		
		public ResourceUsage(GuiTracker.Toolkit toolkit)
		{
			_cpuCounter = new PerformanceCounter();

			//_cpuCounter.CategoryName = "Processor";
			_cpuCounter.CategoryName = "Process";

			_cpuCounter.CounterName = "% Processor Time";
			
			// returns the overall cpu percentage load
			//_cpuCounter.InstanceName = "_Total";

			// Get process by id in mono, or name in .net
			//if (Type.GetType("Mono.Runtime") != null)
			if ((int)toolkit == 1)
			{
				_cpuCounter.InstanceName = Process.GetCurrentProcess().Id.ToString();
			}
			else
			{
				_cpuCounter.InstanceName = Process.GetCurrentProcess().ProcessName;
			}

			// first value returned is always 0, so we run NextValue once to ready the performancecounter
			_cpuCounter.NextValue();
			Thread.Sleep(1000);

			/*_mincpu_snapshot = new ResourceSnapshot(DateTime.MinValue, float.MaxValue, float.MaxValue);
			_minram_snapshot = _mincpu_snapshot;
			_maxcpu_snapshot = new ResourceSnapshot(DateTime.MinValue, float.MinValue, float.MinValue);
			_maxram_snapshot = _maxcpu_snapshot;*/
			
			//_base = new ResourceSnapshot(DateTime.Now, getCurrentCpuUsage(), getAllocatedRAM());
			_snapshots = new List<ResourceSnapshot>();
		}

		private float getCurrentCpuUsage() { return _cpuCounter.NextValue(); }
		private float getAllocatedRAM() { return (float)GC.GetTotalMemory(true); }
		 
		public void TakeSnapshot()
		{
			//_latest_snapshot = new ResourceSnapshot(_base, _count, DateTime.Now, getCurrentCpuUsage(), getAllocatedRAM());
			_latest_snapshot = new ResourceSnapshot(null, _count, DateTime.Now, getCurrentCpuUsage(), getAllocatedRAM());
			_snapshots.Add(_latest_snapshot);
			_count++;

			/*if (_count == 1)
			{
				_mincpu_snapshot = _latest_snapshot;
				_minram_snapshot = _latest_snapshot;
				_maxcpu_snapshot = _latest_snapshot;
				_maxram_snapshot = _latest_snapshot;
			}
			else
			{*/
			if (_latest_snapshot.Cpu < CpuMin) { _mincpu_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Ram < RamMin) { _minram_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Cpu > CpuMax) { _maxcpu_snapshot = _latest_snapshot; }
			if (_latest_snapshot.Ram > RamMax) { _maxram_snapshot = _latest_snapshot; }
			//}
		}

		//public ResourceSnapshot Base { get { return _base; } }
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

		/*public ResourceSnapshot GetNext()
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
		}*/

		public void Stop()
		{
			if (Environment.ProcessorCount > 1)
			{
				foreach (ResourceSnapshot rs in _snapshots)
				{
					rs.RecalculateCpu();
				}
			}
		}
	}
}

