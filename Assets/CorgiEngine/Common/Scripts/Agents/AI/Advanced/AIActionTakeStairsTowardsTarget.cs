using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This Action will let an AI agent climb up or down stairs based on the relative y position of its Target
	/// Requires a CharacterStairs Ability
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Take Stairs Towards Target")]
	public class AIActionTakeStairsTowardsTarget : AIAction
	{
		protected CharacterStairs _characterStairs;

		/// <summary>
		/// On init we grab our CharacterStairs ability
		/// </summary>
		public override void Initialization()
		{
			if (!ShouldInitialize)
			{
				return;
			}
			_characterStairs = gameObject.GetComponent<CharacterStairs>();
		}

		public override void PerformAction()
		{
			MoveTowardsTarget();
		}

		/// <summary>
		/// Sets stairs input based on the relative y position of the target
		/// </summary>
		protected virtual void MoveTowardsTarget()
		{
			if (_brain.Target == null)
			{
				return;
			}
			
			float verticalDistanceToTarget = _brain.Target.position.y - transform.position.y;

			if (_characterStairs != null)
			{
				bool up = verticalDistanceToTarget > 0f;
				bool down = verticalDistanceToTarget < 0f;
				_characterStairs.SetInput(up, down);
			}
		}
	}
}