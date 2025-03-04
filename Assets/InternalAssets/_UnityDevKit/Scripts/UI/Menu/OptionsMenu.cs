using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UnityDevKit.UI_Handlers.Menu
{
	public class OptionsMenu : MonoBehaviour
	{
		public AudioMixer audioMixer;
		public Dropdown resolDropdown;

		Resolution[] resolutions;

		void Start()
		{
			resolutions = Screen.resolutions;
			resolDropdown.ClearOptions();

			var options = new List<string>();

			var currentResolutionIndex = 0;

			for (var i = 0; i < resolutions.Length; i++)
			{
				var option = resolutions[i].width + " x " + resolutions[i].height;
				options.Add(option);

				if (resolutions[i].width == Screen.currentResolution.width &&
				    resolutions[i].height == Screen.currentResolution.height)
				{
					currentResolutionIndex = i;
				}

			}

			resolDropdown.AddOptions(options);
			resolDropdown.value = currentResolutionIndex;
			resolDropdown.RefreshShownValue();

		}

		public void FullscreenMode(bool isFullscreen)
		{
			Screen.fullScreen = isFullscreen;
		}


		public void SetResolution(int resolutionIndex)
		{
			var resolution = resolutions[resolutionIndex];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
		}

		public void SetQuality(int qualityIndex)
		{
			QualitySettings.SetQualityLevel(qualityIndex);
		}

		public void SetVolume(float volume)
		{
			audioMixer.SetFloat("volume", volume);
		}


	}
}