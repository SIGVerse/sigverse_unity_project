using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Kyusyukeigo.Helper;

namespace SIGVerse.Common
{
	[InitializeOnLoad]
	public class GameViewSizeInitializer
	{
		private static readonly GameViewSizeGroupType groupType = GameViewSizeGroupType.Standalone;
		private static readonly GameViewSizeHelper.GameViewSizeType gameViewSizeType = GameViewSizeHelper.GameViewSizeType.AspectRatio;
		private static readonly int width = 16;
		private static readonly int height = 9;
		private static readonly string text = "SIGVerse Default";

		static GameViewSizeInitializer()
		{
			if(!GameViewSizeHelper.Contains(groupType, gameViewSizeType, width, height, text))
			{
				GameViewSizeHelper.AddCustomSize(groupType, gameViewSizeType, width, height, text);
			}
		}

		[MenuItem("SIGVerse/Set Default GameView Size")]
		protected static void SetDefaultGameViewSize()
		{
			GameViewSizeHelper.ChangeGameViewSize(groupType, gameViewSizeType, width, height, text);
		}
	}
}

