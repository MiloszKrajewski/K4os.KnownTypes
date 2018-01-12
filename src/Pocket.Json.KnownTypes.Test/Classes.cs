using System;
using System.Collections.Generic;
using System.Text;

namespace Pocket.Json.KnownTypes.Test
{
	public class Base
	{
		public string Text { get; set; }
	}

	public class Derived: Base
	{
		public int Value { get; set; }
	}

	public class Other: Base
	{
		public double Value { get; set; }
	}

	public class Envelope
	{
		public Base Data { get; set; }
	}
}
