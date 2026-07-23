using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action performs one dash.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Dash")]
	// [RequireComponent(typeof(CharacterDash))]
	public class AIActionDash : AIAction
	{
		/// if this is true, the agent will try to dash towards the AI Brain's Target if there's one
		[Tooltip("if this is true, the agent will try to dash towards the AI Brain's Target if there's one")]
		public bool DashTowardsTarget = false;
		
		protected CharacterDash _characterDash;
		protected Vector2 _dashDirection;

		/// <summary>
		/// On init we grab our CharacterDash component
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			_characterDash = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterDash>();
			if (DashTowardsTarget)
			{
				_characterDash.Aim.AimControl = MMAim.AimControls.Script;
			}
		}

		/// <summary>
		/// On PerformAction we dash
		/// </summary>
		public override void PerformAction()
		{
			Dash();
		}

		/// <summary>
		/// Calls CharacterDash's StartDash method to initiate the dash
		/// </summary>
		protected virtual void Dash()
		{
			if (_brain.Target != null)
			{
				_dashDirection = (_brain.Target.transform.position - this.transform.position).normalized;
				_characterDash.Aim.SetAim(_dashDirection);
			}
			_characterDash.StartDash();
		}
	}
}