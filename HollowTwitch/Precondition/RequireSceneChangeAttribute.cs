using HollowTwitch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Precondition
{
    public class RequireSceneChangeAttribute : PreconditionAttribute
    {
        private string _sceneName = "i hate shoutout memes they are so god damn annoying";

        public override bool Check()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if(_sceneName != currentScene)
            {
                _sceneName = currentScene;
                return true;
            }
            return false;
        }
    }
}
