using UnityEngine.SceneManagement;

//CleanCode if this class is meant to just hold static data, make the class static

public static class Loader
{
    public static SceneNames targetScene;
    public enum SceneNames { MainMenuScene, GameScene, LoadingScene }; 
    public static void Load(SceneNames targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(SceneNames.LoadingScene.ToString());
    }
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
