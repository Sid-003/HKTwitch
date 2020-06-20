using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;

namespace HollowTwitch.Precondition
{
    public class RequireSceneChangeAttribute : PreconditionAttribute
    {
        // They really are
        private string _sceneName = "i hate shoutout memes they are so god damn annoying";

        public override bool Check(string user)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (_sceneName == currentScene) return false;
            
            _sceneName = currentScene;
            
            return true;

        }
    }
}
