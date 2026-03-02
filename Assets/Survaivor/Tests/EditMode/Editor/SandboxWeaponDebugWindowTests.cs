using System.Reflection;
using NUnit.Framework;
using UnityEditor;

/// <summary>
/// Sandbox用Editor拡張の基本動作を検証します。
/// </summary>
public class SandboxWeaponDebugWindowTests
{
    /// <summary>
    /// Sandboxシーン再生時のみ自動起動対象になることを検証します。
    /// </summary>
    [Test]
    public void ShouldOpenForScene_ReturnsTrueOnlyForSandboxScene()
    {
        MethodInfo method = typeof(SandboxPlayModeHook).GetMethod(
            "ShouldOpenForScene",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        Assert.That((bool)method.Invoke(null, new object[] { "Assets/Survaivor/Editor/Sandbox.unity" }), Is.True);
        Assert.That((bool)method.Invoke(null, new object[] { "Assets/Survaivor/Main.unity" }), Is.False);
    }

    /// <summary>
    /// デバッグウィンドウを開いた時に想定タイトルが設定されることを検証します。
    /// </summary>
    [Test]
    public void Open_WhenCalled_SetsExpectedWindowTitle()
    {
        SandboxWeaponDebugWindow window = null;

        try
        {
            SandboxWeaponDebugWindow.Open();
            window = EditorWindow.GetWindow<SandboxWeaponDebugWindow>();

            MethodInfo method = typeof(SandboxWeaponDebugWindow).GetMethod(
                "GetWindowTitle",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(window.titleContent.text, Is.EqualTo((string)method.Invoke(null, null)));
        }
        finally
        {
            window?.Close();
        }
    }
}
