using System;
using System.Timers;

namespace GuiTestLib
{
	public class GuiTracker
	{
		private GuiTestLib.Random _random;
		private ResourceUsage _resourceusage;
		private Timer _timer;
		
		private DateTime _starttime;
		private DateTime _endtime;
		private TimeSpan _executiontime;
		
		private string _applicationname;

		public GuiTracker(string application_name)
		{
			_random = new GuiTestLib.Random();
			_resourceusage = new ResourceUsage();
			_timer = new Timer(1000);
			_timer.Elapsed += new ElapsedEventHandler(Tick);

			_starttime = DateTime.Now;
			//_endtime = null;
			_executiontime = TimeSpan.Zero;
			
			_applicationname = application_name;
			
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
		}
	}
}
