using System;
using MyBox;
using UnityEngine;
using UnityDevKit.Events;

namespace UnityDevKit.Utils.TimeHandlers
{
	public class TimeManager : Patterns.Singleton<TimeManager>, IClock
	{
		[SerializeField] [PositiveValueOnly] private float startTimeScale = 1;
		[SerializeField] [PositiveValueOnly] private RangedFloat timeScaleBounds = new RangedFloat { Min = 0.5f, Max = 16f};
		[SerializeField] [PositiveValueOnly] private float timeScaleStep = 2f;
		public static float CurrentTimeScale => Time.timeScale;
		public bool IsPaused { get; private set; }

		public EventHolder<float> OnTimeModeChanged { get; } = new();

		private float _lastTimeScale;

		private readonly Clock _clock = new Clock();
		
		
		public override void Awake()
		{
			base.Awake();
			SetTimeMode(startTimeScale);
			_lastTimeScale = CurrentTimeScale;
		}

		public void SetTimeScaleBounds(RangedFloat bounds)
		{
			timeScaleBounds = bounds;
		}

		public void SetTimeMode(float timeScale)
		{
			Time.timeScale = timeScale;
			Debug.Log("Time scale changed to " + Time.timeScale);
			OnTimeModeChanged.Invoke(CurrentTimeScale);
		}

		public void SpeedDown()
		{
			if (IsPaused) return;
			var newTimeScale = CurrentTimeScale / timeScaleStep;
			var boundedNewTimeScale = Math.Max(newTimeScale, timeScaleBounds.Min);
			SetTimeMode(boundedNewTimeScale);
		}

		public void SpeedUp()
		{
			if (IsPaused) return;
			var newTimeScale = CurrentTimeScale * timeScaleStep;
			var boundedNewTimeScale = Math.Min(newTimeScale, timeScaleBounds.Max);
			SetTimeMode(boundedNewTimeScale);
		}

		public void SpeedUpLoop()
		{
			if (IsPaused) return;
			var newTimeScale = CurrentTimeScale * timeScaleStep;
			var loopedNewTimeScale = newTimeScale > timeScaleBounds.Max ? timeScaleBounds.Min : newTimeScale;
			SetTimeMode(loopedNewTimeScale);
		}

		public void Launch()
		{
			_clock.Launch();
		}

		public Clock.Data Stop()
		{
			return _clock.Stop();
		}

		public void Pause()
		{
			IsPaused = true;
			_lastTimeScale = CurrentTimeScale;
			SetTimeMode(0f);
			_clock.Pause();
		}

		public void Resume()
		{
			IsPaused = false;
			SetTimeMode(_lastTimeScale);
			_clock.Resume();
		}

		private bool IsInBounds(float timeScale) => timeScale >= timeScaleBounds.Min && timeScale <= timeScaleBounds.Max;
	}
}