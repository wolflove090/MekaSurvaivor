using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 武器レベル段階調整の固定性能テーブルを検証します。
/// </summary>
public class WeaponLevelTuningTests
{
    /// <summary>
    /// BulletWeaponがレベルごとの固定発射間隔を適用することを検証します。
    /// </summary>
    [Test]
    public void BulletWeapon_LevelIntervalsMatchTable()
    {
        float[] expectedIntervals = { 1.5f, 1.0f, 0.8f, 0.4f, 0.2f };

        GameObject player = new GameObject("Player");
        try
        {
            for (int level = 1; level <= 7; level++)
            {
                BulletWeapon weapon = new BulletWeapon(
                    player.transform,
                    null,
                    new RecordingWeaponEffectExecutor(),
                    null,
                    null,
                    () => 1);

                LevelUpTo(weapon, level);

                float interval = GetPrivateField<float>(weapon, "_shootInterval");
                float expected = level <= 5 ? expectedIntervals[level - 1] : expectedIntervals[4];
                Assert.That(interval, Is.EqualTo(expected).Within(0.0001f), $"level={level}");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// ThrowingWeaponがレベルごとの固定発射間隔を適用することを検証します。
    /// </summary>
    [Test]
    public void ThrowingWeapon_LevelIntervalsMatchTable()
    {
        float[] expectedIntervals = { 2.0f, 1.5f, 1.0f, 0.8f, 0.4f };

        GameObject player = new GameObject("Player");
        try
        {
            for (int level = 1; level <= 7; level++)
            {
                ThrowingWeapon weapon = new ThrowingWeapon(
                    player.transform,
                    null,
                    new RecordingWeaponEffectExecutor(),
                    () => 1,
                    () => Vector3.right);

                LevelUpTo(weapon, level);

                float interval = GetPrivateField<float>(weapon, "_shootInterval");
                float expected = level <= 5 ? expectedIntervals[level - 1] : expectedIntervals[4];
                Assert.That(interval, Is.EqualTo(expected).Within(0.0001f), $"level={level}");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// ThrowingWeaponがレベル5で左右2方向へ投擲することを検証します。
    /// </summary>
    [Test]
    public void ThrowingWeapon_LevelFiveFiresTwoSideProjectiles()
    {
        GameObject player = new GameObject("Player");
        try
        {
            RecordingWeaponEffectExecutor levelFourEffectExecutor = new RecordingWeaponEffectExecutor();
            ThrowingWeapon levelFourWeapon = new ThrowingWeapon(
                player.transform,
                null,
                levelFourEffectExecutor,
                () => 1,
                () => Vector3.right);

            LevelUpTo(levelFourWeapon, 4);
            levelFourWeapon.Tick(10f);
            Assert.That(levelFourEffectExecutor.ThrowingRequests.Count, Is.EqualTo(1));
            Assert.That(levelFourEffectExecutor.ThrowingRequests[0].Direction, Is.EqualTo(Vector3.right));

            RecordingWeaponEffectExecutor levelFiveEffectExecutor = new RecordingWeaponEffectExecutor();
            ThrowingWeapon levelFiveWeapon = new ThrowingWeapon(
                player.transform,
                null,
                levelFiveEffectExecutor,
                () => 1,
                () => Vector3.right);

            LevelUpTo(levelFiveWeapon, 5);
            levelFiveWeapon.Tick(10f);

            Assert.That(levelFiveEffectExecutor.ThrowingRequests.Count, Is.EqualTo(2));
            Assert.That(levelFiveEffectExecutor.ThrowingRequests[0].Direction, Is.EqualTo(Vector3.right));
            Assert.That(levelFiveEffectExecutor.ThrowingRequests[1].Direction, Is.EqualTo(Vector3.left));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// DamageFieldWeaponがレベルごとの固定サイズを適用することを検証します。
    /// </summary>
    [Test]
    public void DamageFieldWeapon_AreaScalesMatchTable()
    {
        float[] expectedScales = { 3.0f, 3.5f, 4.0f, 4.5f, 5.0f };

        GameObject player = new GameObject("Player");
        try
        {
            for (int level = 1; level <= 7; level++)
            {
                DamageFieldWeapon weapon = new DamageFieldWeapon(
                    player.transform,
                    null,
                    new RecordingWeaponEffectExecutor(),
                    () => 1);

                LevelUpTo(weapon, level);

                float areaScale = GetPrivateField<float>(weapon, "_currentAreaScale");
                float expected = level <= 5 ? expectedScales[level - 1] : expectedScales[4];
                Assert.That(areaScale, Is.EqualTo(expected).Within(0.0001f), $"level={level}");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// BoundBallWeaponがレベルごとの間隔・バウンド回数と方向本数を適用することを検証します。
    /// </summary>
    [Test]
    public void BoundBallWeapon_IntervalAndBounceAndDirectionMatchTable()
    {
        float[] expectedIntervals = { 2.0f, 1.5f, 1.5f, 0.8f, 0.8f };
        int[] expectedBounces = { 2, 2, 3, 3, 3 };

        GameObject player = new GameObject("Player");
        try
        {
            for (int level = 1; level <= 5; level++)
            {
                RecordingWeaponEffectExecutor effectExecutor = new RecordingWeaponEffectExecutor();
                BoundBallWeapon weapon = new BoundBallWeapon(
                    player.transform,
                    null,
                    effectExecutor,
                    () => 1);

                LevelUpTo(weapon, level);
                weapon.Tick(10f);

                float interval = GetPrivateField<float>(weapon, "_shootInterval");
                Assert.That(interval, Is.EqualTo(expectedIntervals[level - 1]).Within(0.0001f), $"level={level}");

                int expectedRequestCount = level >= 5 ? 2 : 1;
                Assert.That(effectExecutor.BoundBallRequests.Count, Is.EqualTo(expectedRequestCount), $"level={level}");
                foreach (BoundBallFireRequest request in effectExecutor.BoundBallRequests)
                {
                    Assert.That(request.MaxBounceCount, Is.EqualTo(expectedBounces[level - 1]), $"level={level}");
                }

                if (level >= 5)
                {
                    Assert.That(effectExecutor.BoundBallRequests[0].Direction, Is.EqualTo(Vector3.forward));
                    Assert.That(effectExecutor.BoundBallRequests[1].Direction, Is.EqualTo(Vector3.back));
                }
                else
                {
                    Assert.That(effectExecutor.BoundBallRequests[0].Direction, Is.EqualTo(Vector3.back));
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// FlameBottleWeaponがレベルごとの間隔と投射本数を適用することを検証します。
    /// </summary>
    [Test]
    public void FlameBottleWeapon_IntervalAndProjectileCountMatchTable()
    {
        float[] expectedIntervals = { 1.5f, 1.0f, 1.0f, 1.0f, 1.0f };
        int[] expectedProjectileCounts = { 1, 1, 2, 3, 4 };

        GameObject player = new GameObject("Player");
        try
        {
            for (int level = 1; level <= 5; level++)
            {
                RecordingWeaponEffectExecutor effectExecutor = new RecordingWeaponEffectExecutor();
                FlameBottleWeapon weapon = new FlameBottleWeapon(
                    player.transform,
                    null,
                    effectExecutor,
                    () => 1,
                    () => Vector3.right);

                LevelUpTo(weapon, level);
                weapon.Tick(10f);

                float interval = GetPrivateField<float>(weapon, "_throwInterval");
                Assert.That(interval, Is.EqualTo(expectedIntervals[level - 1]).Within(0.0001f), $"level={level}");
                Assert.That(effectExecutor.FlameBottleRequests.Count, Is.EqualTo(expectedProjectileCounts[level - 1]), $"level={level}");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// DroneWeaponがレベルごとの攻撃間隔を適用し、レベル5で2機を位相違いで要求することを検証します。
    /// </summary>
    [Test]
    public void DroneWeapon_LevelFiveDeploysTwoPhasedRequests()
    {
        float[] expectedIntervals = { 2.0f, 1.5f, 1.0f, 0.8f, 0.4f };

        GameObject player = new GameObject("Player");
        try
        {
            for (int level = 1; level <= 5; level++)
            {
                RecordingWeaponEffectExecutor effectExecutor = new RecordingWeaponEffectExecutor();
                DroneWeapon weapon = new DroneWeapon(
                    player.transform,
                    null,
                    effectExecutor,
                    () => 1);

                LevelUpTo(weapon, level);
                weapon.Tick(10f);

                float interval = GetPrivateField<float>(weapon, "_droneShotInterval");
                Assert.That(interval, Is.EqualTo(expectedIntervals[level - 1]).Within(0.0001f), $"level={level}");

                int expectedDeployCount = level >= 5 ? 2 : 1;
                Assert.That(effectExecutor.DroneRequests.Count, Is.EqualTo(expectedDeployCount), $"level={level}");

                foreach (DroneSpawnRequest request in effectExecutor.DroneRequests)
                {
                    Assert.That(request.ShotInterval, Is.EqualTo(expectedIntervals[level - 1]).Within(0.0001f));
                }

                if (level == 5)
                {
                    Assert.That(effectExecutor.DroneRequests[0].PhaseOffsetDegrees, Is.EqualTo(0f).Within(0.0001f));
                    Assert.That(effectExecutor.DroneRequests[1].PhaseOffsetDegrees, Is.EqualTo(180f).Within(0.0001f));
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// RebuildWeapons後も固定テーブル値が崩れないことを検証します。
    /// </summary>
    [Test]
    public void RebuildWeapons_AppliesFixedTablesForEachWeapon()
    {
        GameObject player = new GameObject("Player");

        try
        {
            WeaponService service = new WeaponService(
                player.transform,
                new RecordingWeaponEffectExecutor(),
                () => 2,
                () => Vector3.right);

            Dictionary<WeaponUpgradeUiController.UpgradeCardType, int> targetLevels =
                new Dictionary<WeaponUpgradeUiController.UpgradeCardType, int>
                {
                    { WeaponUpgradeUiController.UpgradeCardType.Shooter, 5 },
                    { WeaponUpgradeUiController.UpgradeCardType.Throwing, 4 },
                    { WeaponUpgradeUiController.UpgradeCardType.DamageField, 3 },
                    { WeaponUpgradeUiController.UpgradeCardType.Drone, 5 },
                    { WeaponUpgradeUiController.UpgradeCardType.BoundBall, 5 },
                    { WeaponUpgradeUiController.UpgradeCardType.FlameBottle, 4 }
                };
            Dictionary<Type, WeaponBase> weapons = new Dictionary<Type, WeaponBase>();

            service.RebuildWeapons(targetLevels, weapons);

            Assert.That(GetPrivateField<float>((BulletWeapon)weapons[typeof(BulletWeapon)], "_shootInterval"), Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(GetPrivateField<float>((ThrowingWeapon)weapons[typeof(ThrowingWeapon)], "_shootInterval"), Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(GetPrivateField<float>((DamageFieldWeapon)weapons[typeof(DamageFieldWeapon)], "_currentAreaScale"), Is.EqualTo(4.0f).Within(0.0001f));
            Assert.That(GetPrivateField<float>((DroneWeapon)weapons[typeof(DroneWeapon)], "_droneShotInterval"), Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(GetPrivateField<float>((BoundBallWeapon)weapons[typeof(BoundBallWeapon)], "_shootInterval"), Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(GetPrivateField<int>((BoundBallWeapon)weapons[typeof(BoundBallWeapon)], "_maxBounceCount"), Is.EqualTo(3));
            Assert.That(GetPrivateField<float>((FlameBottleWeapon)weapons[typeof(FlameBottleWeapon)], "_throwInterval"), Is.EqualTo(1.0f).Within(0.0001f));
            Assert.That(GetPrivateField<int>((FlameBottleWeapon)weapons[typeof(FlameBottleWeapon)], "_projectileCount"), Is.EqualTo(3));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 対象武器を指定レベルまで強化します。
    /// </summary>
    /// <param name="weapon">強化対象の武器</param>
    /// <param name="targetLevel">到達させるレベル</param>
    static void LevelUpTo(WeaponBase weapon, int targetLevel)
    {
        int safeTargetLevel = Mathf.Max(1, targetLevel);
        while (weapon.UpgradeLevel < safeTargetLevel)
        {
            weapon.LevelUp();
        }
    }

    /// <summary>
    /// privateフィールド値を取得します。
    /// </summary>
    /// <typeparam name="T">取得する型</typeparam>
    /// <param name="instance">取得対象インスタンス</param>
    /// <param name="fieldName">フィールド名</param>
    /// <returns>取得したフィールド値</returns>
    static T GetPrivateField<T>(object instance, string fieldName)
    {
        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field not found: {fieldName}");
        return (T)field.GetValue(instance);
    }

    /// <summary>
    /// テスト用に武器要求を記録する実装です。
    /// </summary>
    sealed class RecordingWeaponEffectExecutor : IWeaponEffectExecutor
    {
        /// <summary>
        /// 受け取ったドローン展開要求一覧を取得します。
        /// </summary>
        public List<DroneSpawnRequest> DroneRequests { get; } = new List<DroneSpawnRequest>();

        /// <summary>
        /// 受け取った投擲弾発射要求一覧を取得します。
        /// </summary>
        public List<ThrowingFireRequest> ThrowingRequests { get; } = new List<ThrowingFireRequest>();

        /// <summary>
        /// 受け取ったバウンドボール発射要求一覧を取得します。
        /// </summary>
        public List<BoundBallFireRequest> BoundBallRequests { get; } = new List<BoundBallFireRequest>();

        /// <summary>
        /// 受け取った火炎瓶発射要求一覧を取得します。
        /// </summary>
        public List<FlameBottleFireRequest> FlameBottleRequests { get; } = new List<FlameBottleFireRequest>();

        /// <summary>
        /// 通常弾の発射要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireBullet(BulletFireRequest request)
        {
        }

        /// <summary>
        /// 投擲弾の発射要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireThrowing(ThrowingFireRequest request)
        {
            ThrowingRequests.Add(request);
        }

        /// <summary>
        /// ダメージフィールド生成要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void SpawnDamageField(DamageFieldSpawnRequest request)
        {
        }

        /// <summary>
        /// ドローン展開要求を記録します。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void DeployDrone(DroneSpawnRequest request)
        {
            DroneRequests.Add(request);
        }

        /// <summary>
        /// バウンドボール発射要求を記録します。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireBoundBall(BoundBallFireRequest request)
        {
            BoundBallRequests.Add(request);
        }

        /// <summary>
        /// 火炎瓶発射要求を記録します。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireFlameBottle(FlameBottleFireRequest request)
        {
            FlameBottleRequests.Add(request);
        }

        /// <summary>
        /// 炎エリア生成要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void SpawnFlameArea(FlameAreaSpawnRequest request)
        {
        }
    }
}
