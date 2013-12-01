using System;

namespace GuiTestLib
{
	public class ResourceSnapshot
	{
		//private ResourceSnapshot _baseline;
		
		private int _index;
		private DateTime _time;
		private TimeSpan _runtime;
		private float _cpu;			// in %
		private float _cpuusage;	// in %
		private float _ram;			// in B
		private float _ramusage;	// in B
		
		public ResourceSnapshot(DateTime time, float cpu, float ram) : this(null, -1, time, cpu, ram) {}
		public ResourceSnapshot(ResourceSnapshot baseline, int index, DateTime time, float cpu, float ram)
		{
			//_baseline = baseline;
			_index = index;
			
			_time = time;
			if (baseline != null) { _runtime = time - baseline.TimeStamp; }
			else { _runtime = TimeSpan.Zero; }

			_cpu = cpu;
			_cpuusage = cpu;
			//if (baseline != null) { _cpuusage = cpu - baseline.Cpu; }
			//else { _cpuusage = -100; }
			
			_ram = ram;
			_ramusage = ram; // NOTE: With the current way of fetching RAM, there is no need to subtract the baseline ram.
			//if (baseline != null) { _ramusage = ram - baseline.Ram; }
			//else { _ramusage = -100; }
		}
		
		public int Index { get { return _index; } }
		public DateTime TimeStamp { get { return _time; } }
		public TimeSpan RunTime { get { return _runtime; } }
		public float Cpu { get { return _cpu; } }
		public float CpuUsage { get { return _cpuusage; } }
		public float Ram { get { return _ram; } }
		public float RamUsage { get { return _ramusage; } }

		public void RecalculateCpu()
		{
			_cpu = (_cpu / Environment.ProcessorCount);
		}
		
		/*public static implicit operator bool(ResourceSnapshot rs)
		{
			return true;
		}*/
	}
}

