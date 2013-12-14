using System;

namespace GuiTestLib
{
	[Serializable]
	public class ResourceSnapshot
	{
		private int _index;
		private string _name;
		private DateTime _time;
		private TimeSpan _runtime;
		private float _cpu;			// in %
		private float _ram;			// in B
		
		public ResourceSnapshot(DateTime time, float cpu, float ram) : this(null, -1, time, cpu, ram) {}
		public ResourceSnapshot(ResourceSnapshot baseline, int index, DateTime time, float cpu, float ram) :
			this(baseline, index, string.Empty, time, cpu, ram) {}
		public ResourceSnapshot(ResourceSnapshot baseline, int index, String name, DateTime time, float cpu, float ram)
		{
			_index = index;
			_name = name;
			
			_time = time;
			if (baseline != null) { _runtime = time - baseline.TimeStamp; }
			else { _runtime = TimeSpan.Zero; }

			_cpu = cpu;
			_ram = ram;
		}
		
		public int Index { get { return _index; } }
		public string Name { get { return _name; } }
		public DateTime TimeStamp { get { return _time; } }
		public TimeSpan RunTime { get { return _runtime; } }
		public float Cpu { get { return _cpu; } }
		public float Ram { get { return _ram; } }

		public bool RecalculateCpu()
		{
			_cpu = (_cpu / Environment.ProcessorCount);
			
			bool error = false;
			if (_cpu < 0) { error = true; }
			else if (_cpu > 100) { error = true; }
			return error;
		}
	}
}

