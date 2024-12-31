using Receiver2;
using Receiver2ModdingKit;
using Receiver2ModdingKit.CustomSounds;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saiga9
{
	public class Saiga9 : ModGunScript
	{
		public RotateMover stockRotateMover;

		public float _hammerAccel = 5000f;

		private float lastTriggerAmount = 0f;

		[HideInInspector, NonSerialized]
		public float[] slide_push_hammer_curve = new float[]
		{
			0f,
			0f,
			0.3f,
			1f,
		};

		public override void InitializeGun()
		{
			pooled_muzzle_flash = ReceiverCoreScript.Instance().GetGunPrefab(GunModel.m1911).pooled_muzzle_flash;
		}

		public override void AwakeGun()
		{
			stockRotateMover = new RotateMover();
			stockRotateMover.transform = transform.Find("stock");
			stockRotateMover.rotations[0] = transform.Find("stock_unfolded").rotation;
			stockRotateMover.rotations[1] = transform.Find("stock_folded").rotation;

			hammer.accel = _hammerAccel;

			hammer.amount = 1f;
		}

		public override void OnHolster()
		{
			ToggleStock();
		}

		public override void OnUnholster()
		{
			ToggleStock();
		}

		public override void UpdateGun()
		{
			if (trigger.amount == 1 && lastTriggerAmount < trigger.amount)
			{
				ModAudioManager.PlayOneShotAttached("event:/Saiga9/saiga9_trigger_pressed", trigger.transform);
			}

			lastTriggerAmount = trigger.amount;

			hammer.asleep = true;

			if (slide.amount > 0 && _hammer_state != 3)
			{ // Bolt cocks the hammer when moving back 
				hammer.amount = Mathf.Max(hammer.amount, InterpCurve(slide_push_hammer_curve, slide.amount));
			}

			hammer.UpdateDisplay();

			if (hammer.amount == 1)
				_hammer_state = 3;

			if (IsSafetyOn())
			{
				trigger.amount = Mathf.Min(trigger.amount, 0.1f);

				trigger.UpdateDisplay();
			}

			if (hammer.amount == 0 && _hammer_state == 2)
			{ // If hammer dropped and hammer was cocked then fire gun and decock hammer
				TryFireBullet(1, FireBullet);

				_hammer_state = 0;
			}

			if (!_disconnector_needs_reset)
			{
				_disconnector_needs_reset = slide.amount > 0f && trigger.amount > 0f;
			}

			if (trigger.amount == 0)
			{
				if (_disconnector_needs_reset)
					ModAudioManager.PlayOneShotAttached("event:/Saiga9/saiga9_trigger_reset", trigger.transform);

				_disconnector_needs_reset = false;
			}


			if (slide.amount == 0 && _hammer_state == 3 && _disconnector_needs_reset == false)
			{ // Simulate auto sear
				hammer.amount = Mathf.MoveTowards(hammer.amount, _hammer_cocked_val, Time.deltaTime * Time.timeScale * 50);
				if (hammer.amount == _hammer_cocked_val)
					_hammer_state = 2;
			}

			if (_hammer_state != 3 && ((trigger.amount == 1 && !_disconnector_needs_reset && slide.amount == 0) || hammer.amount != _hammer_cocked_val))
			{
				hammer.asleep = false;
			}

			hammer.TimeStep(Time.deltaTime);
			hammer.UpdateDisplay();

			stockRotateMover.UpdateDisplay();
			stockRotateMover.TimeStep(Time.deltaTime);

			stockRotateMover.UpdateDisplay();
		}

		private void ToggleStock()
		{
			stockRotateMover.asleep = false;
			if (stockRotateMover.target_amount == 1f)
			{
				stockRotateMover.target_amount = 0f;
				stockRotateMover.accel = -1f;
				stockRotateMover.vel = -3f;
				AudioManager.PlayOneShotAttached("event:/Saiga9/saiga9_unholster", stockRotateMover.transform.gameObject);
			}
			else if (stockRotateMover.target_amount == 0f)
			{
				stockRotateMover.target_amount = 1f;
				stockRotateMover.accel = 1;
				stockRotateMover.vel = 3;
				AudioManager.PlayOneShotAttached("event:/Saiga9/saiga9_holster", stockRotateMover.transform.gameObject);
			}
		}
	}
}