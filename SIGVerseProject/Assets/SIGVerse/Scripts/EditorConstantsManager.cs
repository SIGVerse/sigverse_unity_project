using System;
using System.Collections.ObjectModel;

/// <summary>
/// Constants management class
/// </summary>
public static class EditorConstantsManager
{
	public static readonly ReadOnlyCollection<string> sceneNameArray
		= Array.AsReadOnly(new string[] { "SIGVerseStartup", "TurtlebotFollower", "SampleHouse" });
}
