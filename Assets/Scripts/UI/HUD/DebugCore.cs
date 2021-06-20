
//-------------------------------------------------------------------------------------------------
//  Debug Core - Clothes Combat                                                     v0.0_2018.06.17
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the debug console UI element which can be statically referenced by other scripts that
/// are reporting information, and is turned on and off in-game by the player.
/// </summary>
public class DebugCore : MonoBehaviour 
{
	#region Classes

	public class Channel
	{
		public int priority;
		public string contents;

		public Channel(int priority, string contents)
		{
			this.priority = priority;
			this.contents = contents;
		}
	}

	#endregion

	#region Static Fields

	public static DebugCore main;

	#endregion

	#region Inspector Fields

	[Header("References")]
	[Tooltip("A reference to the UI panel GameObject containing the console elements.")]
	[SerializeField]
	private GameObject refConsolePanel;

	[Tooltip("A reference to the main console text element, to update with channel contents.")]
	[SerializeField]
	private Text refConsoleText;

	[Space(10)]

	[Tooltip("The line to write in the console before all channel text.")]
	[SerializeField]
	private string consoleHeader;

	[Tooltip("If false the panel is hidden at start (Default: False).")]
	[SerializeField]
	private bool isDisplayedAtStart = false;

	[Space(10)]

	[Header("Input")]
	[Tooltip("The KeyCodes of keys that when pressed toggle the console window on and off in-game.")]
	[SerializeField]
	private KeyCode[] toggleKeys;

	#endregion

	#region Run-Time Fields

	private List<Channel> channels;

	#endregion

	#region Monobehaviours

	private void Awake()
	{
		if (main != null)
		{
			Destroy(gameObject);
		}

		main = this;

		channels = new List<Channel>();
	}

	private void Start()
	{
		SetConsoleVisibility(isDisplayedAtStart);
	}

	private void Update()
	{
		if (GetPress())
		{
			ToggleConsole();
		}
	}

	#endregion

	#region Private Methods

	private bool GetPress()
	{
		foreach (KeyCode key in toggleKeys)
		{
			if (Input.GetKeyDown(key))
			{
				return true;
			}
		}

		return false;
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Toggles the Console panel on or off.
	/// </summary>
	/// <returns>True if the console UI panel is now currently visible.</returns>
	public bool ToggleConsole()
	{
		SetConsoleVisibility(!refConsolePanel.activeSelf);

		return refConsolePanel.activeSelf;
	}

	/// <summary>
	/// Sets the console panel visibility.
	/// </summary>
	/// <param name="visibility">The console's desired visibility status.</param>
	public void SetConsoleVisibility(bool visibility)
	{
		refConsolePanel.SetActive(visibility);
	}

	/// <summary>
	/// Checks if the debug console panel is currently being displayed.
	/// </summary>
	/// <returns>True if the console UI panel is currently visible.</returns>
	public bool IsConsoleDisplayed()
	{
		return refConsolePanel.activeSelf;
	}

	/// <summary>
	/// Creates a new console channel.
	/// </summary>
	/// <param name="priority">The desired priority level. Smaller priority levels go first
	///  (default: 0).</param>
	/// <param name="contents">The initial contents of the channel (default: "").</param>
	/// <returns>The index of the new channel (new length of the channels list).</returns>
	public int CreateNewChannel(int priority = 0, string contents = "")
	{
		channels.Add(new Channel(priority, contents));

		return channels.Count - 1;
	}

	/// <summary>
	/// Sets the text inside a console channel and optionally updates the console.
	/// </summary>
	/// <param name="channelIndex">The index of the desired channel to update.</param>
	/// <param name="text">The text to update the channel to.</param>
	/// <param name="updateAfter">If true, updates the console text after (default: true).</param>
	public void SetConsoleChannelText(int channelIndex, string text, bool updateAfter = true)
	{
		channels[channelIndex].contents = text;

		if (updateAfter)
		{
			UpdateConsoleText();
		}
	}

	/// <summary>
	/// Updates all of the channels in the console with current text.
	/// </summary>
	public void UpdateConsoleText()
	{
		// Sort
		Channel[] sortedChannels = channels.ToArray();
		Channel tempChannel;
		for (int p = 0; p <= sortedChannels.Length - 2; p++)
		{
			for (int i = 0; i <= sortedChannels.Length - 2; i++)
			{
				if (sortedChannels[i].priority > sortedChannels[i + 1].priority)
				{
					tempChannel = sortedChannels[i + 1];
					sortedChannels[i + 1] = sortedChannels[i];
					sortedChannels[i] = tempChannel;
				}
			}
		}

		string activeText = consoleHeader + "\n";

		foreach (Channel channel in sortedChannels)
		{
			activeText += channel.contents + "\n";
		}

		refConsoleText.text = activeText;
	}

	#endregion
}
