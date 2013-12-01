using System;
using System.Text;

namespace GuiTestLib
{
	public class Random
	{
		private const int STRINGSIZE_SHORT = 8;
		private const int STRINGSIZE_MEDIUM = 32;
		private const int STRINGSIZE_LONG = 128;
		private const string STOPPEDSTRING = "stopped";
		private const int STOPPEDINTEGER = -1;
		private const double STOPPEDDOUBLE = -1d;
		
		private System.Random _rng;
		private StringBuilder _sb;
		
		private bool _disabled = false;
		private int _stringsgenerated = 0;
		private int _stringsgenerated_short = 0;
		private int _stringsgenerated_medium = 0;
		private int _stringsgenerated_long = 0;
		private int _integersgenerated = 0;
		private int _doublesgenerated = 0;
		
		public Random()
		{
			_rng = new System.Random(new System.Random((int)DateTime.Now.Ticks).Next());
			_sb = new StringBuilder();
		}
		
		private string GenerateString(int size)
		{
			_sb.Clear();
			char ch;
			for (int i = 0; i < size; i++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _rng.NextDouble() + 65)));                 
				_sb.Append(ch);
			}
			_stringsgenerated++;
			return _sb.ToString();
		}

		public void Disable()
		{
			_disabled = true;
		}
		
		public int ShortStringsGenerated { get { return _stringsgenerated_short; } }
		public int StringsGenerated { get { return _stringsgenerated_medium; } }
		public int LongStringsGenerated { get { return _stringsgenerated_long; } }
		public int IntegersGenerated { get { return _integersgenerated; } }
		public int DoublesGenerated { get { return _doublesgenerated; } }

		public string String
		{
			get
			{
				if (_disabled)
				{
					return STOPPEDSTRING;
				}
				else
				{
					_stringsgenerated_medium++;
					return GenerateString(STRINGSIZE_MEDIUM);
				}
			}
		}

		public string ShortString
		{
			get
			{
				if (_disabled)
				{
					return STOPPEDSTRING;
				}
				else
				{
					_stringsgenerated_short++;
					return GenerateString(STRINGSIZE_SHORT);
				}
			}
		}

		public string LongString
		{
			get
			{
				if (_disabled)
				{
					return STOPPEDSTRING;
				}
				else
				{
					_stringsgenerated_long++;
					return GenerateString(STRINGSIZE_LONG);
				}
			}
		}

		public int Int
		{
			get
			{
				if (_disabled)
				{
					return STOPPEDINTEGER;
				}
				else
				{
					_integersgenerated++;
					return _rng.Next();
				}
			}
		}

		public double Double
		{
			get
			{
				if (_disabled)
				{
					return STOPPEDDOUBLE;
				}
				else
				{
					_doublesgenerated++;
					return _rng.NextDouble();
				}
			}
		}
	}
}

