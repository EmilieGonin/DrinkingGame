using System;
using TMPro;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public static event Action<Currency, int> OnCounterAdd;
    public static event Action<Currency, int> OnCounterRemove;

    [SerializeField] private TMP_Text _counterNumber;
    [SerializeField] private Currency _currency;
    [SerializeField] private bool _isOwner;

    private bool _shieldIsUp;
    private bool _challengeShieldIsUp;

    private int _counter = 0;

    private void Awake()
    {
        PlayerNetwork.OnChallengeCompleted += PlayerNetwork_OnChallengeCompleted;
        EffectSO.OnActivate += EffectSO_OnActivate;
        EffectSO.OnDeactivate += EffectSO_OnDeactivate;
        EffectSO.OnInflict += EffectSO_OnInflict;

        if (_isOwner) return;
        PlayerNetwork.OnCounterAdd += PlayerNetwork_OnCounterAdd;
        PlayerNetwork.OnGetFriendStats += PlayerNetwork_OnGetFriendStats;
    }

    private void Start()
    {
        if (!_isOwner) return;
        _counter = GameManager.Instance.Currencies[_currency];
        _counterNumber.text = _counter.ToString();
    }

    private void OnDestroy()
    {
        PlayerNetwork.OnChallengeCompleted -= PlayerNetwork_OnChallengeCompleted;
        EffectSO.OnActivate -= EffectSO_OnActivate;
        EffectSO.OnDeactivate -= EffectSO_OnDeactivate;
        EffectSO.OnInflict -= EffectSO_OnInflict;

        if (_isOwner) return;
        PlayerNetwork.OnCounterAdd -= PlayerNetwork_OnCounterAdd;
        PlayerNetwork.OnGetFriendStats -= PlayerNetwork_OnGetFriendStats;
    }

    private void EffectSO_OnActivate(EffectSO effect)
    {
        if (effect is SipTransfer && _currency == Currency.SipsToDrink) RemoveCounter(5);
        else if (effect is ChallengeShield) _challengeShieldIsUp = true;
        else if (effect is Shield) _shieldIsUp = true;
    }

    private void EffectSO_OnDeactivate(EffectSO effect)
    {
        if (effect is Shield) _shieldIsUp = false;
    }

    private void EffectSO_OnInflict(EffectSO effect)
    {
        if (effect is SipTransfer) AddCounter(5);
        else if (effect is Tsunami) AddCounter(10);
    }

    private void PlayerNetwork_OnChallengeCompleted(bool victory)
    {
        if (_currency == Currency.Sips || victory || _shieldIsUp) return;

        if (_challengeShieldIsUp)
        {
            _challengeShieldIsUp = false;
            return;
        }

        AddCounter(5);
    }

    private void PlayerNetwork_OnCounterAdd(Currency currency, int amount)
    {
        if (currency != _currency) return;
        AddCounter(amount);
    }

    private void PlayerNetwork_OnGetFriendStats(int sips, int shots)
    {
        if (_currency == Currency.Sips)
        {
            _counter = sips;
            _counterNumber.text = _counter.ToString();
        }
        else if (_currency == Currency.Shots)
        {
            _counter = shots;
            _counterNumber.text = _counter.ToString();
        }
    }

    public void AddCounter(int amount = 1)
    {
        if (_currency == Currency.SipsToDrink && _shieldIsUp) return;
        if (GameManager.Instance.HasEffect<DoubleSip>()) amount *= 2;
        _counter += amount;
        _counterNumber.text = _counter.ToString();
        if (_isOwner) OnCounterAdd?.Invoke(_currency, amount);
    }

    public void RemoveCounter(int amount = 1)
    {
        if (_counter == 0) return;
        _counter -= amount;
        _counterNumber.text = _counter.ToString();
        OnCounterRemove?.Invoke(_currency, amount);
    }
}