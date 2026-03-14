using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// HealPickupの回復量計算を検証します。
/// </summary>
public class HealPickupTests
{
    const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

    GameObject _playerObject;
    GameObject _pickupObject;
    HealthComponent _healthComponent;
    PlayerExperience _playerExperience;
    HealPickup _healPickup;

    /// <summary>
    /// テストごとのプレイヤーと回復アイテムを初期化します。
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _playerObject = new GameObject("Player");
        _playerObject.tag = "Player";
        _playerObject.AddComponent<CharacterStats>();
        _healthComponent = _playerObject.AddComponent<HealthComponent>();
        _playerExperience = _playerObject.AddComponent<PlayerExperience>();

        InvokePrivateMethod(_healthComponent, "Awake");

        _pickupObject = new GameObject("HealPickup");
        _healPickup = _pickupObject.AddComponent<HealPickup>();
    }

    /// <summary>
    /// テストで生成したオブジェクトを破棄します。
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (_pickupObject != null)
        {
            Object.DestroyImmediate(_pickupObject);
        }

        if (_playerObject != null)
        {
            Object.DestroyImmediate(_playerObject);
        }
    }

    /// <summary>
    /// 通常時は基礎回復量のまま回復することを検証します。
    /// </summary>
    [Test]
    public void CollectPickup_DefaultStyle_HealsBaseAmount()
    {
        _healthComponent.TakeDamage(5);

        InvokePrivateMethod(_healPickup, "OnEnable");
        InvokePrivateMethod(_healPickup, "CollectPickup");

        Assert.That(_healthComponent.CurrentHp, Is.EqualTo(_healthComponent.MaxHp - 2));
    }

    /// <summary>
    /// ナース時は倍率適用後の回復量が丸められて5になることを検証します。
    /// </summary>
    [Test]
    public void CollectPickup_NurseStyle_HealsRoundedAmount()
    {
        _healthComponent.TakeDamage(6);
        ChangeStyle(PlayerStyleType.Nurse);

        InvokePrivateMethod(_healPickup, "OnEnable");
        InvokePrivateMethod(_healPickup, "CollectPickup");

        Assert.That(_healthComponent.CurrentHp, Is.EqualTo(_healthComponent.MaxHp - 1));
    }

    /// <summary>
    /// 生成済みアイテムでも取得時点のスタイル倍率が適用されることを検証します。
    /// </summary>
    [Test]
    public void CollectPickup_PickupCreatedBeforeNurse_UsesCurrentMultiplierAtCollectionTime()
    {
        _healthComponent.TakeDamage(6);

        InvokePrivateMethod(_healPickup, "OnEnable");
        ChangeStyle(PlayerStyleType.Nurse);
        InvokePrivateMethod(_healPickup, "CollectPickup");

        Assert.That(_healthComponent.CurrentHp, Is.EqualTo(_healthComponent.MaxHp - 1));
    }

    /// <summary>
    /// 回復後HPが最大値を超えないことを検証します。
    /// </summary>
    [Test]
    public void CollectPickup_NurseStyle_DoesNotExceedMaxHp()
    {
        _healthComponent.TakeDamage(2);
        ChangeStyle(PlayerStyleType.Nurse);

        InvokePrivateMethod(_healPickup, "OnEnable");
        InvokePrivateMethod(_healPickup, "CollectPickup");

        Assert.That(_healthComponent.CurrentHp, Is.EqualTo(_healthComponent.MaxHp));
    }

    void ChangeStyle(PlayerStyleType styleType)
    {
        PlayerStyleEffectContext context = new PlayerStyleEffectContext(_healthComponent, _playerExperience.State);
        _playerExperience.ChangeStyle(styleType, context);
    }

    static void InvokePrivateMethod(object instance, string methodName)
    {
        MethodInfo method = instance.GetType().GetMethod(methodName, InstanceNonPublic);
        Assert.That(method, Is.Not.Null, $"{instance.GetType().Name}.{methodName} が見つかりません。");
        method.Invoke(instance, null);
    }
}
