using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoreMountains.CorgiEngine;

namespace CountDown.CorgiEngine
{
  /// <summary>
  /// Character Ability that drains players' hp over time.
  /// </summary>
  [RequireComponent(typeof(Health))]
  public class CharacterHealthDrain : CharacterAbility
  {
    public event Action<float, int> OnDrainThreshold;

    [Header("Drain Settings")]
    [Tooltip("the hp points the character looses on every drain. Min: 1")]
    [Min(1)]
    [SerializeField] private float drainPoints = 2;
    [Tooltip("the time in seconds in between each drain. Range: [0.5, 10]")]
    [Range(0.5f, 10f)]
    [SerializeField] private float drainCooldown = 0.05f;
    [Tooltip("the time in seconds before the drain starts. Min: 0 (no delay)")]
    [Min(0)]
    [SerializeField] private float initialDelay = 0.5f;

    [Header("Stack")]
    [Tooltip("the amount of additional drain points you can stack on top of the initial drainPoints")]
    [SerializeField] private int maxStacks = 3;
    [Tooltip("the amount of additional drain points you gain per stack")]
    [Min(1)]
    [SerializeField] private float pointsPerStack = 1;
    [Tooltip("the amount of additional drain cooldown you gain per stack (0 = none)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float cooldownPerStack = 0;

    [Header("Events")]
    [Tooltip("If Flickr on Damage is enabled on the health component, this is the duration of that flickr everytime it drains")]
    [SerializeField] private float flickrDuration = 0.5f;
    [Tooltip("If Invincibility on Damage is enabled on the health component, this is the duration of that invincibility everytime it drains")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    [Space]
    [Tooltip("specify values of hp when you want this component to fire an event")]
    [SerializeField] private List<int> thresholds = new List<int>();

    #region Properties

    // current effective drain points after applying any additional damage types the player might have
    public float EffectiveDrainPoints { get; private set; }

    // current effective drain cooldown after applying any additional damage types the player might have
    public float EffectiveDrainCooldown { get; private set; }
    
    public bool IsDraining { get; private set; }

    private Coroutine _drainCoroutine;
    private int _currentStacks;

    #endregion

    #region CharacterAbility

    protected override void Initialization()
    {
      base.Initialization();
      if (!_character || !_health)
      {
        Debug.LogWarning("Health component not found on " + gameObject.name + " draining disabled");
        _abilityInitialized = false;
        return;
      }
      ResetDrain();
    }

    public override void ProcessAbility()
    {
      if (CanDrain() && !IsDraining)
      {
        StartDrain();
      }
      if (!CanDrain() && IsDraining)
      {
        StopDrain();
      }
    }

    private IEnumerator DrainCoroutine()
    {
      while (true)
      {
        if (!CanDrain()) yield break;
        float previousHealth = _health.CurrentHealth;
        float drainAmount = EffectiveDrainPoints;
        _health.Damage(drainAmount, gameObject, flickrDuration, invincibilityDuration, Vector3.zero);
        TryTriggerDrainThreshold(previousHealth, _health.CurrentHealth, drainAmount);
        yield return new WaitForSeconds(EffectiveDrainCooldown);
      }
    }

    private IEnumerator InitialDelay()
    {
      yield return new WaitForSeconds(initialDelay);
    }

    #endregion

    #region Drain Methods

    private void StartDrain()
    {
      if (!CanDrain()) return;
      StartCoroutine(InitialDelay());
      _drainCoroutine = StartCoroutine(DrainCoroutine());
      IsDraining = true;
    }

    private void StopDrain()
    {
      if (_drainCoroutine != null)
      {
        StopCoroutine(_drainCoroutine);
        _drainCoroutine = null;
      }
      IsDraining = false;
    }

    private void ResetDrain()
    {
      _currentStacks = 0;
      EffectiveDrainPoints = drainPoints;
      EffectiveDrainCooldown = drainCooldown;
    }

    private bool CanDrain()
    {
      return AbilityPermitted && _health && _health.CurrentHealth > 0
             && _character && _character.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead;
    }

    private void TryTriggerDrainThreshold(float previousHealth, float currentHealth, float drainAmount)
    {
      if (currentHealth >= previousHealth)
      {
        return;
      }

      foreach (var threshold in thresholds.Where(threshold => previousHealth > threshold && currentHealth <= threshold))
      {
        print("CharacterHealthDrain: OnDrainThreshold "  + threshold );
        OnDrainThreshold?.Invoke(drainAmount, threshold);
      }
    }

    public void AddStack(int amount = 1)
    {
      amount = Mathf.Clamp(amount, 0, maxStacks);
      if (_currentStacks + amount > maxStacks)
      {
        return;
      }
      _currentStacks += amount;
      EffectiveDrainPoints += amount * pointsPerStack;
      EffectiveDrainCooldown += amount * cooldownPerStack;
    }

    public void RemoveStack(int amount = 1)
    {
      amount = Mathf.Clamp(amount, 0, maxStacks);
      if (_currentStacks - amount < 0)
      {
        return;
      }
      _currentStacks -= amount;
      EffectiveDrainPoints -= amount * pointsPerStack;
      EffectiveDrainCooldown -= amount * cooldownPerStack;
    }

    #endregion

    #region Events

    protected override void OnEnable()
    {
      base.OnEnable();
      ResetDrain();
      StartDrain();
    }

    protected override void OnDisable()
    {
      base.OnDisable();
      StopDrain();
    }

    #endregion

  }
}