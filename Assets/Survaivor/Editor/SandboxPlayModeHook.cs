using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Sandboxシーン再生開始時にデバッグウィンドウを自動表示します。
/// </summary>
[InitializeOnLoad]
public static class SandboxPlayModeHook
{
    const string SandboxScenePath = "Assets/Survaivor/Editor/Sandbox.unity";

    /// <summary>
    /// PlayModeイベント購読を初期化します。
    /// </summary>
    static SandboxPlayModeHook()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary>
    /// 現在のシーンパスが自動起動対象かどうかを返します。
    /// </summary>
    /// <param name="scenePath">判定するシーンパス</param>
    /// <returns>自動起動対象の場合はtrue</returns>
    internal static bool ShouldOpenForScene(string scenePath)
    {
        return scenePath == SandboxScenePath;
    }

    /// <summary>
    /// PlayMode状態変化に応じてウィンドウ自動起動を判定します。
    /// </summary>
    /// <param name="state">遷移後のPlayMode状態</param>
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode)
        {
            return;
        }

        if (!ShouldOpenForScene(SceneManager.GetActiveScene().path))
        {
            return;
        }

        SandboxWeaponDebugWindow.Open();
    }
}
