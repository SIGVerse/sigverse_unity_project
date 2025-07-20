using System.Collections.Generic;
using System;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.RosBridge
{
	public interface IRosConnection
	{
		bool IsConnected();
		void Clear();
		void Close();
	}
}

